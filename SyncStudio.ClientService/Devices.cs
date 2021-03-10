using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using SimpleLed;
using SyncStudio.Core.Services.Device;
using SyncStudio.Domain;

namespace SyncStudio.ClientService
{
    public class Devices : IInterfaceDevices
    {
        public Devices()
        {
            DispatcherTimer eventTimer = new DispatcherTimer();
            eventTimer.Interval = TimeSpan.FromSeconds(1);
            eventTimer.Tick += EventTimerOnTick;
            eventTimer.Start();
        }
        private bool okayToSyncDevices = true;
        private async void EventTimerOnTick(object sender, EventArgs e)
        {
            if (!okayToSyncDevices) return;
            okayToSyncDevices = false;
            if (DeviceAdded != null || DeviceRemoved != null)
            {
                var temp = await easyRest.Get<IEnumerable<InterfaceControlDevice>>("GetDevices");

                if (DeviceAdded != null)
                {
                    var added = temp.Where(p => controlDevices.All(t => p.UniqueIdentifier != t.UniqueIdentifier));
                    foreach (var addedDevice in added)
                    {
                        DeviceAdded(this, new InterfaceEvents.InterfaceDeviceChangeEventArgs(addedDevice));
                        controlDevices.Add(addedDevice);
                    }
                }

                if (DeviceRemoved != null)
                {
                    var removed = controlDevices.Where(p => temp.All(t => p.UniqueIdentifier != t.UniqueIdentifier));
                    foreach (var removedDevice in removed)
                    {
                        DeviceRemoved(this, new InterfaceEvents.InterfaceDeviceChangeEventArgs(removedDevice));
                        controlDevices.Remove(removedDevice);
                    }
                }
            }

            okayToSyncDevices = true;
        }

        private List<InterfaceControlDevice> controlDevices = new List<InterfaceControlDevice>();
        private readonly EasyRest easyRest = new EasyRest("http://localhost:59023/api/Devices/");

        public event InterfaceEvents.InterfaceDeviceChangeEventHandler DeviceAdded;
        public event InterfaceEvents.InterfaceDeviceChangeEventHandler DeviceRemoved;
        public void RemoveProvider(Guid providerId)
        {
            throw new NotImplementedException();
        }

        public InterfaceControlDevice GetControlDeviceFromName(string providerName, string name)
        {
            throw new NotImplementedException();
        }

        public void SyncDevice(string fromUID, string toUID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<InterfaceControlDevice> GetDevices()
        {
            return easyRest.Get<IEnumerable<InterfaceControlDevice>>("GetDevices").Result;
        }

        public Task<IEnumerable<InterfaceControlDevice>> GetDevicesAsync()
        {
            return easyRest.Get<IEnumerable<InterfaceControlDevice>>("GetDevices");
        }

        public void SyncDevice(InterfaceControlDevice @from, InterfaceControlDevice to)
        {
            throw new NotImplementedException();
        }

        public void SetOverride(InterfaceControlDevice device, DeviceOverrides overRide)
        {
            throw new NotImplementedException();
        }

        public Task<DeviceOverrides> GetOverride(InterfaceControlDevice device)
        {
            return easyRest.Post<DeviceOverrides, InterfaceControlDevice>("GetDeviceOverrides", device);
        }

        public void RemoveDevice(InterfaceControlDevice device)
        {
            throw new NotImplementedException();
        }

        public void AddDevice(InterfaceControlDevice device)
        {
            throw new NotImplementedException();
        }
    }
}
