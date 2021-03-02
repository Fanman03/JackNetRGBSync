using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SimpleLed;
using SyncStudio.Core.Helpers;
using SyncStudio.Core.Services.Device;
using SyncStudio.Domain;

namespace SyncStudio.Core.Services.Led
{
    public class LedService
    {
        //public DispatcherTimer SLSTimer;
        public const string SLSPROVIDER_DIRECTORY = "Providers";
        public bool PauseSyncing { get; set; } = false;

        private readonly Dictionary<string, string> pathfun = new Dictionary<string, string>();
        //public RSSBackgroundDevice RssBackgroundDevice = new RSSBackgroundDevice();
        
        public LedService()
        {

        }

        public ControlDevice DeviceBeingAligned;

        private readonly ControlDevice virtualAlignmentDevice = new ControlDevice
        {
            LEDs = new ControlDevice.LedUnit[64]
            {
                new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)},
                new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)},
                new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)},
                new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)}, new ControlDevice.LedUnit{Color = new LEDColor(255,0,0)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)}, new ControlDevice.LedUnit{Color = new LEDColor(0,255,0)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},
                new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)},new ControlDevice.LedUnit{Color = new LEDColor(0,0,255)}
            }
        };

        bool isRunning = false;
        private void SLSUpdate(object state)
        {

            if (isRunning)
            {
                return;
            }

            SyncStudio.Domain.Profile CurrentProfile = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile();


            if (PauseSyncing)
            {
                isRunning = false;
                return;
            }

            if (CurrentProfile == null || CurrentProfile.DeviceProfileSettings == null)
            {
                isRunning = false;
                return;
            }

            List<ControlDevice> devicesToPull = new List<ControlDevice>();

            //using (new BenchMark("Setup pull list"))
            {
                foreach (DeviceProfileSettings currentProfileDeviceProfileSetting in
                    CurrentProfile.DeviceProfileSettings.ToList().Where(x=>x.DestinationUID!=null && x.SourceUID!=null))
                {
                    try
                    {
                        ControlDevice cd = ((Devices)ServiceManager.Devices).SLSDevices.ToList().FirstOrDefault(x => x.UniqueIdentifier == currentProfileDeviceProfileSetting.SourceUID);

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


                foreach (DeviceProfileSettings currentProfileDeviceProfileSetting in CurrentProfile.DeviceProfileSettings.ToList().Where(x => x.DestinationUID != null && x.SourceUID != null))
                {
                    ControlDevice cd = ((Devices)ServiceManager.Devices).SLSDevices.ToArray().FirstOrDefault(x => x.UniqueIdentifier == currentProfileDeviceProfileSetting.SourceUID );
                    ControlDevice dest = ((Devices)ServiceManager.Devices).SLSDevices.ToArray().FirstOrDefault(x => x.UniqueIdentifier == currentProfileDeviceProfileSetting.DestinationUID);

                    if (cd != null && dest != null)
                    {
                        string key = currentProfileDeviceProfileSetting.SourceUID +
                                     currentProfileDeviceProfileSetting.DestinationUID;


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
        //    ObservableCollection<DeviceProfileSettings> temp = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile()?.DeviceProfileSettings;

        //    DeviceProfileSettings thingy = temp.FirstOrDefault(x => x.DestinationUID == device.UniqueIdentifier);

        //    DriverProperties props = device.Driver.GetProperties();

        //    DeviceOverrides overrides = ServiceManager.Devices.GetOverride(device);

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
        //        Overrides = ServiceManager.Devices.GetOverride(device)

        //    };

        //    return tmp;
        //}





        public bool OverridesDirty = false;
    

        public void LoadSLSProviders()
        {
            
            //UnloadSLSProviders();

            string deviceProvierDir =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty,
                        SLSPROVIDER_DIRECTORY);

            if (!Directory.Exists(deviceProvierDir)) return;
            string[] pluginFolders = Directory.GetDirectories(deviceProvierDir);
            ServiceManager.LoadingMessage("Loading SLS plugins");
            ServiceManager.LoadingMax(pluginFolders.Length);

            int ct = 0;
            foreach (string pluginFolder in pluginFolders)
            {
                //LoadingSplash.Activate();
                ct++;

                ServiceManager.LoadingAmount(ct);
                ServiceManager.Store.LoadPluginFolder(pluginFolder);
            }

            //SolidColorDevice = new SolidColorDriver();
            //GradientDriver = new GradientDriver();

            //ServiceManager.Instance.SLSManager.Drivers.Add(RssBackgroundDevice);
            //ServiceManager.Instance.SLSManager.Drivers.Add(SolidColorDevice);
            //ServiceManager.Instance.SLSManager.Drivers.Add(GradientDriver);

            //RssBackgroundDevice.DeviceAdded += SlsDriver_DeviceAdded;
            //RssBackgroundDevice.DeviceRemoved += SlsDriver_DeviceRemoved;

            //SolidColorDevice.DeviceAdded += SlsDriver_DeviceAdded;
            //SolidColorDevice.DeviceRemoved += SlsDriver_DeviceRemoved;

            //GradientDriver.DeviceAdded += SlsDriver_DeviceAdded;
            //GradientDriver.DeviceRemoved += SlsDriver_DeviceRemoved;

            //SolidColorDevice.Configure(new DriverDetails());
            //GradientDriver.Configure(new DriverDetails());

            //HarnessDriver(new CUEDriver());

            //SLSManager.RescanRequired += Rescan;

            ServiceManager.LoadingMessage("Updating SLS devices");
            UpdateSLSDevices();
        }
        
        public void UpdateSLSDevices()
        {
            ServiceManager.LoadingMessage("Loading Configs");
            foreach (ISimpleLed drv in ServiceManager.SLSManager.Drivers)
            {
                if (drv is ISimpleLedWithConfig cfgdrv)
                {
                    try
                    {
                        ServiceManager.SLSManager.LoadConfig(cfgdrv);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }


            ServiceManager.LoadingMessage("Getting devices");
            //SLSDevices = SLSManager.GetDevices();
            ServiceManager.LoadingAmount(0);
            ServiceManager.LoadingMax(ServiceManager.SLSManager.Drivers.Count);

        }
        
        public void SetUpdateRate(double tmr2)
        {
            if (UpdateTick != null)
            {
                Debug.WriteLine("Updating up timer with ms of " + tmr2);
                UpdateTickRate = (int)tmr2;
            }
            else
            {
                Debug.WriteLine("Setting up timer with ms of " + tmr2);
                UpdateTick = new Thread(UpdateTickLogic);
                UpdateTickRate = (int)tmr2;
                UpdateTick.Start();
            }

        }

        DateTime lastBenchmark = DateTime.Now;
        private void UpdateTickLogic()
        {
            DateTime lastSync = DateTime.Now;
            while (true)
            {
                //if ((DateTime.Now - lastBenchmark).TotalSeconds > 20)
                //{
                //    lastBenchmark = DateTime.Now;
                //    BenchMarkProvider.Output();
                //}

                lastSync = DateTime.Now;

                SLSUpdate(null);

                var msTaken = (DateTime.Now - lastSync).TotalMilliseconds;

                var remaining = UpdateTickRate - msTaken;

                if (remaining > 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(remaining));
                }
            }
        }

        private int UpdateTickRate = 33;
        private Thread UpdateTick;
        
    }
}
