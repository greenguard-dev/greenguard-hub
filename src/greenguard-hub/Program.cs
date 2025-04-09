using greenguard_hub.Services.MiFlora;
using Microsoft.Extensions.DependencyInjection;
using greenguard_hub.Services;
using greenguard_hub.Services.Wireless;
using System.Diagnostics;
using greenguard_hub.Services.Device;
using greenguard_hub.Services.Configuration;
using Microsoft.Extensions.Hosting;
using greenguard_hub.Services.Mqtt;

namespace greenguard_hub
{
    public class Program
    {
        public static void Main()
        {
            IHost host = CreateHostBuilder().Build();
            var configurationStore = new ConfigurationStore();
            var configuration = configurationStore.GetConfig();

            if (!Wifi.IsEnabled())
            {
                var ssid = configuration.WifiSsid;
                var password = configuration.WifiPassword;

                var success = Wifi.Configure(ssid, password);

                if (success)
                {
                    Debug.WriteLine("Connected to Wifi (" + Wifi.GetCurrentIPAddress() + ")");
                }
                else
                {
                    Debug.WriteLine("Could not connect to Wifi");
                }
            }
            else
            {
                if (Wifi.Connect())
                {
                    Debug.WriteLine("Connected to Wifi (" + Wifi.GetCurrentIPAddress() + ")");
                }
                else
                {
                    Debug.WriteLine("Could not connect to Wifi");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(typeof(DeviceScanner));
                    services.AddSingleton(typeof(MiFloraService));
                    services.AddSingleton(typeof(MqttService));
                    services.AddHostedService(typeof(HealthCheckBackgroundService));
                });
    }
}