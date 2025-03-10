using nanoFramework.Device.Bluetooth.Advertisement;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;

namespace greenguard_hub.Services.MiFlora
{
    class MiFloraMonitorService : BackgroundService
    {
        private readonly MiFloraService _miFloraService;
        private readonly static Hashtable _miFloraDevices = new();

        public MiFloraMonitorService(MiFloraService miFloraService)
        {
            _miFloraService = miFloraService;
        }

        protected override void ExecuteAsync()
        {

            while (!CancellationRequested)
            {
                try
                {
                    BluetoothLEAdvertisementWatcher watcher = new()
                    {
                        ScanningMode = BluetoothLEScanningMode.Active
                    };

                    watcher.Received += OnAdvertisementReceived;
                    watcher.Start();

                    Thread.Sleep(10000);

                    watcher.Stop();

                    foreach (DictionaryEntry entry in _miFloraDevices)
                    {
                        BluetoothLEDevice device = entry.Value as BluetoothLEDevice;

                        if (ConnectAndRegister(device))
                        {
                        }
                    }

                    Thread.Sleep(60000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex}");
                }
            }
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (_miFloraService.IsValidDevice(args.Advertisement))
            {
                if (!_miFloraDevices.Contains(args.BluetoothAddress))
                {
                    Debug.WriteLine($"Adding MiFlora device: {args.BluetoothAddress}");
                    _miFloraDevices.Add(args.BluetoothAddress, BluetoothLEDevice.FromBluetoothAddress(args.BluetoothAddress, args.BluetoothAddressType));
                }
            }
        }

        private static bool ConnectAndRegister(BluetoothLEDevice device)
        {
            bool result = false;

            var realtimeServiceUuidParsed = Guid.TryParseGuidWithDashes(MiFloraConstants.RealtimeServiceUuid, out var realtimeServiceUuid);
            var batteryAndFirmwareCharacteristicUuidParsed = Guid.TryParseGuidWithDashes(MiFloraConstants.BatteryAndFirmwareCharacteristicUuid, out var batteryAndFirmwareCharacteristicUuid);

            GattDeviceServicesResult sr = device.GetGattServicesForUuid(realtimeServiceUuid);
            if (realtimeServiceUuidParsed && batteryAndFirmwareCharacteristicUuidParsed && sr.Status == GattCommunicationStatus.Success)
            {
                result = true;

                Debug.WriteLine($"Connected to MiFlora device: {device.Name}");

                foreach (GattDeviceService service in sr.Services)
                {
                    Console.WriteLine($"Service UUID {service.Uuid}");

                    GattCharacteristicsResult cr = service.GetCharacteristicsForUuid(batteryAndFirmwareCharacteristicUuid);

                    if (cr.Status == GattCommunicationStatus.Success)
                    {
                        foreach (GattCharacteristic gc in cr.Characteristics)
                        {
                        }
                    }
                }
            }
            return result;
        }
    }
}
