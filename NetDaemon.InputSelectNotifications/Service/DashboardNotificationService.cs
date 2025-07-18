using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.InputSelectNotifications.Interact;
using NetDaemon.RuntimeState;
using System.Reactive.Concurrency;
using NetDaemon.InputSelectNotifications.Config;

namespace NetDaemon.InputSelectNotifications.Service;

internal class DashboardNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly InputSelectNotificationInitializationConfig[] _inputSelectNotificationConfigs;
    private readonly NetDaemonRuntimeStateService _netDaemonRuntimeStateService;

    public DashboardNotificationService(
        IServiceProvider serviceProvider,
        IEnumerable<InputSelectNotificationInitializationConfig> inputSelectNotificationEntities, 
        NetDaemonRuntimeStateService netDaemonRuntimeStateService)
    {
        _serviceProvider = serviceProvider;
        _inputSelectNotificationConfigs = inputSelectNotificationEntities.ToArray();
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

        var handlers = _inputSelectNotificationConfigs.Select(config =>
            new InputSelectNotificationHandler(
                haContext,
                scheduler,
                new Entity(haContext, config.InputSelectEntityId),
                config.InputNumberEntityId == null ? null : new Entity(haContext, config.InputNumberEntityId),
                _serviceProvider
                    .GetRequiredKeyedService<InputSelectNotificationEntity>(config.InputSelectEntityId))).ToArray();

        await Task.Delay(Timeout.Infinite, cancellationToken);

        foreach (var handler in handlers)
        {
            handler.Dispose();
        }
    }
}