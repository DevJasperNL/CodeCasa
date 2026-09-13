using CodeCasa.Lights.Extensions;
using Occurify;
using Occurify.Extensions;
using Occurify.Reactive.Extensions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.Lights.Timelines.Extensions;

/// <summary>
/// Provides reactive extension methods for <see cref="Dictionary{TKey, TValue}"/> collections
/// where the keys are <see cref="ITimeline"/> instances.
/// </summary>
public static class TimelineValueCollectionExtensions
{
    /// <summary>
    /// Converts a timeline dictionary into an observable stream of <see cref="LightTransition"/> objects,
    /// including an immediate interpolated starting value.
    /// </summary>
    /// <param name="sceneTimeline">The dictionary mapping timeline points to <see cref="LightParameters"/>.</param>
    /// <param name="scheduler">The Rx scheduler used to manage timing and initial delay.</param>
    /// <param name="transitionTimeForTimelineState">
    /// The duration of the initial fade from current state. Defaults to 400ms if null.
    /// </param>
    /// <returns>An observable that emits the current interpolated state, then follows the scheduled timeline.</returns>
    /// <remarks>
    /// The current time is read from <paramref name="scheduler"/> when the observable is subscribed to.
    /// When several timelines share an instant, the value of the first of them in enumeration order is used.
    /// </remarks>
    public static IObservable<LightTransition> ToLightTransitionObservableIncludingCurrent(
        this Dictionary<ITimeline, LightParameters> sceneTimeline,
        IScheduler scheduler,
        TimeSpan? transitionTimeForTimelineState = null)
    {
        return CreateTimelineObservableIncludingInitialInterpolatedValue(sceneTimeline,
            (lightParameters, transitionTime) => lightParameters.AsTransition(transitionTime),
            (previous, next, fraction) => previous.Interpolate(next, fraction),
            EqualityComparer<LightParameters>.Default,
            scheduler,
            transitionTimeForTimelineState);
    }

    /// <summary>
    /// Converts a nested timeline dictionary into an observable stream of light scenes,
    /// where each emission contains a dictionary of transitions for multiple light sources.
    /// </summary>
    /// <param name="sceneTimeline">A dictionary mapping timeline points to a collection of light states keyed by ID.</param>
    /// <param name="scheduler">The Rx scheduler used to manage timing and initial delay.</param>
    /// <param name="transitionTimeForTimelineState">
    /// The duration of the initial fade for all lights in the scene. Defaults to 400ms if null.
    /// </param>
    /// <returns>An observable that emits a dictionary of transitions representing the current scene state, followed by scheduled updates.</returns>
    /// <remarks>
    /// This method utilizes a custom dictionary comparer to ensure updates are only emitted when
    /// at least one light in the scene has changed its parameters.
    /// A light that is only present in one of two consecutive scenes keeps the parameters of that scene in the interpolated starting value.
    /// The current time is read from <paramref name="scheduler"/> when the observable is subscribed to.
    /// When several timelines share an instant, the value of the first of them in enumeration order is used.
    /// </remarks>
    public static IObservable<Dictionary<string, LightTransition>> ToLightTransitionSceneObservableIncludingCurrent(
        this Dictionary<ITimeline, Dictionary<string, LightParameters>> sceneTimeline,
        IScheduler scheduler,
        TimeSpan? transitionTimeForTimelineState = null)
    {
        return CreateTimelineObservableIncludingInitialInterpolatedValue(sceneTimeline,
            (lightParametersDict, transitionTime) => lightParametersDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsTransition(transitionTime)),
            InterpolateScene,
            new DictionaryComparer<string, LightParameters>(EqualityComparer<LightParameters>.Default),
            scheduler,
            transitionTimeForTimelineState);
    }

    private static Dictionary<string, LightParameters> InterpolateScene(
        Dictionary<string, LightParameters> previousScene,
        Dictionary<string, LightParameters> nextScene,
        double fraction)
    {
        return previousScene.Keys.Union(nextScene.Keys).ToDictionary(lightId => lightId, lightId =>
            previousScene.TryGetValue(lightId, out var previous) && nextScene.TryGetValue(lightId, out var next)
                ? previous.Interpolate(next, fraction)
                : previousScene.GetValueOrDefault(lightId) ?? nextScene[lightId]);
    }

    /// <summary>
    /// Creates an observable that immediately emits an interpolated initial state based on the scheduler's current time,
    /// followed by the standard timeline transitions after a specified delay.
    /// </summary>
    private static IObservable<TOut> CreateTimelineObservableIncludingInitialInterpolatedValue<TIn, TOut>(
        Dictionary<ITimeline, TIn> sceneTimeline,
        Func<TIn, TimeSpan, TOut> transformer,
        Func<TIn, TIn, double, TIn> interpolator,
        IEqualityComparer<TIn> comparer,
        IScheduler scheduler,
        TimeSpan? transitionTimeForTimelineState)
    {
        var timeline = sceneTimeline.ToArray();
        var fadeTime = transitionTimeForTimelineState ?? TimeSpan.FromMilliseconds(400);

        // Deferred so the current time (and everything derived from it) is evaluated per subscription, not when the observable is composed.
        return Observable.Defer(Evaluate);

        IObservable<TOut> Evaluate()
        {
            var now = scheduler.Now.UtcDateTime;
            var valuesAtPrevious = timeline.GetValuesAtPreviousUtcInstant(now);
            var valuesAtCurrentOrNext = timeline.GetValuesAtCurrentOrNextUtcInstant(now);

            TIn sceneNow;
            TIn sceneBefore;
            if (valuesAtCurrentOrNext.Key == now)
            {
                sceneNow = valuesAtCurrentOrNext.Value.First();
                sceneBefore = sceneNow;
            }
            else if (valuesAtPrevious.Key == null)
            {
                if (valuesAtCurrentOrNext.Key == null)
                {
                    return Observable.Empty<TOut>();
                }

                // The timeline has not started yet, so there is nothing to interpolate. Re-evaluate once the first instant is reached.
                return Observable.Timer(valuesAtCurrentOrNext.Key.Value, scheduler).SelectMany(_ => Observable.Defer(Evaluate));
            }
            else if (valuesAtCurrentOrNext.Key == null)
            {
                // The timeline has ended: hold its last value.
                sceneNow = valuesAtPrevious.Value.First();
                sceneBefore = sceneNow;
            }
            else
            {
                sceneBefore = valuesAtPrevious.Value.First();
                var sceneNext = valuesAtCurrentOrNext.Value.First();
                var fraction = CalculateFraction(valuesAtPrevious.Key.Value, valuesAtCurrentOrNext.Key.Value, now);
                sceneNow = interpolator(sceneBefore, sceneNext, fraction);
            }

            // The timeline is a continuous ramp: every sample emits the *next* value with the time until it is reached.
            // After the initial catch-up fade the ramp towards the next instant has to be resumed explicitly, otherwise the
            // light would hold the interpolated value until that instant arrives.
            var valuesAtNext = timeline.GetValuesAtNextUtcInstant(now);
            var resumeRamp = Observable.Empty<TOut>();
            if (valuesAtNext.Key != null)
            {
                var remainingUntilNext = valuesAtNext.Key.Value - now - fadeTime;
                var sceneNext = valuesAtNext.Value.First();
                if (remainingUntilNext <= TimeSpan.Zero)
                {
                    // The next instant is reached during the initial fade, so fade to its value directly. Only the sample of the
                    // instant before it would emit that value, and that sample lies before the subscription; without this the
                    // final value of a timeline would never be applied.
                    sceneNow = sceneNext;
                }
                else if (!comparer.Equals(sceneBefore, sceneNext))
                {
                    resumeRamp = Observable.Return(transformer(sceneNext, remainingUntilNext));
                }
            }

            // Sampling is anchored to the subscription time rather than to the moment the fade ends, so instants inside
            // the fade window are still emitted (immediately after the fade) instead of being skipped.
            var timelineObservable = CreateTimelineObservable(timeline, now, transformer, comparer, scheduler);

            // We delay the timeline observable to allow the initial scene transition to be emitted/activated first.
            var delayedTimelineObservable = Observable
                .Timer(fadeTime, scheduler)
                .SelectMany(_ => resumeRamp.Concat(timelineObservable));

            return Observable.Return(transformer(sceneNow, fadeTime)).Concat(delayedTimelineObservable);
        }
    }

    /// <summary>
    /// Creates an observable stream that emits transformed values based on state transitions
    /// between consecutive instants in a timeline, starting with the first instant after <paramref name="relativeTo"/>.
    /// </summary>
    private static IObservable<TOut> CreateTimelineObservable<TIn, TOut>(
        KeyValuePair<ITimeline, TIn>[] timeline,
        DateTime relativeTo,
        Func<TIn, TimeSpan, TOut> transformer,
        IEqualityComparer<TIn> comparer,
        IScheduler scheduler)
    {
        return timeline
            .ToSampleObservable(relativeTo, scheduler, emitSampleUponSubscribe: false)
            .SelectMany(s =>
            {
                var nextValues = timeline.GetValuesAtNextUtcInstant(s.Key);
                if (nextValues.Key == null)
                {
                    return [];
                }

                var current = s.Value.First();
                var next = nextValues.Value.First();
                if (comparer.Equals(current, next))
                {
                    return [];
                }

                // The sample may be handled after its instant (scheduler lag, or an instant that fell inside the initial fade);
                // the ramp still has to end at the next instant.
                var transitionTimeSpan = nextValues.Key.Value - scheduler.Now.UtcDateTime;
                if (transitionTimeSpan < TimeSpan.Zero)
                {
                    transitionTimeSpan = TimeSpan.Zero;
                }

                return new[] { transformer(next, transitionTimeSpan) };
            });
    }

    private static double CalculateFraction(DateTime previous, DateTime next, DateTime current)
    {
        var timeFromPrevious = current - previous;
        var totalTransition = next - previous;
        return timeFromPrevious / totalTransition;
    }
}
