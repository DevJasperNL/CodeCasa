using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.Pipeline.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodeCasaPipeline(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddTransient(typeof(IPipeline<>), typeof(Pipeline<>))
            .AddTransient(typeof(Pipeline<>), typeof(Pipeline<>));
    }
}