using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MadLedFrameworkSDK;
using RGB.NET.Core;

namespace RGBSyncPlus.Model
{
    public class DeviceMappingModels
    {
        public class DeviceMap
        {
            public ControlDevice Source { get; set; }
            public List<ControlDevice> Dest { get; set; }
        }
        public class DeviceMapping
        {
            public DeviceProxy SourceDevice { get; set; }
            public List<DeviceProxy> DestinationDevices { get; set; }
        }

        public class DeviceProxy
        {
            public string DriverName { get; set; }
            public string DeviceName { get; set; }
            public DeviceProxy(){}

            public DeviceProxy(ControlDevice device)
            {
                DriverName = device.Driver.Name();
                DeviceName = device.Name;
            }
        }

        public class DeviceMappingViewModel : AbstractBindable
        {
            public DeviceMappingViewModel()
            {
                DestinationDevices.CollectionChanged += DestinationDevices_CollectionChanged;
            }

            private void DestinationDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {

                SyncBack?.Invoke(this);
            }

            public Action<object> SyncBack;
            private bool suspendRollUp = false;

            private ControlDevice sourceDevice;
            public ControlDevice SourceDevice { get=>sourceDevice; set=>SetProperty(ref sourceDevice, value); }

            private bool enabled;

            public bool Enabled
            {
                get => enabled;
                set
                {
                    SetProperty(ref enabled, value); 
                    SyncBack?.Invoke(this);
                }
            }
            public ObservableCollection<DeviceMappingItemViewModel> DestinationDevices { get; set; } = new ObservableCollection<DeviceMappingItemViewModel>();
            public bool expanded;
            public bool Expanded
            {
                get => expanded;
                set
                {
                    SetProperty(ref expanded, value);
                    SyncBack?.Invoke(this);
                }
            }

            public Guid Id { get; set; }
        }

        public class DeviceMappingItemViewModel : AbstractBindable
        {
            public Action<object> SyncBack;
            public ControlDevice DestinationDevice { get; set; }
            private bool enabled;

            public bool Enabled
            {
                get => enabled;
                set
                {
                    SetProperty(ref enabled, value);
                    SyncBack?.Invoke(this);
                }
            }

            public Guid ParentId { get; set; }
        }


    }
}
