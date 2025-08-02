using System.Reactive.Linq;
using CodeCasa.Shared.Extensions;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Utilities.Entities
{
    public record
        EnumEntity<TEnum, TEntity, TEntityState, TAttributes> : Entity<TEntity, TEntityState, TAttributes>
        where TEntity : EnumEntity<TEnum, TEntity, TEntityState, TAttributes>
        where TEntityState : EnumEntityState<TEnum, TAttributes>
        where TAttributes : class
        where TEnum : struct, Enum
    {
        private readonly Dictionary<TEnum, string> _typeToValueMapper;
        private readonly Dictionary<string, TEnum> _valueToTypeMapper;

        public EnumEntity(IHaContext haContext, string entityId)
            : base(haContext, entityId)
        {
            _typeToValueMapper = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .ToDictionary(value => value, value => value.ToString());
            _valueToTypeMapper = _typeToValueMapper.Inverse(StringComparer.OrdinalIgnoreCase);
        }

        public EnumEntity(IEntityCore entity)
            : base(entity)
        {
            _typeToValueMapper = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .ToDictionary(value => value, value => value.ToString());
            _valueToTypeMapper = _typeToValueMapper.Inverse(StringComparer.OrdinalIgnoreCase);
        }

        public EnumEntity(IHaContext haContext, string entityId, Dictionary<TEnum, string> typeToValueMapper)
            : base(haContext, entityId)
        {
            _typeToValueMapper = typeToValueMapper;
            _valueToTypeMapper = _typeToValueMapper.Inverse(StringComparer.OrdinalIgnoreCase);
        }

        public EnumEntity(IEntityCore entity, Dictionary<TEnum, string> typeToValueMapper)
            : base(entity)
        {
            _typeToValueMapper = typeToValueMapper;
            _valueToTypeMapper = _typeToValueMapper.Inverse(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>The current state of this Entity converted to enum if possible, null if it is not</summary>
        public new TEnum? State => EntityState?.State;

        /// <summary>The full state of this Entity</summary>
        public override TEntityState? EntityState
        {
            get
            {
                var entityState = HaContext.GetState(EntityId);
                return entityState == null ? null : (TEntityState)new EnumEntityState<TEnum, TAttributes>(entityState, _valueToTypeMapper);
            }
        }

        public override IObservable<StateChange<TEntity, TEntityState>> StateAllChanges() =>
            HaContext.StateAllChanges().Where(e => e.Entity.EntityId == EntityId)
                .Select(e => new StateChange<TEntity, TEntityState>((TEntity)this,
                    e.Old == null
                        ? null
                        : (TEntityState)new EnumEntityState<TEnum, TAttributes>(e.Old, _valueToTypeMapper),
                    e.New == null
                        ? null
                        : (TEntityState)new EnumEntityState<TEnum, TAttributes>(e.New, _valueToTypeMapper)));

        /// <inheritdoc/>
        public override IObservable<StateChange<TEntity, TEntityState>> StateChanges() =>
            StateAllChanges().Where(c => !Nullable.Equals(c.New?.State, c.Old?.State));

        public string ConvertEnumToState(TEnum e) => _typeToValueMapper[e];
    }
}
