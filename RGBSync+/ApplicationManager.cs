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
using MadLedFrameworkSDK;
using RGB.NET.Brushes;

namespace RGBSyncPlus
{
    public class ApplicationManager
    {
        #region Constants
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        private const string DEVICEPROVIDER_DIRECTORY = "DeviceProvider";
        private const string SLSPROVIDER_DIRECTORY = "SLSProvider";

        #endregion

        #region Properties & Fields

        public static ApplicationManager Instance { get; } = new ApplicationManager();

        private ConfigurationWindow _configurationWindow;

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

        public void Initialize()
        {
            SLSManager = new SLSManager();

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

            RGBSurface surface = RGBSurface.Instance;
            LoadSLSProviders();
            LoadDeviceProviders();
            surface.AlignDevices();

            SetUpMappedDevicesFromConfig();

            foreach (IRGBDevice device in surface.Devices)
                device.UpdateMode = DeviceUpdateMode.Sync | DeviceUpdateMode.SyncBack;

            var tmr = 1.0 / MathHelper.Clamp(AppSettings.UpdateRate, 1, 100);
            var tmr2 = 1000.0 / MathHelper.Clamp(AppSettings.UpdateRate, 1, 100);
            UpdateTrigger = new TimerUpdateTrigger { UpdateFrequency = tmr };

            surface.RegisterUpdateTrigger(UpdateTrigger);
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
        }

        public Timer slsTimer;
        public void SetUpMappedDevicesFromConfig()
        {

            MappedDevices=new List<DeviceMappingModels.DeviceMap>();
            if (Settings.DeviceMappingProxy != null)
            {
                foreach (var deviceMapping in Settings.DeviceMappingProxy)
                {
                    DeviceMappingModels.DeviceMap dm = new DeviceMappingModels.DeviceMap
                    {
                        Source = SLSDevices.First(x =>
                            x.Name == deviceMapping.SourceDevice.DeviceName &&
                            x.Driver.Name() == deviceMapping.SourceDevice.DriverName),
                        Dest = new List<ControlDevice>()
                    };

                    foreach (var deviceMappingDestinationDevice in deviceMapping.DestinationDevices)
                    {
                        var tmp = SLSDevices.FirstOrDefault(x =>
                            x.Name == deviceMappingDestinationDevice.DeviceName &&
                            x.Driver.Name() == deviceMappingDestinationDevice.DriverName);

                        dm.Dest.Add(tmp);
                    }

                    MappedDevices.Add(dm);
                }
            }
        }

        public void DeviceMapSync()
        {
            foreach (var d in MappedDevices.Where(x=>x.Source!=null && x.Dest!=null && x.Dest.Count>0))
            {
                if (d.Source.Driver.GetProperties().SupportsPull)
                {
                    d.Source.Pull();
                }

                foreach (var controlDevice in d.Dest)
                {
                    controlDevice.MapLEDs(d.Source);
                    controlDevice.Push();
                }
            }
        }
    

    public List<DeviceMappingModels.DeviceMap> MappedDevices = new List<DeviceMappingModels.DeviceMap>();
    private void SLSUpdate(object state)
    {
        List<ControlDevice> devicesToPush = new List<ControlDevice>();
        foreach (SyncGroup syncGroup in Instance.Settings.SyncGroups.ToArray())
        {
            int r = 0;
            int g = 0;
            int b = 0;
            if (syncGroup.SyncLed.SLSLedUnit != null)
            {
                r = syncGroup.SyncLed.SLSLedUnit.Color.Red;
                g = syncGroup.SyncLed.SLSLedUnit.Color.Green;
                b = syncGroup.SyncLed.SLSLedUnit.Color.Blue;
                syncGroup.LedGroup.Brush = new SolidColorBrush(new Color((byte)r, (byte)g, (byte)b));
            }
            else
            {
                Led sled = syncGroup.SyncLed.GetLed();
                if (sled != null)
                {
                    r = sled.Color.GetR();
                    g = sled.Color.GetG();
                    b = sled.Color.GetB();
                }
            }

            foreach (var l in syncGroup.Leds.Where(x => x.SLSLedUnit != null))
            {
                l.SLSLedUnit.Color = new ControlDevice.LEDColor(r, g, b);
                if (!devicesToPush.Contains(l.ControlDevice))
                {
                    devicesToPush.Add(l.ControlDevice);
                }
            }

        }

        foreach (var cd in devicesToPush)
        {
            cd.Push();
        }

        DeviceMapSync();
    }

    private void LoadDeviceProviders()
    {
        string deviceProvierDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty, DEVICEPROVIDER_DIRECTORY);
        if (!Directory.Exists(deviceProvierDir)) return;

        foreach (string file in Directory.GetFiles(deviceProvierDir, "*.dllxx"))
        {
            try
            {
                Logger.Debug("Loading provider " + file);
                Assembly assembly = Assembly.LoadFrom(file);
                foreach (Type loaderType in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass
                                                                           && typeof(IRGBDeviceProviderLoader).IsAssignableFrom(t)))
                {
                    if (Activator.CreateInstance(loaderType) is IRGBDeviceProviderLoader deviceProviderLoader)
                    {
                        //TODO DarthAffe 03.06.2018: Support Initialization
                        if (deviceProviderLoader.RequiresInitialization) continue;

                        var deviceTypes = AppSettings.DeviceTypes;


                        try
                        {
                            RGBSurface.Instance.LoadDevices(deviceProviderLoader, deviceTypes);
                            Logger.Debug(file + " has been loaded");
                        }
                        catch
                        {
                            RGBSurface.Instance.LoadDevices(deviceProviderLoader, RGBDeviceType.All);
                            Logger.Debug(file + " has been loaded with all types.");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading " + file);
                Logger.Error(ex);
            }
        }



    }


    private void LoadSLSProviders()
    {
        string deviceProvierDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty, SLSPROVIDER_DIRECTORY);

        if (!Directory.Exists(deviceProvierDir)) return;

        foreach (string file in Directory.GetFiles(deviceProvierDir, "*.dll"))
        {
            try
            {
                Logger.Debug("Loading provider " + file);
                Assembly assembly = Assembly.LoadFrom(file);
                foreach (Type loaderType in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass && typeof(ISimpleLEDDriver).IsAssignableFrom(t)))
                {
                    if (Activator.CreateInstance(loaderType) is ISimpleLEDDriver slsDriver)
                    {
                        SLSManager.Drivers.Add(slsDriver);
                        slsDriver.Configure(null);
                    }
                }

            }
            catch
            {
            }
        }

        UpdateSLSDevices();
    }

    public List<ControlDevice> SLSDevices = new List<ControlDevice>();

    public void UpdateSLSDevices()
    {
        SLSDevices = SLSManager.GetDevices();
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
            if (_configurationWindow.IsVisible)
                _configurationWindow.Hide();
        }
        else
            _configurationWindow.WindowState = WindowState.Minimized;
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
                    LargeImageText = "JackNet RGB Sync",
                    SmallImageKey = "small_image",
                    SmallImageText = "by Fanman03"
                }
            });
        }
        if (_configurationWindow == null) _configurationWindow = new ConfigurationWindow();
        if (!_configurationWindow.IsVisible)
            _configurationWindow.Show();

        if (_configurationWindow.WindowState == WindowState.Minimized)
            _configurationWindow.WindowState = WindowState.Normal;

        try
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
        }

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
        try { RGBSurface.Instance?.Dispose(); } catch { /* well, we're shuting down anyway ... */ }
        try { client.Dispose(); } catch { /* well, we're shuting down anyway ... */ }

        Application.Current.Shutdown();
    }
    #endregion
}
}
