using nanoFramework.Networking;
using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace greenguard_hub.Services.Wireless
{
    public class Wifi
    {
        public static bool IsEnabled()
        {
            Wireless80211Configuration wconf = GetConfiguration();
            return !string.IsNullOrEmpty(wconf.Ssid);
        }

        public static string GetCurrentIPAddress()
        {
            NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

            // get first NI ( Wifi on ESP32 )
            return ni.IPv4Address.ToString();
        }

        public static bool Connect()
        {
            Debug.WriteLine("Try to reconnect");

            if (!WifiNetworkHelper.Reconnect(true, token: new CancellationTokenSource(30_000).Token))
            {
                return false;
            }

            return true;
        }

        public static void Disable()
        {
            Wireless80211Configuration wconf = GetConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.None | Wireless80211Configuration.ConfigurationOptions.SmartConfig;
            wconf.SaveConfiguration();
        }

        public static bool Configure(string ssid, string password)
        {
            // Make sure we are disconnected before we start connecting otherwise
            // ConnectDhcp will just return success instead of reconnecting.
            WifiAdapter wa = WifiAdapter.FindAllAdapters()[0];
            wa.Disconnect();

            CancellationTokenSource cs = new(30_000);
            Debug.WriteLine("Trying to connect to: " + ssid);
            WifiNetworkHelper.Disconnect();

            // Reconfigure properly the normal wifi
            Wireless80211Configuration wconf = GetConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.AutoConnect | Wireless80211Configuration.ConfigurationOptions.Enable;
            wconf.Ssid = ssid;
            wconf.Password = password;
            wconf.SaveConfiguration();

            WifiNetworkHelper.Disconnect();
            bool success;

            success = WifiNetworkHelper.ConnectDhcp(ssid, password, WifiReconnectionKind.Automatic, true, token: cs.Token);

            if (!success)
            {
                wa.Disconnect();
                // Bug in network helper, we've most likely try to connect before, let's make it manual
                var res = wa.Connect(ssid, WifiReconnectionKind.Automatic, password);
                success = res.ConnectionStatus == WifiConnectionStatus.Success;
                Debug.WriteLine($"Connected: {res.ConnectionStatus}");
            }

            return success;
        }

        public static Wireless80211Configuration GetConfiguration()
        {
            NetworkInterface ni = GetInterface();
            return Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
        }

        public static NetworkInterface GetInterface()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in Interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    return ni;
                }
            }
            return null;
        }
    }
}
