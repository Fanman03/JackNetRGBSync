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
        public string NGPROFILES_DIRECTORY;
        public string SLSCONFIGS_DIRECTORY;
        public ConfigService(string ngprofiles_dir, string configs_dir)
        {
            NGPROFILES_DIRECTORY = ngprofiles_dir;
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

        public DeviceMappingModels.NGSettings NGSettings = new DeviceMappingModels.NGSettings
        {
            BackgroundOpacity = 0.5f
        };

        public LauncherPrefs LauncherPrefs { get; set; } = new LauncherPrefs();

        public void LoadNGSettings()
        {
            if (File.Exists("Settings.json"))
            {
                string json = File.ReadAllText("Settings.json");
                //try
                {
                    NGSettings = JsonConvert.DeserializeObject<DeviceMappingModels.NGSettings>(json);
                    if (NGSettings == null)
                    {
                        NGSettings = new DeviceMappingModels.NGSettings();
                    }
                    ServiceManager.Instance.Logger.Info("Settings loaded");
                    HotLoadNGSettings();
                }
                //catch
                //{
                //    Logger.Error("error loading settings");
                //}
            }
            else
            {
                NGSettings = new DeviceMappingModels.NGSettings();
                SaveNGSettings();
                HotLoadNGSettings();
            }
        }

        public DateTime TimeSettingsLastSave = DateTime.MinValue;

        public void SaveNGSettings()
        {
            try
            {
                string json = JsonConvert.SerializeObject(NGSettings);
                File.WriteAllText("Settings.json", json);
                TimeSettingsLastSave = DateTime.Now;
                NGSettings.AreSettingsStale = false;
            }
            catch
            {
            }
        }


        public bool SettingsRequiresSave()
        {
            if (ServiceManager.Instance.ConfigService.NGSettings != null)
            {
                if (ServiceManager.Instance.ConfigService.NGSettings.AreSettingsStale) return true;
                if (ServiceManager.Instance.ConfigService.NGSettings.DeviceSettings != null)
                {
                    if (ServiceManager.Instance.ConfigService.NGSettings.DeviceSettings.Any(x => x.AreDeviceSettingsStale)) return true;
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
                    HotLoadNGSettings();

                    SaveNGSettings();

                }

                if (ServiceManager.Instance.ProfileService.ProfilesRequiresSave())
                {
                    ServiceManager.Instance.ProfileService.SaveCurrentNGProfile();
                }

                if (ServiceManager.Instance.LedService.OverridesDirty)
                {
                    ServiceManager.Instance.LedService.OverridesDirty = false;
                    try
                    {
                        string json = JsonConvert.SerializeObject(ServiceManager.Instance.LedService.DeviceOverrides.ToList());
                        File.WriteAllText("NGOverrides.json", json);
                    }
                    catch
                    {
                    }

                }
            }


        }

        public void HotLoadNGSettings()
        {
            if (isHotLoading) return;

            isHotLoading = true;

            ServiceManager.Instance.LedService.LoadOverrides();

            ServiceManager.Instance.ProfileService.profilePathMapping.Clear();
            if (!Directory.Exists(NGPROFILES_DIRECTORY))
            {
                Directory.CreateDirectory(NGPROFILES_DIRECTORY);
                ServiceManager.Instance.ProfileService.GenerateNewProfile("Default");
                isHotLoading = false;
                return;
            }

            string[] profiles = Directory.GetFiles(NGPROFILES_DIRECTORY, "*.rsprofile");

            if (profiles == null || profiles.Length == 0)
            {
                ServiceManager.Instance.ProfileService.GenerateNewProfile("Default");
            }

            NGSettings.ProfileNames = new ObservableCollection<string>();

            foreach (string profile in profiles)
            {
                string profileName = ServiceManager.Instance.ProfileService.GetProfileFromPath(profile)?.Name;
                if (!string.IsNullOrWhiteSpace(profileName))
                {
                    try
                    {

                        ServiceManager.Instance.ProfileService.profilePathMapping.Add(profileName, profile);

                        if (NGSettings.ProfileNames == null)
                        {
                            NGSettings.ProfileNames = new ObservableCollection<string>();
                        }
                        NGSettings.ProfileNames.Add(profileName);
                        ServiceManager.Instance.ProfileService.OnProfilesChangedInvoke(this, new EventArgs());
                    }
                    catch
                    {
                    }
                }
            }

            if (NGSettings.ProfileNames == null)
            {
                NGSettings.ProfileNames = new ObservableCollection<string>();
            }

            if (NGSettings.ProfileNames.Contains(NGSettings.CurrentProfile))
            {
                ServiceManager.Instance.ProfileService.LoadProfileFromName(NGSettings.CurrentProfile);
            }
            else
            {
                NGSettings.CurrentProfile = "Default";
                ServiceManager.Instance.ProfileService.LoadProfileFromName(NGSettings.CurrentProfile);
            }


            double tmr2 = 1000.0 / MathHelper.Clamp(NGSettings.UpdateRate, 1, 100);

            ServiceManager.Instance.LedService.SetUpdateRate(tmr2);

            ServiceManager.Instance.ApiServerService.Stop();

            if (NGSettings.ApiEnabled)
            {
                ServiceManager.Instance.ApiServerService.Start();
            }

            if (NGSettings.DeviceSettings != null)
            {
                foreach (DeviceMappingModels.NGDeviceSettings ngSettingsDeviceSetting in NGSettings.DeviceSettings)
                {
                    ControlDevice cd = ServiceManager.Instance.LedService.GetControlDeviceFromName(ngSettingsDeviceSetting.ProviderName, ngSettingsDeviceSetting.Name);

                    if (cd != null)
                    {
                        cd.LedShift = ngSettingsDeviceSetting.LEDShift;
                        cd.Reverse = ngSettingsDeviceSetting.Reverse;

                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(NGSettings.SimpleLedAuthToken))
            {
                ServiceManager.Instance.SLSAuthService.SimpleLedAuth.AccessToken = NGSettings.SimpleLedAuthToken;
                ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserName = NGSettings.SimpleLedUserName;
                ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserId = NGSettings.SimpleLedUserId;
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

                    NGSettings.SimpleLedAuthToken = "";
                    NGSettings.SimpleLedUserId = Guid.Empty;
                    NGSettings.SimpleLedUserName = "";
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
