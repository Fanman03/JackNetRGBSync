using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Threading;
using RGB.NET.Core;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;
using RGBSyncPlus.UI;
using DiscordRPC;
using System.Globalization;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Security.Principal;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Windows.Controls;
using System.Windows.Threading;
using RGBSyncPlus.UI.Tabs;
using SimpleLed;

using Swashbuckle.Application;

namespace RGBSyncPlus
{
    public class ApplicationManager
    {
        #region Constants
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        private const string DEVICEPROVIDER_DIRECTORY = "DeviceProvider";
        public const string SLSPROVIDER_DIRECTORY = "SLSProvider";
        private const string NGPROFILES_DIRECTORY = "NGProfiles";
        private const string SLSCONFIGS_DIRECTORY = "SLSConfigs";
        #endregion

        #region Properties & Fields
        public DeviceMappingModels.NGSettings NGSettings = new DeviceMappingModels.NGSettings();
        public bool PauseSyncing { get; set; } = false;
        public static ApplicationManager Instance { get; } = new ApplicationManager();

        public MainWindow ConfigurationWindow;

        public Settings Settings { get; set; } = new Settings();

      //  public AppSettings AppSettings { get; set; } = new AppSettings();
        public TimerUpdateTrigger UpdateTrigger { get; private set; } = new TimerUpdateTrigger();

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


        #endregion

        #region Constructors

        private ApplicationManager()
        {
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
            throw new NotImplementedException();
        }

        public DiscordRpcClient client;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public SLSManager SLSManager;
        #endregion

        #region Methods

        private Dictionary<string, string> profilePathMapping = new Dictionary<string, string>();
        public void LoadNGSettings()
        {
            if (File.Exists("NGSettings.json"))
            {
                string json = File.ReadAllText("NGSettings.json");
                //try
                {
                    NGSettings = JsonConvert.DeserializeObject<DeviceMappingModels.NGSettings>(json);
                    if (NGSettings == null)
                    {
                        NGSettings = new DeviceMappingModels.NGSettings();
                    }
                    Logger.Info("Settings loaded");
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
            string json = JsonConvert.SerializeObject(NGSettings);
            File.WriteAllText("NGSettings.json", json);
            TimeSettingsLastSave = DateTime.Now;
            NGSettings.AreSettingsStale = false;
        }

        public void SaveCurrentNGProfile()
        {
            if (CurrentProfile.Id == Guid.Empty)
            {
                CurrentProfile.Id = Guid.NewGuid();
            }
            var id = CurrentProfile.Id;
            
            string json = JsonConvert.SerializeObject(CurrentProfile);
            string path;
            if (profilePathMapping.ContainsKey(CurrentProfile.Name))
            {
                path = profilePathMapping[CurrentProfile.Name];
            }
            else
            {
                path = NGPROFILES_DIRECTORY+"\\"+ id + ".rsprofile";
                profilePathMapping.Add(CurrentProfile.Name,path);
            }

            TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }


        public void SaveNGProfile(DeviceMappingModels.NGProfile profile)
        {
            string json = JsonConvert.SerializeObject(profile);
            string path = profilePathMapping[profile.Name];
            TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

        public void SaveNGProfile(DeviceMappingModels.NGProfile profile, string path)
        {
            string json = JsonConvert.SerializeObject(profile);
            TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

        public static bool isHotLoading = false;
        public void HotLoadNGSettings()
        {
            if (isHotLoading) return;

            isHotLoading = true;
            profilePathMapping.Clear();
            if (!Directory.Exists(NGPROFILES_DIRECTORY))
            {
                Directory.CreateDirectory(NGPROFILES_DIRECTORY);
                GenerateNewProfile("Default");
                isHotLoading = false;
                return;
            }

            var profiles = Directory.GetFiles(NGPROFILES_DIRECTORY, "*.rsprofile");

            if (profiles == null || profiles.Length == 0)
            {
                GenerateNewProfile("Default");
            }

            NGSettings.ProfileNames = new ObservableCollection<string>();
            foreach (var profile in profiles)
            {
                string profileName = GetProfileFromPath(profile)?.Name;
                if (!string.IsNullOrWhiteSpace(profileName))
                {
                    profilePathMapping.Add(profileName, profile);
                    NGSettings.ProfileNames.Add(profileName);
                }
            }

            if (NGSettings.ProfileNames.Contains(NGSettings.CurrentProfile))
            {
                LoadProfileFromName(NGSettings.CurrentProfile);
            }
            else
            {
                NGSettings.CurrentProfile = "Default";
                LoadProfileFromName(NGSettings.CurrentProfile);
            }

            if (UpdateTrigger != null)
            {
                UpdateTrigger.Stop();
                UpdateTrigger.Dispose();
            }

            var tmr = 1.0 / MathHelper.Clamp(NGSettings.UpdateRate, 1, 100);
            var tmr2 = 1000.0 / MathHelper.Clamp(NGSettings.UpdateRate, 1, 100);
            UpdateTrigger = new TimerUpdateTrigger { UpdateFrequency = tmr };

          //  loadingSplash.LoadingText.Text = "Registering Update Trigger";

            UpdateTrigger.Start();

            slsTimer = new Timer(SLSUpdate, null, 0, (int)tmr2);

            server?.CloseAsync().Wait();

            if (NGSettings.ApiEnabled)
            {
                StartApiServer();
            }

            if (NGSettings.DeviceSettings != null)
            {
                foreach (var ngSettingsDeviceSetting in NGSettings.DeviceSettings)
                {
                    ControlDevice cd = GetControlDeviceFromName(ngSettingsDeviceSetting.ProviderName,
                        ngSettingsDeviceSetting.Name);

                    if (cd != null)
                    {
                        cd.LedShift = ngSettingsDeviceSetting.LEDShift;
                        cd.Reverse = ngSettingsDeviceSetting.Reverse;

                    }
                }
            }

            isHotLoading = false;
        }

        public bool SettingsRequiresSave()
        {
            if (NGSettings != null)
            {
                if (NGSettings.AreSettingsStale) return true;
                if (NGSettings.DeviceSettings != null)
                {
                    if (NGSettings.DeviceSettings.Any(x => x.AreDeviceSettingsStale)) return true;
                }
            }

            return false;
        }

        public bool ProfilesRequiresSave()
        {
            if (CurrentProfile == null) return false;
            return (CurrentProfile.IsProfileStale);
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

                if (ProfilesRequiresSave())
                {
                    SaveCurrentNGProfile();
                }
            }
        }

        public void GenerateNewProfile(string name)
        {
            if (NGSettings.ProfileNames != null && NGSettings.ProfileNames.Any(x => x.ToLower() == name.ToLower()))
            {
                throw new ArgumentException("Profile name already exists");
            }

            if (!Directory.Exists(NGPROFILES_DIRECTORY))
            {
                Directory.CreateDirectory(NGPROFILES_DIRECTORY);
            }

            DeviceMappingModels.NGProfile newProfile = new DeviceMappingModels.NGProfile();
            newProfile.Name = name;

            Guid idGuid = Guid.NewGuid();
            newProfile.Id = idGuid;
            string filename = idGuid.ToString() + ".rsprofile";
            string fullPath = NGPROFILES_DIRECTORY + "\\" + filename;
            string json = JsonConvert.SerializeObject(newProfile);
            File.WriteAllText(fullPath, json);

            //profilePathMapping.Add(name,fullPath);
            NGSettings.CurrentProfile = name;
            CurrentProfile = newProfile;
            profilePathMapping.Clear();
            HotLoadNGSettings();

        }

        public ControlDevice GetControlDeviceFromName(string providerName, string name)
        {
            return SLSDevices.FirstOrDefault(x => x.Name == name && x.Driver.Name() == providerName);
        }

        public void StartApiServer()
        {
            //setup API
            //todo make this be able to be toggled:
            Debug.WriteLine("Setting up API");
            var apiconfig = new HttpSelfHostConfiguration("http://localhost:59022");
            

            apiconfig.Routes.MapHttpRoute("API Default", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            server = new HttpSelfHostServer(apiconfig);
            apiconfig.EnableSwagger(c => c.SingleApiVersion("v1", "RGBSync API")).EnableSwaggerUi();
            //server.OpenAsync();

            Task.Run(() => server.OpenAsync());
            Debug.WriteLine("API Running");
        }

        public void LoadProfileFromName(string profileName)
        {
            if (profilePathMapping.ContainsKey(profileName))
            {
                var map = profilePathMapping[profileName];
                CurrentProfile = GetProfileFromPath(map);

                if (CurrentProfile.Id == Guid.Empty)
                {
                    string gid = map.Split('\\').Last().Split('.').First();
                    CurrentProfile.Id = Guid.Parse(gid);
                }

                NGSettings.CurrentProfile = profileName;
            }
        }

        public DeviceMappingModels.NGProfile GetProfileFromName(string profileName)
        {
            if (profilePathMapping.ContainsKey(profileName))
            {
                var map = profilePathMapping[profileName];
                var result= GetProfileFromPath(map);

                if (result.Id == Guid.Empty)
                {
                    string gid = map.Split('\\').Last().Split('.').First();
                    result.Id = Guid.Parse(gid);
                }

                return result;
            }

            return null;
        }

        public DeviceMappingModels.NGProfile GetProfileFromPath(string path)
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<DeviceMappingModels.NGProfile>(json);
        }


        public DeviceMappingModels.NGProfile CurrentProfile;
        private SplashLoader loadingSplash;
        public void Initialize()
        {
            loadingSplash = new SplashLoader();
            loadingSplash.Show();

            loadingSplash.LoadingText.Text = "Initializing";

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            if (!Directory.Exists(SLSCONFIGS_DIRECTORY))
            {
                Directory.CreateDirectory(SLSCONFIGS_DIRECTORY);
            }

            loadingSplash.Activate();
            SLSManager = new SLSManager(SLSCONFIGS_DIRECTORY);

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "rgbsync.log" };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            Logger.Debug("============ JackNet RGB Sync is Starting ============");

            //if (AppSettings.RunAsAdmin == true && !Debugger.IsAttached)
            //{
            //    Logger.Debug("App should be run as administrator.");
            //    Logger.Debug("Checking to see if app is running as administrator...");
            //    var identity = WindowsIdentity.GetCurrent();
            //    var principal = new WindowsPrincipal(identity);
            //    bool isRunningAsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            //    if (isRunningAsAdmin == false)
            //    {
            //        Logger.Debug("App is not running as administrator, restarting...");
            //        ExecuteAsAdmin("RGBSync+.exe");
            //        Exit();
            //    }
            //    else
            //    {
            //        Logger.Debug("App is running as administrator, proceding with startup.");
            //    }
            //}

            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (NGSettings.Lang == null)
            {
                Logger.Debug("Language is not set, inferring language from system culture. Lang=" + ci.TwoLetterISOLanguageName);
                NGSettings.Lang = ci.TwoLetterISOLanguageName;
            }
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(NGSettings.Lang);

            loadingSplash.LoadingText.Text = "Starting Discord";
            client = new DiscordRpcClient("581567509959016456");
            client.Initialize();

            string tempSetup = Directory.GetCurrentDirectory() + "\\~TEMP_setup.exe";
            if (File.Exists(tempSetup))
            {
                Logger.Debug("Found old installer, removing...");
                try
                {
                    File.Delete(tempSetup);
                    Logger.Debug("Old installer successfully removed.");
                }
                catch (Exception ex)
                {
                    Logger.Error("Error deleting file: " + ex.ToString());
                }
            }

           // int delay = AppSettings.StartDelay * 1000;

            LoadSLSProviders();
            //LoadDeviceProviders();
            //surface.AlignDevices();

            loadingSplash.LoadingText.Text = "Mapping from config";
            SetUpMappedDevicesFromConfig();

            configTimer = new Timer(ConfigUpdate, null, 0, (int)5000);

            loadingSplash.LoadingText.Text = "Loading Settings";
            LoadNGSettings();
            loadingSplash.LoadingText.Text = "All done";

            DispatcherTimer closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromSeconds(2);
            closeTimer.Tick += (sender, args) =>
            {
                loadingSplash.Close();
                closeTimer.Stop();
            };

            closeTimer.Start();

            profileTriggerTimer = new DispatcherTimer();
            profileTriggerTimer.Interval = TimeSpan.FromSeconds(1);
            profileTriggerTimer.Tick += (sender, args) => ProfileTriggerManager.CheckTriggers();
            profileTriggerTimer.Start();
        }


        public ProfileTriggerManager ProfileTriggerManager = new ProfileTriggerManager();

        public HttpSelfHostServer server;
        public DispatcherTimer profileTriggerTimer;
        public Timer slsTimer;
        public Timer configTimer;
        public void SetUpMappedDevicesFromConfig()
        {
            List<ControlDevice> alreadyBeingSyncedTo = new List<ControlDevice>();
            MappedDevices = new List<DeviceMappingModels.DeviceMap>();
            if (Settings.DeviceMappingProxy != null)
            {
                foreach (var deviceMapping in Settings.DeviceMappingProxy)
                {
                    var src = SLSDevices.FirstOrDefault(x =>
                        x.Name == deviceMapping.SourceDevice.DeviceName &&
                        x.Driver.Name() == deviceMapping.SourceDevice.DriverName);
                    if (src != null)
                    {
                        DeviceMappingModels.DeviceMap dm = new DeviceMappingModels.DeviceMap
                        {
                            Source = src,
                            Dest = new List<ControlDevice>()
                        };

                        foreach (var deviceMappingDestinationDevice in deviceMapping.DestinationDevices)
                        {
                            ControlDevice tmp = SLSDevices.FirstOrDefault(x =>
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

        private ControlDevice virtualAlignmentDevice = new ControlDevice
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
            CheckSettingStale();
            foreach (ISimpleLed slsManagerDriver in SLSManager.Drivers.ToList().Where(x => x is ISimpleLedWithConfig).ToList())
            {
                ISimpleLedWithConfig cfgable = slsManagerDriver as ISimpleLedWithConfig;
                if (cfgable.GetIsDirty())
                {
                    SLSManager.SaveConfig(cfgable);
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
        public class PushListItem
        {
            public ISimpleLed Driver { get; set; }
            public ControlDevice Device { get; set; }
            public string Key { get; set; }

        }
        private void SLSUpdate(object state)
        {
            try
            {

                if (PauseSyncing)
                {
                    return;
                }

                if (CurrentProfile == null || CurrentProfile.DeviceProfileSettings == null)
                {
                    return;
                }

                List<ControlDevice> devicesToPull = new List<ControlDevice>();

                foreach (var currentProfileDeviceProfileSetting in CurrentProfile.DeviceProfileSettings.ToList())
                {
                    ControlDevice cd = SLSDevices.FirstOrDefault(x =>
                        x.Name == currentProfileDeviceProfileSetting.SourceName &&
                        x.Driver.Name() == currentProfileDeviceProfileSetting.SourceProviderName &&
                        x.ConnectedTo == currentProfileDeviceProfileSetting.SourceConnectedTo);

                    if (cd != null)
                    {
                        if (!devicesToPull.Contains(cd))
                        {
                            devicesToPull.Add(cd);
                        }
                    }
                }

                foreach (var controlDevice in devicesToPull)
                {
                    if (controlDevice.Driver.GetProperties().SupportsPull)
                    {
                        controlDevice.Pull();
                    }
                }

                List<PushListItem> pushMe = new List<PushListItem>();
                foreach (var currentProfileDeviceProfileSetting in CurrentProfile.DeviceProfileSettings.ToList())
                {
                    ControlDevice cd = SLSDevices.FirstOrDefault(x =>
                        x.Name == currentProfileDeviceProfileSetting.SourceName &&
                        x.Driver.Name() == currentProfileDeviceProfileSetting.SourceProviderName &&
                        x.ConnectedTo == currentProfileDeviceProfileSetting.SourceConnectedTo);

                    ControlDevice dest = SLSDevices.FirstOrDefault(x =>
                        x.Name == currentProfileDeviceProfileSetting.Name &&
                        x.Driver.Name() == currentProfileDeviceProfileSetting.ProviderName &&
                        x.ConnectedTo == currentProfileDeviceProfileSetting.ConnectedTo);

                    if (cd != null && dest != null)
                    {
                        string key = currentProfileDeviceProfileSetting.SourceName +
                                     currentProfileDeviceProfileSetting.SourceProviderName +
                                     currentProfileDeviceProfileSetting.SourceConnectedTo +
                                     currentProfileDeviceProfileSetting.Name +
                                     currentProfileDeviceProfileSetting.ProviderName +
                                     currentProfileDeviceProfileSetting.ConnectedTo;

                        if (!isMapping.ContainsKey(key) || isMapping[key] == false)
                        {
                            if (!isMapping.ContainsKey(key))
                            {
                                isMapping.Add(key, true);
                            }

                            try
                            {
                                dest.MapLEDs(cd);
                                pushMe.Add(new PushListItem
                                {
                                    Device = dest,
                                    Driver = dest.Driver,
                                    Key = key
                                });
                            }
                            catch
                            {
                            }

                            isMapping[key] = true;
                        }


                    }

                }

                foreach (IGrouping<ISimpleLed, PushListItem> gp in pushMe.GroupBy(x => x.Driver))
                {
                    Task.Run(async () =>
                   {
                       foreach (PushListItem t in gp)
                       {
                           try
                           {
                               gp.Key.Push(t.Device);
                           }
                           catch { }
                       }
                       foreach (PushListItem t in gp)
                       {
                           try
                           {
                               isMapping[t.Key] = false;
                           }
                           catch { }
                       }
                   }
                   );
                }
            }
            catch (Exception ex)
            {
                // Process unhandled exception
                var crashWindow = new CrashWindow();
                crashWindow.errorName.Text = ex.GetType().ToString();
                crashWindow.message.Text = ex.Message;

                crashWindow.stackTrace.Text = ex.StackTrace;
                crashWindow.Show();

            }
        }

        public Dictionary<string, bool> isMapping = new Dictionary<string, bool>();

        public void UnloadSLSProviders()
        {
            SLSManager.Drivers.ToList().ForEach(x =>
            {
                try
                {
                    x.Dispose();
                }
                catch
                {
                }
            });

            try
            {
                if (SLSManager.Drivers == null)
                {
                    SLSManager.Drivers = new ObservableCollection<ISimpleLed>();
                }

                SLSManager.Drivers.Clear();
            }
            catch
            {
            }

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }

        public void LoadChildAssemblies(Assembly assembly, string basePath)
        {

            var names = assembly.GetReferencedAssemblies();

            foreach (AssemblyName assemblyName in names)
            {
                try
                {
                    if (File.Exists(basePath + "\\" + assemblyName.Name + ".dll"))
                    {
                        var temp = Assembly.Load(File.ReadAllBytes(basePath + "\\" + assemblyName.Name + ".dll"));
                        LoadChildAssemblies(temp, basePath);
                    }
                    else
                    {
                        Assembly.Load(assemblyName);
                    }

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }
        internal ISimpleLed LoadDll(string basePath, string dllFileName)
        {
            ISimpleLed result = null;

            ResolveEventHandler delly = (sender, args) => CurrentDomainOnAssemblyResolve(sender, args, basePath);

            AppDomain.CurrentDomain.AssemblyResolve += delly;

            Assembly assembly = Assembly.Load(File.ReadAllBytes(basePath + "\\" + dllFileName));
            //Assembly assembly = Assembly.LoadFrom(file);
            var typeroo = assembly.GetTypes();
            var pat2 = typeroo.Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass).ToList();

            List<Type> pat3 = pat2.Where(t => typeof(ISimpleLed).IsAssignableFrom(t)).ToList();

            foreach (Type loaderType in pat3)
            {
                if (Activator.CreateInstance(loaderType) is ISimpleLed slsDriver)
                {
                    if (slsDriver is ISimpleLedWithConfig slsWithConfig)
                    {
                        UserControl temp = slsWithConfig.GetCustomConfig(null);

                    }


                    LoadChildAssemblies(assembly, basePath);

                    result = slsDriver;
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= delly;

            return result;
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args, string basePath)
        {
            string assemblyName = new AssemblyName(args.Name).Name;

            string dllName = assemblyName + ".dll";
            string dllFullPath = Path.Combine(basePath, dllName);

            if (File.Exists(dllFullPath))
            {
                return Assembly.Load(File.ReadAllBytes(dllFullPath));
            }

            return null;
        }

        public static List<string> BasePaths = new List<string>();


        public void LoadSLSProviders()
        {
            UnloadSLSProviders();

            string deviceProvierDir =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty,
                        SLSPROVIDER_DIRECTORY);

            if (!Directory.Exists(deviceProvierDir)) return;
            var pluginFolders = Directory.GetDirectories(deviceProvierDir);
            loadingSplash.LoadingText.Text = "Loading SLS plugins";
            loadingSplash.ProgressBar.Maximum = pluginFolders.Length;

            int ct = 0;
            foreach (var pluginFolder in pluginFolders)
            {
                loadingSplash.Activate();
                ct++;
                
                loadingSplash.ProgressBar.Value = ct;
                loadingSplash.ProgressBar.Refresh();
                LoadPlungFolder(pluginFolder);


            }

            SLSManager.RescanRequired += Rescan;

            loadingSplash.LoadingText.Text = "Updating SLS devices";
            UpdateSLSDevices();
        }

        public void LoadPlungFolder(string pluginFolder)
        {

            var files = Directory.GetFiles(pluginFolder, "*.dll");

            
            List<Guid> driversAdded = new List<Guid>();
            foreach (string file in files)
            {
                Debug.WriteLine("Checking " + file);
                string filename = file.Split('\\').Last();
                string justPath = file.Substring(0, file.Length - filename.Length);
                if (filename.ToLower().StartsWith("driver") || filename.ToLower().StartsWith("source") || filename.ToLower().StartsWith("gameintegration"))
                {
                    try
                    {
                        loadingSplash.LoadingText.Text = "Loading " + file.Split('\\').Last().Split('.').First();
                        Logger.Debug("Loading provider " + file);

                        var slsDriver = LoadDll(justPath, filename);

                        if (slsDriver != null)
                        {
                            try
                            {
                                if (!driversAdded.Contains(slsDriver.GetProperties().Id))
                                {
                                    //slsDriver.Configure(null);
                                    Debug.WriteLine("We got one! " + "Loading " + slsDriver.Name());
                                    loadingSplash.LoadingText.Text = "Loading " + slsDriver.Name();
                                    loadingSplash.UpdateLayout();
                                    loadingSplash.Refresh();
                                    loadingSplash.LoadingText.Refresh();
                                    Task.Delay(33).Wait();
                                    SLSManager.Drivers.Add(slsDriver);
                                    driversAdded.Add(slsDriver.GetProperties().Id);
                                    Debug.WriteLine("all loaded: " + slsDriver.Name());
                                    slsDriver.DeviceAdded += SlsDriver_DeviceAdded;
                                    slsDriver.DeviceRemoved += SlsDriver_DeviceRemoved;
                                    slsDriver.Configure(new DriverDetails() { HomeFolder = justPath });
                                    Debug.WriteLine("Have Initialized: " + slsDriver.Name());
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        private void SlsDriver_DeviceRemoved(object sender, Events.DeviceChangeEventArgs e)
        {
            SLSDevices.Remove(e.ControlDevice);
        }

        private void SlsDriver_DeviceAdded(object sender, Events.DeviceChangeEventArgs e)
        {
            SLSDevices.Add(e.ControlDevice);
        }

        public void Rescan(object sender, EventArgs args)
        {
            StoreViewModel vm = null;
            if (ConfigurationWindow?.StoreUserControl != null)
            {
                vm = ConfigurationWindow.StoreUserControl.DataContext as StoreViewModel;
            }

            using (new SimpleModal((MainWindowViewModel)ConfigurationWindow?.DataContext, "Reloading Plugins"))
            {
                ConfigurationWindow.Refresh();

                ApplicationManager.Instance.UpdateSLSDevices();
                vm?.LoadStoreAndPlugins();

                if (ConfigurationWindow?.DevicesUserControl.Content != null)
                {
                    DevicesViewModel ducvm = ((Devices)ConfigurationWindow.DevicesUserControl.Content).DataContext as DevicesViewModel;
                    ducvm?.SetUpDeviceMapViewModel();

                }
            }
        }

        public ObservableCollection<ControlDevice> SLSDevices = new ObservableCollection<ControlDevice>();

        public void UpdateSLSDevices()
        {
            loadingSplash.LoadingText.Text = "Loading Configs";
            foreach (var drv in SLSManager.Drivers)
            {
                if (drv is ISimpleLedWithConfig cfgdrv)
                {
                    try
                    {
                        SLSManager.LoadConfig(cfgdrv);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }


            loadingSplash.LoadingText.Text = "Getting devices";
            //SLSDevices = SLSManager.GetDevices();
            loadingSplash.ProgressBar.Value = 0;
            loadingSplash.ProgressBar.Maximum = SLSManager.Drivers.Count;

            int ct = 0;
            List<ControlDevice> controlDevices = new List<ControlDevice>();
            foreach (var simpleLedDriver in SLSManager.Drivers)
            {
                ct++;
                loadingSplash.ProgressBar.Value = ct;
                try
                {
                    loadingSplash.LoadingText.Text = "Getting devices from " + simpleLedDriver.Name();
                    //var devices = simpleLedDriver.GetDevices();
                    //if (devices != null)
                    {
                      //  controlDevices.AddRange(devices);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }

   
        private void HideConfiguration()
        {
            if (NGSettings.EnableDiscordRPC == true)
            {
                if (client.IsDisposed == false)
                {
                    client.Dispose();
                }
            }
            if (NGSettings.MinimizeToTray)
            {
                if (ConfigurationWindow.IsVisible)
                    ConfigurationWindow.Hide();
            }
            else
                ConfigurationWindow.WindowState = WindowState.Minimized;
        }

        public void OpenConfiguration()
        {
            if (NGSettings.EnableDiscordRPC == true)
            {
                Logger.Info("Discord RPC enabled.");
                if (client.IsDisposed == true)
                {
                    Logger.Info("Discord RPC client disposed, initializing new one.");
                    client = new DiscordRpcClient("581567509959016456");
                    client.Initialize();
                }
                Logger.Info("Setting Discord presensce.");
                client.SetPresence(new RichPresence()
                {
                    State = "Profile: " + Settings.Name,
                    Details = "Syncing lighting effects",
                    Assets = new Assets()
                    {
                        LargeImageKey = "large_image",
                        LargeImageText = "RGB Sync",
                        SmallImageKey = "small_image",
                        SmallImageText = "by Fanman03"
                    }
                });
            }
            if (ConfigurationWindow == null) ConfigurationWindow = new MainWindow();
            if (!ConfigurationWindow.IsVisible)
                ConfigurationWindow.Show();

            if (ConfigurationWindow.WindowState == WindowState.Minimized)
                ConfigurationWindow.WindowState = WindowState.Normal;

            //TODO make better update check system
            /*            try
                        {
                            using (WebClient w = new WebClient())
                            {
                                Logger.Info("Checking for update...");
                                var json = w.DownloadString(ApplicationManager.Instance.AppSettings.versionURL);
                                ProgVersion versionFromApi = JsonConvert.DeserializeObject<ProgVersion>(json);
                                int versionMajor = Version.Major;
                                int versionMinor = Version.Minor;
                                int versionBuild = Version.Build;

                                if (versionFromApi.major > versionMajor)
                                {
                                    GetUpdateWindow getUpdateWindow = new GetUpdateWindow();
                                    getUpdateWindow.Show();
                                    Logger.Info("Update available. (major)");
                                }
                                else if (versionFromApi.minor > versionMinor)
                                {
                                    GetUpdateWindow getUpdateWindow = new GetUpdateWindow();
                                    getUpdateWindow.Show();
                                    Logger.Info("Update available. (minor)");
                                }
                                else if (versionFromApi.build > versionBuild)
                                {
                                    GetUpdateWindow getUpdateWindow = new GetUpdateWindow();
                                    Logger.Info("Update available. (build)");
                                    getUpdateWindow.Show();
                                }
                                else
                                {
                                    Logger.Info("No update available.");
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Unable to check for updates. Download failed with exception: " + ex);
                        }*/

        }

        public void RestartApp()
        {
            Logger.Debug("App is restarting.");
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            if (NGSettings.EnableDiscordRPC == true)
            {
                if (client.IsDisposed == false)
                {
                    client.Dispose();
                }
            }
            Application.Current.Shutdown();
        }

        public void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        private void TechSupport() => System.Diagnostics.Process.Start("https://rgbsync.com/discord");

        public void Exit()
        {
            Logger.Debug("============ App is Shutting Down ============");
            try { client.Dispose(); } catch { /* well, we're shuting down anyway ... */ }

            Application.Current.Shutdown();
        }

        #endregion

        public void DeleteProfile(string dcName)
        {
            string path = profilePathMapping[dcName];
            TimeSettingsLastSave = DateTime.Now;
            File.Delete(path);
            HotLoadNGSettings();
        }

        public void RenameProfile(string currentProfileOriginalName, string currentProfileName)
        {
            if (profilePathMapping.ContainsKey(currentProfileOriginalName))
            {
                var map = profilePathMapping[currentProfileOriginalName];
                var profile = GetProfileFromPath(map);

                profile.Name = currentProfileName;

                SaveNGProfile(profile, map);
                HotLoadNGSettings();
            }
        }
    }
}
