using Microsoft.Extensions.DependencyInjection;
using nanoFramework.WebServer;
using System;
using System.Net;

namespace greenguard_hub.Services
{
    public class GreenGuardWebserver : WebServer
    {
        private readonly IServiceProvider _serviceProvider;

        public GreenGuardWebserver(int port, HttpProtocol protocol, Type[] controllers, IServiceProvider serviceProvider) : base(port, protocol, controllers)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void InvokeRoute(CallbackRoutes route, HttpListenerContext context)
        {
            route.Callback.Invoke(ActivatorUtilities.CreateInstance(_serviceProvider, route.Callback.DeclaringType), new object[] { new WebServerEventArgs(context) });
        }
    }
}
