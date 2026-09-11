using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Extensions.Observables;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class ObservableExtensions
{
    /// <summary>
    /// Returns an observable that emits the same result as the source observable, but will repeat the last result when one of the provided entities becomes available.
    /// </summary>
    public static IObservable<T> RepeatWhenEntitiesBecomeAvailable<T>(this IObservable<T> observable,
        params Entity[] entities) => InternalRepeatWhenEntitiesBecomeAvailable(observable, entities);

    /// <summary>
    /// Returns an observable that emits the same result as the source observable, but will repeat the last result when one of the provided entities becomes available.
    /// </summary>
    public static IObservable<T> RepeatWhenEntitiesBecomeAvailable<T>(this IObservable<T> observable,
        IEnumerable<Entity> entities) => InternalRepeatWhenEntitiesBecomeAvailable(observable, entities);

    private static IObservable<T>
        InternalRepeatWhenEntitiesBecomeAvailable<T>(
            this IObservable<T> observable, IEnumerable<Entity> entities)
    {
        // Publish(selector) subscribes both consumers before connecting, so the source is subscribed exactly once and
        // a value emitted synchronously on subscribe still reaches the CombineLatest.
        return observable.Publish(shared =>
        {
            var triggers = entities
                .StateChanges()
                .CombineLatest(shared)
                .Where(values =>
                    values.First.Old?.State != null &&
                    values.First.New?.State != null &&
                    Constants.EntityUnavailableStates.Contains(values.First.Old.State,
                        StringComparer.OrdinalIgnoreCase) &&
                    !Constants.EntityUnavailableStates.Contains(values.First.New.State,
                        StringComparer.OrdinalIgnoreCase))
                .Select(values => values.Second);

            return shared.Merge(triggers);
        });
    }
}
