using greenguard_hub.Services.MiFlora;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System;
using nanoFramework.WebServer;
using greenguard_hub.Services;
using greenguard_hub.Controller;
using nanoFramework.Hosting;
using greenguard_hub.Services.Wireless;
using System.Diagnostics;

namespace greenguard_hub
{
    public class Program
    {
        public static void Main()
        {
            var serviceProvider = ConfigureServices();
            Type[] controllers;

            if (!Wifi.IsEnabled())
            {
                Debug.WriteLine("Wifi is not enabled");
                AccessPoint.SetWifiAp();

                controllers = new Type[] { typeof(AccessPointController) };
            }
            else
            {
                if (Wifi.Connect())
                {
                    Debug.WriteLine("Connected to Wifi");
                    controllers = new Type[] { typeof(GreenGuardController) };
                }
                else
                {
                    Debug.WriteLine("Could not connect to Wifi");
                    Debug.WriteLine("Falling back to AccessPoint");
                    AccessPoint.SetWifiAp();
                    controllers = new Type[] { typeof(AccessPointController) };
                }
            }

            using var webServer = new GreenGuardWebserver(80, HttpProtocol.Http, controllers, serviceProvider);

            webServer.Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(typeof(MiFloraService))
                .AddHostedService(typeof(MiFloraMonitorService))
                .BuildServiceProvider();
        }
    }
}