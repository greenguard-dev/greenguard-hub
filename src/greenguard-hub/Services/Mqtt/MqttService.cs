using greenguard_hub.Services.Configuration;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System.Diagnostics;
using System.Text;

namespace greenguard_hub.Services.Mqtt
{
    public class MqttService
    {
        private readonly MqttClient _mqttClient;
        private readonly string _id;

        public MqttService()
        {
            var configurationStore = new ConfigurationStore();
            var configuration = configurationStore.GetConfig();

            _id = configuration.Id;

            _mqttClient = new MqttClient(configuration.GreenguardEndpoint, 1883, false, null, null, MqttSslProtocols.TLSv1_2);
            _mqttClient.Connect(_id);
        }

        public void Publish(string topic, string payload)
        {
            try
            {
                if (_mqttClient.IsConnected)
                {
                    _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(payload), null, null, MqttQoSLevel.AtLeastOnce, false);
                }
                else
                {
                    Debug.WriteLine("MQTT client is not connected.");

                    _mqttClient.Connect(_id);
                    _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(payload), null, null, MqttQoSLevel.AtLeastOnce, false);
                }
            }
            catch (System.Exception)
            {
                Debug.WriteLine("Error publishing message to MQTT broker.");
            }
        }
    }
}
