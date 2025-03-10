using System;
using System.Text;

namespace greenguard_hub.Services.MiFlora
{
    public class MiFloraSensorReading
    {
        // in 0.1 °C
        public short Temperature { get; private set; }

        // in LUX
        public uint Brightness { get; private set; }

        // in %
        public byte Moisture { get; private set; }

        // in µS/cm
        public ushort Conductivity { get; private set; }

        public MiFloraSensorReading(byte[] value)
        {
            if (value.Length < 16)
            {
                throw new ArgumentException("Value must be at least 16 bytes long.");
            }

            // 2 bytes at pos 00-01: "0E 01" -> 270 * 0.1°C = 27.0 °C
            Temperature = BitConverter.ToInt16(value, 0);

            // 4 bytes at pos 03-06: "48 02 00 00" -> 584 lux
            Brightness = BitConverter.ToUInt32(value, 3);

            // 1 byte at pos 07: "28" -> 40%
            Moisture = value[7];

            // 2 bytes at pos 08-09: "D0 00" -> 208 µS/cm
            Conductivity = BitConverter.ToUInt16(value, 8);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Temperature: {Temperature / 10.0:F1} °C");
            sb.AppendLine($"Brightness: {Brightness} lux");
            sb.AppendLine($"Soil moisture: {Moisture} %");
            sb.AppendLine($"Conductivity: {Conductivity} µS/cm");
            return sb.ToString();
        }
    }
}
