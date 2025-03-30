using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.Advertisement;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using System;
using System.Diagnostics;

namespace greenguard_hub.Services.MiFlora
{
    public class MiFloraService
    {
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
                                var batteryLevel = GetBatteryLevel(value.Value);
                                var firmwareVersion = GetFirmwareVersion(value.Value);
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
                                var sensorReading = GetSensorReading(value.Value);

                                Debug.WriteLine(sensorReading.ToString());
                            }
                        }
                    }
                }
            }
        }

        public bool IsValidDevice(BluetoothLEAdvertisement advertisement)
        {
            var parsed = Guid.TryParseGuidWithDashes(MiFloraConstants.AdvertisementServiceUuid, out var guid);

            if (parsed && advertisement.ServiceUuids.Length > 0 && advertisement.ServiceUuids[0].Equals(MiFloraConstants.AdvertisementServiceUuid))
            {
                return true;
            }

            if (advertisement.LocalName.ToLower() == "flower care")
            {
                return true;
            }

            return false;
        }

        public int GetBatteryLevel(Buffer value)
        {
            DataReader rdr = DataReader.FromBuffer(value);
            return rdr.ReadByte();
        }

        public string GetFirmwareVersion(Buffer value)
        {
            DataReader rdr = DataReader.FromBuffer(value);
            rdr.ReadByte();
            rdr.ReadByte();
            return rdr.ReadString(value.Length - 2);
        }

        public MiFloraSensorReading GetSensorReading(Buffer value)
        {
            DataReader rdr = DataReader.FromBuffer(value);
            byte[] data = new byte[value.Length];
            rdr.ReadBytes(data);
            return new MiFloraSensorReading(data);
        }
    }
}
