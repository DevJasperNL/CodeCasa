using CodeCasa.CustomEntities.Notifications.Dashboards;
using CodeCasa.CustomEntities.Notifications.Phones;
using CodeCasa.CustomEntities.People;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.CustomEntities.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodeCasaCustomEntities(this IServiceCollection serviceCollection)
    {
        return serviceCollection

            // Dashboard Notifications
            .AddTransient<LivingRoomPanelDashboardNotifications>()
            .AddTransient<JaneDashboardNotifications>()
            .AddTransient<JasperDashboardNotifications>()

            // Phone Notifications
            .AddTransient<JanePhoneNotifications>()
            .AddTransient<JasperPhoneNotifications>()
            
            // People
            .AddTransient<Jane>()
            .AddTransient<Jasper>()
            .AddTransient<PeopleEntities>();
    }
}