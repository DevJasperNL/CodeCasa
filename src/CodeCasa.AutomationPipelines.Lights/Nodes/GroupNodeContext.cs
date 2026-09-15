using System.Reactive.Concurrency;
using CodeCasa.Lights;
using Microsoft.Extensions.Logging;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    internal class GroupNodeContext(IScheduler scheduler, ILogger<Pipeline<LightTransition>>? logger)
    {
        private readonly List<GroupInfo> _groups = new();
        private readonly Lock _lock = new();

        public void Register(GroupNode groupNode, ILight lightGroup, TimeSpan groupDuration, EqualityComparer<LightTransition> equalityComparer)
        {
            lock (_lock)
            {
                // Matched by id: separate UseLightGroup calls may pass different instances for the same group entity.
                var existingGroup = _groups.FirstOrDefault(g => g.LightGroup.Id == lightGroup.Id);
                if (existingGroup == null)
                {
                    existingGroup = new GroupInfo(lightGroup, groupNode, equalityComparer, groupDuration, scheduler, logger);
                    _groups.Add(existingGroup);
                }
                else
                {
                    if (existingGroup.GroupDuration != groupDuration || !Equals(existingGroup.EqualityComparer, equalityComparer))
                    {
                        throw new InvalidOperationException(
                            $"Light group {lightGroup.Id} is used with different time spans or comparers. Use the same settings for every light in the group.");
                    }
                    existingGroup.AddMember(groupNode);
                }
            }
        }

        public void Process(GroupNode groupNode, LightTransition transition)
        {
            var inputInfo = new InputInfo(scheduler.Now.UtcDateTime, groupNode, transition);
            lock (_lock)
            {
                foreach (var group in _groups)
                {
                    group.Process(inputInfo);
                }
            }
            
        }

        public void Unregister(GroupNode groupNode)
        {
            lock (_lock)
            {
                foreach (var group in _groups.ToArray())
                {
                    if (group.RemoveMember(groupNode))
                    {
                        _groups.Remove(group);
                        group.Dispose();
                    }
                }
            }
        }

        internal class InputInfo(DateTime timestamp, GroupNode groupNode, LightTransition lightTransition)
        {
            public LightTransition Transition { get; } = lightTransition;
            public GroupNode GroupNode { get; } = groupNode;
            public DateTime Timestamp { get; } = timestamp;
            public bool HasExecuted { get; private set; }
            public void Execute(bool appliedByGroup = false)
            {
                if (HasExecuted)
                {
                    return;
                }
                HasExecuted = true;
                GroupNode.SetOutput(Transition, appliedByGroup);
            }
        }

        internal class GroupInfo(
            ILight lightGroup,
            GroupNode firstGroupNode,
            IEqualityComparer<LightTransition> equalityComparer,
            TimeSpan groupDuration,
            IScheduler scheduler,
            ILogger<Pipeline<LightTransition>>? logger)
            : IDisposable
        {
            public ILight LightGroup { get; } = lightGroup;
            public TimeSpan GroupDuration { get; } = groupDuration;
            public IEqualityComparer<LightTransition> EqualityComparer { get; } = equalityComparer;
            private readonly List<GroupNode> _groupNodes = [firstGroupNode];
            private readonly Dictionary<GroupNode, InputInfo> _groupInputs = new();
            private readonly Dictionary<GroupNode, IDisposable> _scheduledWork = new();
            private readonly Lock _lock = new();

            public void AddMember(GroupNode groupNode)
            {
                lock (_lock)
                {
                    _groupNodes.Add(groupNode);
                }
            }

            public bool RemoveMember(GroupNode groupNode)
            {
                lock (_lock)
                {
                    _groupNodes.Remove(groupNode);
                    _groupInputs.Remove(groupNode);
                    CleanupScheduledWork(groupNode);

                    return !_groupNodes.Any();
                }
            }

            public void Process(InputInfo inputInfo)
            {
                lock (_lock)
                {
                    if (!_groupNodes.Contains(inputInfo.GroupNode))
                    {
                        return;
                    }

                    // Clean up expired or executed inputs
                    CleanupExpiredInputs(inputInfo.Timestamp);

                    // If there's an existing input for this light, execute it first
                    if (_groupInputs.TryGetValue(inputInfo.GroupNode, out var existingInput))
                    {
                        existingInput.Execute();
                        CleanupScheduledWork(inputInfo.GroupNode);
                    }

                    // Add the new input
                    _groupInputs[inputInfo.GroupNode] = inputInfo;

                    // Check if all group members now have matching transitions
                    if (AllMembersHaveMatchingTransitions(inputInfo.Transition))
                    {
                        // All members are in sync - apply to the group instead
                        var groupInputs = _groupInputs.Values.ToArray();
                        _groupInputs.Clear();
                        CleanupAllScheduledWork();
                        if (groupInputs.All(groupInput => groupInput.GroupNode.IsSuppressedAsDuplicate(groupInput.Transition)))
                        {
                            // Every member pipeline would suppress this transition as a duplicate, so the group should not receive it either.
                            logger?.LogTrace($"Group [{LightGroup.Id}] not used. Transition equals the current output of all members: {inputInfo.Transition}");
                        }
                        else
                        {
                            logger?.LogInformation($"Group [{LightGroup.Id}] used. All members have matching transition: {inputInfo.Transition}");
                            LightGroup.ApplyTransition(inputInfo.Transition);
                        }

                        // Member pipelines still need to see the transition as their output (distinct comparison,
                        // telemetry, LightPipelineContext); only the individual light call is skipped.
                        foreach (var groupInput in groupInputs)
                        {
                            groupInput.Execute(appliedByGroup: true);
                        }
                        return;
                    }

                    // Schedule this input for individual execution if no group consensus is reached
                    var scheduledWork = scheduler.Schedule(GroupDuration, () =>
                    {
                        lock (_lock)
                        {
                            if (_groupInputs.TryGetValue(inputInfo.GroupNode, out var info) && !info.HasExecuted)
                            {
                                info.Execute();
                                _groupInputs.Remove(inputInfo.GroupNode);
                            }
                            _scheduledWork.Remove(inputInfo.GroupNode);
                        }
                    });
                    _scheduledWork[inputInfo.GroupNode] = scheduledWork;
                }
            }

            private void CleanupExpiredInputs(DateTime currentTime)
            {
                foreach (var kvp in _groupInputs.ToArray())
                {
                    var info = kvp.Value;
                    if (info.HasExecuted)
                    {
                        // This can occur if the light is in multiple groups at once.
                        _groupInputs.Remove(info.GroupNode);
                        CleanupScheduledWork(info.GroupNode);
                    }
                    else if (info.Timestamp + GroupDuration < currentTime)
                    {
                        // We waited long enough for this light to be part of the group,
                        // but it never received a transition that matched the other lights in the group.
                        info.Execute();
                        _groupInputs.Remove(info.GroupNode);
                        CleanupScheduledWork(info.GroupNode);
                    }
                }
            }

            private bool AllMembersHaveMatchingTransitions(LightTransition transition)
            {
                // We need inputs from ALL group nodes
                if (_groupInputs.Count != _groupNodes.Count)
                {
                    return false;
                }

                // All inputs must have matching transitions
                return _groupInputs.Values.All(info => EqualityComparer.Equals(info.Transition, transition));
            }

            private void CleanupScheduledWork(GroupNode groupNode)
            {
                if (_scheduledWork.TryGetValue(groupNode, out var disposable))
                {
                    disposable.Dispose();
                    _scheduledWork.Remove(groupNode);
                }
            }

            private void CleanupAllScheduledWork()
            {
                foreach (var disposable in _scheduledWork.Values)
                {
                    disposable.Dispose();
                }
                _scheduledWork.Clear();
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    CleanupAllScheduledWork();
                }
            }
        }
    }
}
