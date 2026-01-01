using CodeCasa.NetDaemon.RuntimeState.Extensions;
using CodeCasa.Notifications.InputSelect.NetDaemon.Config;
using CodeCasa.Notifications.InputSelect.NetDaemon.Interact;
using CodeCasa.Notifications.InputSelect.NetDaemon.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetDaemon.HassModel;
using NetDaemon.Runtime;

namespace CodeCasa.Notifications.InputSelect.NetDaemon.Extensions;

/// <summary>
/// Provides extension methods for registering input select notification services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds input select notification services to the specified <see cref="IServiceCollection"/>,
    /// configured from the specified <see cref="IConfiguration"/>.
    /// Expects a configuration section named <c>"InputSelectNotificationEntities"</c> containing an array of <see cref="InputSelectNotificationItem"/>.
    /// </summary>
    public static IServiceCollection AddInputSelectNotifications(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var inputSelectNotificationEntities = configuration
            .GetSection("InputSelectNotificationEntities")
            .Get<InputSelectNotificationItem[]>();

        if (inputSelectNotificationEntities == null || inputSelectNotificationEntities.Length == 0)
        {
            throw new InvalidOperationException("Configuration for 'InputSelectNotificationEntities' is missing or empty.");
        }

        return AddInputSelectNotifications(serviceCollection, inputSelectNotificationEntities);
    }

    /// <summary>
    /// Adds input select notification services to the specified <see cref="IServiceCollection"/>,
    /// configured from the provided array of <see cref="InputSelectNotificationItem"/>.
    /// </summary>
    public static IServiceCollection AddInputSelectNotifications(this IServiceCollection serviceCollection, InputSelectNotificationItem[] inputSelectNotificationEntities)
    {
        serviceCollection.VerifyNetDaemonDependencies();

        serviceCollection.AddSingleton(inputSelectNotificationEntities);

        var duplicateKeys = inputSelectNotificationEntities.GroupBy(i => i.InputSelectEntityId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateKeys.Any())
        {
            throw new InvalidOperationException($"Duplicate InputSelectId(s) found: {string.Join(", ", duplicateKeys)}");
        }

        serviceCollection.AddNetDaemonRuntimeStateService();
        serviceCollection.AddHostedService<DashboardNotificationBackgroundService>();

        foreach (var item in inputSelectNotificationEntities)
        {
            var inputSelectEntityId = item.InputSelectEntityId;
            if (string.IsNullOrWhiteSpace(inputSelectEntityId))
            {
                throw new InvalidOperationException("Each InputSelectNotificationEntity must have a non-empty InputSelectEntityId.");
            }
            
            // Register the InputSelectNotificationContext as a keyed singleton so it can be resolved by the input select entity ID.
            serviceCollection.TryAddKeyedSingleton<IInputSelectNotificationEntity>(inputSelectEntityId, new InputSelectNotificationEntityMediator(inputSelectEntityId, item.InputNumberEntityId));
            serviceCollection.TryAddKeyedSingleton(inputSelectEntityId, (serviceProvider, key) => (InputSelectNotificationEntityMediator)serviceProvider.GetRequiredKeyedService<IInputSelectNotificationEntity>(key));

            // We also register the InputSelectNotificationContext as a non-keyed singleton so the user could request an IEnumerable for all contexts.
            serviceCollection.AddSingleton(serviceProvider => serviceProvider.GetRequiredKeyedService<IInputSelectNotificationEntity>(inputSelectEntityId));
            serviceCollection.AddSingleton(serviceProvider => serviceProvider.GetRequiredKeyedService<InputSelectNotificationEntityMediator>(inputSelectEntityId));
        }

        return serviceCollection;
    }

    private static void VerifyNetDaemonDependencies(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        if (serviceProvider.GetService<IHaContext>() == null)
        {
            throw new InvalidOperationException(
                $"Cannot register input notifications. Missing required NetDaemon services. " +
                $"Ensure these are registered by calling {nameof(HostBuilderExtensions.UseNetDaemonRuntime)} before calling {nameof(AddInputSelectNotifications)}.");
        }
    }
}