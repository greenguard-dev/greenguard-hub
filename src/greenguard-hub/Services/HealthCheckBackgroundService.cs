using greenguard_hub.Services.Mqtt;
using greenguard_hub.Services.Wireless;
using Microsoft.Extensions.Hosting;
using nanoFramework.Json;
using System.Threading;

namespace greenguard_hub.Services
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        private readonly MqttService _mqttService;

        public HealthCheckBackgroundService(MqttService mqttService)
        {
            _mqttService = mqttService;
        }

        protected override void ExecuteAsync(CancellationToken stoppingToken)
        {
            var ipAddress = Wifi.GetCurrentIPAddress();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Wifi.IsEnabled())
                {
                    var healthCheckMessageJson = JsonSerializer.SerializeObject(new HealthCheckMessage
                    {
                        IpAddress = ipAddress
                    });

                    _mqttService.Publish("healthcheck", healthCheckMessageJson);
                }

                Thread.Sleep(60_000);
            }
        }

        private class HealthCheckMessage
        {
            public string IpAddress { get; set; }
        }
    }
}
