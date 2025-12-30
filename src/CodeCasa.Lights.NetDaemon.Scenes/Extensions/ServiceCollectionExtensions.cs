using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.Lights.NetDaemon.Scenes.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring services in the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="LightSceneService"/> to the service collection as a transient service.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add the service to.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddLightSceneService(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<LightSceneService>();
        }
    }
}
