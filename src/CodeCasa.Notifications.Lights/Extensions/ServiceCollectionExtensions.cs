using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.Notifications.Lights.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLightNotifications(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<LightNotificationManager>()
            .AddTransient<LightNotificationManagerContext>()
            .AddTransient<LightNotificationContext>();
    }
}