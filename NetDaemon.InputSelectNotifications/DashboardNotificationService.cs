using System.Reactive.Concurrency;
using Microsoft.Extensions.Hosting;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.InputSelectNotifications.Config;
using NetDaemon.RuntimeState;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.InputSelectNotifications.Interact;

namespace NetDaemon.InputSelectNotifications
{
    internal class DashboardNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string[] _inputSelectNotificationEntityIds;
        private readonly NetDaemonRuntimeStateService _netDaemonRuntimeStateService;

        public DashboardNotificationService(
            IServiceProvider serviceProvider,
            IEnumerable<InputSelectNotificationEntityId> inputSelectNotificationEntities, 
            NetDaemonRuntimeStateService netDaemonRuntimeStateService)
        {
            _serviceProvider = serviceProvider;
            _inputSelectNotificationEntityIds = inputSelectNotificationEntities.Select(e => e.InputSelectEntityId).ToArray();
            _netDaemonRuntimeStateService = netDaemonRuntimeStateService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Wait for the NetDaemon runtime to be initialized.
            await _netDaemonRuntimeStateService.WaitForInitializationAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var haContext = scope.ServiceProvider.GetRequiredService<IHaContext>();
            var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();

            var handlers = _inputSelectNotificationEntityIds.Select(entityId =>
                new DashboardNotificationHandler(
                    haContext,
                    scheduler,
                    new Entity(haContext, entityId),
                    _serviceProvider.GetRequiredKeyedService<InputSelectNotificationEntity>(entityId))).ToArray();

            await Task.Delay(Timeout.Infinite, cancellationToken);

            foreach (var handler in handlers)
            {
                handler.Dispose();
            }
        }
    }
}
