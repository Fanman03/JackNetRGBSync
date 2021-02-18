using Newtonsoft.Json;
using RGBSyncStudio.Helper;
using RGBSyncStudio.Model;
using SharedCode;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace RGBSyncStudio.Services
{
    public class ConfigService
    {
        public string ProfileS_DIRECTORY;
        public string SLSCONFIGS_DIRECTORY;
        public ConfigService(string Profiles_dir, string configs_dir)
        {
            ProfileS_DIRECTORY = Profiles_dir;
            SLSCONFIGS_DIRECTORY = configs_dir;

            if (!Directory.Exists(SLSCONFIGS_DIRECTORY))
            {
                Directory.CreateDirectory(SLSCONFIGS_DIRECTORY);
            }

            configTimer = new Timer(ConfigUpdate, null, 0, 5000);
        }

        public Timer configTimer;
        public List<DeviceMappingModels.DeviceMap> MappedDevices = new List<DeviceMappingModels.DeviceMap>();
        public bool isHotLoading;


        private List<DeviceMappingModels.DeviceMapping> deviceMappingProxy;

        public List<DeviceMappingModels.DeviceMapping> DeviceMappingProxy
        {
            get => deviceMappingProxy;
            set { deviceMappingProxy = value; }
        }

        //TODO does this actually run? I cant see that we ever set up the proxy
        public void SetUpMappedDevicesFromConfig()
        {
            List<ControlDevice> alreadyBeingSyncedTo = new List<ControlDevice>();
            MappedDevices = new List<DeviceMappingModels.DeviceMap>();
            if (ServiceManager.Instance.ConfigService.DeviceMappingProxy != null)
            {
                foreach (DeviceMappingModels.DeviceMapping deviceMapping in ServiceManager.Instance.ConfigService.DeviceMappingProxy)
                {
                    ControlDevice src = ServiceManager.Instance.LedService.SLSDevices.FirstOrDefault(x =>
                        x.Name == deviceMapping.SourceDevice.DeviceName &&
                        x.Driver.Name() == deviceMapping.SourceDevice.DriverName);
                    if (src != null)
                    {
                        DeviceMappingModels.DeviceMap dm = new DeviceMappingModels.DeviceMap
                        {
                            Source = src,
                            Dest = new List<ControlDevice>()
                        };

                        foreach (DeviceMappingModels.DeviceProxy deviceMappingDestinationDevice in deviceMapping.DestinationDevices)
                        {
                            ControlDevice tmp = ServiceManager.Instance.LedService.SLSDevices.FirstOrDefault(x =>
                                x.Name == deviceMappingDestinationDevice.DeviceName &&
                                x.Driver.Name() == deviceMappingDestinationDevice.DriverName);

                            if (alreadyBeingSyncedTo.Contains(tmp) == false)
                            {
                                if (tmp != null)
                                {
                                    dm.Dest.Add(tmp);

                                    alreadyBeingSyncedTo.Add(tmp);
                                }
                            }
                        }

                        MappedDevices.Add(dm);
                    }
                }
            }
        }

        private void ConfigUpdate(object state)
        {
            ServiceManager.Instance.ConfigService.CheckSettingStale();
            foreach (ISimpleLed slsManagerDriver in ServiceManager.Instance.SLSManager.Drivers.ToList().Where(x => x is ISimpleLedWithConfig).ToList())
            {
                ISimpleLedWithConfig cfgable = slsManagerDriver as ISimpleLedWithConfig;
                if (cfgable.GetIsDirty())
                {
                    ServiceManager.Instance.SLSManager.SaveConfig(cfgable);
                }
            }
        }

        public DeviceMappingModels.Settings Settings = new DeviceMappingModels.Settings
        {
            BackgroundOpacity = 0.5f
        };

        public LauncherPrefs LauncherPrefs { get; set; } = new LauncherPrefs();

        public void LoadSettings()
        {
            if (File.Exists("Settings.json"))
            {
                string json = File.ReadAllText("Settings.json");
                //try
                {
                    Settings = JsonConvert.DeserializeObject<DeviceMappingModels.Settings>(json);
                    if (Settings == null)
                    {
                        Settings = new DeviceMappingModels.Settings();
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
                Settings = new DeviceMappingModels.Settings();
                SaveSettings();
                HotLoadSettings();
            }
        }

        public DateTime TimeSettingsLastSave = DateTime.MinValue;

        public void SaveSettings()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Settings);
                File.WriteAllText("Settings.json", json);
                TimeSettingsLastSave = DateTime.Now;
                Settings.AreSettingsStale = false;
            }
            catch
            {
            }
        }


        public bool SettingsRequiresSave()
        {
            if (ServiceManager.Instance.ConfigService.Settings != null)
            {
                if (ServiceManager.Instance.ConfigService.Settings.AreSettingsStale) return true;
                if (ServiceManager.Instance.ConfigService.Settings.DeviceSettings != null)
                {
                    if (ServiceManager.Instance.ConfigService.Settings.DeviceSettings.Any(x => x.AreDeviceSettingsStale)) return true;
                }
            }

            return false;
        }


        public void CheckSettingStale()
        {
            if ((DateTime.Now - TimeSettingsLastSave).TotalSeconds > 5)
            {
                if (SettingsRequiresSave())
                {
                    //HotLoadSettings();

                    //SaveSettings();

                }

                if (ServiceManager.Instance.ProfileService.ProfilesRequiresSave())
                {
                    ServiceManager.Instance.ProfileService.SaveCurrentProfile();
                }

                if (ServiceManager.Instance.LedService.OverridesDirty)
                {
                    ServiceManager.Instance.LedService.OverridesDirty = false;
                    try
                    {
                        string json = JsonConvert.SerializeObject(ServiceManager.Instance.LedService.DeviceOverrides.ToList());
                        File.WriteAllText("Overrides.json", json);
                    }
                    catch
                    {
                    }

                }
            }


        }

        public void HotLoadSettings()
        {
            if (isHotLoading) return;

            isHotLoading = true;

            ServiceManager.Instance.LedService.LoadOverrides();

            ServiceManager.Instance.ProfileService.profilePathMapping.Clear();
            if (!Directory.Exists(ProfileS_DIRECTORY))
            {
                Directory.CreateDirectory(ProfileS_DIRECTORY);
                ServiceManager.Instance.ProfileService.GenerateNewProfile("Default");
                isHotLoading = false;
                return;
            }

            string[] profiles = Directory.GetFiles(ProfileS_DIRECTORY, "*.rsprofile");

            if (profiles == null || profiles.Length == 0)
            {
                ServiceManager.Instance.ProfileService.GenerateNewProfile("Default");
            }

            Settings.ProfileNames = new ObservableCollection<string>();

            foreach (string profile in profiles)
            {
                string profileName = ServiceManager.Instance.ProfileService.GetProfileFromPath(profile)?.Name;
                if (!string.IsNullOrWhiteSpace(profileName))
                {
                    try
                    {

                        ServiceManager.Instance.ProfileService.profilePathMapping.Add(profileName, profile);

                        if (Settings.ProfileNames == null)
                        {
                            Settings.ProfileNames = new ObservableCollection<string>();
                        }
                        Settings.ProfileNames.Add(profileName);
                        ServiceManager.Instance.ProfileService.OnProfilesChangedInvoke(this, new EventArgs());
                    }
                    catch
                    {
                    }
                }
            }

            if (Settings.ProfileNames == null)
            {
                Settings.ProfileNames = new ObservableCollection<string>();
            }

            if (Settings.ProfileNames.Contains(Settings.CurrentProfile))
            {
                ServiceManager.Instance.ProfileService.LoadProfileFromName(Settings.CurrentProfile);
            }
            else
            {
                Settings.CurrentProfile = "Default";
                ServiceManager.Instance.ProfileService.LoadProfileFromName(Settings.CurrentProfile);
            }


            double tmr2 = 1000.0 / MathHelper.Clamp(Settings.UpdateRate, 1, 100);

            ServiceManager.Instance.LedService.SetUpdateRate(tmr2);

            ServiceManager.Instance.ApiServerService.Stop();

            if (Settings.ApiEnabled)
            {
                ServiceManager.Instance.ApiServerService.Start();
            }

            if (Settings.DeviceSettings != null)
            {
                foreach (DeviceMappingModels.NGDeviceSettings SettingsDeviceSetting in Settings.DeviceSettings)
                {
                    ControlDevice cd = ServiceManager.Instance.LedService.GetControlDeviceFromName(SettingsDeviceSetting.ProviderName, SettingsDeviceSetting.Name);

                    if (cd != null)
                    {
                        cd.LedShift = SettingsDeviceSetting.LEDShift;
                        cd.Reverse = SettingsDeviceSetting.Reverse;

                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(Settings.SimpleLedAuthToken))
            {
                ServiceManager.Instance.SLSAuthService.SimpleLedAuth.AccessToken = Settings.SimpleLedAuthToken;
                ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserName = Settings.SimpleLedUserName;
                ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserId = Settings.SimpleLedUserId;
                try
                {
                    ServiceManager.Instance.SLSAuthService.SimpleLedAuth.Authenticate(() =>
                    {
                        Debug.WriteLine("Authenticated");
                        ServiceManager.Instance.SLSAuthService.SimpleLedAuthenticated = true;
                    });
                }
                catch
                {
                    ServiceManager.Instance.SLSAuthService.SimpleLedAuth.AccessToken = "";
                    ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserName = "";
                    ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserId = Guid.Empty;
                    ServiceManager.Instance.SLSAuthService.SimpleLedAuthenticated = false;

                    Settings.SimpleLedAuthToken = "";
                    Settings.SimpleLedUserId = Guid.Empty;
                    Settings.SimpleLedUserName = "";
                }
            }

            if (File.Exists("launcherPrefs.json"))
            {
                LauncherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText("launcherPrefs.json"));
            }
            else
            {
                LauncherPrefs = new LauncherPrefs();
            }

            isHotLoading = false;
        }

    }
}
