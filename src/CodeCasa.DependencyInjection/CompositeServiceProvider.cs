using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.DependencyInjection;

public class CompositeServiceProvider(params IServiceProvider[] providers) :
    IKeyedServiceProvider, IServiceScopeFactory, IServiceProviderIsService
{
    private readonly IEnumerable<IServiceProvider> _providers = providers;

    public object? GetService(Type serviceType)
    {
        if (serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        // Check if the request is for IEnumerable<T>
        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var elementType = serviceType.GetGenericArguments()[0];

            // Collect all instances from all providers
            var allServices = _providers
                .SelectMany(p => (IEnumerable<object>)p.GetServices(elementType))
                .ToList();

            // Convert the List<object> to the specific array/list type expected (T[])
            var castedArray = Array.CreateInstance(elementType, allServices.Count);
            for (var i = 0; i < allServices.Count; i++)
            {
                castedArray.SetValue(allServices[i], i);
            }

            return castedArray;
        }

        foreach (var provider in _providers)
        {
            var service = provider.GetService(serviceType);
            if (service != null)
            {
                return service;
            }
        }

        return null;
    }

    public bool IsService(Type serviceType) =>
        _providers.Any(p => p.GetRequiredService<IServiceProviderIsService>().IsService(serviceType));

    public IServiceScope CreateScope()
    {
        return new CompositeServiceScope(_providers
            .Select(p => p.CreateScope())
            .ToArray());
    }

    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        foreach (var provider in _providers)
        {
            if (provider is not IKeyedServiceProvider keyed)
            {
                continue;
            }
            var service = keyed.GetKeyedService(serviceType, serviceKey);
            if (service != null)
            {
                return service;
            }
        }
        return null;
    }

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        => GetKeyedService(serviceType, serviceKey) ??
           throw new InvalidOperationException($"Keyed service {serviceType} not found.");
}