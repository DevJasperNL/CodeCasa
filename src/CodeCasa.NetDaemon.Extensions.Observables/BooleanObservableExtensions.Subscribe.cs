using Reactive.Boolean;

namespace CodeCasa.NetDaemon.Extensions.Observables;

/// <summary>
/// Provides extension methods for <see cref="IObservable{Boolean}"/>.
/// </summary>
public static partial class BooleanObservableExtensions
{
    /// <summary>
    /// Subscribes actions to be executed when the observable emits true (open) or false (closed).
    ///
    /// Internally, this method uses the IsOn and IsOff extension methods to determine the entity's state. It works for entities whose states on/off state can be interpreted as open/closed. For entities like covers, which use "open" and "close" states, consider providing a custom predicate.
    /// </summary>
    public static IDisposable SubscribeOpenClosed(this IObservable<bool> observable, Action openAction, Action closedAction) => observable.SubscribeTrueFalse(openAction, closedAction);

    /// <summary>
    /// Subscribes an action to be executed when the observable emits true (open).
    ///
    /// Internally, this method uses the IsOn and IsOff extension methods to determine the entity's state. It works for entities whose states on/off state can be interpreted as open/closed. For entities like covers, which use "open" and "close" states, consider providing a custom predicate.
    /// </summary>
    public static IDisposable SubscribeOpen(this IObservable<bool> observable, Action openAction) => observable.SubscribeTrue(openAction);

    /// <summary>
    /// Subscribes an action to be executed when the observable emits true (closed).
    ///
    /// Internally, this method uses the IsOn and IsOff extension methods to determine the entity's state. It works for entities whose states on/off state can be interpreted as open/closed. For entities like covers, which use "open" and "close" states, consider providing a custom predicate.
    /// </summary>
    public static IDisposable SubscribeClosed(this IObservable<bool> observable, Action closedAction) => observable.SubscribeFalse(closedAction);

    /// <summary>
    /// Subscribes actions to be executed when the observable emits true (on) or false (off).
    /// </summary>
    public static IDisposable SubscribeOnOff(this IObservable<bool> observable, Action onAction, Action offAction) => observable.SubscribeTrueFalse(onAction, offAction);

    /// <summary>
    /// Subscribes an action to be executed when the observable emits true (on).
    /// </summary>
    public static IDisposable SubscribeOn(this IObservable<bool> observable, Action onAction) => observable.SubscribeTrue(onAction);

    /// <summary>
    /// Subscribes an action to be executed when the observable emits false (off).
    /// </summary>
    public static IDisposable SubscribeOff(this IObservable<bool> observable, Action offAction) => observable.SubscribeFalse(offAction);
}