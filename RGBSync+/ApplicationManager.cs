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


        

        #region Constants
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public const string SLSPROVIDER_DIRECTORY = "SLSProvider";
        private const string NGPROFILES_DIRECTORY = "NGProfiles";
        private const string SLSCONFIGS_DIRECTORY = "SLSConfigs";
        #endregion

        #region Properties & Fields



        
        public static ApplicationManager Instance { get; } = new ApplicationManager();

        public MainWindow ConfigurationWindow;

        #endregion

        #region Commands

        private ActionCommand _openConfiguration;
        public ActionCommand OpenConfigurationCommand => _openConfiguration ?? (_openConfiguration = new ActionCommand(OpenConfiguration));

        private ActionCommand _hideConfiguration;
        public ActionCommand HideConfigurationCommand => _hideConfiguration ?? (_hideConfiguration = new ActionCommand(HideConfiguration));

        private ActionCommand _restartApp;
        public ActionCommand RestartAppCommand => _restartApp ?? (_restartApp = new ActionCommand(RestartApp));

        private ActionCommand _techSupport;
        public ActionCommand TechSupportCommand => _techSupport ?? (_techSupport = new ActionCommand(TechSupport));

        private ActionCommand _exitCommand;
        public ActionCommand ExitCommand => _exitCommand ?? (_exitCommand = new ActionCommand(Exit));

        public void FireLanguageChangedEvent()
        {
            LanguageChangedEvent?.Invoke(this, new EventArgs());
        }

        public event EventHandler LanguageChangedEvent;

        #endregion

        #region Constructors

        private ApplicationManager()
        {
            try
            {
                if (!Directory.Exists(NGPROFILES_DIRECTORY))
                {
                    Directory.CreateDirectory(NGPROFILES_DIRECTORY);
                    ServiceManager.Instance.ProfileService.GenerateNewProfile("Default", false);
                    ServiceManager.Instance.ConfigService.isHotLoading = false;
                    return;
                }
            }
            catch
            {
            }

            //AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            //{
            //    string dllName = args.Name.Split(',').First() + ".dll";

            //    foreach (string basePath in BasePaths)
            //    {
            //        if (File.Exists(basePath + "\\" + dllName))
            //        {
            //            return Assembly.Load(File.ReadAllBytes(basePath + "\\" + dllName));
            //        }
            //    }

            //    Debug.WriteLine("Failed to Load: "+dllName);
            //    return null;

            //};
        }

        internal void ShowModal(ModalModel modalModel)
        {
            MainWindowViewModel vm = ((MainWindowViewModel)ConfigurationWindow.DataContext);

            vm.ModalText = modalModel.ModalText;
            vm.ModalShowPercentage = false;
            vm.ShowModalCloseButton = true;
            vm.ShowModal = true;
        }

        internal void ShowSimpleModal(string text)
        {
            MainWindowViewModel vm = ((MainWindowViewModel)ConfigurationWindow.DataContext);

            vm.ModalText = text;
            vm.ModalShowPercentage = false;
            vm.ShowModalCloseButton = true;
            vm.ShowModal = true;
        }

        #endregion

        #region Methods

        public void NavigateToTab(string tab)
        {
            if (ConfigurationWindow != null)
            {
                ConfigurationWindow.SetTab(tab);
            }
        }

        public List<ColorProfile> GetColorProfiles()
        {
            if (!Directory.Exists("ColorProfiles"))
            {
                Directory.CreateDirectory("ColorProfiles");
            }

            var dir = Directory.GetFiles("ColorProfiles");
            var result = dir.Select(s => JsonConvert.DeserializeObject<ColorProfile>(File.ReadAllText(s))).ToList();
            if (result.Count == 0)
            {
                result = new List<ColorProfile>
                {
                    new ColorProfile
                    {
                        Id = Guid.Empty,
                        ProfileName = "Default",
                        ColorBanks = new ObservableCollection<ColorBank>()
                        {
                            new ColorBank()
                            {
                                BankName = "Basics",
                                Colors = new ObservableCollection<ColorObject>
                                {
                                    new ColorObject {Color = new ColorModel(255,0,0)},
                                    new ColorObject {Color = new ColorModel(255,255,0)},
                                    new ColorObject {Color = new ColorModel(0,255,0)},
                                    new ColorObject {Color = new ColorModel(0,0,255)},
                                    new ColorObject {Color = new ColorModel(255,255,255)},
                                }
                            },
                            new ColorBank()
                            {
                                BankName = "Rainbow",
                                Colors = new ObservableCollection<ColorObject>
                                {
                                    new ColorObject {Color = new ColorModel(255,0,0)},
                                    new ColorObject {Color = new ColorModel(255,153,0)},
                                    new ColorObject {Color = new ColorModel(204,255,0)},
                                    new ColorObject {Color = new ColorModel(51, 255, 0)},
                                    new ColorObject {Color = new ColorModel(0, 255, 102)},
                                    new ColorObject {Color = new ColorModel(0, 255, 255)},
                                    new ColorObject {Color = new ColorModel(0, 102, 255)},
                                    new ColorObject {Color = new ColorModel(51,0,255)},
                                    new ColorObject {Color = new ColorModel(204, 0, 255)},
                                    new ColorObject {Color = new ColorModel(255, 0, 153)},
                                }
                            },
                            new ColorBank
                            {
                                BankName = "Swatch 3",
                                Colors = new ObservableCollection<ColorObject>
                                {
                                    new ColorObject {ColorString = "#0000ff"}, new ColorObject {ColorString = "#000000"}
                                }
                            },
                            new ColorBank
                            {
                                BankName = "Swatch 4",
                                Colors = new ObservableCollection<ColorObject>
                                {
                                    new ColorObject {ColorString = "#ff00ff"}, new ColorObject {ColorString = "#000000"}
                                }
                            }
                        }
                    }
                };
            }

            return result;
        }
        
        public SplashLoader LoadingSplash;
        public void Initialize()
        {
            LoadingSplash = new SplashLoader();
            LoadingSplash.Show();

            LoadingSplash.LoadingText.Text = "Initializing";

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            LoadingSplash.Activate();
            ServiceManager.Initialize(SLSCONFIGS_DIRECTORY, NGPROFILES_DIRECTORY);



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



            //foreach (var controlDevice in SLSDevices)
            //{
            //    if (controlDevice.Driver is ISimpleLedWithConfig slsConfig)
            //    {
            //        if (slsConfig.GetIsDirty())
            //        {
            //            SLSManager.SaveConfig(slsConfig);
            //        }
            //    }
            //}
        }
        
        private void SLSUpdateLoop()
        {

        }
       




        public static List<string> BasePaths = new List<string>();

        public bool ExecuteWithTimeout(Action action, int timeoutMs = 5000)
        {

            return ExecuteAsyncWithTimeout(() => InvokeIfNecessary(action)).ConfigureAwait(false).GetAwaiter().GetResult();

        }

        public static void InvokeIfNecessary(Action action)
        {
            if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
                action();
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }

        public bool RunCodeWithTimeout(Action action, int timeoutMs = 5000)
        {
            var task = Task.Run(() => action);
            if (task.Wait(TimeSpan.FromMilliseconds(timeoutMs)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Task<bool> ExecuteAsyncWithTimeout(Action action, int timeoutMs = 5000)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            var ct = new CancellationTokenSource(timeoutMs);
            ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

            Task.Run(action, ct.Token);

            return tcs.Task;
        }

        private void HideConfiguration()
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


        private void TechSupport() => System.Diagnostics.Process.Start("https://rgbsync.com/discord");

        public void Exit()
        {
            ServiceManager.Instance.Logger.Debug("============ App is Shutting Down ============");
            try
            {
                ServiceManager.Instance.DiscordService.Stop();

            }
            catch { /* well, we're shuting down anyway ... */ }

            Application.Current.Shutdown();
        }

        #endregion


    }
}
