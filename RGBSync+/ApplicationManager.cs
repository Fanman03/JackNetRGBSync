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

namespace RGBSyncPlus
{
    public class ApplicationManager
    {
        public SimpleLed.LoginSystem SimpleLedAuth = new LoginSystem();
        public bool SimpleLedAuthenticated = false;
        public void SimpleLedLogIn(Action onLoginAction)
        {
            Process.Start(ApplicationManager.Instance.SimpleLedAuth.Login(() =>
            {
                NGSettings.SimpleLedUserId = SimpleLedAuth.UserId.Value;
                NGSettings.SimpleLedUserName = SimpleLedAuth.UserName;
                NGSettings.SimpleLedAuthToken = SimpleLedAuth.AccessToken;
                SimpleLedAuthenticated = true;
                onLoginAction?.Invoke();

            }));
        }

        #region Constants
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public const string SLSPROVIDER_DIRECTORY = "SLSProvider";
        private const string NGPROFILES_DIRECTORY = "NGProfiles";
        private const string SLSCONFIGS_DIRECTORY = "SLSConfigs";
        #endregion

        #region Properties & Fields
        public DeviceMappingModels.NGSettings NGSettings = new DeviceMappingModels.NGSettings
        {
            BackgroundOpacity = 0.5f
        };

        public RSSBackgroundDevice RssBackgroundDevice = new RSSBackgroundDevice();

        public bool PauseSyncing { get; set; } = false;
        public static ApplicationManager Instance { get; } = new ApplicationManager();

        public MainWindow ConfigurationWindow;

        public Settings Settings { get; set; } = new Settings();

        public LauncherPrefs LauncherPrefs { get; set; } = new LauncherPrefs();

        //  public AppSettings AppSettings { get; set; } = new AppSettings();
        //public TimerUpdateTrigger UpdateTrigger { get; private set; } = new TimerUpdateTrigger();

        //public DispatcherTimer UpdateTrigger { get; private set; } = new DispatcherTimer();
        public System.Timers.Timer SLSTimer;
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
            if (!Directory.Exists(NGPROFILES_DIRECTORY))
            {
                Directory.CreateDirectory(NGPROFILES_DIRECTORY);
                GenerateNewProfile("Default", false);
                isHotLoading = false;
                return;
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

        public DiscordRpcClient client;

        public static readonly SimpleLogger Logger = new SimpleLogger();
        public SLSManager SLSManager;
        #endregion

        #region Methods

        public void NavigateToTab(string tab)
        {
            ConfigurationWindow.SetTab(tab);
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
                        ColorBanks = new ObservableCollection<ColorBank>
                        {
                            new ColorBank
                            {
                                BankName = "Primary",
                                Colors = new ObservableCollection<ColorObject>
                                {
                                    new ColorObject {ColorString = "#ff0000"}, new ColorObject {ColorString = "#000000"}
                                }
                            },
                            new ColorBank
                            {
                                BankName = "Secondary",
                                Colors = new ObservableCollection<ColorObject>
                                {
                                    new ColorObject {ColorString = "#00ff00"}, new ColorObject {ColorString = "#000000"}
                                }
                            },
                            new ColorBank
                            {
                                BankName = "Tertiary",
                                Colors = new ObservableCollection<ColorObject>
                                {
                                    new ColorObject {ColorString = "#0000ff"}, new ColorObject {ColorString = "#000000"}
                                }
                            },
                            new ColorBank
                            {
                                BankName = "Auxilary",
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

        private readonly Dictionary<string, string> profilePathMapping = new Dictionary<string, string>();
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
            try
            {
                string json = JsonConvert.SerializeObject(NGSettings);
                File.WriteAllText("NGSettings.json", json);
                TimeSettingsLastSave = DateTime.Now;
                NGSettings.AreSettingsStale = false;
            }
            catch
            {
            }
        }

        public void SaveCurrentNGProfile()
        {
            if (CurrentProfile.Id == Guid.Empty)
            {
                CurrentProfile.Id = Guid.NewGuid();
            }
            Guid id = CurrentProfile.Id;

            string json = JsonConvert.SerializeObject(CurrentProfile);
            string path;
            if (profilePathMapping.ContainsKey(CurrentProfile.Name))
            {
                path = profilePathMapping[CurrentProfile.Name];
            }
            else
            {
                path = NGPROFILES_DIRECTORY + "\\" + id + ".rsprofile";
                profilePathMapping.Add(CurrentProfile.Name, path);
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

            if (File.Exists("NGOverrides.json"))
            {
                string json = File.ReadAllText("NGOverrides.json");
                DeviceOverrides = new ObservableCollection<DeviceMappingModels.DeviceOverrides>(JsonConvert.DeserializeObject<List<DeviceMappingModels.DeviceOverrides>>(json));
            }

            profilePathMapping.Clear();
            if (!Directory.Exists(NGPROFILES_DIRECTORY))
            {
                Directory.CreateDirectory(NGPROFILES_DIRECTORY);
                GenerateNewProfile("Default");
                isHotLoading = false;
                return;
            }

            string[] profiles = Directory.GetFiles(NGPROFILES_DIRECTORY, "*.rsprofile");

            if (profiles == null || profiles.Length == 0)
            {
                GenerateNewProfile("Default");
            }

            NGSettings.ProfileNames = new ObservableCollection<string>();

            foreach (string profile in profiles)
            {
                string profileName = GetProfileFromPath(profile)?.Name;
                if (!string.IsNullOrWhiteSpace(profileName))
                {
                    try
                    {

                        profilePathMapping.Add(profileName, profile);

                        if (NGSettings.ProfileNames == null)
                        {
                            NGSettings.ProfileNames = new ObservableCollection<string>();
                        }
                        NGSettings.ProfileNames.Add(profileName);
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
                LoadProfileFromName(NGSettings.CurrentProfile);
            }
            else
            {
                NGSettings.CurrentProfile = "Default";
                LoadProfileFromName(NGSettings.CurrentProfile);
            }

            if (SLSTimer != null)
            {
                SLSTimer.Stop();
                SLSTimer.Dispose();
                SLSTimer = null;
            }

            double tmr = 1.0 / MathHelper.Clamp(NGSettings.UpdateRate, 1, 100);
            double tmr2 = 1000.0 / MathHelper.Clamp(NGSettings.UpdateRate, 1, 100);
            //UpdateTrigger = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(tmr) };
            //UpdateTrigger.Tick += OnUpdateTriggerOnTick;

            //  loadingSplash.LoadingText.Text = "Registering Update Trigger";

            //UpdateTrigger.Start();

            Debug.WriteLine("Setting up timer with ms of " + tmr2);
            SLSTimer = new System.Timers.Timer(tmr2);
            SLSTimer.AutoReset = true;
            SLSTimer.Elapsed += (sender, args) => SLSUpdate(null);
            SLSTimer.Start();


            server?.CloseAsync().Wait();

            if (NGSettings.ApiEnabled)
            {
                StartApiServer();
            }

            if (NGSettings.DeviceSettings != null)
            {
                foreach (DeviceMappingModels.NGDeviceSettings ngSettingsDeviceSetting in NGSettings.DeviceSettings)
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

            if (!string.IsNullOrWhiteSpace(NGSettings.SimpleLedAuthToken))
            {
                SimpleLedAuth.AccessToken = NGSettings.SimpleLedAuthToken;
                SimpleLedAuth.UserName = NGSettings.SimpleLedUserName;
                SimpleLedAuth.UserId = NGSettings.SimpleLedUserId;
                try
                {
                    SimpleLedAuth.Authenticate(() =>
                    {
                        Debug.WriteLine("Authenticated");
                        SimpleLedAuthenticated = true;
                    });
                }
                catch
                {
                    SimpleLedAuth.AccessToken = "";
                    SimpleLedAuth.UserName = "";
                    SimpleLedAuth.UserId = Guid.Empty;
                    SimpleLedAuthenticated = false;

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

        private void OnUpdateTriggerOnTick(object sender, EventArgs args)
        {
            SLSUpdate(null);
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

                if (overridesDirty)
                {
                    overridesDirty = false;
                    string json = JsonConvert.SerializeObject(DeviceOverrides.ToList());
                    File.WriteAllText("NGOverrides.json", json);
                }
            }


        }

        public void GenerateNewProfile(string name, bool hotLoad = true)
        {
            if (NGSettings?.ProfileNames != null && NGSettings.ProfileNames.Any(x => x.ToLower() == name.ToLower()))
            {
                throw new ArgumentException("Profile name '" + name + "' already exists");
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
            if (hotLoad) HotLoadNGSettings();

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
            HttpSelfHostConfiguration apiconfig = new HttpSelfHostConfiguration("http://localhost:59022");


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
                string map = profilePathMapping[profileName];
                CurrentProfile = GetProfileFromPath(map);

                if (CurrentProfile.Id == Guid.Empty)
                {
                    string gid = map.Split('\\').Last().Split('.').First();
                    CurrentProfile.Id = Guid.Parse(gid);
                }

                if (CurrentProfile.ColorProfileId != null)
                {
                    try
                    {
                        CurrentProfile.LoadedColorProfile =
                            JsonConvert.DeserializeObject<ColorProfile>(
                                File.ReadAllText("ColorProfiles\\" + CurrentProfile.ColorProfileId + ".json"));

                        SLSManager.ColorProfile = CurrentProfile.LoadedColorProfile;
                    }
                    catch (Exception ee)
                    {
                        Logger.Info("Color Profile not loaded, " + ee.Message);
                    }
                }

                if (CurrentProfile.LoadedColorProfile == null)
                {
                    CurrentProfile.LoadedColorProfile = new ColorProfile
                    {
                        Id = Guid.Empty,
                        ProfileName = profileName,
                        ColorBanks = new ObservableCollection<ColorBank>
                        {
                            new ColorBank
                            {
                                BankName = "Primary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#ff0000" } , new ColorObject { ColorString = "#000000" } }
                            },
                            new ColorBank
                            {
                                BankName = "Secondary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#00ff00" } , new ColorObject { ColorString = "#000000" } }
                            },new ColorBank
                            {
                                BankName = "Tertiary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#0000ff" } , new ColorObject { ColorString = "#000000" } }
                            },new ColorBank
                            {
                                BankName = "Auxilary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#ff00ff" } , new ColorObject { ColorString = "#000000" } }
                            }
                        }
                    };

                    CurrentProfile.ColorProfileId = Guid.Empty;
                    CurrentProfile.IsProfileStale = true;
                }

                NGSettings.CurrentProfile = profileName;
            }
        }

        public DeviceMappingModels.NGProfile GetProfileFromName(string profileName)
        {
            if (profilePathMapping.ContainsKey(profileName))
            {
                string map = profilePathMapping[profileName];
                DeviceMappingModels.NGProfile result = GetProfileFromPath(map);

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

            profilePathMapping.Clear();


            if (!Directory.Exists(SLSCONFIGS_DIRECTORY))
            {
                Directory.CreateDirectory(SLSCONFIGS_DIRECTORY);
            }

            loadingSplash.Activate();
            SLSManager = new SLSManager(SLSCONFIGS_DIRECTORY);


            Logger.Debug("============ JackNet RGB Sync is Starting ============");

            List<LanguageModel> langs = LanguageManager.Languages;
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

            LoadSLSProviders();

            loadingSplash.LoadingText.Text = "Mapping from config";
            SetUpMappedDevicesFromConfig();

            configTimer = new Timer(ConfigUpdate, null, 0, (int)5000);

            loadingSplash.LoadingText.Text = "Loading Settings";
            LoadNGSettings();
            loadingSplash.LoadingText.Text = "All done";

            OpenConfiguration();

            DispatcherTimer closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromSeconds(1);
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
        //public Timer slsTimer;
        public Timer configTimer;
        public void SetUpMappedDevicesFromConfig()
        {
            List<ControlDevice> alreadyBeingSyncedTo = new List<ControlDevice>();
            MappedDevices = new List<DeviceMappingModels.DeviceMap>();
            if (Settings.DeviceMappingProxy != null)
            {
                foreach (DeviceMappingModels.DeviceMapping deviceMapping in Settings.DeviceMappingProxy)
                {
                    ControlDevice src = SLSDevices.FirstOrDefault(x =>
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

        private void SLSUpdateLoop()
        {

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

                foreach (DeviceMappingModels.NGDeviceProfileSettings currentProfileDeviceProfileSetting in CurrentProfile.DeviceProfileSettings.ToList())
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

                foreach (ControlDevice controlDevice in devicesToPull)
                {
                    if (controlDevice.Driver.GetProperties().SupportsPull)
                    {
                        controlDevice.Pull();
                    }
                }

                List<PushListItem> pushMe = new List<PushListItem>();
                foreach (DeviceMappingModels.NGDeviceProfileSettings currentProfileDeviceProfileSetting in CurrentProfile.DeviceProfileSettings.ToList())
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
                       foreach (PushListItem t in gp.ToList())
                       {
                           try
                           {
                               gp.Key.Push(t.Device);
                               await Task.Delay(0);
                           }
                           catch { }
                       }
                       foreach (PushListItem t in gp.ToList())
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
                ApplicationManager.Logger.CrashWindow(ex);

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
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }


                try
                {

                    x = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
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
            Thread.Sleep(1000);
        }

        public void LoadChildAssemblies(Assembly assembly, string basePath)
        {

            AssemblyName[] names = assembly.GetReferencedAssemblies();

            foreach (AssemblyName assemblyName in names)
            {
                try
                {
                    if (File.Exists(basePath + "\\" + assemblyName.Name + ".dll"))
                    {
                        Assembly temp = Assembly.Load(File.ReadAllBytes(basePath + "\\" + assemblyName.Name + ".dll"));
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
            Type[] typeroo = assembly.GetTypes();
            List<Type> pat2 = typeroo.Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass).ToList();

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
            string[] pluginFolders = Directory.GetDirectories(deviceProvierDir);
            loadingSplash.LoadingText.Text = "Loading SLS plugins";
            loadingSplash.ProgressBar.Maximum = pluginFolders.Length;

            int ct = 0;
            foreach (string pluginFolder in pluginFolders)
            {
                loadingSplash.Activate();
                ct++;

                loadingSplash.ProgressBar.Value = ct;
                loadingSplash.ProgressBar.Refresh();
                LoadPlungFolder(pluginFolder);


            }

            SolidColorDevice = new SolidColorDriver();
            GradientDriver = new GradientDriver();



            SLSManager.Drivers.Add(RssBackgroundDevice);
            SLSManager.Drivers.Add(SolidColorDevice);
            SLSManager.Drivers.Add(GradientDriver);
            RssBackgroundDevice.DeviceAdded += SlsDriver_DeviceAdded;
            RssBackgroundDevice.DeviceRemoved += SlsDriver_DeviceRemoved;

            SolidColorDevice.DeviceAdded += SlsDriver_DeviceAdded;
            SolidColorDevice.DeviceRemoved += SlsDriver_DeviceRemoved;

            GradientDriver.DeviceAdded += SlsDriver_DeviceAdded;
            GradientDriver.DeviceRemoved += SlsDriver_DeviceRemoved;

            SolidColorDevice.Configure(new DriverDetails());
            GradientDriver.Configure(new DriverDetails());

            SLSManager.RescanRequired += Rescan;
            loadingSplash.LoadingText.Text = "Updating SLS devices";
            UpdateSLSDevices();
        }

        public SolidColorDriver SolidColorDevice { get; set; }
        public GradientDriver GradientDriver { get; set; }

        public void LoadPlung(string file)
        {
            string filename = file.Split('\\').Last();
            string justPath = file.Substring(0, file.Length - filename.Length);
            if (filename.ToLower().StartsWith("driver") || filename.ToLower().StartsWith("source") || filename.ToLower().StartsWith("gameintegration"))
            {
                try
                {
                    loadingSplash.LoadingText.Text = "Loading " + file.Split('\\').Last().Split('.').First();
                    Logger.Debug("Loading provider " + file);

                    ISimpleLed slsDriver = LoadDll(justPath, filename);

                    if (slsDriver != null)
                    {
                        try
                        {
                            if (SLSManager.Drivers.All(p => p.GetProperties().Id != slsDriver.GetProperties().Id))
                            {
                                //slsDriver.Configure(null);
                                Debug.WriteLine("We got one! " + "Loading " + slsDriver.Name());
                                loadingSplash.LoadingText.Text = "Loading " + slsDriver.Name();
                                loadingSplash.UpdateLayout();
                                loadingSplash.Refresh();
                                loadingSplash.LoadingText.Refresh();
                                Task.Delay(33).Wait();
                                SLSManager.Drivers.Add(slsDriver);
                                // driversAdded.Add(slsDriver.GetProperties().Id);
                                Debug.WriteLine("all loaded: " + slsDriver.Name());
                                slsDriver.DeviceAdded += SlsDriver_DeviceAdded;
                                slsDriver.DeviceRemoved += SlsDriver_DeviceRemoved;
                                try
                                {
                                    slsDriver.Configure(new DriverDetails() { HomeFolder = justPath });
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }

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

        public void LoadPlungFolder(string pluginFolder)
        {

            string[] files = Directory.GetFiles(pluginFolder, "*.dll");


            List<Guid> driversAdded = new List<Guid>();
            foreach (string file in files)
            {
                Debug.WriteLine("Checking " + file);
                LoadPlung(file);
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
        public ObservableCollection<DeviceMappingModels.DeviceOverrides> DeviceOverrides = new ObservableCollection<DeviceMappingModels.DeviceOverrides>();
        private bool overridesDirty = false;
        public void SetOverride(DeviceMappingModels.DeviceOverrides overrider)
        {
            DeviceMappingModels.DeviceOverrides existing = DeviceOverrides.FirstOrDefault(x =>
                x.Name == overrider.Name && x.ConnectedTo == overrider.ConnectedTo &&
                x.ProviderName == overrider.ProviderName);

            if (existing != null)
            {
                DeviceOverrides.Remove(existing);
            }

            DeviceOverrides.Add(overrider);
            overridesDirty = true;

        }

        public DeviceMappingModels.DeviceOverrides GetOverride(ControlDevice cd)
        {
            DeviceMappingModels.DeviceOverrides existing = DeviceOverrides.FirstOrDefault(x =>
                x.Name == cd.Name && x.ConnectedTo == cd.ConnectedTo &&
                x.ProviderName == cd.Driver.Name());

            if (existing == null)
            {
                existing = new DeviceMappingModels.DeviceOverrides
                {
                    Name = cd.Name,
                    ConnectedTo = cd.ConnectedTo,
                    ProviderName = cd.Driver?.Name(),
                    TitleOverride = string.IsNullOrWhiteSpace(cd.TitleOverride) ? cd.Driver.Name() : cd.TitleOverride,
                    ChannelOverride = cd.ConnectedTo,
                    SubTitleOverride = cd.Name
                };
            }

            return existing;
        }

        public void UpdateSLSDevices()
        {
            loadingSplash.LoadingText.Text = "Loading Configs";
            foreach (ISimpleLed drv in SLSManager.Drivers)
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

        private void ConnectDiscord()
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

        public void OpenConfiguration()
        {
            if (NGSettings.EnableDiscordRPC == true)
            {
                ConnectDiscord();
            }

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
                string map = profilePathMapping[currentProfileOriginalName];
                DeviceMappingModels.NGProfile profile = GetProfileFromPath(map);

                profile.Name = currentProfileName;

                SaveNGProfile(profile, map);
                HotLoadNGSettings();
            }
        }
    }
}
