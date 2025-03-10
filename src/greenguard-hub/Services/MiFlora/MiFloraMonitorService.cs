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

                    Debug.WriteLine("Starting MiFlora watcher");
                    watcher.Start();

                    Thread.Sleep(10000);

                    Debug.WriteLine("Stopping MiFlora watcher");
                    watcher.Stop();

                    foreach (DictionaryEntry entry in _miFloraDevices)
                    {
                        BluetoothLEDevice device = entry.Value as BluetoothLEDevice;
                        ConnectAndReadData(device);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex}");
                }

                Thread.Sleep(10000);
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

        private void ConnectAndReadData(BluetoothLEDevice device)
        {
            var realtimeServiceUuidParsed = Guid.TryParseGuidWithDashes(MiFloraConstants.RealtimeServiceUuid, out var realtimeServiceUuid);
            var batteryAndFirmwareCharacteristicUuidParsed = Guid.TryParseGuidWithDashes(MiFloraConstants.BatteryAndFirmwareCharacteristicUuid, out var batteryAndFirmwareCharacteristicUuid);
            var requestRealtimeReadCharacteristicUuidParsed = Guid.TryParseGuidWithDashes(MiFloraConstants.RequestRealtimeReadCharacteristicUuid, out var requestRealtimeReadCharacteristicUuid);
            var realtimeDataCharacteristicUuidParsed = Guid.TryParseGuidWithDashes(MiFloraConstants.RealtimeDataCharacteristicUuid, out var realtimeDataCharacteristicUuid);

            GattDeviceServicesResult realtimeService = device.GetGattServicesForUuid(realtimeServiceUuid);

            if (realtimeServiceUuidParsed
                && batteryAndFirmwareCharacteristicUuidParsed
                && requestRealtimeReadCharacteristicUuidParsed
                && realtimeDataCharacteristicUuidParsed
                && realtimeService.Status == GattCommunicationStatus.Success)
            {
                Debug.WriteLine($"Connected to MiFlora device: {device.Name}");

                foreach (GattDeviceService service in realtimeService.Services)
                {
                    Console.WriteLine($"Service UUID {service.Uuid}");

                    GattCharacteristicsResult batteryAndFirmwareCharacteristic = service.GetCharacteristicsForUuid(batteryAndFirmwareCharacteristicUuid);

                    if (batteryAndFirmwareCharacteristic.Status == GattCommunicationStatus.Success)
                    {
                        foreach (GattCharacteristic gc in batteryAndFirmwareCharacteristic.Characteristics)
                        {
                            GattReadResult value = gc.ReadValue();

                            if (value != null)
                            {
                                var batteryLevel = _miFloraService.GetBatteryLevel(value.Value);
                                var firmwareVersion = _miFloraService.GetFirmwareVersion(value.Value);
                                Debug.WriteLine($"Battery Level: {batteryLevel}");
                                Debug.WriteLine($"Firmware Version: {firmwareVersion}");
                            }
                        }
                    }

                    GattCharacteristicsResult requestRealtimeReadCharacteristic = service.GetCharacteristicsForUuid(requestRealtimeReadCharacteristicUuid);

                    if (requestRealtimeReadCharacteristic.Status == GattCommunicationStatus.Success)
                    {
                        foreach (GattCharacteristic gc in requestRealtimeReadCharacteristic.Characteristics)
                        {
                            Buffer requestRealtimeReadCommandBuffer = new(MiFloraConstants.RequestRealtimeReadCommand);
                            GattWriteResult writeResult = gc.WriteValueWithResult(requestRealtimeReadCommandBuffer, GattWriteOption.WriteWithResponse);

                            Debug.WriteLine("Executed realtime read command");
                        }
                    }

                    GattCharacteristicsResult realtimeDataCharacteristic = service.GetCharacteristicsForUuid(realtimeDataCharacteristicUuid);

                    if (realtimeDataCharacteristic.Status == GattCommunicationStatus.Success)
                    {
                        foreach (GattCharacteristic gc in realtimeDataCharacteristic.Characteristics)
                        {
                            GattReadResult value = gc.ReadValue();

                            if (value != null)
                            {
                                var sensorReading = _miFloraService.GetSensorReading(value.Value);

                                Debug.WriteLine(sensorReading.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}
