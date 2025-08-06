using CodeCasa.CustomEntities.InputSelect;
using CodeCasa.CustomEntities.Notifications.Dashboards;
using CodeCasa.CustomEntities.Notifications.Phones;
using CodeCasa.CustomEntities.People;
using CodeCasa.CustomEntities.Weather;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.CustomEntities.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodeCasaCustomAutomationEntities(this IServiceCollection serviceCollection)
    {
        return serviceCollection

            // Entities used in frontend are always available for backend
            .AddCodeCasaCustomFrontEndEntities()

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

    public static IServiceCollection AddCodeCasaCustomFrontEndEntities(this IServiceCollection serviceCollection)
    {// todo: more me to a separate project.
        return serviceCollection

            // Input Select Entities
            .AddTransient<LivingRoomWallPanelView>()
            
            .AddTransient<ForecastHome>();
    }
}