using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace CodeCasa.NetDaemon.Sensors.Composite.Extensions
{
    public static class BooleanObservableExtensions
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
    }
}
