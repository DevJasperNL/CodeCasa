using System.Reactive.Linq;
using NetDaemon.HassModel;

namespace CodeCasa.NetDaemon.Utilities.Extensions;

public static class EventObservableExtensions
{
    public static IObservable<Event> Filter(this IObservable<Event> events, string eventType)
    {
        return events.Where(e => e.EventType == eventType);
    }
}