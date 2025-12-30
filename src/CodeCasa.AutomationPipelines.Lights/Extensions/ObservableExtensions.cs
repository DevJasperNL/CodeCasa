using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class ObservableExtensions
    {
        public static IObservable<Unit> ToPulsesWhenTrue(this IObservable<bool> source, TimeSpan timeBetweenPulses, IScheduler scheduler)
        {
            return source
                .Select(b =>
                    b
                        ? Observable.Timer(TimeSpan.Zero, timeBetweenPulses, scheduler).Select(_ => Unit.Default)
                        : Observable.Empty<Unit>())
                .Switch();
        }

        public static IObservable<TValue> ToCycleObservable<TTrigger, TValue>(
            this IObservable<TTrigger> triggerObservable, 
            IEnumerable<(Func<TValue> valueFactory, Func<bool> valueIsActiveFunc)> cycleValues)
        {
            var cycleValuesList = cycleValues.ToList();
            return triggerObservable.Select(_ =>
            {
                var index = cycleValuesList.FindIndex(n => n.valueIsActiveFunc()) + 1;
                if (index >= cycleValuesList.Count)
                {
                    index = 0;
                }

                return cycleValuesList[index].valueFactory();
            });
        }

        public static IObservable<TValue?> ToToggleObservable<TTrigger, TValue>(
            this IObservable<TTrigger> triggerObservable,
            Func<bool> offCondition,
            Func<TValue> offValueFactory,
            IEnumerable<Func<TValue>> valueFactories,
            TimeSpan timeout,
            bool? includeOff)
        {
            var valueFactoryArray = valueFactories.ToArray();
            var includeOffBool = includeOff ?? valueFactoryArray.Length <= 1;
            if (!includeOffBool && valueFactoryArray.Length <= 1)
            {
                throw new InvalidOperationException("When only supplying one factory, off should be included.");
            }
            DateTime? previousLastChanged = null;
            var index = 0;
            var maxIndexValue = includeOffBool ? valueFactoryArray.Length : valueFactoryArray.Length - 1;
            return triggerObservable
                .Select(_ =>
                {
                    var utcNow = DateTime.UtcNow;
                    var consecutive = previousLastChanged != null && utcNow - previousLastChanged < timeout;
                    previousLastChanged = utcNow;

                    if (!consecutive)
                    {
                        index = 0;
                        if (offCondition())
                        {
                            return offValueFactory();
                        }
                    }

                    var value = index >= valueFactoryArray.Length ? offValueFactory() : valueFactoryArray[index]();
                    index = index < maxIndexValue ? index + 1 : 0;
                    return value;
                });
        }
    }
}
