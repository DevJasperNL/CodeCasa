using CodeCasa.CustomEntities.People;
using CodeCasa.CustomEntities.Phones;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.CustomEntities.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodeCasaCustomEntities(this IServiceCollection serviceCollection)
    {
        return serviceCollection

            // Phones
            .AddTransient<PhoneJane>()
            .AddTransient<PhoneJasper>()

            // People
            .AddTransient<Jane>()
            .AddTransient<Jasper>();
    }
}