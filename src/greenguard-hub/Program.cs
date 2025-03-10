using greenguard_hub.Services.MiFlora;
using Microsoft.Extensions.DependencyInjection;
using nanoFramework.Hosting;

namespace greenguard_hub
{
    public class Program
    {
        public static void Main()
        {
            var host = CreateHostBuilder().Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(typeof(MiFloraService));
                services.AddHostedService(typeof(MiFloraMonitorService));
            });
    }
}