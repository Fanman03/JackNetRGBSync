using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using RGB.NET.Core;
using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;
using RGBSyncPlus.Properties;
using DragDropEffects = System.Windows.DragDropEffects;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;
using MessageBox = System.Windows.MessageBox;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Configuration.Legacy;
using Settings = RGBSyncPlus.Configuration.Settings;
using AppSettings = RGBSyncPlus.Configuration.AppSettings;
using Application = System.Windows.Application;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net;

namespace RGBSyncPlus.UI
{
    public sealed class ConfigurationViewModel : AbstractBindable, IDropTarget
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        #region Properties & Fields

        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public double UpdateRate
        {
            get => 1.0 / ApplicationManager.Instance.UpdateTrigger.UpdateFrequency;
            set
            {
                double val = MathHelper.Clamp(value, 1, 100);
                ApplicationManager.Instance.AppSettings.UpdateRate = val;
                ApplicationManager.Instance.UpdateTrigger.UpdateFrequency = 1.0 / val;
                OnPropertyChanged();
            }
        }

        public int StartupDelay
        {
            get => ApplicationManager.Instance.AppSettings.StartDelay;
            set
            {
                ApplicationManager.Instance.AppSettings.StartDelay = value;
                OnPropertyChanged();
            }
        }

        public string SelectedLanguage
        {
            get => ApplicationManager.Instance.AppSettings.Lang;
            set
            {
                ApplicationManager.Instance.AppSettings.Lang = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => ApplicationManager.Instance.Settings.Name;
            set
            {
                ApplicationManager.Instance.Settings.Name = value;
                OnPropertyChanged();
            }
        }

        public bool IsMinimized
        {
            get => ApplicationManager.Instance.AppSettings.MinimizeToTray;
            set
            {
                ApplicationManager.Instance.AppSettings.MinimizeToTray = value;
                OnPropertyChanged();
            }
        }

        public bool EnableDiscordRPC
        {
            get => ApplicationManager.Instance.AppSettings.EnableDiscordRPC;
            set
            {
                ApplicationManager.Instance.AppSettings.EnableDiscordRPC = value;
                OnPropertyChanged();
            }
        }

        public bool EnableServer
        {
            get => ApplicationManager.Instance.AppSettings.EnableServer;
            set
            {
                ApplicationManager.Instance.AppSettings.EnableServer = value;
                OnPropertyChanged();
            }
        }

        public bool EnableClient
        {
            get => ApplicationManager.Instance.AppSettings.EnableClient;
            set
            {
                ApplicationManager.Instance.AppSettings.EnableClient = value;
                OnPropertyChanged();
            }
        }

        public string ClientButton
        {
            get
            {
                JObject config = JObject.Parse(File.ReadAllText("config.json", Encoding.UTF8));
                JObject client = (JObject)config["client"];
                return client["status"].ToString();
            }
            set
            {
                JObject config = JObject.Parse(File.ReadAllText("config.json", Encoding.UTF8));
                JObject client = (JObject)config["client"];
                client["status"] = value;
                File.WriteAllText("config.json", config.ToString());
            }
        }

        public string ClientIP
        {
            get
            {
                JObject config = JObject.Parse(File.ReadAllText("config.json", Encoding.UTF8));
                JObject client = (JObject)config["client"];
                return client["host"].ToString();
            }
            set
            {
                JObject config = JObject.Parse(File.ReadAllText("config.json", Encoding.UTF8));
                JObject client = (JObject)config["client"];
                client["host"] = value;
                File.WriteAllText("config.json", config.ToString());
            }
        }

        public string ClientPort
        {
            get
            {
                JObject config = JObject.Parse(File.ReadAllText("config.json", Encoding.UTF8));
                JObject client = (JObject)config["client"];
                return client["port"].ToString();
            }
            set
            {
                JObject config = JObject.Parse(File.ReadAllText("config.json", Encoding.UTF8));
                JObject client = (JObject)config["client"];
                client["port"] = value;
                File.WriteAllText("config.json", config.ToString());
            }
        }

        public string ServerPort
        {
            get
            {
                JObject config = JObject.Parse(File.ReadAllText("config.json", Encoding.UTF8));
                JObject client = (JObject)config["server"];
                return client["port"].ToString();
            }
            set
            {
                JObject config = JObject.Parse(File.ReadAllText("config.json", Encoding.UTF8));
                JObject client = (JObject)config["server"];
                client["port"] = value;
                File.WriteAllText("config.json", config.ToString());
            }
        }

        private ObservableCollection<SyncGroup> _syncGroups;
        public ObservableCollection<SyncGroup> SyncGroups
        {
            get => _syncGroups;
            set => SetProperty(ref _syncGroups, value);
        }

        private SyncGroup _selectedSyncGroup;
        public SyncGroup SelectedSyncGroup
        {
            get => _selectedSyncGroup;
            set
            {
                if (SetProperty(ref _selectedSyncGroup, value))
                    UpdateLedLists();
            }
        }

        private ListCollectionView _availableSyncLeds;
        public ListCollectionView AvailableSyncLeds
        {
            get => _availableSyncLeds;
            set => SetProperty(ref _availableSyncLeds, value);
        }

        private ListCollectionView _availableLeds;
        public ListCollectionView AvailableLeds
        {
            get => _availableLeds;
            set => SetProperty(ref _availableLeds, value);
        }

        private ListCollectionView _synchronizedLeds;
        public ListCollectionView SynchronizedLeds
        {
            get => _synchronizedLeds;
            set => SetProperty(ref _synchronizedLeds, value);
        }

        #endregion

        #region Commands

        private ActionCommand _openHomepageCommand;
        public ActionCommand OpenHomepageCommand => _openHomepageCommand ?? (_openHomepageCommand = new ActionCommand(OpenHomepage));

        private ActionCommand _exportCommand;
        public ActionCommand ExportCommand => _exportCommand ?? (_exportCommand = new ActionCommand(Export));

        private ActionCommand _importCommand;
        public ActionCommand ImportCommand => _importCommand ?? (_importCommand = new ActionCommand(Import));

        private ActionCommand _openSetCommand;
        public ActionCommand OpenSetCommand => _openSetCommand ?? (_openSetCommand = new ActionCommand(OpenSet));

        private ActionCommand _rpcCommand;
        public ActionCommand SetRPCCommand => _rpcCommand ?? (_rpcCommand = new ActionCommand(SetRPC));

        private ActionCommand _startClientCommand;
        public ActionCommand StartClientCommand => _startClientCommand ?? (_startClientCommand = new ActionCommand(StartClient));

        private ActionCommand _startServerCommand;
        public ActionCommand StartServerCommand => _startServerCommand ?? (_startServerCommand = new ActionCommand(StartServer));

        private ActionCommand _toggleServerCommand;
        public ActionCommand ToggleServerCommand => _toggleServerCommand ?? (_toggleServerCommand = new ActionCommand(ToggleServer));

        private ActionCommand _toggleClientCommand;
        public ActionCommand ToggleClientCommand => _toggleClientCommand ?? (_toggleClientCommand = new ActionCommand(ToggleClient));

        private ActionCommand _discord;
        public ActionCommand DiscordCommand => _discord ?? (_discord = new ActionCommand(Discord));

        private ActionCommand _checkUpdate;
        public ActionCommand CheckUpdateCommand => _checkUpdate ?? (_checkUpdate = new ActionCommand(checkUpdate));

        private ActionCommand _attribs;
        public ActionCommand AttribCommand => _attribs ?? (_attribs = new ActionCommand(Attribs));

        private ActionCommand _addSyncGroupCommand;
        public ActionCommand AddSyncGroupCommand => _addSyncGroupCommand ?? (_addSyncGroupCommand = new ActionCommand(AddSyncGroup));

        private ActionCommand<SyncGroup> _removeSyncGroupCommand;
        public ActionCommand<SyncGroup> RemoveSyncGroupCommand => _removeSyncGroupCommand ?? (_removeSyncGroupCommand = new ActionCommand<SyncGroup>(RemoveSyncGroup));

        #endregion

        #region Constructors

        public ConfigurationViewModel()
        {
            SyncGroups = new ObservableCollection<SyncGroup>(ApplicationManager.Instance.Settings.SyncGroups);

            AvailableSyncLeds = GetGroupedLedList(RGBSurface.Instance.Leds.Where(x => x.Device.DeviceInfo.SupportsSyncBack));
            OnPropertyChanged(nameof(AvailableSyncLeds));
        }

        #endregion

        #region Methods

        private ListCollectionView GetGroupedLedList(IEnumerable<Led> leds) => GetGroupedLedList(leds.Select(led => new SyncLed(led)).ToList());

        private ListCollectionView GetGroupedLedList(IList syncLeds)
        {
            ListCollectionView collectionView = new ListCollectionView(syncLeds);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SyncLed.Device)));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(SyncLed.Device), ListSortDirection.Ascending));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(SyncLed.LedId), ListSortDirection.Ascending));
            collectionView.Refresh();
            return collectionView;
        }

        private void UpdateLedLists()
        {
            try
            {
                SynchronizedLeds = GetGroupedLedList(SelectedSyncGroup.Leds);

                OnPropertyChanged(nameof(SynchronizedLeds));

                AvailableLeds = GetGroupedLedList(RGBSurface.Instance.Leds.Where(led => !SelectedSyncGroup.Leds.Any(sc => (sc.LedId == led.Id) && (sc.Device == led.Device.GetDeviceName()))));
                OnPropertyChanged(nameof(AvailableLeds));
            }
            catch(Exception ex)
            {
                Logger.Error("Error updating LedLists." + ex);
            }
        }

        private void OpenHomepage() => Process.Start("https://www.fanman03.com/");

        private void Discord() => Process.Start("https://discordapp.com/invite/pRyBKPr");

        private void Attribs() => Process.Start("https://fanman03.com/?page=attribution");

        private void checkUpdate()
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
                    PromptUpdate();
                    Logger.Info("Update available. (major)");
                }
                else if (versionFromApi.minor > versionMinor)
                {
                    PromptUpdate();
                    Logger.Info("Update available. (minor)");
                }
                else if (versionFromApi.build > versionBuild)
                {
                    PromptUpdate();
                    Logger.Info("Update available. (build)");
                }
                else
                {
                    NoUpdate();
                    Logger.Info("No update available.");
                }

            }
        }

        public void PromptUpdate()
        {
            GetUpdateWindow getUpdateWindow = new GetUpdateWindow();
            getUpdateWindow.Show();
        }

        public void NoUpdate()
        {
            NoUpdateDialog noUpdate = new NoUpdateDialog();
            noUpdate.Show();
        }

        private void OpenSet()
        {
            if(ApplicationManager.Instance.AppSettings.MinimizeToTray == true)
            {
                IsMinimized = true;
            }
            else if (ApplicationManager.Instance.AppSettings.MinimizeToTray == false)
            {
                IsMinimized = false;
            }
        }

        private void SetRPC()
        {
            if (ApplicationManager.Instance.AppSettings.EnableDiscordRPC == true)
            {
                EnableDiscordRPC = true;
            }
            else if (ApplicationManager.Instance.AppSettings.EnableDiscordRPC == false)
            {
                EnableDiscordRPC = false;
            }
        }

        public void ToggleServer()
        {
            if(EnableServer == true)
            {
                EnableServer = true;
                EnableClient = false;
            } else
            {
                EnableServer = false;
            }

        }

        public void ToggleClient()
        {
            if (EnableClient == true)
            {
                EnableClient = true;
                EnableServer = false;
            }
            else
            {
                EnableClient = false;
            }

        }

        private void StartClient()
        {
            Process[] pname = Process.GetProcessesByName("netsync");
            if (pname.Length == 0) {
                if (!File.Exists("netplugin\\netsync.exe"))
                {
                    Logger.Info("User does not have premium, cannot start client.");
                    PremiumMessageBox notAvailable = new PremiumMessageBox();
                    notAvailable.Show();
                }
                else
                {
                    Logger.Info("Starting client...");
                    Process.Start("netplugin\\netsync.exe");
                }

            }
            else
            {
                foreach (var process in Process.GetProcessesByName("netsync"))
                {
                    process.Kill();
                }
            }
        }

        private void StartServer()
        {
            Process[] pname = Process.GetProcessesByName("netserve");
            if (pname.Length == 0)
            {
                if (!File.Exists("netplugin\\netserve.exe"))
                {
                    PremiumMessageBox notAvailable = new PremiumMessageBox();
                    notAvailable.Show();
                    Logger.Info("User does not have premium, cannot start server.");
                }
                else
                {
                    Logger.Info("Starting server...");
                    Process.Start("netplugin\\netserve.exe");
                }
            }
            else
            {
                foreach (var process in Process.GetProcessesByName("netserve"))
                {
                    process.Kill();

                }
            };
        }

        private void Export()
        {
            string settingsPath = "Profile.json";
            try {
                File.WriteAllText(settingsPath, JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
                string data = System.IO.File.ReadAllText(settingsPath);

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Profiles|*.json";
                saveFileDialog1.Title = "Export Profile";
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, data);
                }
                else
                {

                }
            }
            catch
            {
                GenericErrorDialog infoDialog = new GenericErrorDialog("Access is denied", "Error");
                infoDialog.Show();
            }
            
        }

        private void Import()
        {
            string settingsPath = "Profile.json";
            string path = Environment.CurrentDirectory;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Browse for a profile";
            openFileDialog1.Filter = "Profiles (*.json)|*.json";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                //Save the file
                string filename = openFileDialog1.FileName;
                string data = System.IO.File.ReadAllText(@filename);
                string settingsFile = path + "\\Profile.json";
                System.IO.File.WriteAllText("Profile.json", data);

                //Apply the changes
                Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath), new ColorSerializer());
                ConfigurationUpdates.PerformOn(settings);
                ApplicationManager.Instance.Settings = settings;
                ApplicationManager.Instance.Initialize();
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            else
            {
            }
        }

        private void AddSyncGroup()
        {
            SyncGroup syncGroup = new SyncGroup();
            SyncGroups.Add(syncGroup);
            ApplicationManager.Instance.AddSyncGroup(syncGroup);
        }

        private void RemoveSyncGroup(SyncGroup syncGroup)
        {
            if (syncGroup == null) return;
            DeleteLayerDialog dld = new DeleteLayerDialog(syncGroup.DisplayName);
            if ((bool)dld.ShowDialog())
            {
                SyncGroups.Remove(syncGroup);
                ApplicationManager.Instance.RemoveSyncGroup(syncGroup);
            }
            else
            {
                return;
            }


        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if ((dropInfo.Data is SyncLed || dropInfo.Data is IEnumerable<SyncLed>) && (dropInfo.TargetCollection is ListCollectionView))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetCollection is ListCollectionView targetList)) return;

            //HACK DarthAffe 04.06.2018: Super ugly hack - I've no idea how to do this correctly ...
            ListCollectionView sourceList = targetList == AvailableLeds ? SynchronizedLeds : AvailableLeds;

            if (dropInfo.Data is SyncLed syncLed)
            {
                targetList.AddNewItem(syncLed);
                sourceList.Remove(syncLed);

                targetList.CommitNew();
                sourceList.CommitEdit();
            }
            else if (dropInfo.Data is IEnumerable<SyncLed> syncLeds)
            {
                foreach (SyncLed led in syncLeds)
                {
                    targetList.AddNewItem(led);
                    sourceList.Remove(led);
                }
                targetList.CommitNew();
                sourceList.CommitEdit();
            }
        }

        #endregion
    }
}
