using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceSettings : BaseViewModel
    {
        [JsonIgnore] public bool AreDeviceSettingsStale;

        private string name;
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
                AreDeviceSettingsStale = true;
            }
        }


        private string providerName;
        public string ProviderName
        {
            get => providerName;
            set
            {
                SetProperty(ref providerName, value);
                AreDeviceSettingsStale = true;
            }
        }

        private int ledShift = 0;

        public int LEDShift
        {
            get => ledShift;
            set
            {
                SetProperty(ref ledShift, value);
                AreDeviceSettingsStale = true;
            }
        }

        private bool reverse = false;

        public bool Reverse
        {
            get => reverse;
            set
            {
                SetProperty(ref reverse, value);
                AreDeviceSettingsStale = true;
            }
        }


        private bool ledCountOverride = false;

        public bool LEDCountOverride
        {
            get => ledCountOverride;
            set
            {
                SetProperty(ref ledCountOverride, value);
                AreDeviceSettingsStale = true;
            }
        }

        private int ledCountOverrideValue = 0;

        public int LEDCountOverrideValue
        {
            get => ledCountOverrideValue;
            set
            {
                SetProperty(ref ledCountOverrideValue, value);
                AreDeviceSettingsStale = true;
            }
        }
    }

}
