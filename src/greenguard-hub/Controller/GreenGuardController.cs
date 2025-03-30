using greenguard_hub.Services.Device;
using nanoFramework.Json;
using nanoFramework.WebServer;

namespace greenguard_hub.Controller
{
    public class GreenGuardController
    {
        private readonly DeviceScanner _deviceScanner;

        public GreenGuardController(DeviceScanner deviceScanner)
        {
            _deviceScanner = deviceScanner;
        }

        [Route("devices/scan")]
        [Method("GET")]
        public void ScanDevices(WebServerEventArgs e)
        {
            var devices = _deviceScanner.Scan();

            var json = JsonSerializer.SerializeObject(devices);

            e.Context.Response.ContentType = "application/json";
            WebServer.OutPutStream(e.Context.Response, json);
        }
    }
}
