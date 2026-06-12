using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Extensions.Observables;

public static partial class EntityExtensions
{
    /// <summary>
    /// <para>
    /// Returns a boolean observable that emits <see langword="true"/> when the entity opens 
    /// and emits <see langword="false"/> when the entity closes or changes to any other state.
    /// The observable is distinct until changed.
    /// </para>
    /// <para>
    /// Any state other than "on" (such as off, unknown, unavailable, or a null state when 
    /// the entity is removed) is treated as closed and will emit <see langword="false"/>.
    /// </para>
    /// </summary>
    public static IObservable<bool> ToOpenClosedObservable(this Entity entity) => entity.ToBooleanObservable();

    /// <summary>
    /// <para>
    /// Returns a boolean observable that emits <see langword="true"/> when the entity turns on 
    /// and emits <see langword="false"/> when the entity turns off or changes to any other state.
    /// The observable is distinct until changed.
    /// </para>
    /// <para>
    /// Any state other than "on" (such as off, unknown, unavailable, or a null state when 
    /// the entity is removed) is treated as off and will emit <see langword="false"/>.
    /// </para>
    /// </summary>
    public static IObservable<bool> ToOnOffObservable(this Entity entity) => entity.ToBooleanObservable();

    /// <summary>
    /// <para>
    /// Returns a boolean observable that emits true when the entity turns on and emits false when the entity changes to any other state (including off, unknown, unavailable or null).
    /// The observable is distinct until changed. 
    /// </para>
    /// <para>
    /// A null entity state, which typically indicates the entity has been removed, will emit false.
    /// </para>
    /// </summary>
    public static IObservable<bool> ToBooleanObservable(this Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity.Stateful()
            .Select(s => s.New?.IsOn() ?? false)
            .DistinctUntilChanged();
    }

    /// <summary>
    /// Returns a boolean observable that reflects the result of the provided predicate on the new state of the provided entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable is distinct until changed. A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// </summary>
    public static IObservable<bool> ToBooleanObservable(this Entity entity, Func<EntityState, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        return entity
            .StatefulAll()
            .Where(s => s.New != null)
            .Select(s => predicate(s.New!))
            .DistinctUntilChanged();
    }

    /// <summary>
    /// Returns a boolean observable that reflects the result of the provided predicate on the new state of the provided entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable is distinct until changed. A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// </summary>
    public static IObservable<bool>
        ToBooleanObservable<TEntity, TEntityState, TAttributes>(
            this Entity<TEntity, TEntityState, TAttributes> entity,
            Func<TEntityState, bool> predicate)
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        return entity
            .StatefulAll()
            .Where(s => s.New != null)
            .Select(s => predicate(s.New!))
            .DistinctUntilChanged();
    }

    /// <summary>
    /// <para>
    /// Returns a boolean observable that emits true when the entity turns on and emits false when the entity changes to any other state (including off, unknown, unavailable or null).
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// </para>
    /// <para>
    /// The observable only emits subsequent changes and will not emit the initial state.
    /// A null entity state, which typically indicates the entity has been removed, will emit false.
    /// </para>
    /// </summary>
    public static IObservable<bool> ToChangesOnlyBooleanObservable(this Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity.StateAllChanges()
            .Select(s => s.New?.IsOn() ?? false)
            .DistinctUntilChanged();
    }

    /// <summary>
    /// Returns a boolean observable that reflects the result of the provided predicate on the new state of the provided entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable only emits changes and will not emit the initial state.
    /// The observable is distinct until changed. A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// </summary>
    public static IObservable<bool> ToChangesOnlyBooleanObservable(this Entity entity, Func<EntityState, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        return entity
            .StateAllChanges()
            .Where(s => s.New != null)
            .Select(s => predicate(s.New!))
            .DistinctUntilChanged();
    }

    /// <summary>
    /// Returns a boolean observable that reflects the result of the provided predicate on the new state of the provided entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable only emits changes and will not emit the initial state.
    /// The observable is distinct until changed. A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// </summary>
    public static IObservable<bool>
        ToChangesOnlyBooleanObservable<TEntity, TEntityState, TAttributes>(
            this Entity<TEntity, TEntityState, TAttributes> entity,
            Func<TEntityState, bool> predicate)
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        return entity
            .StateAllChanges()
            .Where(s => s.New != null)
            .Select(s => predicate(s.New!))
            .DistinctUntilChanged();
    }
}