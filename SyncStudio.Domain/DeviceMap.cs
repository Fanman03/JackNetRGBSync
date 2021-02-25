using System.Collections.Generic;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceMap
    {
        public ControlDevice Source { get; set; }
        public List<ControlDevice> Dest { get; set; }
    }
}