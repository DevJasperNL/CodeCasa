using System.Reactive;
using System.Reactive.Linq;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CodeCasa.AutomationPipelines.Lights.Mqtt.Extensions
{
    /// <summary>
    /// Extension methods for adding MQTT telemetry publishing to an <see cref="ILightTransitionPipelineConfigurator{TLight}"/>.
    /// </summary>
    public static class ILightTransitionPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Configures the pipeline to publish its state as JSON to the MQTT broker via <paramref name="publisher"/>
        /// whenever a light transition completes or telemetry is emitted.
        /// </summary>
        /// <typeparam name="TLight">The type of light managed by the pipeline.</typeparam>
        /// <param name="configurator">The pipeline configurator to extend.</param>
        /// <param name="publisher">The <see cref="MqttPublisher"/> used to send messages to the broker.</param>
        /// <returns>The same <paramref name="configurator"/> instance for chaining.</returns>
        public static ILightTransitionPipelineConfigurator<TLight> PublishTelemetryToMqtt<TLight>(this ILightTransitionPipelineConfigurator<TLight> configurator, MqttPublisher publisher) where TLight : ILight
        {
            configurator.OnCompleted(e =>
            {
                PublishPipelineState(publisher, e.Pipeline, e.Light.Id).GetAwaiter().GetResult();
            });
            configurator.ConfigureTelemetrySubscriber(stream =>
            {
                return stream.SelectMany(async t =>
                {
                    await PublishPipelineState(publisher, t.Pipeline, t.Light.Id);
                    return Unit.Default;
                }).Subscribe();
            });
            return configurator;
        }

        /// <summary>
        /// Serializes the given <paramref name="pipeline"/> to indented JSON (excluding observable properties)
        /// and publishes it to the MQTT broker under the topic for <paramref name="lightEntityId"/>.
        /// </summary>
        /// <param name="publisher">The <see cref="MqttPublisher"/> used to send the message.</param>
        /// <param name="pipeline">The pipeline whose state will be serialized and published.</param>
        /// <param name="lightEntityId">The entity ID of the light, used to construct the MQTT topic.</param>
        private static async Task PublishPipelineState(MqttPublisher publisher, IPipeline<LightTransition> pipeline, string lightEntityId)
        {
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented, ContractResolver = new IgnoreObservablesContractResolver(), ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            var jsonString = JsonConvert.SerializeObject(pipeline, settings);
            await publisher.PublishAsync(lightEntityId, jsonString);
        }

        /// <summary>
        /// A <see cref="DefaultContractResolver"/> that excludes all <see cref="IObservable{T}"/> properties
        /// from JSON serialization to prevent circular references and non-serializable streams.
        /// </summary>
        private class IgnoreObservablesContractResolver : DefaultContractResolver
        {
            /// <summary>
            /// Creates a <see cref="JsonProperty"/> for the given member, marking any
            /// <see cref="IObservable{T}"/> property as excluded from serialization.
            /// </summary>
            /// <param name="member">The member to create a property for.</param>
            /// <param name="memberSerialization">The member serialization mode.</param>
            /// <returns>A <see cref="JsonProperty"/> describing how the member should be serialized.</returns>
            protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyType is not null &&
                    property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(IObservable<>))
                {
                    property.ShouldSerialize = _ => false;
                }

                return property;
            }
        }
    }
}
