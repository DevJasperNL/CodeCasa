using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

/// <summary>
/// Extension methods for registering light pipeline services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all required services for light pipelines in the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register services in.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddLightPipelines(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddTransient<LightPipelineFactory>()
            .AddTransient<ReactiveNodeFactory>();
    }

}