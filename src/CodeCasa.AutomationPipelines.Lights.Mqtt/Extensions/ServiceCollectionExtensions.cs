using CodeCasa.AutomationPipelines.Lights.Mqtt.BackgroundService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace CodeCasa.AutomationPipelines.Lights.Mqtt.Extensions;

/// <summary>
/// Extension methods for registering CodeCasa MQTT services with an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the MQTT client, <see cref="MqttPublisher"/>, and the <see cref="MqttWorker"/> hosted service
    /// using settings bound from the <c>MqttOptions</c> configuration section.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration used to bind <see cref="MqttOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddCodeCasaMqtt(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<MqttOptions>(configuration.GetSection("MqttOptions"));

        serviceCollection
            .AddSingleton(new MqttClientFactory().CreateMqttClient())
            .AddSingleton<MqttPublisher>();
        serviceCollection.AddHostedService<MqttWorker>();

        return serviceCollection;
    }
}