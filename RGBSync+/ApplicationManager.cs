using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Threading;
using RGB.NET.Core;
using RGB.NET.Groups;
using RGBSyncPlus.Brushes;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;
using RGBSyncPlus.UI;
using DiscordRPC;
using DiscordRPC.Message;
using System.Globalization;
using Newtonsoft.Json;
using System.Net;
using NLog;
using System.Diagnostics;
using System.Security.Principal;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Windows.Documents;
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

        public AppSettings AppSettings { get; set; } = new AppSettings();
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

        private ApplicationManager() { }

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
            string json = JsonConvert.SerializeObject(CurrentProfile);
            string path = profilePathMapping[CurrentProfile.Name];
            TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

        public void HotLoadNGSettings()
        {
            if (!Directory.Exists(NGPROFILES_DIRECTORY))
            {
                Directory.CreateDirectory(NGPROFILES_DIRECTORY);
                GenerateNewProfile("Default");
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

            string filename = Guid.NewGuid().ToString() + ".rsprofile";
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
            var apiconfig = new HttpSelfHostConfiguration("http://localhost:9022");

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
                CurrentProfile = GetProfileFromPath(profilePathMapping[profileName]);
                NGSettings.CurrentProfile = profileName;

                if (ConfigurationWindow != null)
                {
                    ConfigurationWindow.Dispatcher.Invoke(() =>
                    {
                        if (ConfigurationWindow?.DataContext != null)
                        {

                         //   DevicesViewModel vm = (DevicesViewModel)ConfigurationWindow.DevicesUserControl.DataContext;
                         //   vm.EnsureCorrectProfileIndex();

                        }
                    });
                }
            }
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

            if (!Directory.Exists(SLSCONFIGS_DIRECTORY))
            {
                Directory.CreateDirectory(SLSCONFIGS_DIRECTORY);
            }

            SLSManager = new SLSManager(SLSCONFIGS_DIRECTORY);

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "rgbsync.log" };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            Logger.Debug("============ JackNet RGB Sync is Starting ============");

            if (AppSettings.RunAsAdmin == true && !Debugger.IsAttached)
            {
                Logger.Debug("App should be run as administrator.");
                Logger.Debug("Checking to see if app is running as administrator...");
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                bool isRunningAsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isRunningAsAdmin == false)
                {
                    Logger.Debug("App is not running as administrator, restarting...");
                    ExecuteAsAdmin("RGBSync+.exe");
                    Exit();
                }
                else
                {
                    Logger.Debug("App is running as administrator, proceding with startup.");
                }
            }

            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (AppSettings.Lang == null)
            {
                Logger.Debug("Language is not set, inferring language from system culture. Lang=" + ci.TwoLetterISOLanguageName);
                AppSettings.Lang = ci.TwoLetterISOLanguageName;
            }
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(AppSettings.Lang);

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

            int delay = AppSettings.StartDelay * 1000;

            LoadSLSProviders();
            //LoadDeviceProviders();
            //surface.AlignDevices();

            loadingSplash.LoadingText.Text = "Mapping from config";
            SetUpMappedDevicesFromConfig();

            var tmr = 1.0 / MathHelper.Clamp(AppSettings.UpdateRate, 1, 100);
            var tmr2 = 1000.0 / MathHelper.Clamp(AppSettings.UpdateRate, 1, 100);
            UpdateTrigger = new TimerUpdateTrigger { UpdateFrequency = tmr };

            loadingSplash.LoadingText.Text = "Registering Update Trigger";

            UpdateTrigger.Start();


            try
            {
                foreach (SyncGroup syncGroup in Settings.SyncGroups)
                    RegisterSyncGroup(syncGroup);
            }
            catch
            {
                SyncGroup newSyncGroup = new SyncGroup();
                newSyncGroup.Name = "Default Group";

                Settings.SyncGroups = new List<SyncGroup>();
                Settings.SyncGroups.Add(newSyncGroup);

            }

            slsTimer = new Timer(SLSUpdate, null, 0, (int)tmr2);
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
            foreach (ISimpleLed slsManagerDriver in SLSManager.Drivers.Where(x=>x is ISimpleLedWithConfig))
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

        private void SLSUpdate(object state)
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
                    x.Driver.Name() == currentProfileDeviceProfileSetting.SourceProviderName);
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


            foreach (var currentProfileDeviceProfileSetting in CurrentProfile.DeviceProfileSettings.ToList())
            {
                ControlDevice cd = SLSDevices.FirstOrDefault(x =>
                    x.Name == currentProfileDeviceProfileSetting.SourceName &&
                    x.Driver.Name() == currentProfileDeviceProfileSetting.SourceProviderName);

                ControlDevice dest = SLSDevices.FirstOrDefault(x =>
                    x.Name == currentProfileDeviceProfileSetting.Name &&
                    x.Driver.Name() == currentProfileDeviceProfileSetting.ProviderName);

                if (cd != null && dest != null)
                {
                    string key = currentProfileDeviceProfileSetting.SourceName +
                                 currentProfileDeviceProfileSetting.SourceProviderName +
                                 currentProfileDeviceProfileSetting.Name +
                                 currentProfileDeviceProfileSetting.ProviderName;

                    if (!isMapping.ContainsKey(key) || isMapping[key] == false)
                    {
                        if (!isMapping.ContainsKey(key))
                        {
                            isMapping.Add(key, true);
                        }

                        try
                        {
                            dest.MapLEDs(cd);
                        }
                        catch
                        {
                        }

                        isMapping[key] = true;
                        Task.Run(() =>
                        {
                            try
                            {
                                if (dest.Driver.GetProperties().SupportsPush)
                                {
                                    dest.Push();
                                }
                            }
                            catch
                            {
                            }

                            isMapping[key] = false;

                        });
                    }
                }
            }
        }

        public Dictionary<string,bool> isMapping = new Dictionary<string, bool>();

        public void UnloadSLSProviders()
        {
            SLSManager.Drivers.ForEach(x =>
            {
                try
                {
                    x.Dispose();
                }
                catch
                {
                }
            });

            SLSManager.Drivers.Clear();

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }




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
            List<Exception> log = new List<Exception>();
            foreach (var pluginFolder in pluginFolders)
            {
                ct++;
                loadingSplash.ProgressBar.Value = ct;
                loadingSplash.ProgressBar.Refresh();

                var files = Directory.GetFiles(pluginFolder, "*.dll");

                loadingSplash.ProgressBar.Value = ct;
                List<Guid> driversAdded = new List<Guid>();
                foreach (string file in files)
                {
                    Debug.WriteLine("Checking " + file);
                    string filename = file.Split('\\').Last();

                    if (filename.ToLower().StartsWith("driver") || filename.ToLower().StartsWith("source"))
                    {
                        try
                        {
                            loadingSplash.LoadingText.Text = "Loading " + file.Split('\\').Last().Split('.').First();
                            Logger.Debug("Loading provider " + file);
                            Assembly assembly = Assembly.Load(File.ReadAllBytes(file));
                            //Assembly assembly = Assembly.LoadFrom(file);
                            var typeroo = assembly.GetTypes();
                            var pat2 = typeroo.Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass).ToList();


                            var testy = pat2.Where(x => x == typeof(ISimpleLed) || x == typeof(ISimpleLedWithConfig));

                            List<Type> pat3 = pat2.Where(t => typeof(ISimpleLed).IsAssignableFrom(t)).ToList();

                            foreach (Type loaderType in pat3)
                            {
                                if (Activator.CreateInstance(loaderType) is ISimpleLed slsDriver)
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
                                            slsDriver.Configure(new DriverDetails());
                                            Debug.WriteLine("Have Initialized: " + slsDriver.Name());
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex.Message);
                                        log.Add(ex);


                                    }

                                }
                                else
                                {
                                    Debug.WriteLine("isnt ISimpleLed");
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                            log.Add(e);
                        }
                    }
                }


            }

/*            ISimpleLed phillips = new PhillipsHue();
            phillips.Configure(null);
            SLSManager.Drivers.Add(phillips);*/


            
            //SLSManager.Init();
            Debug.WriteLine(log);
            loadingSplash.LoadingText.Text = "Updating SLS devices";
            UpdateSLSDevices();
        }

        public List<ControlDevice> SLSDevices = new List<ControlDevice>();

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
            SLSDevices = SLSManager.GetDevices();
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
                    var devices = simpleLedDriver.GetDevices();
                    if (devices != null)
                    {
                        controlDevices.AddRange(devices);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }

        public void AddSyncGroup(SyncGroup syncGroup)
        {
            Settings.SyncGroups.Add(syncGroup);
            RegisterSyncGroup(syncGroup);
        }

        public void RegisterSyncGroup(SyncGroup syncGroup)
        {
            try
            {
                syncGroup.LedGroup = new ListLedGroup(syncGroup.Leds.GetLeds()) { Brush = new SyncBrush(syncGroup) };
                syncGroup.LedsChangedEventHandler = (sender, args) => UpdateLedGroup(syncGroup.LedGroup, args);
                syncGroup.Leds.CollectionChanged += syncGroup.LedsChangedEventHandler;
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering group: " + syncGroup.Name);
                Logger.Error(ex);
            }

        }

        public void RemoveSyncGroup(SyncGroup syncGroup)
        {
            Settings.SyncGroups.Remove(syncGroup);
            syncGroup.Leds.CollectionChanged -= syncGroup.LedsChangedEventHandler;
            syncGroup.LedGroup.Detach();
            syncGroup.LedGroup = null;
        }
        private void UpdateLedGroup(ListLedGroup group, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                List<Led> leds = group.GetLeds().ToList();
                group.RemoveLeds(leds);
            }
            else
            {
                if (args.NewItems != null)
                    group.AddLeds(args.NewItems.Cast<SyncLed>().GetLeds());

                if (args.OldItems != null)
                    group.RemoveLeds(args.OldItems.Cast<SyncLed>().GetLeds());
            }
        }
        private void HideConfiguration()
        {
            if (AppSettings.EnableDiscordRPC == true)
            {
                if (client.IsDisposed == false)
                {
                    client.Dispose();
                }
            }
            if (AppSettings.MinimizeToTray)
            {
                if (ConfigurationWindow.IsVisible)
                    ConfigurationWindow.Hide();
            }
            else
                ConfigurationWindow.WindowState = WindowState.Minimized;
        }

        public void OpenConfiguration()
        {
            if (AppSettings.EnableDiscordRPC == true)
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
            if (AppSettings.EnableDiscordRPC == true)
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
    }
}
