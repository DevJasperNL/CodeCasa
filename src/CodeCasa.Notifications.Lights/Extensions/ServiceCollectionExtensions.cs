using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.Notifications.Lights.Extensions;

/// <summary>
/// Extension methods for setting up light notification services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds light notification services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddLightNotifications(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<LightNotificationManager>()
            .AddTransient<LightNotificationManagerContext>()
            .AddTransient<LightNotificationContext>();
    }
}