
namespace CodeCasa.AutomationPipelines.Lights.Mqtt
{
    /// <summary>
    /// Configuration options for connecting to an MQTT broker.
    /// </summary>
    public class MqttOptions
    {
        /// <summary>
        /// Gets or sets the hostname or IP address of the MQTT broker.
        /// </summary>
        public string Host { get; set; } = null!;

        /// <summary>
        /// Gets or sets the port number of the MQTT broker. Defaults to <c>1883</c>.
        /// </summary>
        public int Port { get; set; } = 1883;

        /// <summary>
        /// Gets or sets the username used to authenticate with the MQTT broker.
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// Gets or sets the password used to authenticate with the MQTT broker.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the base topic prefix used when publishing messages to the MQTT broker.
        /// </summary>
        public string BaseTopic { get; set; } = string.Empty;
    }
}
