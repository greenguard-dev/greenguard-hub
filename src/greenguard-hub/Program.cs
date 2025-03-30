using greenguard_hub.Services.MiFlora;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System;
using greenguard_hub.Services;
using greenguard_hub.Controller;
using greenguard_hub.Services.Wireless;
using System.Diagnostics;
using greenguard_hub.Services.Device;
using greenguard_hub.Services.Configuration;
using Microsoft.Extensions.Hosting;
using nanoFramework.WebServer;

namespace greenguard_hub
{
    public class Program
    {
        public static void Main()
        {
            IHost host = CreateHostBuilder().Build();
            var configurationStore = new ConfigurationStore();

            if (!Wifi.IsEnabled())
            {
                var configuration = configurationStore.GetConfig();
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

            using var webServer = new GreenGuardWebserver(80, HttpProtocol.Http, new Type[] { typeof(GreenGuardController) }, host.Services);

            webServer.Start();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(typeof(DeviceScanner));
                    services.AddSingleton(typeof(MiFloraService));
                    services.AddHostedService(typeof(HealthCheckBackgroundService));
                });
    }
}