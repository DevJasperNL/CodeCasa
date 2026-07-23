using System.Reactive.Concurrency;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    public class GroupNodeContext : IGroupNodeContext
    {
        private readonly IScheduler _scheduler;
        private readonly List<GroupInfo> _groups = new();
        private readonly Lock _lock = new();

        public GroupNodeContext(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Register(ILight light, ILight lightGroup, TimeSpan groupDuration, EqualityComparer<LightTransition>? equalityComparer)
        {
            lock (_lock)
            {
                var existingGroup = _groups.FirstOrDefault(g => g.LightGroup == lightGroup);
                if (existingGroup == null)
                {
                    existingGroup = new GroupInfo(lightGroup, light, equalityComparer ?? EqualityComparer<LightTransition>.Default, groupDuration, _scheduler);
                    _groups.Add(existingGroup);
                }
                else
                {
                    existingGroup.AddMember(light);
                }
            }
        }

        public void Process(ILight light, LightTransition transition)
        {
            var inputInfo = new InputInfo(DateTime.UtcNow, light, transition);
            lock (_lock)
            {
                foreach (var group in _groups)
                {
                    group.Process(inputInfo);
                }
            }
            
        }

        public void Unregister(ILight light)
        {
            lock (_lock)
            {
                foreach (var group in _groups.ToArray())
                {
                    if (group.RemoveMember(light))
                    {
                        _groups.Remove(group);
                        group.Dispose();
                    }
                }
            }
        }
    }

    public class InputInfo(DateTime timestamp, ILight light, LightTransition lightTransition)
    {
        public LightTransition Transition { get; } = lightTransition;
        public ILight Light { get; } = light;
        public DateTime Timestamp { get; } = timestamp;
        public bool HasExecuted { get; private set; }
        public void Execute()
        {
            if (HasExecuted)
            {
                return;
            }
            Light.ApplyTransition(Transition);
            HasExecuted = true;
        }
    }

    public class GroupInfo : IDisposable
    {
        private readonly IEqualityComparer<LightTransition> _equalityComparer;
        public ILight LightGroup { get; }
        private readonly List<ILight> _groupMembers;
        private readonly Dictionary<ILight, InputInfo> _groupInputs = new();
        private readonly Dictionary<ILight, IDisposable> _scheduledWork = new();
        private readonly TimeSpan _groupDuration;
        private readonly IScheduler _scheduler;
        private readonly Lock _lock = new();

        public GroupInfo(ILight lightGroup, ILight firstGroupMember, IEqualityComparer<LightTransition> equalityComparer, TimeSpan groupDuration, IScheduler scheduler)
        {
            LightGroup = lightGroup;
            _groupMembers = new List<ILight> { firstGroupMember };
            _equalityComparer = equalityComparer;
            _scheduler = scheduler;
            _groupDuration = groupDuration;
        }

        public void AddMember(ILight member)
        {
            lock (_lock)
            {
                _groupMembers.Add(member);
            }
        }

        public bool RemoveMember(ILight member)
        {
            lock (_lock)
            {
                _groupMembers.Remove(member);
                _groupInputs.Remove(member);
                CleanupScheduledWork(member);

                return !_groupMembers.Any();
            }
        }

        public void Process(InputInfo inputInfo)
        {
            lock (_lock)
            {
                if (!_groupMembers.Contains(inputInfo.Light))
                {
                    return;
                }

                // Clean up expired or executed inputs
                CleanupExpiredInputs(inputInfo.Timestamp);

                // If there's an existing input for this light, execute it first
                if (_groupInputs.TryGetValue(inputInfo.Light, out var existingInput))
                {
                    existingInput.Execute();
                    CleanupScheduledWork(inputInfo.Light);
                }

                // Add the new input
                _groupInputs[inputInfo.Light] = inputInfo;

                // Check if all group members now have matching transitions
                if (AllMembersHaveMatchingTransitions(inputInfo.Transition))
                {
                    // All members are in sync - apply to the group instead
                    _groupInputs.Clear();
                    CleanupAllScheduledWork();
                    LightGroup.ApplyTransition(inputInfo.Transition);
                    return;
                }

                // Schedule this input for individual execution if no group consensus is reached
                var scheduledWork = _scheduler.Schedule(_groupDuration, () =>
                {
                    lock (_lock)
                    {
                        if (_groupInputs.TryGetValue(inputInfo.Light, out var info) && !info.HasExecuted)
                        {
                            info.Execute();
                            _groupInputs.Remove(inputInfo.Light);
                        }
                        _scheduledWork.Remove(inputInfo.Light);
                    }
                });
                _scheduledWork[inputInfo.Light] = scheduledWork;
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
                    _groupInputs.Remove(info.Light);
                    CleanupScheduledWork(info.Light);
                }
                else if (info.Timestamp + _groupDuration < currentTime)
                {
                    // We waited long enough for this light to be part of the group,
                    // but it never received a transition that matched the other lights in the group.
                    info.Execute();
                    _groupInputs.Remove(info.Light);
                    CleanupScheduledWork(info.Light);
                }
            }
        }

        private bool AllMembersHaveMatchingTransitions(LightTransition transition)
        {
            // We need inputs from ALL group members
            if (_groupInputs.Count != _groupMembers.Count)
            {
                return false;
            }

            // All inputs must have matching transitions
            return _groupInputs.Values.All(info => _equalityComparer.Equals(info.Transition, transition));
        }

        private void CleanupScheduledWork(ILight light)
        {
            if (_scheduledWork.TryGetValue(light, out var disposable))
            {
                disposable.Dispose();
                _scheduledWork.Remove(light);
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

    internal interface IGroupNodeContext
    {
        void Process(ILight light, LightTransition input);
        void Unregister(ILight light);
    }
}
