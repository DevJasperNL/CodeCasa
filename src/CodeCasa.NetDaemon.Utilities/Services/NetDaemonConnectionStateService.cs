using NetDaemon.Client;
using NetDaemon.Runtime;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.NetDaemon.Utilities.Services
{
    public class NetDaemonConnectionStateService : IDisposable
    {
        private readonly Lock _lock = new();
        private readonly BehaviorSubject<NetDaemonStates> _netDaemonConnected = new(NetDaemonStates.Initializing);

        private bool _disposed;
        private IDisposable? _connectionSubscription;

        public NetDaemonConnectionStateService(
            INetDaemonRuntime netDaemonRuntime,
            IHomeAssistantRunner homeAssistantRunner)
        {
            _ = Task.Run(async () =>
            {
                await netDaemonRuntime.WaitForInitializationAsync();

                lock (_lock)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _connectionSubscription = Observable
                        .Defer(() =>
                        {
                            var initial = homeAssistantRunner.CurrentConnection != null;
                            return homeAssistantRunner.OnDisconnect.Select(_ => false)
                                .Merge(homeAssistantRunner.OnConnect.Select(_ => true))
                                .StartWith(initial);
                        })
                        .DistinctUntilChanged()
                        .Subscribe(
                            onNext: c =>
                                _netDaemonConnected.OnNext(c
                                    ? NetDaemonStates.Connected
                                    : NetDaemonStates.Disconnected),
                            onError: _ => _netDaemonConnected.OnNext(NetDaemonStates.Disconnected),
                            onCompleted: () => _netDaemonConnected.OnNext(NetDaemonStates.Disconnected));
                }
            });
        }

        public NetDaemonStates NetDaemonState => _netDaemonConnected.Value;
        public IObservable<NetDaemonStates> ConnectedChangesWithCurrent() => _netDaemonConnected.AsObservable();
        public IObservable<NetDaemonStates> ConnectedChanges() => _netDaemonConnected.Skip(1).AsObservable();

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            lock (_lock)
            {
                _connectionSubscription?.Dispose();

                _netDaemonConnected.OnCompleted();
                _netDaemonConnected.Dispose();

                _disposed = true;
            }
        }
    }
}
