using DiscordRPC;
using DiscordRPC;
using Newtonsoft.Json;

using RGBSyncPlus.Configuration;
using RGBSyncPlus.Helper;
using RGBSyncPlus.Languages;
using RGBSyncPlus.Model;
using RGBSyncPlus.UI;
using RGBSyncPlus.UI.Tabs;
using SharedCode;
using SimpleLed;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using RGBSyncPlus.Services;


namespace RGBSyncPlus
{
    public class ApplicationManager
    {
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public const string SLSPROVIDER_DIRECTORY = "SLSProvider";
        private const string NGPROFILES_DIRECTORY = "NGProfiles";
        private const string SLSCONFIGS_DIRECTORY = "SLSConfigs";

        //public static ApplicationManager Instance { get; } = new ApplicationManager();

        public MainWindow ConfigurationWindow;
        
        public void FireLanguageChangedEvent()
        {
            LanguageChangedEvent?.Invoke(this, new EventArgs());
        }

        public event EventHandler LanguageChangedEvent;

        public ApplicationManager()
        {
            try
            {
                if (!Directory.Exists(NGPROFILES_DIRECTORY))
                {
                    Directory.CreateDirectory(NGPROFILES_DIRECTORY);
                    ServiceManager.Instance.ProfileService.GenerateNewProfile("Default", false);
                    ServiceManager.Instance.ConfigService.isHotLoading = false;
                }
            }
            catch
            {
            }
        }


        #region Methods

        public void NavigateToTab(string tab)
        {
            if (ConfigurationWindow != null)
            {
                ConfigurationWindow.SetTab(tab);
            }
        }

        
        public SplashLoader LoadingSplash;
        public void Initialize()
        {
            LoadingSplash = new SplashLoader();
            LoadingSplash.Show();

            LoadingSplash.LoadingText.Text = "Initializing";

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            LoadingSplash.Activate();
            

            ServiceManager.Instance.Logger.Debug("============ JackNet RGB Sync is Starting ============");

            List<LanguageModel> langs = LanguageManager.Languages;
            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (ServiceManager.Instance.ConfigService.NGSettings.Lang == null)
            {
                ServiceManager.Instance.Logger.Debug("Language is not set, inferring language from system culture. Lang=" + ci.TwoLetterISOLanguageName);
                ServiceManager.Instance.ConfigService.NGSettings.Lang = ci.TwoLetterISOLanguageName;
            }

            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(ServiceManager.Instance.ConfigService.NGSettings.Lang);

            LoadingSplash.LoadingText.Text = "Starting Discord";

            ServiceManager.Instance.LedService.LoadOverrides();
            ServiceManager.Instance.LedService.LoadSLSProviders();

            LoadingSplash.LoadingText.Text = "Mapping from config";
            SetUpMappedDevicesFromConfig();

            configTimer = new Timer(ConfigUpdate, null, 0, (int)5000);

            LoadingSplash.LoadingText.Text = "Loading Settings";
            ServiceManager.Instance.ConfigService.LoadNGSettings();
            LoadingSplash.LoadingText.Text = "All done";

            OpenConfiguration();

            DispatcherTimer closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromSeconds(1);
            closeTimer.Tick += (sender, args) =>
            {
                LoadingSplash.Close();
                closeTimer.Stop();
            };

            closeTimer.Start();

            profileTriggerTimer = new DispatcherTimer();
            profileTriggerTimer.Interval = TimeSpan.FromSeconds(1);
            profileTriggerTimer.Tick += (sender, args) => ProfileTriggerManager.CheckTriggers();
            profileTriggerTimer.Start();
        }


        public ProfileTriggerManager ProfileTriggerManager = new ProfileTriggerManager();


        public DispatcherTimer profileTriggerTimer;
        //public Timer slsTimer;
        public Timer configTimer;
        public void SetUpMappedDevicesFromConfig()
        {
            List<ControlDevice> alreadyBeingSyncedTo = new List<ControlDevice>();
            MappedDevices = new List<DeviceMappingModels.DeviceMap>();
            if (ServiceManager.Instance.ConfigService.Settings.DeviceMappingProxy != null)
            {
                foreach (DeviceMappingModels.DeviceMapping deviceMapping in ServiceManager.Instance.ConfigService.Settings.DeviceMappingProxy)
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

        public List<DeviceMappingModels.DeviceMap> MappedDevices = new List<DeviceMappingModels.DeviceMap>();

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

        public void HideConfiguration()
        {
            if (ServiceManager.Instance.ConfigService.NGSettings.EnableDiscordRPC == true)
            {
                ServiceManager.Instance.DiscordService.Stop();
            }
            if (ServiceManager.Instance.ConfigService.NGSettings.MinimizeToTray)
            {
                if (ConfigurationWindow.IsVisible)
                    ConfigurationWindow.Hide();
            }
            else
                ConfigurationWindow.WindowState = WindowState.Minimized;
        }

        public void OpenConfiguration()
        {
            if (ConfigurationWindow == null) ConfigurationWindow = new MainWindow();
            if (!ConfigurationWindow.IsVisible)
            {
                ConfigurationWindow.Show();
            }

            if (ConfigurationWindow.WindowState == WindowState.Minimized)
            {
                ConfigurationWindow.WindowState = WindowState.Normal;
            }
        }

        public void RestartApp()
        {
            ServiceManager.Instance.Logger.Debug("App is restarting.");
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            if (ServiceManager.Instance.ConfigService.NGSettings.EnableDiscordRPC == true)
            {
                ServiceManager.Instance.DiscordService.Stop();
            }
            Application.Current.Shutdown();
        }
        
        public void Exit()
        {
            ServiceManager.Instance.Logger.Debug("============ App is Shutting Down ============");
            try
            {
                ServiceManager.Instance.DiscordService.Stop();

            }
            catch
            {

            }

            Application.Current.Shutdown();
        }

        #endregion


    }
}
