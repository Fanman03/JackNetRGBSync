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

namespace RGBSyncPlus
{
    public class ApplicationManager
    {
        #region Constants
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        private const string DEVICEPROVIDER_DIRECTORY = "DeviceProvider";

        #endregion

        #region Properties & Fields

        public static ApplicationManager Instance { get; } = new ApplicationManager();

        private ConfigurationWindow _configurationWindow;

        public Settings Settings { get; set; }

        public AppSettings AppSettings { get; set; }
        public TimerUpdateTrigger UpdateTrigger { get; private set; }

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

        #endregion

        #region Methods

        public void Initialize()
        {

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "rgbsync.log" };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            Logger.Debug("============ JackNet RGB Sync is Starting ============");

            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (AppSettings.Lang == null)
            {
                Logger.Debug("Language is not set, inferring language from system culture. Lang="+ ci.TwoLetterISOLanguageName);
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
                catch(Exception ex)
                {
                    Logger.Error("Error deleting file: "+ex.ToString());
                }
            }

            int delay = AppSettings.StartDelay * 1000;
            Thread.Sleep(delay);

            RGBSurface surface = RGBSurface.Instance;
            LoadDeviceProviders();
            surface.AlignDevices();

            foreach (IRGBDevice device in surface.Devices)
                device.UpdateMode = DeviceUpdateMode.Sync | DeviceUpdateMode.SyncBack;

            UpdateTrigger = new TimerUpdateTrigger { UpdateFrequency = 1.0 / MathHelper.Clamp(AppSettings.UpdateRate, 1, 100) };
            surface.RegisterUpdateTrigger(UpdateTrigger);
            UpdateTrigger.Start();

            foreach (SyncGroup syncGroup in Settings.SyncGroups)
                RegisterSyncGroup(syncGroup);
        }

        private void LoadDeviceProviders()
        {
            string deviceProvierDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty, DEVICEPROVIDER_DIRECTORY);
            if (!Directory.Exists(deviceProvierDir)) return;

            foreach (string file in Directory.GetFiles(deviceProvierDir, "*.dll"))
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

                            RGBSurface.Instance.LoadDevices(deviceProviderLoader);
                            Logger.Debug(file+" has been loaded");
                        }
                    }
                }
                catch(Exception ex) 
                { 
                    Logger.Error("Error loading " + file);
                    Logger.Error(ex);
                }
            }
        }

        public void AddSyncGroup(SyncGroup syncGroup)
        {
            Settings.SyncGroups.Add(syncGroup);
            RegisterSyncGroup(syncGroup);
        }

        private void RegisterSyncGroup(SyncGroup syncGroup)
        {
            syncGroup.LedGroup = new ListLedGroup(syncGroup.Leds.GetLeds()) { Brush = new SyncBrush(syncGroup) };
            syncGroup.LedsChangedEventHandler = (sender, args) => UpdateLedGroup(syncGroup.LedGroup, args);
            syncGroup.Leds.CollectionChanged += syncGroup.LedsChangedEventHandler;
        }

        public void RemoveSyncGroup(SyncGroup syncGroup)
        {
            Settings.SyncGroups.Remove(syncGroup);
            syncGroup.Leds.CollectionChanged -= syncGroup.LedsChangedEventHandler;
            syncGroup.LedGroup.Detach();
            syncGroup.LedGroup = null;
        }
        private void OnJoin(object sender, JoinMessage args)
        {
            System.Diagnostics.Process.Start("https://fanman03.com");
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
                } else
                {
                    Logger.Info("No update available.");
                }

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

        private void TechSupport() => System.Diagnostics.Process.Start("https://discordapp.com/invite/pRyBKPr");

        public void Exit()
        {
            Logger.Debug("============ App is Shutting Down ============");
            try { RGBSurface.Instance?.Dispose(); } catch { /* well, we're shuting down anyway ... */ }
            client.Dispose();
            Application.Current.Shutdown();
        }

        #endregion
    }
}
