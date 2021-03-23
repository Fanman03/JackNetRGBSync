using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceSettings
    {
        [JsonIgnore] public bool AreDeviceSettingsStale;
        public string Name { get; set; }
        
        public string ProviderName { get; set; }
        
        public int LEDShift { get; set; }
        

        public bool Reverse { get; set; }
        
        public bool LEDCountOverride { get; set; }
        
        public int LEDCountOverrideValue { get; set; }
    }

}
