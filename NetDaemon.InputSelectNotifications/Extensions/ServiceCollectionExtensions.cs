using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NetDaemon.InputSelectNotifications.Config;
using NetDaemon.InputSelectNotifications.Interact;
using NetDaemon.RuntimeState.Extensions;

namespace NetDaemon.InputSelectNotifications.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterInputSelectNotifications(this IServiceCollection serviceCollection, string inputSelectEntityId)
        {
            // todo: check if key is registered
            if (!serviceCollection.IsServiceRegistered<DashboardNotificationService>())
            {
                // RegisterInputSelectNotifications can be called multiple times (for different input select entities). We only want to register the background service the first time.
                serviceCollection.AddNetDaemonRuntimeStateService();
                serviceCollection.AddHostedService<DashboardNotificationService>();
            }
            
            // Register the InputSelectNotificationContext as a keyed singleton so it can be resolved by the input select entity ID.
            serviceCollection.TryAddKeyedSingleton<IInputSelectNotificationEntity, InputSelectNotificationEntity>(inputSelectEntityId); 
            serviceCollection.TryAddKeyedSingleton(inputSelectEntityId, (serviceProvider, key) => (InputSelectNotificationEntity)serviceProvider.GetRequiredKeyedService<IInputSelectNotificationEntity>(key));

            // We also register the InputSelectNotificationContext as a non-keyed singleton so the user could request an IEnumerable for all contexts.
            serviceCollection.AddSingleton(serviceProvider =>
            {
                var inputSelectNotificationContext = serviceProvider.GetRequiredKeyedService<IInputSelectNotificationEntity>(inputSelectEntityId);
                return inputSelectNotificationContext;
            });
            serviceCollection.AddSingleton(serviceProvider =>
            {
                var inputSelectNotificationContext = serviceProvider.GetRequiredKeyedService<InputSelectNotificationEntity>(inputSelectEntityId);
                return inputSelectNotificationContext;
            });

            serviceCollection.AddTransient(_ => new InputSelectNotificationEntityId(inputSelectEntityId));
            //inputSelectNotificationEntities.Add(inputSelectEntityId);

            return serviceCollection;
        }

        public static bool IsServiceRegistered<TService>(this IServiceCollection services)
        {
            return services.Any(sd => sd.ServiceType == typeof(TService));
        }
    }
}
