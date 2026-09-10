using System.Reactive.Disposables;
using NetDaemon.HassModel.Entities;
using Reactive.Boolean;

namespace CodeCasa.NetDaemon.Extensions.Observables;

public static partial class BooleanObservableExtensions
{
    /// <summary>
    /// Binds an observable boolean sequence to a single entity's on/off state using generic Home Assistant actions.
    /// When the observable emits <c>true</c>, the entity will be turned on; when it emits <c>false</c>, it will be turned off.
    /// </summary>
    /// <param name="observable">The observable sequence of boolean values.</param>
    /// <param name="entity">The entity to control.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription. Dispose to stop listening to the observable.</returns>
    public static IDisposable BindToOnOff(this IObservable<bool> observable, IEntityCore entity)
    {
        return observable.SubscribeTrueFalse(
            () => entity.CallService("homeassistant.turn_on"),
            () => entity.CallService("homeassistant.turn_off"));
    }

    /// <summary>
    /// Binds an observable boolean sequence to multiple entities' on/off states using generic Home Assistant actions.
    /// When the observable emits <c>true</c>, all entities will be turned on; when it emits <c>false</c>, all entities will be turned off.
    /// </summary>
    /// <param name="observable">The observable sequence of boolean values.</param>
    /// <param name="entities">The collection of entities to control. Must contain at least one entity.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription. Dispose to stop listening to the observable.</returns>
    public static IDisposable BindToOnOff(this IObservable<bool> observable, IEnumerable<IEntityCore> entities)
    {
        var entityArray = entities.ToArray();
        if (!entityArray.Any())
        {
            return Disposable.Empty;
        }

        return observable.SubscribeTrueFalse(() =>
        {
            entityArray.CallService("homeassistant.turn_on");
        }, () =>
        {
            entityArray.CallService("homeassistant.turn_off");
        });
    }
}