using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetDaemon.Client;
using NetDaemon.Runtime;

namespace CodeCasa.NetDaemon.RuntimeState.Extensions;

/// <summary>
/// Extension methods for registering the <see cref="NetDaemonRuntimeStateService"/> in the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="NetDaemonRuntimeStateService"/> in the service collection.
    /// </summary>
    public static IServiceCollection AddNetDaemonRuntimeStateService(this IServiceCollection services)
    {
        services.VerifyNetDaemonDependencies();

        // Singleton: every instance subscribes to the runner's connect/disconnect events for the lifetime of the container.
        services.TryAddSingleton<NetDaemonRuntimeStateService>();
        return services;
    }

    private static void VerifyNetDaemonDependencies(this IServiceCollection services)
    {
        // Registrations are inspected rather than resolved: building a throwaway provider would instantiate NetDaemon's singletons a second time.
        List<string> missing = [];
        if (services.All(sd => sd.ServiceType != typeof(INetDaemonRuntime)))
        {
            missing.Add(nameof(INetDaemonRuntime));
        }
        if (services.All(sd => sd.ServiceType != typeof(IHomeAssistantRunner)))
        {
            missing.Add(nameof(IHomeAssistantRunner));
        }

        if (missing.Any())
        {
            throw new InvalidOperationException(
                $"Cannot register {nameof(NetDaemonRuntimeStateService)}. Missing required services: {string.Join(", ", missing)}. " +
                $"Ensure these are registered by calling {nameof(HostBuilderExtensions.UseNetDaemonRuntime)} before calling {nameof(AddNetDaemonRuntimeStateService)}.");
        }
    }
}
