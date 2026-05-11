using NetDaemon.HassModel.Entities;
using Reactive.Boolean;

namespace CodeCasa.NetDaemon.Extensions.Observables;

/// <summary>
/// Provides extension methods for <see cref="Entity"/>.
/// </summary>
public static partial class EntityExtensions
{
    /// <summary>
    /// Subscribes actions to be executed on the open or closed state of an entity.
    /// Initial state is also taken into account, meaning an action might execute immediately if the entity is currently open or closed.
    ///
    /// Internally, this method uses the IsOn and IsOff extension methods to determine the entity's state. It works for entities whose states on/off state can be interpreted as open/closed. For entities like covers, which use "open" and "close" states, consider providing a custom predicate.
    /// </summary>
    public static IDisposable SubscribeOpenClosed(this Entity entity, Action openAction, Action closedAction)
        => entity.ToBooleanObservable().SubscribeOpenClosed(openAction, closedAction);

    /// <summary>
    /// Subscribes actions to be executed on the open state of an entity.
    /// Initial state is also taken into account, meaning the action might execute immediately if the entity is currently open.
    ///
    /// Internally, this method uses the IsOn and IsOff extension methods to determine the entity's state. It works for entities whose states on/off state can be interpreted as open/closed. For entities like covers, which use "open" and "close" states, consider providing a custom predicate.
    /// </summary>
    public static IDisposable SubscribeOpen(this Entity entity, Action openAction)
        => entity.ToBooleanObservable().SubscribeOpen(openAction);

    /// <summary>
    /// Subscribes actions to be executed on the closed state of an entity.
    /// Initial state is also taken into account, meaning the action might execute immediately if the entity is currently closed.
    ///
    /// Internally, this method uses the IsOn and IsOff extension methods to determine the entity's state. It works for entities whose states on/off state can be interpreted as open/closed. For entities like covers, which use "open" and "close" states, consider providing a custom predicate.
    /// </summary>
    public static IDisposable SubscribeClosed(this Entity entity, Action closedAction)
        => entity.ToBooleanObservable().SubscribeClosed(closedAction);

    /// <summary>
    /// Subscribes actions to be executed on the on or off state of an entity.
    /// Initial state is also taken into account, meaning an action might execute immediately if the entity is currently on or off.
    /// </summary>
    public static IDisposable SubscribeOnOff(this Entity entity, Action onAction, Action offAction)
        => entity.ToBooleanObservable().SubscribeOnOff(onAction, offAction);

    /// <summary>
    /// Subscribes actions to be executed on the on state of an entity.
    /// Initial state is also taken into account, meaning the action might execute immediately if the entity is currently on.
    /// </summary>
    public static IDisposable SubscribeOn(this Entity entity, Action onAction)
        => entity.ToBooleanObservable().SubscribeOn(onAction);

    /// <summary>
    /// Subscribes actions to be executed on the off state of an entity.
    /// Initial state is also taken into account, meaning the action might execute immediately if the entity is currently off.
    /// </summary>
    public static IDisposable SubscribeOff(this Entity entity, Action offAction)
        => entity.ToBooleanObservable().SubscribeOff(offAction);

    /// <summary>
    /// Subscribes actions to be executed on the on or off state of an entity.
    /// Initial state is also taken into account, meaning an action might execute immediately if the entity is currently on or off.
    /// </summary>
    public static IDisposable SubscribeTrueFalse(this Entity entity, Action trueAction, Action falseAction)
        => entity.ToBooleanObservable().SubscribeTrueFalse(trueAction, falseAction);

    /// <summary>
    /// Subscribes actions to be executed on the result of the predicate applied to the state of an entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// Initial state is also taken into account, meaning an action might execute immediately.
    /// </summary>
    public static IDisposable SubscribeTrueFalse(this Entity entity, Action trueAction, Action falseAction, Func<EntityState, bool> predicate)
        => entity.ToBooleanObservable(predicate).SubscribeTrueFalse(trueAction, falseAction);

    /// <summary>
    /// Subscribes actions to be executed on the result of the predicate applied to the state of an entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// Initial state is also taken into account, meaning an action might execute immediately.
    /// </summary>
    public static IDisposable
        SubscribeTrueFalse<TEntity, TEntityState, TAttributes>(
            this Entity<TEntity, TEntityState, TAttributes> entity,
            Action trueAction, Action falseAction, Func<TEntityState, bool> predicate)
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class =>
        entity.ToBooleanObservable(predicate).SubscribeTrueFalse(trueAction, falseAction);
        
    /// <summary>
    /// Subscribes actions to be executed on the on state of an entity.
    /// Initial state is also taken into account, meaning the action might execute immediately if the entity is currently on.
    /// </summary>
    public static IDisposable SubscribeTrue(this Entity entity, Action trueAction)
        => entity.ToBooleanObservable().SubscribeTrue(trueAction);

    /// <summary>
    /// Subscribes actions to be executed on the off state of an entity.
    /// Initial state is also taken into account, meaning the action might execute immediately if the entity is currently off.
    /// </summary>
    public static IDisposable SubscribeFalse(this Entity entity, Action falseAction)
        => entity.ToBooleanObservable().SubscribeFalse(falseAction);
}