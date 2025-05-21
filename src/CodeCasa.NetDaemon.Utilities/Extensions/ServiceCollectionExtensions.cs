using CodeCasa.NetDaemon.Utilities.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.NetDaemon.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNetDaemonConnectionUtils(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddTransient<NetDaemonConnectionStateService>();
    }
}