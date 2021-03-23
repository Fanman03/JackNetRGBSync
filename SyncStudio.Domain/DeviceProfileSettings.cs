using System.Security.AccessControl;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceProfileSettings 
    {
        public string SourceUID { get; set; }

        public string DestinationUID { get; set; }
    }
}