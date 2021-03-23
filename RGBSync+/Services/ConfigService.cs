using Newtonsoft.Json;
using SyncStudio.WPF.Model;
using SharedCode;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using SyncStudio.Core.Services.Device;
using SyncStudio.Domain;
using SyncStudio.WPF.Helper;

namespace SyncStudio.WPF.Services
{
    public class ConfigService
    {
        IInterfaceDevices _devices;
        public string ProfileS_DIRECTORY;
        public string SLSCONFIGS_DIRECTORY;

        public ConfigService(string Profiles_dir, string configs_dir, IInterfaceDevices devices)
        {
            _devices = devices;

            ProfileS_DIRECTORY = Profiles_dir;
            SLSCONFIGS_DIRECTORY = configs_dir;

            if (!Directory.Exists(SLSCONFIGS_DIRECTORY))
            {
                Directory.CreateDirectory(SLSCONFIGS_DIRECTORY);
            }

            configTimer = new Timer(ConfigUpdate, null, 0, 5000);
        }

        public Timer configTimer;
        public List<DeviceMap> MappedDevices = new List<DeviceMap>();
        public bool isHotLoading;


        private List<DeviceMapping> deviceMappingProxy;

        public List<DeviceMapping> DeviceMappingProxy
        {
            get => deviceMappingProxy;
            set { deviceMappingProxy = value; }
        }

        //TODO does this actually run? I cant see that we ever set up the proxy
        //public void SetUpMappedDevicesFromConfig()
        //{
        //    List<InterfaceControlDevice> alreadyBeingSyncedTo = new List<InterfaceControlDevice>();
        //    MappedDevices = new List<DeviceMap>();
        //    if (ServiceManager.Instance.ConfigService.DeviceMappingProxy != null)
        //    {
        //        foreach (DeviceMapping deviceMapping in ServiceManager.Instance.ConfigService.DeviceMappingProxy)
        //        {
        //            InterfaceControlDevice src = _devices.GetDevices().FirstOrDefault(x =>
        //                x.Name == deviceMapping.SourceDevice.DeviceName &&
        //                x.InterfaceDriverProperties.Name == deviceMapping.SourceDevice.DriverName);
        //            if (src != null)
        //            {
        //                DeviceMap dm = new DeviceMap
        //                {
        //                    Source = src,
        //                    Dest = new List<InterfaceControlDevice>()
        //                };

        //                foreach (DeviceProxy deviceMappingDestinationDevice in deviceMapping.DestinationDevices)
        //                {
        //                    InterfaceControlDevice tmp = _devices.GetDevices().FirstOrDefault(x =>
        //                        x.Name == deviceMappingDestinationDevice.DeviceName &&
        //                        x.InterfaceDriverProperties.Name == deviceMappingDestinationDevice.DriverName);

        //                    if (alreadyBeingSyncedTo.Contains(tmp) == false)
        //                    {
        //                        if (tmp != null)
        //                        {
        //                            dm.Dest.Add(tmp);

        //                            alreadyBeingSyncedTo.Add(tmp);
        //                        }
        //                    }
        //                }

        //                MappedDevices.Add(dm);
        //            }
        //        }
        //    }
        //}

        private void ConfigUpdate(object state)
        {
            //todo
            //ServiceManager.Instance.ConfigService.CheckSettingStale();
            //foreach (ISimpleLed slsManagerDriver in ServiceManager.Instance.SLSManager.Drivers.ToList().Where(x => x is ISimpleLedWithConfig).ToList())
            //{
            //    ISimpleLedWithConfig cfgable = slsManagerDriver as ISimpleLedWithConfig;
            //    if (cfgable.GetIsDirty())
            //    {
            //        ServiceManager.Instance.SLSManager.SaveConfig(cfgable);
            //    }
            //}
        }

        public Settings Settings = null;

        public LauncherPrefs LauncherPrefs { get; set; } = new LauncherPrefs();

        public void LoadSettings()
        {
            if (File.Exists("Settings.json"))
            {
                string json = File.ReadAllText("Settings.json");
                //try
                {
                    Settings = JsonConvert.DeserializeObject<Settings>(json);
                    if (Settings == null)
                    {
                        Settings = new Settings();
                    }

                    ServiceManager.Instance.Logger.Info("Settings loaded");
                    HotLoadSettings();
                }
                //catch
                //{
                //    Logger.Error("error loading settings");
                //}
            }
            else
            {
                Settings = new Settings();
                SaveSettings();
                HotLoadSettings();
            }
        }

        public DateTime TimeSettingsLastSave = DateTime.MinValue;

        private bool actuallySaveRunning = false;
        private bool actualySaveContention = false;

        private async Task ActuallySave()
        {
            if (Settings != null)
            {
                if (actuallySaveRunning)
                {
                    actualySaveContention = true;
                    return;
                }

                actuallySaveRunning = true;

                DateTime start = DateTime.Now;

                while ((DateTime.Now - start).TotalMilliseconds < 1000)
                {
                    await Task.Delay(10);
                    if (actualySaveContention)
                    {
                        start = DateTime.Now;
                        actualySaveContention = false;
                    }
                }

                try
                {
                    Debug.WriteLine(
                        "********************************************************************************************* SAVING SETTINGS!");
                    string json = JsonConvert.SerializeObject(Settings);
                    File.WriteAllText("Settings.json", json);
                    TimeSettingsLastSave = DateTime.Now;
                    Settings.AreSettingsStale = false;
                }
                catch
                {
                }

                actuallySaveRunning = false;
            }
        }

        public void SaveSettings()
        {
            ActuallySave();
        }


        public bool SettingsRequiresSave()
        {
            if (ServiceManager.Instance.ConfigService.Settings != null)
            {
                if (Settings.AreSettingsStale) return true;
                if (Settings.DeviceSettings != null)
                {
                    if (ServiceManager.Instance.ConfigService.Settings.DeviceSettings.Any(x => x.AreDeviceSettingsStale)
                    ) return true;
                }
            }

            return false;
        }

        public void HotLoadSettings()
        {
            //    if (isHotLoading) return;

            //    isHotLoading = true;

            //    CultureInfo ci = CultureInfo.InstalledUICulture;
            //    if (ServiceManager.Instance.ConfigService.Settings.Lang == null)
            //    {
            //        ServiceManager.Instance.Logger.Debug("Language is not set, inferring language from system culture. Lang=" + ci.TwoLetterISOLanguageName);
            //        ServiceManager.Instance.ConfigService.Settings.Lang = ci.TwoLetterISOLanguageName;
            //    }

            //    Thread.CurrentThread.CurrentUICulture = new CultureInfo(ServiceManager.Instance.ConfigService.Settings.Lang);

            //    if (ServiceManager.Instance.ConfigService.Settings.EnableDiscordRPC)
            //    {
            //        ServiceManager.Instance.DiscordService.ConnectDiscord();
            //    }

            //    //todo
            //    //SyncStudio.Core.ServiceManager.LedService.LoadOverrides();

            //    ServiceManager.Instance.ProfileService.profilePathMapping.Clear();
            //    if (!Directory.Exists(ProfileS_DIRECTORY))
            //    {
            //        Directory.CreateDirectory(ProfileS_DIRECTORY);
            //        ServiceManager.Instance.ProfileService.GenerateNewProfile("Default");
            //        isHotLoading = false;
            //        return;
            //    }

            //    string[] profiles = Directory.GetFiles(ProfileS_DIRECTORY, "*.json");

            //    if (profiles == null || profiles.Length == 0)
            //    {
            //        ServiceManager.Instance.ProfileService.GenerateNewProfile("Default");
            //    }

            //    Settings.ProfileNames = new ObservableCollection<string>();

            //    foreach (string profile in profiles)
            //    {
            //        string profileName = ServiceManager.Instance.ProfileService.GetProfileFromPath(profile)?.Name;
            //        if (!string.IsNullOrWhiteSpace(profileName))
            //        {
            //            try
            //            {

            //                ServiceManager.Instance.ProfileService.profilePathMapping.Add(profileName, profile);

            //                if (Settings.ProfileNames == null)
            //                {
            //                    Settings.ProfileNames = new ObservableCollection<string>();
            //                }
            //                Settings.ProfileNames.Add(profileName);
            //                ServiceManager.Instance.ProfileService.OnProfilesChangedInvoke(this, new EventArgs());
            //            }
            //            catch
            //            {
            //            }
            //        }
            //    }

            //    if (Settings.ProfileNames == null)
            //    {
            //        Settings.ProfileNames = new ObservableCollection<string>();
            //    }

            //    //todo
            //    if (false && Settings.ProfileNames.Contains(Settings.CurrentProfile))
            //    {

            //        ServiceManager.Instance.ProfileService.LoadProfileFromName(Settings.CurrentProfile);
            //    }
            //    else
            //    {
            //        Settings.CurrentProfile = "Default";
            //        ServiceManager.Instance.ProfileService.LoadProfileFromName(Settings.CurrentProfile);
            //    }


            //    double tmr2 = 1000.0 / MathHelper.Clamp(Settings.UpdateRate, 1, 100);
            //    //todo
            //    //SyncStudio.Core.ServiceManager.LedService.SetUpdateRate(tmr2);

            //    ServiceManager.Instance.ApiServerService.Stop();

            //    if (Settings.ApiEnabled)
            //    {
            //        ServiceManager.Instance.ApiServerService.Start();
            //    }

            //    if (Settings.DeviceSettings != null)
            //    {
            //        foreach (DeviceSettings SettingsDeviceSetting in Settings.DeviceSettings)
            //        {
            //            InterfaceControlDevice cd = _devices.GetControlDeviceFromName(SettingsDeviceSetting.ProviderName, SettingsDeviceSetting.Name);

            //            if (cd != null)
            //            {
            //                cd.LedShift = SettingsDeviceSetting.LEDShift;
            //                cd.Reverse = SettingsDeviceSetting.Reverse;

            //            }
            //        }
            //    }

            //    if (!string.IsNullOrWhiteSpace(Settings.SimpleLedAuthToken))
            //    {
            //        ServiceManager.Instance.SLSAuthService.SimpleLedAuth.AccessToken = Settings.SimpleLedAuthToken;
            //        ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserName = Settings.SimpleLedUserName;
            //        ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserId = Settings.SimpleLedUserId;
            //        try
            //        {
            //            ServiceManager.Instance.SLSAuthService.SimpleLedAuth.Authenticate(() =>
            //            {
            //                Debug.WriteLine("Authenticated");
            //                ServiceManager.Instance.SLSAuthService.SimpleLedAuthenticated = true;
            //            });
            //        }
            //        catch
            //        {
            //            ServiceManager.Instance.SLSAuthService.SimpleLedAuth.AccessToken = "";
            //            ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserName = "";
            //            ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserId = Guid.Empty;
            //            ServiceManager.Instance.SLSAuthService.SimpleLedAuthenticated = false;

            //            Settings.SimpleLedAuthToken = "";
            //            Settings.SimpleLedUserId = Guid.Empty;
            //            Settings.SimpleLedUserName = "";
            //        }
            //    }

            //    if (File.Exists("launcherPrefs.json"))
            //    {
            //        LauncherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText("launcherPrefs.json"));
            //    }
            //    else
            //    {
            //        LauncherPrefs = new LauncherPrefs();
            //    }

            //    isHotLoading = false;
            //}

        }
    }
}
