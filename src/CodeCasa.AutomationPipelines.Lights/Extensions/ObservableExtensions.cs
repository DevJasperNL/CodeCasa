using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

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
        Func<DateTime?, bool> offCondition,
        Func<TValue> offValueFactory,
        IEnumerable<Func<TValue>> valueFactories,
        TimeSpan timeout,
        bool? includeOff,
        IScheduler scheduler)
    {
        var valueFactoryArray = valueFactories.ToArray();
        return triggerObservable
            .ToToggleIndexObservable(offCondition, valueFactoryArray.Length, timeout, includeOff, scheduler)
            .Select(index => index == ToggleOffIndex ? offValueFactory() : valueFactoryArray[index]());
    }

    /// <summary>
    /// The index emitted by <see cref="ToToggleIndexObservable{TTrigger}"/> when the toggle should turn off.
    /// </summary>
    public const int ToggleOffIndex = -1;

    /// <summary>
    /// Emits, for every trigger, the index of the toggle value to activate, or <see cref="ToggleOffIndex"/> to turn off.
    /// </summary>
    public static IObservable<int> ToToggleIndexObservable<TTrigger>(
        this IObservable<TTrigger> triggerObservable,
        Func<DateTime?, bool> offCondition,
        int valueCount,
        TimeSpan timeout,
        bool? includeOff,
        IScheduler scheduler)
    {
        var includeOffBool = includeOff ?? valueCount <= 1;
        if (!includeOffBool && valueCount <= 1)
        {
            throw new InvalidOperationException("When only supplying one factory, off should be included.");
        }
        DateTime? previousLastChanged = null;
        var index = 0;
        var maxIndexValue = includeOffBool ? valueCount : valueCount - 1;
        return triggerObservable
            .Select(_ =>
            {
                var utcNow = scheduler.Now.UtcDateTime;
                var consecutive = previousLastChanged != null && utcNow - previousLastChanged < timeout;

                if (!consecutive)
                {
                    index = 0;
                    if (offCondition(previousLastChanged))
                    {
                        previousLastChanged = utcNow;
                        return ToggleOffIndex;
                    }
                }

                var value = index >= valueCount ? ToggleOffIndex : index;
                index = index < maxIndexValue ? index + 1 : 0;
                previousLastChanged = utcNow;
                return value;
            });
    }
}