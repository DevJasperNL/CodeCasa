using CodeCasa.CustomEntities.Automation.Notifications.Dashboards;
using CodeCasa.CustomEntities.Automation.Notifications.Phones;
using CodeCasa.CustomEntities.Automation.People;
using CodeCasa.CustomEntities.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.CustomEntities.Automation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodeCasaCustomAutomationEntities(this IServiceCollection serviceCollection)
    {
        return serviceCollection

            .AddCodeCasaCustomCoreEntities()

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