using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;
using SyncStudio.Domain;

namespace SyncStudio.Core.Services.Device
{
    public interface IDevices
    {
        event Events.DeviceChangeEventHandler DeviceAdded;

        event Events.DeviceChangeEventHandler DeviceRemoved;

        void AddDevice(ControlDevice device);
        void RemoveDevice(ControlDevice device);
        void RemoveProvider(Guid providerId);

        DeviceOverrides GetOverride(ControlDevice device);
        void SetOverride(ControlDevice device, DeviceOverrides overRide);

        ControlDevice GetControlDeviceFromName(string providerName, string name);

        void SyncDevice(ControlDevice from, ControlDevice to);
        void SyncDevice(string fromUID, string toUID);
        IEnumerable<ControlDevice> GetDevices();
        Task<IEnumerable<ControlDevice>> GetDevicesAsync();
    }

    public interface IInterfaceDevices
    {
        event InterfaceEvents.InterfaceDeviceChangeEventHandler DeviceAdded;

        event InterfaceEvents.InterfaceDeviceChangeEventHandler DeviceRemoved;

        void AddDevice(InterfaceControlDevice device);
        void RemoveDevice(InterfaceControlDevice device);
        void RemoveProvider(Guid providerId);

        Task<DeviceOverrides> GetOverride(InterfaceControlDevice device);
        void SetOverride(InterfaceControlDevice device, DeviceOverrides overRide);

        InterfaceControlDevice GetControlDeviceFromName(string providerName, string name);

        void SyncDevice(InterfaceControlDevice from, InterfaceControlDevice to);
        void SyncDevice(string fromUID, string toUID);
        IEnumerable<InterfaceControlDevice> GetDevices();
        Task<IEnumerable<InterfaceControlDevice>> GetDevicesAsync();
    }
}
