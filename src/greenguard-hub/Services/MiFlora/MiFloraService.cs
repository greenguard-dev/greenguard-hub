using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.Advertisement;
using System;
using System.Diagnostics;

namespace greenguard_hub.Services.MiFlora
{
    public class MiFloraService
    {
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

        /// <summary>
        /// Battery level is stored in the first hex
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Battery level</returns>
        public int GetBatteryLevel(Buffer value)
        {
            DataReader rdr = DataReader.FromBuffer(value);
            return rdr.ReadByte();
        }

        /// <summary>
        /// Firmware version is stored by an offset of 2 till the end (as ASCII)
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Firmware version</returns>
        public string GetFirmwareVersion(Buffer value)
        {
            DataReader rdr = DataReader.FromBuffer(value);
            rdr.ReadByte();
            rdr.ReadByte();
            return rdr.ReadString(value.Length - 2);
        }

        /// <summary>
        /// Get the real-time entry from the buffer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public MiFloraSensorReading GetSensorReading(Buffer value)
        {
            DataReader rdr = DataReader.FromBuffer(value);
            byte[] data = new byte[value.Length];
            rdr.ReadBytes(data);
            return new MiFloraSensorReading(data);
        }
    }
}
