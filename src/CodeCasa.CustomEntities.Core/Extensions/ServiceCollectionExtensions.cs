using CodeCasa.CustomEntities.Core.GoogleHome;
using CodeCasa.CustomEntities.Core.InputSelect;
using CodeCasa.CustomEntities.Core.Weather;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.CustomEntities.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodeCasaCustomCoreEntities(this IServiceCollection serviceCollection)
    {
        return serviceCollection

            .AddTransient<GoogleHomeAlarmEntities>()
            .AddTransient<GoogleHomeTimerEntities>()

            // Input Select Entities
            .AddTransient<LivingRoomWallPanelView>()
            
            .AddTransient<ForecastHome>();
    }
}