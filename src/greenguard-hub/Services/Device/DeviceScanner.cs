using nanoFramework.Device.Bluetooth.Advertisement;
using nanoFramework.Device.Bluetooth;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using greenguard_hub.Services.MiFlora;

namespace greenguard_hub.Services.Device
{
    public class DeviceScanner
    {
        private readonly Hashtable _devices = new();

        private readonly MiFloraService _miFloraService;

        public DeviceScanner(MiFloraService miFloraService)
        {
            _miFloraService = miFloraService;
        }

        public ArrayList Scan(int scanIntervalMs = 10000)
        {
            BluetoothLEAdvertisementWatcher watcher = new()
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            watcher.Received += OnAdvertisementReceived;
            watcher.Start();

            Thread.Sleep(scanIntervalMs);

            watcher.Stop();

            ArrayList deviceArray = new();

            foreach (DictionaryEntry device in _devices)
            {
                deviceArray.Add(device.Value as Device);
            }

            return deviceArray;

            void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
            {
                if (_miFloraService.IsValidDevice(args.Advertisement))
                {
                    if (!_devices.Contains(args.BluetoothAddress))
                    {
                        Debug.WriteLine($"Adding device: {args.BluetoothAddress}");

                        _devices.Add(args.BluetoothAddress, new Device
                        {
                            Name = args.Advertisement.LocalName,
                            Address = args.BluetoothAddress.ToString(),
                            Type = DeviceType.MiFlora
                        });
                    }
                }
            }
        }
    }
}
