using System.Reactive.Concurrency;
using CodeCasa.NetDaemon.RuntimeState;
using CodeCasa.Notifications.InputSelect.NetDaemon.Config;
using CodeCasa.Notifications.InputSelect.NetDaemon.Interact;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.Notifications.InputSelect.NetDaemon.Service;

internal class DashboardNotificationBackgroundService(
    IServiceProvider serviceProvider,
    InputSelectNotificationItem[] inputSelectNotificationItems,
    NetDaemonRuntimeStateService netDaemonRuntimeStateService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Wait for the NetDaemon runtime to be initialized.
        await netDaemonRuntimeStateService.WaitForInitializationAsync(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var haContext = scope.ServiceProvider.GetRequiredService<IHaContext>();
        var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();

        var handlers = inputSelectNotificationItems.Select(config =>
            new InputSelectNotificationHandler(
                haContext,
                scheduler,
                new Entity(haContext, config.InputSelectEntityId),
                config.InputNumberEntityId == null ? null : new Entity(haContext, config.InputNumberEntityId),
                serviceProvider
                    .GetRequiredKeyedService<InputSelectNotificationEntityMediator>(config.InputSelectEntityId))).ToArray();

        await Task.Delay(Timeout.Infinite, cancellationToken);

        foreach (var handler in handlers)
        {
            handler.Dispose();
        }
    }
}