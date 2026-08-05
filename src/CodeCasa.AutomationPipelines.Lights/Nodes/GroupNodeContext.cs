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
                var existingGroup = _groups.FirstOrDefault(g => g.LightGroup == lightGroup);
                if (existingGroup == null)
                {
                    existingGroup = new GroupInfo(lightGroup, groupNode, equalityComparer, groupDuration, scheduler, logger);
                    _groups.Add(existingGroup);
                }
                else
                {
                    existingGroup.AddMember(groupNode);
                }
            }
        }

        public void Process(GroupNode groupNode, LightTransition transition)
        {
            var inputInfo = new InputInfo(DateTime.UtcNow, groupNode, transition);
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
            public void Execute()
            {
                if (HasExecuted)
                {
                    return;
                }
                GroupNode.SetOutput(Transition);
                HasExecuted = true;
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
                        _groupInputs.Clear();
                        CleanupAllScheduledWork();
                        logger?.LogInformation($"Group [{LightGroup.Id}] used. All members have matching transition: {inputInfo.Transition}");
                        LightGroup.ApplyTransition(inputInfo.Transition);
                        return;
                    }

                    // Schedule this input for individual execution if no group consensus is reached
                    var scheduledWork = scheduler.Schedule(groupDuration, () =>
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
                    else if (info.Timestamp + groupDuration < currentTime)
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
                return _groupInputs.Values.All(info => equalityComparer.Equals(info.Transition, transition));
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
