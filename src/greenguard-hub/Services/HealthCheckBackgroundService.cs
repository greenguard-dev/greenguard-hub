using greenguard_hub.Services.Configuration;
using greenguard_hub.Services.Wireless;
using greenguard_hub_client;
using Microsoft.Extensions.Hosting;
using System.Threading;


namespace greenguard_hub.Services
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        protected override void ExecuteAsync(CancellationToken stoppingToken)
        {
            var configurationStore = new ConfigurationStore();
            var configuration = configurationStore.GetConfig();
            var greenGuardHttpClient = new GreenGuardHttpClient(Wifi.GetCurrentIPAddress());

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Wifi.IsEnabled())
                {
                    greenGuardHttpClient.SendHealthCheck(configuration.GreenguardEndpoint, configuration.Id);
                }

                Thread.Sleep(30_000);
            }
        }
    }
}
