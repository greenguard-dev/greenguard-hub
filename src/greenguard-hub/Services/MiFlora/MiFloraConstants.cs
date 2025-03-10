using System;

namespace greenguard_hub.Services.MiFlora
{
    public static class MiFloraConstants
    {
        public static string AdvertisementServiceUuid = "0000fe95-0000-1000-8000-00805f9b34fb";

        public static string RealtimeServiceUuid = "00001204-0000-1000-8000-00805f9b34fb";

        // Needs to be written to in order to receive real-time data
        public static string RequestRealtimeReadCharacteristicUuid = "00001a00-0000-1000-8000-00805f9b34fb";
        public static byte[] RequestRealtimeReadCommand = { 0xA0, 0x1F };

        // Can be read to get real-time data
        public static string RealtimeDataCharacteristicUuid = "00001a01-0000-1000-8000-00805f9b34fb";

        // Can be read to get battery level and firmware version without enabling real-time data
        public static string BatteryAndFirmwareCharacteristicUuid = "00001a02-0000-1000-8000-00805f9b34fb";
    }
}
