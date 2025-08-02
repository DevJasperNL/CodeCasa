using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Utilities.Entities
{
    /// <summary>
    /// State for an Enum Entity with specific types of Attributes
    /// </summary>
    public record EnumEntityState<TEnum, TAttributes> : EntityState<TAttributes>
        where TAttributes : class where TEnum : struct, Enum
    {
        private readonly Dictionary<string, TEnum> _valueToTypeMapper;

        /// <summary>Copy constructor from base class</summary>
        public EnumEntityState(EntityState source, Dictionary<string, TEnum> valueToTypeMapper) : base(source)
        {
            _valueToTypeMapper = valueToTypeMapper;
        }

        /// <summary>The state converted to TEnum if possible, null if it is not</summary>
        public new TEnum? State
        {
            get
            {
                var baseState = base.State;
                return baseState == null ||
                       !_valueToTypeMapper.TryGetValue(baseState,
                           out var state)
                    ? null
                    : state;
            }
        }
    }
}
