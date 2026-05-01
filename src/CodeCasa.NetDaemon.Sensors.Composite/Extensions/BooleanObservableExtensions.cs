using System.Reactive.Concurrency;
using System.Reactive.Linq;

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

        public static IObservable<bool> CombineWithBrightness(this IObservable<bool> motion, IObservable<bool> brightnessLessThanThreshold)
        {
            return Observable.Create<bool>(observer =>
            {
                bool? triggered = null;
                return motion.CombineLatest(brightnessLessThanThreshold, (motionTriggered, brightnessTriggered) =>
                {
                    if (motionTriggered && brightnessTriggered)
                    {
                        triggered = true;
                    }
                    else
                    {
                        if (triggered == false)
                        {
                            // Prevent duplicate false emissions.
                            return null;
                        }

                        if (!motionTriggered)
                        {
                            triggered = false;
                        }
                    }

                    return triggered;
                }).Where(b => b != null).Select(b => b!.Value).Subscribe(observer);
            });
        }
    }
}
