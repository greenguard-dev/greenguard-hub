using nanoFramework.Runtime.Native;
using System;
using System.Net.NetworkInformation;
using System.Net;
using Iot.Device.DhcpServer;
using System.Diagnostics;

namespace greenguard_hub.Services.Wireless
{
    public class AccessPoint
    {
        public static string IpAddress { get; set; } = "192.168.178.8";
        public static string Ssid { get; set; } = "GreenGuard-Hub";

        public static void SetWifiAp()
        {
            Wifi.Disable();

            if (Setup() == false)
            {
                Debug.WriteLine($"Setup Soft AP, Rebooting device");
                Power.RebootDevice();
            }

            Debug.WriteLine($"Soft AP setup complete");

            var dhcpserver = new DhcpServer
            {
                CaptivePortalUrl = $"http://{IpAddress}"
            };

            var dhcpInitResult = dhcpserver.Start(IPAddress.Parse(IpAddress), new IPAddress(new byte[] { 255, 255, 255, 0 }));
            if (!dhcpInitResult)
            {
                Debug.WriteLine($"Error initializing DHCP server.");
                Power.RebootDevice();
            }

            Debug.WriteLine($"DHCP server started");
        }

        public static void Disable()
        {
            WirelessAPConfiguration wapconf = GetConfiguration();
            wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.None;
            wapconf.SaveConfiguration();
        }

        public static bool Setup()
        {
            NetworkInterface ni = GetInterface();
            WirelessAPConfiguration wapconf = GetConfiguration();

            if (wapconf.Options == (WirelessAPConfiguration.ConfigurationOptions.Enable |
                                    WirelessAPConfiguration.ConfigurationOptions.AutoStart) &&
                ni.IPv4Address == IpAddress)
            {
                return true;
            }

            ni.EnableStaticIPv4(IpAddress, "255.255.255.0", IpAddress);

            wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.AutoStart |
                            WirelessAPConfiguration.ConfigurationOptions.Enable;

            wapconf.Ssid = Ssid;
            wapconf.MaxConnections = 1;
            wapconf.Authentication = System.Net.NetworkInformation.AuthenticationType.Open;
            wapconf.Password = "";
            wapconf.SaveConfiguration();

            return false;
        }

        public static WirelessAPConfiguration GetConfiguration()
        {
            NetworkInterface ni = GetInterface();
            return WirelessAPConfiguration.GetAllWirelessAPConfigurations()[ni.SpecificConfigId];
        }

        public static NetworkInterface GetInterface()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in Interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.WirelessAP)
                {
                    return ni;
                }
            }
            return null;
        }

        public static string GetIP()
        {
            NetworkInterface ni = GetInterface();
            return ni.IPv4Address;
        }

    }
}
