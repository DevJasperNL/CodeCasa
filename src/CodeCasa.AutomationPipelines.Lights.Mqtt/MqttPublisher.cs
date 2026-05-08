using Microsoft.Extensions.Options;
using MQTTnet;

namespace CodeCasa.AutomationPipelines.Lights.Mqtt
{
    /// <summary>
    /// Publishes messages to an MQTT broker on topics derived from entity IDs.
    /// </summary>
    public class MqttPublisher(IMqttClient mqttClient, IOptions<MqttOptions> options)
    {
        private readonly MqttOptions _options = options.Value;

        /// <summary>
        /// Publishes a message to the MQTT broker under the topic <c>{BaseTopic}/{entityId}</c> with the retain flag set.
        /// Returns <see langword="null"/> if the client is not currently connected.
        /// </summary>
        /// <param name="entityId">The entity identifier used to construct the MQTT topic.</param>
        /// <param name="message">The message payload to publish.</param>
        /// <returns>
        /// A <see cref="MqttClientPublishResult"/> if the message was published successfully;
        /// otherwise <see langword="null"/>.
        /// </returns>
        public async Task<MqttClientPublishResult?> PublishAsync(string entityId, string message)
        {
            if (!mqttClient.IsConnected)
            {
                return null;
            }

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"{_options.BaseTopic}/{entityId}")
                .WithPayload(message)
                .WithRetainFlag()
                .Build();

            return await mqttClient.PublishAsync(mqttMessage);
        }
    }
}
