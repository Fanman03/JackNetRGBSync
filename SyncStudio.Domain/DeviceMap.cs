using System.Collections.Generic;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceMap
    {
        public InterfaceControlDevice Source { get; set; }
        public List<InterfaceControlDevice> Dest { get; set; }
    }
}