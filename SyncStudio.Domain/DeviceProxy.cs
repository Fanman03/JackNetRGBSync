using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceProxy
    {
        public string DriverName { get; set; }
        public string DeviceName { get; set; }
        public DeviceProxy() { }

        public DeviceProxy(ControlDevice device)
        {
            DriverName = device.Driver.Name();
            DeviceName = device.Name;
        }
    }
}