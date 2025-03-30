namespace greenguard_hub.Services.Device
{
    public class Device
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public DeviceType Type { get; set; }
    }

    public enum DeviceType
    {
        MiFlora
    }
}
