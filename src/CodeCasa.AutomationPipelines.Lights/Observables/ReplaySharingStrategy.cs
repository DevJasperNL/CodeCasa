using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace CodeCasa.AutomationPipelines.Lights.Observables
{
    internal class ReplaySharingStrategy : IObservableSharingStrategy
    {
        public IObservable<T> Apply<T>(IObservable<T> source) =>
            source.Replay(1).RefCount();
    }
}
