using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadLedFrameworkSDK;
using RGB.NET.Core;

namespace RGBSyncPlus.Model
{
    public class DeviceMappingModels
    {
        public class DeviceMapping
        {
            public ControlDevice SourceDevice { get; set; }
            public List<ControlDevice> DestinationDevices { get; set; }
        }

        public class DeviceMappingViewModel : AbstractBindable
        {
            private ControlDevice sourceDevice;
            public ControlDevice SourceDevice { get=>sourceDevice; set=>SetProperty(ref sourceDevice, value); }

            private bool enabled;

            public bool Enabled
            {
                get => enabled;
                set => SetProperty(ref enabled, value);
            }
            public ObservableCollection<DeviceMappingItemViewModel> DestinationDevices { get; set; }
            public bool expanded;
            public bool Expanded
            {
                get => enabled;
                set => SetProperty(ref expanded, value);
            }
        }

        public class DeviceMappingItemViewModel : AbstractBindable
        {
            public ControlDevice DestinationDevice { get; set; }
            public bool Enabled { get; set; }

        }


    }
}
