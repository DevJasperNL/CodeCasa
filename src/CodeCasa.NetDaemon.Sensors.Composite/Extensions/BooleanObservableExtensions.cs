using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace CodeCasa.NetDaemon.Sensors.Composite.Extensions
{
    internal static class BooleanObservableExtensions
    {
        public static IObservable<bool> PersistTrue(
            this IObservable<bool> source,
            TimeSpan persistentFor,
            IScheduler scheduler)
        {
            return Observable.Create<bool>(observer =>
            {
                DateTimeOffset? lastTrueAt = null;

                return source.Select(value =>
                    {
                        var now = scheduler.Now;

                        if (value)
                        {
                            lastTrueAt = now;
                            return Observable.Return(true);
                        }

                        if (lastTrueAt == null)
                        {
                            return Observable.Return(false);
                        }

                        var elapsed = now - lastTrueAt.Value;
                        var remaining = persistentFor - elapsed;

                        return remaining <= TimeSpan.Zero
                            ? Observable.Return(false)
                            : Observable.Return(false).Delay(remaining, scheduler);
                    })
                    .Switch()
                    .Subscribe(observer);
            });
        }

        /// <summary>
        /// Combines a motion observable and a brightness observable into a latched motion state.
        /// </summary>
        /// <param name="motion">An observable that emits the current (persisted) motion state.</param>
        /// <param name="brightnessLessThanThreshold">An observable that emits <c>true</c> when brightness is below the threshold.</param>
        /// <returns>
        /// An <see cref="IObservable{T}"/> that emits <c>true</c> when motion is detected under the brightness threshold,
        /// and remains <c>true</c> until motion ceases, ignoring subsequent brightness changes.
        /// </returns>
        public static IObservable<bool> CombineWithBrightness(this IObservable<bool> motion, IObservable<bool> brightnessLessThanThreshold)
        {
            bool? triggered = null;
            return motion.CombineLatest(brightnessLessThanThreshold, (motionTriggered, brightnessTriggered) =>
            {
                if (motionTriggered && brightnessTriggered)
                {
                    triggered = true;
                }
                else if (!motionTriggered)
                {
                    if (triggered == false)
                    {
                        // Prevent duplicate false emissions.
                        return null;
                    }
                    triggered = false;
                }

                return triggered;
            }).Where(b => b != null).Select(b => b!.Value);
        }
    }
}
