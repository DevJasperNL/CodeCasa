using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;
using Reactive.Boolean;

namespace CodeCasa.NetDaemon.Extensions.Observables;

public static partial class EntityExtensions
{
    /// <summary>
    /// Returns an observable that will automatically emit "false" if <paramref name="entity"/> does not emit a closed (false) itself within <paramref name="timeSpan"/> after emitting open (true).
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "false" even if the time did not pass during runtime.
    /// Resulting observable is distinct.
    /// </summary>
    public static IObservable<bool> LimitOpenDuration(this Entity entity, TimeSpan timeSpan,
        IScheduler scheduler) => entity.LimitTrueDuration(timeSpan, scheduler);

    /// <summary>
    /// Returns an observable that will automatically emit "false" if <paramref name="entity"/> does not emit an off (false) itself within <paramref name="timeSpan"/> after emitting on (true).
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "false" even if the time did not pass during runtime.
    /// Resulting observable is distinct.
    /// </summary>
    public static IObservable<bool> LimitOnDuration(this Entity entity, TimeSpan timeSpan,
        IScheduler scheduler) => entity.LimitTrueDuration(timeSpan, scheduler);

    /// <summary>
    /// Returns an observable that will automatically emit "false" if <paramref name="entity"/> does not emit an off (false) itself within <paramref name="timeSpan"/> after emitting on (true).
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "false" even if the time did not pass during runtime.
    /// Resulting observable is distinct.
    /// </summary>
    public static IObservable<bool> LimitTrueDuration(this Entity entity, TimeSpan timeSpan,
        IScheduler scheduler)
        => entity.LimitTrueDuration(timeSpan, scheduler, () => DateTime.UtcNow);

    internal static IObservable<bool> LimitTrueDuration(
        this Entity entity,
        TimeSpan timeSpan,
        IScheduler scheduler,
        Func<DateTime> utcNowProvider)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(scheduler);

        return Observable.Defer(() =>
        {
            var utcNow = utcNowProvider();
            var lastChangedUtc = entity.EntityState?.LastChanged?.ToUniversalTime();
            if (!entity.IsOn() || lastChangedUtc == null)
            {
                return entity.ToBooleanObservable().LimitTrueDuration(timeSpan, scheduler);
            }

            var moment = utcNow - timeSpan;
            var remainingTimeSpan = lastChangedUtc.Value - moment;
            return remainingTimeSpan.Ticks <= 0 ?
                entity.ToBooleanObservable().LimitTrueDuration(timeSpan, scheduler).Skip(1).Prepend(false) :
                entity.ToBooleanObservable().LimitTrueDuration(remainingTimeSpan, scheduler);
        });
    }

    /// <summary>
    /// Returns an observable that will automatically emit "false" if <paramref name="predicate"/> applied to the state of <paramref name="entity"/> does not emit a "false" itself within <paramref name="timeSpan"/> after emitting "true".
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "false" even if the time did not pass during runtime.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// Resulting observable is distinct.
    /// </summary>
    public static IObservable<bool> LimitTrueDuration(this Entity entity, TimeSpan timeSpan,
        Func<EntityState, bool> predicate, IScheduler scheduler)
        => entity.LimitTrueDuration(timeSpan, predicate, scheduler, () => DateTime.UtcNow);

    internal static IObservable<bool> LimitTrueDuration(this Entity entity, TimeSpan timeSpan,
        Func<EntityState, bool> predicate, IScheduler scheduler,
        Func<DateTime> utcNowProvider)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(scheduler);

        return Observable.Defer(() =>
        {
            var utcNow = utcNowProvider();
            var lastChangedUtc = entity.EntityState?.LastChanged?.ToUniversalTime();
            if (entity.EntityState == null || !predicate(entity.EntityState) || lastChangedUtc == null)
            {
                return entity.ToBooleanObservable(predicate).LimitTrueDuration(timeSpan, scheduler);
            }

            var moment = utcNow - timeSpan;
            var remainingTimeSpan = lastChangedUtc.Value - moment;
            return remainingTimeSpan.Ticks <= 0 ?
                entity.ToBooleanObservable(predicate).LimitTrueDuration(timeSpan, scheduler).Skip(1).Prepend(false) :
                entity.ToBooleanObservable(predicate).LimitTrueDuration(remainingTimeSpan, scheduler);
        });
    }

    /// <summary>
    /// Returns an observable that will automatically emit "false" if <paramref name="predicate"/> applied to the state of <paramref name="entity"/> does not emit a "false" itself within <paramref name="timeSpan"/> after emitting "true".
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "false" even if the time did not pass during runtime.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// Resulting observable is distinct.
    /// </summary>
    public static IObservable<bool> LimitTrueDuration<TEntity, TEntityState, TAttributes>(
        this Entity<TEntity, TEntityState, TAttributes> entity,
        TimeSpan timeSpan,
        Func<TEntityState, bool> predicate,
        IScheduler scheduler)
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
        => entity.LimitTrueDuration(timeSpan, predicate, scheduler, () => DateTime.UtcNow);

    internal static IObservable<bool> LimitTrueDuration<TEntity, TEntityState, TAttributes>(
        this Entity<TEntity, TEntityState, TAttributes> entity,
        TimeSpan timeSpan,
        Func<TEntityState, bool> predicate,
        IScheduler scheduler,
        Func<DateTime> utcNowProvider)
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(scheduler);

        return Observable.Defer(() =>
        {
            var utcNow = utcNowProvider();
            var lastChangedUtc = entity.EntityState?.LastChanged?.ToUniversalTime();
            if (entity.EntityState == null || !predicate(entity.EntityState) || lastChangedUtc == null)
            {
                return entity.ToBooleanObservable(predicate).LimitTrueDuration(timeSpan, scheduler);
            }

            var moment = utcNow - timeSpan;
            var remainingTimeSpan = lastChangedUtc.Value - moment;
            return remainingTimeSpan.Ticks <= 0 ?
                entity.ToBooleanObservable(predicate).LimitTrueDuration(timeSpan, scheduler).Skip(1).Prepend(false) :
                entity.ToBooleanObservable(predicate).LimitTrueDuration(remainingTimeSpan, scheduler);
        });
    }
}