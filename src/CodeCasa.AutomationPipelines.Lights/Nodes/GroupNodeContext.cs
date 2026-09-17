using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using CodeCasa.Lights;
using Microsoft.Extensions.Logging;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    /*
     * Group bookkeeping is protected by a single lock, but nothing that calls out of this class (applying a transition to
     * the group entity or setting a member's output, which runs the member pipeline and its output handler) is executed
     * while holding it. Those actions are queued in order while the lock is held and drained afterwards by one thread at a
     * time, so a feedback path back into Process from another thread cannot deadlock and outputs keep their order.
     */
    internal class GroupNodeContext(IScheduler scheduler, ILogger<Pipeline<LightTransition>>? logger)
    {
        private readonly List<GroupInfo> _groups = new();
        private readonly Lock _lock = new();
        private readonly ConcurrentQueue<Action> _pendingActions = new();
        private int _drainingThreadId;

        private IScheduler Scheduler => scheduler;
        private ILogger<Pipeline<LightTransition>>? Logger => logger;

        public void Register(GroupNode groupNode, ILight lightGroup, TimeSpan groupDuration, EqualityComparer<LightTransition> equalityComparer)
        {
            lock (_lock)
            {
                // Matched by id: separate UseLightGroup calls may pass different instances for the same group entity.
                var existingGroup = _groups.FirstOrDefault(g => g.LightGroup.Id == lightGroup.Id);
                if (existingGroup == null)
                {
                    existingGroup = new GroupInfo(this, lightGroup, groupNode, equalityComparer, groupDuration);
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
            RunPendingActions();
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

        private void OnGroupWindowElapsed(GroupInfo group, InputInfo inputInfo)
        {
            lock (_lock)
            {
                group.ExecuteIfStillPending(inputInfo);
            }
            RunPendingActions();
        }

        private void EnqueueAction(Action action) => _pendingActions.Enqueue(action);

        private void RunPendingActions()
        {
            var currentThreadId = Environment.CurrentManagedThreadId;
            while (!_pendingActions.IsEmpty && Interlocked.CompareExchange(ref _drainingThreadId, currentThreadId, 0) == 0)
            {
                try
                {
                    while (_pendingActions.TryDequeue(out var action))
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception e)
                        {
                            logger?.LogError(e, "Applying a light group transition failed.");
                        }
                    }
                }
                finally
                {
                    Volatile.Write(ref _drainingThreadId, 0);
                }
            }
        }

        internal class InputInfo(DateTime timestamp, GroupNode groupNode, LightTransition lightTransition)
        {
            public LightTransition Transition { get; } = lightTransition;
            public GroupNode GroupNode { get; } = groupNode;
            public DateTime Timestamp { get; } = timestamp;
            public bool HasExecuted { get; private set; }

            public void Execute(GroupNodeContext context, bool appliedByGroup = false)
            {
                if (!MarkExecuted())
                {
                    return;
                }
                context.EnqueueAction(() => GroupNode.SetOutput(Transition, appliedByGroup));
            }

            /// <summary>
            /// Marks this input as handled, so it is not applied twice when its light is part of several groups.
            /// Returns false when it was already handled.
            /// </summary>
            public bool MarkExecuted()
            {
                if (HasExecuted)
                {
                    return false;
                }
                HasExecuted = true;
                return true;
            }
        }

        /// <summary>
        /// Tracks the pending inputs of one light group. All members are only accessed while holding the context lock.
        /// </summary>
        internal class GroupInfo(
            GroupNodeContext context,
            ILight lightGroup,
            GroupNode firstGroupNode,
            IEqualityComparer<LightTransition> equalityComparer,
            TimeSpan groupDuration)
            : IDisposable
        {
            public ILight LightGroup { get; } = lightGroup;
            public TimeSpan GroupDuration { get; } = groupDuration;
            public IEqualityComparer<LightTransition> EqualityComparer { get; } = equalityComparer;
            private readonly List<GroupNode> _groupNodes = [firstGroupNode];
            private readonly Dictionary<GroupNode, InputInfo> _groupInputs = new();
            private readonly Dictionary<GroupNode, IDisposable> _scheduledWork = new();

            public void AddMember(GroupNode groupNode)
            {
                _groupNodes.Add(groupNode);
            }

            public bool RemoveMember(GroupNode groupNode)
            {
                _groupNodes.Remove(groupNode);
                _groupInputs.Remove(groupNode);
                CleanupScheduledWork(groupNode);

                return !_groupNodes.Any();
            }

            public void Process(InputInfo inputInfo)
            {
                if (!_groupNodes.Contains(inputInfo.GroupNode))
                {
                    return;
                }

                CleanupExpiredInputs(inputInfo.Timestamp);

                // A newer input for the same light supersedes the pending one; the pending transition is never applied.
                CleanupScheduledWork(inputInfo.GroupNode);
                _groupInputs[inputInfo.GroupNode] = inputInfo;

                if (AllMembersHaveMatchingTransitions(inputInfo.Transition))
                {
                    var groupInputs = _groupInputs.Values.ToArray();
                    _groupInputs.Clear();
                    CleanupAllScheduledWork();

                    var inputsToApply = groupInputs.Where(groupInput => groupInput.MarkExecuted()).ToArray();
                    if (groupInputs.All(groupInput => groupInput.GroupNode.IsSuppressedAsDuplicate(groupInput.Transition)))
                    {
                        // Every member pipeline would suppress this transition as a duplicate, so the group should not receive it either.
                        context.Logger?.LogTrace($"Group [{LightGroup.Id}] not used. Transition equals the current output of all members: {inputInfo.Transition}");
                        context.EnqueueAction(() => SetMemberOutputs(inputsToApply, appliedByGroup: true));
                    }
                    else
                    {
                        var transition = inputInfo.Transition;
                        context.EnqueueAction(() =>
                        {
                            // Whether the members may skip their individual transition is only known once the group call succeeded.
                            var appliedByGroup = true;
                            try
                            {
                                LightGroup.ApplyTransition(transition);
                                context.Logger?.LogInformation($"Group [{LightGroup.Id}] used. All members have matching transition: {transition}");
                            }
                            catch (Exception e)
                            {
                                context.Logger?.LogError(e, $"Group [{LightGroup.Id}] could not be used. Applying the transition to its members individually.");
                                appliedByGroup = false;
                            }
                            SetMemberOutputs(inputsToApply, appliedByGroup);
                        });
                    }

                    return;
                }

                // Apply this input individually if no group consensus is reached within the window.
                _scheduledWork[inputInfo.GroupNode] = context.Scheduler.Schedule(GroupDuration, () => context.OnGroupWindowElapsed(this, inputInfo));
            }

            public void ExecuteIfStillPending(InputInfo inputInfo)
            {
                if (!_groupInputs.TryGetValue(inputInfo.GroupNode, out var pendingInput) || !ReferenceEquals(pendingInput, inputInfo))
                {
                    // Superseded or already applied; any scheduled work for the newer input stays in place.
                    return;
                }

                pendingInput.Execute(context);
                _groupInputs.Remove(inputInfo.GroupNode);
                _scheduledWork.Remove(inputInfo.GroupNode);
            }

            /// <summary>
            /// Member pipelines always need to see the transition as their output (distinct comparison, telemetry,
            /// <see cref="Pipeline.LightPipelineContext"/>). Only when the group entity received it is the individual light call skipped.
            /// </summary>
            private void SetMemberOutputs(InputInfo[] inputs, bool appliedByGroup)
            {
                foreach (var input in inputs)
                {
                    // Setting the output drives the member light, so one failing light must not keep the others from being driven.
                    try
                    {
                        input.GroupNode.SetOutput(input.Transition, appliedByGroup);
                    }
                    catch (Exception e)
                    {
                        context.Logger?.LogError(e, $"Applying a transition to a member of group [{LightGroup.Id}] failed.");
                    }
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
                        info.Execute(context);
                        _groupInputs.Remove(info.GroupNode);
                        CleanupScheduledWork(info.GroupNode);
                    }
                }
            }

            private bool AllMembersHaveMatchingTransitions(LightTransition transition)
            {
                if (_groupInputs.Count != _groupNodes.Count)
                {
                    return false;
                }

                return _groupInputs.Values.All(info => EqualityComparer.Equals(info.Transition, transition));
            }

            private void CleanupScheduledWork(GroupNode groupNode)
            {
                if (_scheduledWork.Remove(groupNode, out var disposable))
                {
                    disposable.Dispose();
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
                CleanupAllScheduledWork();
            }
        }
    }
}
