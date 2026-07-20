using System.Reactive.Concurrency;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    public class GroupNodeContext : IGroupNodeContext
    {
        private readonly IScheduler _scheduler;
        private readonly List<GroupInfo> _groups = new();

        public GroupNodeContext(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Register(ILight light, ILight lightGroup, TimeSpan groupDuration, EqualityComparer<LightTransition> equalityComparer)
        {
            var existingGroup = _groups.FirstOrDefault(g => g.LightGroup == lightGroup);
            if (existingGroup == null)
            {
                existingGroup = new GroupInfo(lightGroup, light, equalityComparer, groupDuration, _scheduler);
                _groups.Add(existingGroup);
            }
            else
            {
                existingGroup.AddMember(light);
            }
        }

        public void Process(ILight light, LightTransition transition)
        {
            var inputInfo = new InputInfo(DateTime.UtcNow, light, transition);
            foreach (var group in _groups)
            {
                group.Process(inputInfo);
            }
        }

        public void Unregister(ILight light)
        {
            foreach (var group in _groups.ToArray())
            {
                if (group.RemoveMember(light))
                {
                    _groups.Remove(group);
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

    public class GroupInfo
    {
        private readonly IEqualityComparer<LightTransition> _equalityComparer;
        public ILight LightGroup { get; }
        private readonly List<ILight> _groupMembers;
        private readonly Dictionary<ILight, InputInfo> _groupInputs = new();
        private readonly TimeSpan _groupDuration;
        private readonly IScheduler _scheduler;

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
            _groupMembers.Add(member);
        }

        public bool RemoveMember(ILight member)
        {
             _groupMembers.Remove(member);
             return _groupMembers.Any();
        }

        public IDisposable? Process(InputInfo inputInfo)
        {
            if (!_groupMembers.Contains(inputInfo.Light))
            {
                return null;
            }

            var utcNow = inputInfo.Timestamp;
            foreach (var info in _groupInputs.Values.ToArray())
            {
                if (info.HasExecuted)
                {
                    // This can occur if the light is in multiple groups at once.
                    _groupInputs.Remove(info.Light);
                    continue;
                }
                if (info.Timestamp + _groupDuration < utcNow)
                {
                    // We waited long enough for this light to be part of the group, but it never received a transition that matched the other lights in the group.
                    info.Execute();
                    _groupInputs.Remove(info.Light);
                }
            }
            if (_groupInputs.Where(kvp => kvp.Key != inputInfo.Light).All(kvp => _equalityComparer.Equals(kvp.Value.Transition, inputInfo.Transition)))
            {
                _groupInputs.Clear();
                LightGroup.ApplyTransition(inputInfo.Transition);
                return null;
            }

            if (_groupInputs.TryGetValue(inputInfo.Light, out var existingInput))
            {
                existingInput.Execute();
            }
            _groupInputs[inputInfo.Light] = inputInfo;
            return _scheduler.Schedule(_groupDuration, () =>
            {
                inputInfo.Execute();
            });
        }
    }

    internal interface IGroupNodeContext
    {
        void Process(ILight light, LightTransition input);
        void Unregister(ILight light);
    }
}
