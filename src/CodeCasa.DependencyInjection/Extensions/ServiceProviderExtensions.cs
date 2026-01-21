using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.DependencyInjection.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IServiceScope CreateContextScope(this IServiceProvider serviceProvider,
            Action<IServiceCollection> contextBuilder, ServiceProviderOptions? options = null)
        {
            IServiceScope parentScope = serviceProvider.CreateScope();

            try
            {
                var contextServices = new ServiceCollection();
                contextBuilder(contextServices);

                var contextServiceProvider = options == null
                    ? contextServices.BuildServiceProvider()
                    : contextServices.BuildServiceProvider(options);
                var compositeServiceProvider = new CompositeServiceProvider(
                    contextServiceProvider, // We want to prioritize context services. This also follows the pattern of a service provider providing the last registered service.
                    parentScope.ServiceProvider);

                return new LinkedContextScope(compositeServiceProvider, contextServiceProvider, parentScope);
            }
            catch
            {
                parentScope.Dispose();
                throw;
            }
        }


    }
}
