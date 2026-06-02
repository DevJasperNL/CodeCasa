using System.Reactive.Linq;
using CodeCasa.NetDaemon.TypedEntities.Extensions;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.TypedEntities;

/// <summary>
/// Entity that has a strongly typed (enum) State value
/// </summary>
public record
    EnumEntity<TEnum, TEntity, TEntityState, TAttributes> : Entity<TEntity, TEntityState, TAttributes>
    where TEntity : EnumEntity<TEnum, TEntity, TEntityState, TAttributes>
    where TEntityState : EnumEntityState<TEnum, TAttributes>
    where TAttributes : class
    where TEnum : struct, Enum
{
    private readonly Func<TEnum, string> _typeToValueFunc;
    private readonly Func<string, TEnum?> _valueToTypeFunc;

    /// <summary>
    /// Creates a new EnumEntity with the given entity ID. 
    /// Uses the enum names as string values by default.
    /// </summary>
    public EnumEntity(IHaContext haContext, string entityId)
        : base(haContext, entityId)
    {
        var typeToValueMap = Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .ToDictionary(value => value, value => value.ToString());
        var valueToTypeMap = typeToValueMap.Inverse(StringComparer.OrdinalIgnoreCase);
        _typeToValueFunc = e => typeToValueMap[e];
        _valueToTypeFunc = s => valueToTypeMap.TryGetValue(s, out var enumState) ? enumState : null;
    }

    /// <summary>
    /// Creates a new EnumEntity from an existing entity. 
    /// Uses the enum names as string values by default.
    /// </summary>
    public EnumEntity(IEntityCore entity)
        : base(entity)
    {
        var typeToValueMap = Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .ToDictionary(value => value, value => value.ToString());
        var valueToTypeMap = typeToValueMap.Inverse(StringComparer.OrdinalIgnoreCase);
        _typeToValueFunc = e => typeToValueMap[e];
        _valueToTypeFunc = s => valueToTypeMap.TryGetValue(s, out var enumState) ? enumState : null;
    }

    /// <summary>
    /// Creates a new EnumEntity with the given entity ID, using a custom enum-to-string mapping.
    /// </summary>
    public EnumEntity(IHaContext haContext, string entityId, Dictionary<TEnum, string> typeToValueMap)
        : base(haContext, entityId)
    {
        var valueToTypeMap = typeToValueMap.Inverse(StringComparer.OrdinalIgnoreCase);
        _typeToValueFunc = e => typeToValueMap[e];
        _valueToTypeFunc = s => valueToTypeMap.TryGetValue(s, out var enumState) ? enumState : null;
    }

    /// <summary>
    /// Creates a new EnumEntity from an existing entity, using a custom enum-to-string mapping.
    /// </summary>
    public EnumEntity(IEntityCore entity, Dictionary<TEnum, string> typeToValueMap)
        : base(entity)
    {
        var valueToTypeMap = typeToValueMap.Inverse(StringComparer.OrdinalIgnoreCase);
        _typeToValueFunc = e => typeToValueMap[e];
        _valueToTypeFunc = s => valueToTypeMap.TryGetValue(s, out var enumState) ? enumState : null;
    }

    /// <summary>
    /// Creates a new EnumEntity with the given entity ID, using custom conversion functions.
    /// </summary>
    public EnumEntity(IHaContext haContext, string entityId, Func<TEnum, string> typeToValueFunc, Func<string, TEnum?> valueToTypeFunc)
        : base(haContext, entityId)
    {
        _typeToValueFunc = typeToValueFunc;
        _valueToTypeFunc = valueToTypeFunc;
    }

    /// <summary>
    /// Creates a new EnumEntity from an existing entity, using custom conversion functions.
    /// </summary>
    public EnumEntity(IEntityCore entity, Func<TEnum, string> typeToValueFunc, Func<string, TEnum?> valueToTypeFunc)
        : base(entity)
    {
        _typeToValueFunc = typeToValueFunc;
        _valueToTypeFunc = valueToTypeFunc;
    }

    /// <summary>The current state of this Entity converted to enum if possible, null if it is not</summary>
    public new TEnum? State => EntityState?.State;

    /// <summary>The full state of this Entity</summary>
    public override TEntityState? EntityState
    {
        get
        {
            var entityState = HaContext.GetState(EntityId);
            return entityState == null ? null : (TEntityState)new EnumEntityState<TEnum, TAttributes>(entityState, _valueToTypeFunc);
        }
    }

    /// <summary>
    /// The observable state stream, all changes including attributes
    /// </summary>
    public override IObservable<StateChange<TEntity, TEntityState>> StateAllChanges() =>
        HaContext.StateAllChanges().Where(e => e.Entity.EntityId == EntityId)
            .Select(e => new StateChange<TEntity, TEntityState>((TEntity)this,
                e.Old == null
                    ? null
                    : (TEntityState)new EnumEntityState<TEnum, TAttributes>(e.Old, _valueToTypeFunc),
                e.New == null
                    ? null
                    : (TEntityState)new EnumEntityState<TEnum, TAttributes>(e.New, _valueToTypeFunc)));

    /// <inheritdoc/>
    public override IObservable<StateChange<TEntity, TEntityState>> StateChanges() =>
        StateAllChanges().Where(c => !Nullable.Equals(c.New?.State, c.Old?.State));

    /// <summary>
    /// Converts the specified enum value to its corresponding string state
    /// using the configured mapping function.
    /// </summary>
    /// <param name="e">The enum value to convert.</param>
    /// <returns>The string representation of the given enum value.</returns>
    public string ConvertEnumToState(TEnum e) => _typeToValueFunc(e);
}