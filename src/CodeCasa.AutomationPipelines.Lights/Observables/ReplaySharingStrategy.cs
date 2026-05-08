using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Observables
{
    internal class ReplaySharingStrategy : IObservableSharingStrategy
    {
        public IObservable<T> Apply<T>(IObservable<T> source) =>
            source.Replay(1).RefCount();
    }
}
