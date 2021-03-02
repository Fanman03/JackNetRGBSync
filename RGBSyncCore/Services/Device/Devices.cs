using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SimpleLed;
using SyncStudio.Core.Helpers;
using SyncStudio.Domain;

namespace SyncStudio.Core.Services.Device
{
    public class Devices : IDevices
    {
        public event Events.DeviceChangeEventHandler DeviceAdded;

        public event Events.DeviceChangeEventHandler DeviceRemoved;

        private bool isRunning;
        private bool PauseSyncing;
        internal List<ControlDevice> SLSDevices = new List<ControlDevice>();
        public List<DeviceOverrides> DeviceOverrides = new List<DeviceOverrides>();

        public void AddDevice(ControlDevice device)
        {
            SLSDevices.Add(device);
            DeviceAdded?.Invoke(this, new Events.DeviceChangeEventArgs(device));
        }

        public void RemoveDevice(ControlDevice device)
        {
            SLSDevices.Remove(device);
            DeviceRemoved?.Invoke(this, new Events.DeviceChangeEventArgs(device));
        }

        public void RemoveProvider(Guid providerId)
        {
            throw new NotImplementedException();
        }

        public void SetOverride(ControlDevice cd, DeviceOverrides overrider)
        {
            DeviceOverrides existing = DeviceOverrides.FirstOrDefault(x =>
                x.Name == overrider.Name && x.ConnectedTo == overrider.ConnectedTo &&
                x.ProviderName == overrider.ProviderName);

            if (existing != null)
            {
                DeviceOverrides.Remove(existing);
            }

            DeviceOverrides.Add(overrider);

            DriverProperties props = cd.Driver.GetProperties();
            if (cd.OverrideSupport != OverrideSupport.None)
            {
                if (props?.SetDeviceOverride != null && overrider.CustomDeviceSpecification.LedCount > 0)
                {
                    props.SetDeviceOverride(cd, overrider.CustomDeviceSpecification);
                    cd.CustomDeviceSpecification = overrider.CustomDeviceSpecification;
                }

                cd.GridWidth = overrider.CustomDeviceSpecification.GridWidth;
                cd.GridHeight = overrider.CustomDeviceSpecification.GridHeight;
            }

        }

        public ControlDevice GetControlDeviceFromName(string providerName, string name)
        {
            return SLSDevices.FirstOrDefault(x => x.Driver.Name() == providerName && x.Name == name);
        }

        public void SyncDevice(ControlDevice @from, ControlDevice to)
        {
            SyncDevice(@from.UniqueIdentifier, to.UniqueIdentifier);
        }

        public void SyncDevice(string fromUID, string toUID)
        {
            var profile = ServiceManager.Profiles.GetCurrentProfile();
            var removeList = profile.DeviceProfileSettings.Where(x => x.DestinationUID == toUID).ToList();

            foreach (DeviceProfileSettings deviceProfileSettings in removeList)
            {
                profile.DeviceProfileSettings.Remove(deviceProfileSettings);
            }

            profile.DeviceProfileSettings.Add(new DeviceProfileSettings
            {
                SourceUID = fromUID,
                DestinationUID = toUID
            });

            ServiceManager.Profiles.SetCurrentProfile(profile);

            ServiceManager.Profiles.SaveProfile(ServiceManager.Profiles.GetCurrentProfile());
        }

        public IEnumerable<ControlDevice> GetDevices()
        {
            return SLSDevices;
        }


        public DeviceOverrides GetOverride(ControlDevice cd)
        {
            DeviceOverrides existing = DeviceOverrides.ToList().FirstOrDefault(x => x.UID == cd.UniqueIdentifier);

            if (existing == null)
            {
                existing = GenerateOverride(cd);
            }

            return existing;
        }

        private DeviceOverrides GenerateOverride(ControlDevice cd)
        {
            DeviceOverrides existing = new DeviceOverrides
            {
                Name = cd.Name,
                UID = cd.UniqueIdentifier,
                ProviderName = cd.Driver?.Name(),
                TitleOverride = string.IsNullOrWhiteSpace(cd.TitleOverride) ? cd.Driver.Name() : cd.TitleOverride,
                SubTitleOverride = cd.Name,
                CustomDeviceSpecification = new CustomDeviceSpecification()
            };

            DeviceOverrides.Add(existing);

            return existing;
        }

        private void SLSUpdate(object state)
        {

            if (isRunning)
            {
                return;
            }

            Profile profile = ServiceManager.Profiles.GetCurrentProfile();


            if (PauseSyncing)
            {
                isRunning = false;
                return;
            }

            if (profile == null || profile.DeviceProfileSettings == null)
            {
                isRunning = false;
                return;
            }

            List<ControlDevice> devicesToPull = new List<ControlDevice>();

            //using (new BenchMark("Setup pull list"))
            {
                foreach (DeviceProfileSettings currentProfileDeviceProfileSetting in profile.DeviceProfileSettings.ToList())
                {
                    try
                    {
                        ControlDevice cd = SLSDevices.ToList().FirstOrDefault(x =>x.UniqueIdentifier == currentProfileDeviceProfileSetting.DestinationUID);

                        if (cd != null)
                        {
                            if (!devicesToPull.Contains(cd))
                            {
                                devicesToPull.Add(cd);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            foreach (ControlDevice controlDevice in devicesToPull)
            {
                if (controlDevice.Driver.GetProperties().SupportsPull)
                {
                    //using (new BenchMark("Pulling " + controlDevice.Name))
                    {
                        controlDevice.Pull();
                    }
                }
            }

            List<PushListItem> pushMe = new List<PushListItem>();

            //using (new BenchMark("mapping"))
            {


                foreach (DeviceProfileSettings currentProfileDeviceProfileSetting in
                    profile.DeviceProfileSettings.ToList())
                {
                    ControlDevice cd = SLSDevices.ToArray().FirstOrDefault(x => x.UniqueIdentifier== currentProfileDeviceProfileSetting.SourceUID);

                    ControlDevice dest = SLSDevices.ToArray().FirstOrDefault(x => x.UniqueIdentifier == currentProfileDeviceProfileSetting.DestinationUID);

                    if (cd != null && dest != null)
                    {
                        string key = currentProfileDeviceProfileSetting.DestinationUID + currentProfileDeviceProfileSetting.SourceUID;
                        
                        //using (new BenchMark("Mapping " + dest.Name))
                        {
                            dest.MapLEDs(cd);
                        }

                        pushMe.Add(new PushListItem
                        {
                            Device = dest,
                            Driver = dest.Driver,
                            Key = key
                        });

                    }
                }
            }

            //using (new BenchMark("Push loop"))
            {
                Parallel.ForEach(pushMe.GroupBy(x => x.Driver), gp =>
                {

                    Parallel.ForEach(gp.ToList(), t =>
                    {
                        try
                        {
                            if (!isMapping.Contains(t.Device.Name))
                            {
                                isMapping.Add(t.Device.Name);
                                Task.Run(() =>
                                {
                                    gp.Key.Push(t.Device);
                                    isMapping.Remove(t.Device.Name);
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                        }
                    });

                });

                // );
            }

            isRunning = false;
        }

        //public Dictionary<string, bool> isMapping = new Dictionary<string, bool>();

        public List<string> isMapping = new List<string>();

        public class PushListItem
        {
            public ISimpleLed Driver { get; set; }
            public ControlDevice Device { get; set; }
            public string Key { get; set; }

        }


        //public Domain.Device ToDevice(ControlDevice device)
        //{
        //    ObservableCollection<DeviceProfileSettings> temp = ServiceManager.Profiles.GetCurrentProfile()?.DeviceProfileSettings;

        //    DeviceProfileSettings thingy = temp.FirstOrDefault(x => x.DestinationUID == device.UniqueIdentifier);

        //    DriverProperties props = device.Driver.GetProperties();

        //    DeviceOverrides overrides = GetOverride(device);

        //    BitmapImage bmp = null;

        //    try
        //    {
        //        if (overrides?.CustomDeviceSpecification?.Bitmap != null)
        //        {
        //            bmp = overrides.CustomDeviceSpecification.Bitmap.ToBitmapImage();
        //        }
        //        else
        //        {
        //            bmp = device.ProductImage.ToBitmapImage();
        //        }
        //    }
        //    catch
        //    {
        //    }

        //    Domain.Device tmp = new Domain.Device
        //    {
        //        SunkTo = thingy?.SourceName ?? "",
        //        ControlDevice = device,
        //        Image = bmp,
        //        Name = device.Name,
        //        ProviderName = device.Driver.Name(),
        //        SupportsPull = props.SupportsPull,
        //        SupportsPush = props.SupportsPush,
        //        DriverProps = props,
        //        Title = string.IsNullOrWhiteSpace(device.TitleOverride)
        //            ? device.Driver.Name()
        //            : device.TitleOverride,
        //        ConnectedTo = device.ConnectedTo,
        //        Overrides = GetOverride(device)

        //    };

        //    return tmp;
        //}


    }
}
