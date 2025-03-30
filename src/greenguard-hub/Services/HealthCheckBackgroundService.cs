using greenguard_hub.Services.Configuration;
using greenguard_hub.Services.Wireless;
using greenguard_hub_client;
using Microsoft.Extensions.Hosting;
using System.Threading;


namespace greenguard_hub.Services
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        private readonly GreenGuardHttpClient _greenGuardHttpClient = new();

        protected override void ExecuteAsync(CancellationToken stoppingToken)
        {
            var configurationStore = new ConfigurationStore();
            var configuration = configurationStore.GetConfig();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Wifi.IsEnabled())
                {
                    _greenGuardHttpClient.SendHealthCheck(configuration.GreenguardEndpoint, configuration.Id, Wifi.GetCurrentIPAddress());
                }

                Thread.Sleep(30_000);
            }
        }
    }
}
