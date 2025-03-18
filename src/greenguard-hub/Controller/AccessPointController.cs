using greenguard_hub.Resources;
using greenguard_hub.Services.Wireless;
using nanoFramework.Runtime.Native;
using nanoFramework.WebServer;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace greenguard_hub.Controller
{
    public class AccessPointController
    {
        [Route("beer.min.css")]
        [Method("GET")]
        public void BeerCssCss(WebServerEventArgs e)
        {
            WebServer.SendFileOverHTTP(e.Context.Response, "I:\\beer.min.css", "text/css");
        }

        [Route("beer.min.js")]
        [Method("GET")]
        public void BeerCssJs(WebServerEventArgs e)
        {
            WebServer.SendFileOverHTTP(e.Context.Response, "I:\\beer.min.js", "text/javascript");
        }

        [Route("style.css")]
        [Method("GET")]
        public void StyleCss(WebServerEventArgs e)
        {
            var style = Views.GetBytes(Views.BinaryResources.Style);
            e.Context.Response.ContentType = "text/css";

            var responseAsString = Encoding.UTF8.GetString(style, 0, style.Length);

            WebServer.OutPutStream(e.Context.Response, responseAsString);
        }

        [Route("")]
        [Method("GET")]
        public void Index(WebServerEventArgs e)
        {
            var index = Views.GetBytes(Views.BinaryResources.Index);
            e.Context.Response.ContentType = "text/html";

            var responseAsString = Encoding.UTF8.GetString(index, 0, index.Length);

            WebServer.OutPutStream(e.Context.Response, responseAsString);
        }

        [Route("")]
        [Method("POST")]
        public void Credentials(WebServerEventArgs e)
        {
            Hashtable hashPars = ParseParamsFromStream(e.Context.Request.InputStream);

            var ssid = (string)hashPars["ssid"];
            var password = (string)hashPars["password"];

            ssid = HttpUtility.UrlDecode(ssid);

            Debug.WriteLine($"Wireless parameters SSID:{ssid} PASSWORD:{password}");

            bool success = Wifi.Configure(ssid, password);

            byte[] bytes;

            if (success)
            {
                bytes = Views.GetBytes(Views.BinaryResources.Success);
            }
            else
            {
                bytes = Views.GetBytes(Views.BinaryResources.Error);
            }

            e.Context.Response.ContentType = "text/html";
            var responseAsString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            WebServer.OutPutStream(e.Context.Response, responseAsString);

            e.Context.Response.Close();

            Thread.Sleep(1000);

            if (!string.IsNullOrEmpty(ssid) && !string.IsNullOrEmpty(password))
            {
                Wifi.Configure(ssid, password);
                AccessPoint.Disable();

                Thread.Sleep(1000);

                Power.RebootDevice();
            }
        }

        static Hashtable ParseParamsFromStream(Stream inputStream)
        {
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, (int)inputStream.Length);

            return ParseParams(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
        }

        static Hashtable ParseParams(string rawParams)
        {
            Hashtable hash = new Hashtable();

            string[] parPairs = rawParams.Split('&');
            foreach (string pair in parPairs)
            {
                string[] nameValue = pair.Split('=');
                hash.Add(nameValue[0], nameValue[1]);
            }

            return hash;
        }
    }
}
