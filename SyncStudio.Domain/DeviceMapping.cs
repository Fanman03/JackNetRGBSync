using System.Collections.Generic;

namespace SyncStudio.Domain
{
    public class DeviceMapping
    {
        public DeviceProxy SourceDevice { get; set; }
        public List<DeviceProxy> DestinationDevices { get; set; }
    }
}