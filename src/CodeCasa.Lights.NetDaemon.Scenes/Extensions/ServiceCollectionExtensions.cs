using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.Lights.NetDaemon.Scenes.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLightSceneService(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<LightSceneService>();
        }
    }
}
