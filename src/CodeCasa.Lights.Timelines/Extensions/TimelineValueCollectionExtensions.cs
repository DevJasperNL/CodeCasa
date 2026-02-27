using CodeCasa.Lights.Extensions;
using Occurify;
using Occurify.Extensions;
using Occurify.Reactive.Extensions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.Lights.Timelines.Extensions
{
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
        /// The duration of the initial fade from current state. Defaults to 500ms if null.
        /// </param>
        /// <returns>An observable that emits the current interpolated state, then follows the scheduled timeline.</returns>
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
        /// The duration of the initial fade for all lights in the scene. Defaults to 500ms if null.
        /// </param>
        /// <returns>An observable that emits a dictionary of transitions representing the current scene state, followed by scheduled updates.</returns>
        /// <remarks>
        /// This method utilizes a custom dictionary comparer to ensure updates are only emitted when 
        /// at least one light in the scene has changed its parameters.
        /// </remarks>
        public static IObservable<Dictionary<string, LightTransition>> ToLightTransitionSceneObservableIncludingCurrent(
            this Dictionary<ITimeline, Dictionary<string, LightParameters>> sceneTimeline,
            IScheduler scheduler,
            TimeSpan? transitionTimeForTimelineState = null)
        {
            return CreateTimelineObservableIncludingInitialInterpolatedValue(sceneTimeline,
                (lightParametersDict, transitionTime) => lightParametersDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsTransition(transitionTime)),
                (previousDict, nextDict, fraction) => previousDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Interpolate(nextDict[kvp.Key], fraction)),
                new DictionaryComparer<string, LightParameters>(EqualityComparer<LightParameters>.Default),
                scheduler,
                transitionTimeForTimelineState);
        }

        /// <summary>
        /// Creates an observable that immediately emits an interpolated initial state based on <see cref="DateTime.UtcNow"/>, 
        /// followed by the standard timeline transitions after a specified delay.
        /// </summary>
        private static IObservable<TOut> CreateTimelineObservableIncludingInitialInterpolatedValue<TIn, TOut>(
            this Dictionary<ITimeline, TIn> sceneTimeline,
            Func<TIn, TimeSpan, TOut> transformer,
            Func<TIn, TIn, double, TIn> interpolator,
            IEqualityComparer<TIn> comparer,
            IScheduler scheduler,
            TimeSpan? transitionTimeForTimelineState = null)
        {
            var transitionObservable = CreateTimelineObservable(sceneTimeline, transformer, comparer, scheduler);

            var utcNow = DateTime.UtcNow;
            var valuesAtPrevious = sceneTimeline.GetValuesAtPreviousUtcInstant(utcNow);
            var valuesAtCurrentOrNext = sceneTimeline.GetValuesAtCurrentOrNextUtcInstant(utcNow);

            if (valuesAtPrevious.Key == null || valuesAtCurrentOrNext.Key == null)
            {
                return transitionObservable;
            }

            var fraction = CalculateFraction(valuesAtPrevious.Key.Value, valuesAtCurrentOrNext.Key.Value, utcNow);

            var previousScene = valuesAtPrevious.Value.First();
            var nextScene = valuesAtCurrentOrNext.Value.First();

            var initialSceneTransition = interpolator(previousScene, nextScene, fraction);

            var timeSpan = transitionTimeForTimelineState ?? TimeSpan.FromMilliseconds(500);
            // We delay the timeline observable to allow the initial scene transition to be emitted/activated first.
            var delayedTimelineObservable = Observable
                .Timer(timeSpan, scheduler)
                .SelectMany(_ => transitionObservable);

            return Observable.Return(transformer(initialSceneTransition, timeSpan)).Concat(delayedTimelineObservable);
        }

        /// <summary>
        /// Creates an observable stream that emits transformed values based on state transitions 
        /// between consecutive instants in a timeline.
        /// </summary>
        private static IObservable<TOut> CreateTimelineObservable<TIn, TOut>(
            Dictionary<ITimeline, TIn> timeline,
            Func<TIn, TimeSpan, TOut> transformer,
            IEqualityComparer<TIn> comparer,
            IScheduler scheduler)
        {
            return timeline
                .ToSampleObservable(scheduler, emitSampleUponSubscribe: false)
                .Select(s =>
                {
                    var instant = s.Key;
                    var nextValues = timeline.GetValuesAtNextUtcInstant(instant);
                    var nextInstant = nextValues.Key;
                    if (nextInstant == null)
                    {
                        return Maybe<TOut>.None;
                    }

                    var current = s.Value.First();
                    var next = nextValues.Value.First();
                    if (comparer.Equals(current, next))
                    {
                        return Maybe<TOut>.None;
                    }
                    var transitionTimeSpan = nextInstant.Value - instant;

                    return Maybe<TOut>.Some(transformer(next, transitionTimeSpan));
                })
                .Where(s => s.HasValue)
                .Select(s => s.Value!);
        }

        private sealed record Maybe<T>(bool HasValue, T? Value)
        {
            public static Maybe<T> None => new(false, default);
            public static Maybe<T> Some(T value) => new(true, value);
        }

        private static double CalculateFraction(DateTime previous, DateTime next, DateTime current)
        {
            var timeFromPrevious = current - previous;
            var totalTransition = next - previous;
            return timeFromPrevious / totalTransition;
        }
    }
}
