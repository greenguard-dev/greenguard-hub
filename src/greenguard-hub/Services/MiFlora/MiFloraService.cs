using nanoFramework.Device.Bluetooth.Advertisement;
using System;

namespace greenguard_hub.Services.MiFlora
{
    public class MiFloraService
    {
        public bool IsValidDevice(BluetoothLEAdvertisement advertisement)
        {
            var parsed = Guid.TryParseGuidWithDashes(MiFloraConstants.AdvertisementServiceUuid, out var guid);

            if (parsed && advertisement.ServiceUuids.Length > 0 && advertisement.ServiceUuids[0].Equals(guid))
            {
                return true;
            }

            if (advertisement.LocalName.ToLower() == "flower care")
            {
                return true;
            }

            return false;
        }
    }
}
