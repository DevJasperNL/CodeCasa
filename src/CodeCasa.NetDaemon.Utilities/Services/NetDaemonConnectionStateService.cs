using System.Reactive;
using NetDaemon.Client;
using NetDaemon.Runtime;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.NetDaemon.Utilities.Services
{
    public class NetDaemonConnectionStateService : IDisposable
    {
        private readonly BehaviorSubject<NetDaemonStates> _netDaemonConnected = new(NetDaemonStates.Initializing);

        private IDisposable? _connectionSubscription;

        public NetDaemonConnectionStateService(
            INetDaemonRuntime netDaemonRuntime,
            IHomeAssistantRunner homeAssistantRunner)
        {
            var connectionGate = Observable.FromAsync(async () =>
            {
                await netDaemonRuntime.WaitForInitializationAsync();
                return homeAssistantRunner.CurrentConnection != null
                    ? NetDaemonStates.Connected
                    : NetDaemonStates.Disconnected;
            });

            var postInitChanges =
                homeAssistantRunner.OnDisconnect.Select(_ => NetDaemonStates.Disconnected)
                    .Merge(homeAssistantRunner.OnConnect.Select(_ => NetDaemonStates.Connected))
                    .SkipUntil(connectionGate);

            var connectionChanges = connectionGate.Concat(postInitChanges);

            _connectionSubscription = connectionChanges.DistinctUntilChanged().Subscribe(_netDaemonConnected);
        }

        public NetDaemonStates NetDaemonState => _netDaemonConnected.Value;
        public IObservable<NetDaemonStates> ConnectedChangesWithCurrent() => _netDaemonConnected.AsObservable();
        public IObservable<NetDaemonStates> ConnectedChanges() => _netDaemonConnected.Skip(1).AsObservable();

        public void Dispose()
        {
            if (_connectionSubscription == null)
            {
                return;
            }

            _connectionSubscription?.Dispose();
            _connectionSubscription = null;

            _netDaemonConnected.OnCompleted();
            _netDaemonConnected.Dispose();
        }
    }
}
