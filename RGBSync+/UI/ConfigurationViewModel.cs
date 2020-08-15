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
using System.Resources;
using MadLedFrameworkSDK;

namespace RGBSyncPlus.UI
{
    public sealed class ConfigurationViewModel : AbstractBindable, IDropTarget
    {
        private bool isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());


        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        #region Properties & Fields



        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public static string PremiumStatus
        {
            get
            {
                if (Directory.Exists("netplugin"))
                {
                    //netplugin is installed
                    return "Visible";
                }
                else
                {
                    return "Hidden";
                }
            }
        }

        public string NotPremiumStatus
        {
            get
            {
                if (Directory.Exists("netplugin"))
                {
                    //netplugin is installed
                    return "Hidden";
                }
                else
                {
                    return "Visible";
                }
            }
        }

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


        public bool RunAsAdmin
        {
            get => ApplicationManager.Instance.AppSettings.RunAsAdmin;
            set
            {
                ApplicationManager.Instance.AppSettings.RunAsAdmin = value;
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
                JObject config = JObject.Parse(ReadConfig());
                JObject client = (JObject)config["client"];
                return client["status"].ToString();
            }
            set
            {
                JObject config = JObject.Parse(ReadConfig());
                JObject client = (JObject)config["client"];
                client["status"] = value;
                WriteConfig(config);
            }
        }

        public string ClientIP
        {
            get
            {
                JObject config = JObject.Parse(ReadConfig());
                JObject client = (JObject)config["client"];
                return client["host"].ToString();
            }
            set
            {
                JObject config = JObject.Parse(ReadConfig());
                JObject client = (JObject)config["client"];
                client["host"] = value;
                WriteConfig(config);
            }
        }

        public string ClientPort
        {
            get
            {
                JObject config = JObject.Parse(ReadConfig());
                JObject client = (JObject)config["client"];
                return client["port"].ToString();
            }
            set
            {
                JObject config = JObject.Parse(ReadConfig());
                JObject client = (JObject)config["client"];
                client["port"] = value;
                WriteConfig(config);
            }
        }

        public string ServerPort
        {
            get
            {
                JObject config = JObject.Parse(ReadConfig());
                JObject client = (JObject)config["server"];
                return client["port"].ToString();
            }
            set
            {
                JObject config = JObject.Parse(ReadConfig());
                JObject client = (JObject)config["server"];
                client["port"] = value;
                WriteConfig(config);
            }
        }

        private void WriteConfig(JObject config)
        {
            File.WriteAllText("config.json", config.ToString());
        }
        private string ReadConfig()
        {
            if (File.Exists("config.json"))
            {
                return File.ReadAllText("config.json", Encoding.UTF8);
            }
            else
            {
                return "{\r\n  \"client\": {\r\n    \"host\": \"1.1.1.1\",\r\n    \"port\": \"3784\",\r\n    \"status\": \"Stop Client\"\r\n  },\r\n  \"server\": {\r\n    \"port\": \"3784\"\r\n  }\r\n}";
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

        private ActionCommand _exportCloudCommand;
        public ActionCommand ExportCloudCommand => _exportCloudCommand ?? (_exportCloudCommand = new ActionCommand(ExportCloud));

        private ActionCommand _importCommand;
        public ActionCommand ImportCommand => _importCommand ?? (_importCommand = new ActionCommand(Import));

        private ActionCommand _importCloudCommand;
        public ActionCommand ImportCloudCommand => _importCloudCommand ?? (_importCloudCommand = new ActionCommand(ImportCloud));

        private ActionCommand _openSetCommand;
        public ActionCommand OpenSetCommand => _openSetCommand ?? (_openSetCommand = new ActionCommand(OpenSet));

        private ActionCommand _openExcludeToolCommand;
        public ActionCommand OpenExcludeToolCommand => _openExcludeToolCommand ?? (_openExcludeToolCommand = new ActionCommand(OpenExcludeTool));

        private ActionCommand _rpcCommand;
        public ActionCommand SetRPCCommand => _rpcCommand ?? (_rpcCommand = new ActionCommand(SetRPC));

        private ActionCommand _adminCommand;
        public ActionCommand SetAdminCommand => _adminCommand ?? (_adminCommand = new ActionCommand(SetAdmin));

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

            AvailableSyncLeds = GetGroupedLedList(GetSyncLeds());
            OnPropertyChanged(nameof(AvailableSyncLeds));
        }

        #endregion

        #region Methods
        public ObservableCollection<DeviceGroup> Devices { get; set; } = new ObservableCollection<DeviceGroup>();
        private ListCollectionView GetGroupedLedList(IEnumerable<Led> leds)
        {
            var thing = GetGroupedLedList(leds.Select(led => new SyncLed(led)).ToList());

            return thing;
        }

        public List<SyncLed> GetSyncLeds()
        {
            List<SyncLed> leds = new List<SyncLed>();

            leds.AddRange(RGBSurface.Instance.Leds.Where(x => x.Device.DeviceInfo.SupportsSyncBack).Select(p=>new SyncLed(p)));

            foreach (var cd in ApplicationManager.Instance.SLSDevices.Where(v => v.Driver.GetProperties().IsSource))
            {
                leds.AddRange(cd.LEDs.OrderBy(x=>x.Data.LEDNumber).Select(l=>new SyncLed(cd,l)));
            }

            return leds;
        }

        private ListCollectionView GetGroupedLedList(IList syncLeds)
        {
            ListCollectionView collectionView = new ListCollectionView(syncLeds);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SyncLed.Device)));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(SyncLed.Device), ListSortDirection.Ascending));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(SyncLed.Index), ListSortDirection.Ascending));
            collectionView.Refresh();
            return collectionView;
        }

        private void UpdateLedLists()
        {
            try
            {
                List<ControlDevice> slsDevices = ApplicationManager.Instance.SLSDevices;
                foreach (var controlDevice in slsDevices.Where(p=>p.Driver.GetProperties().SupportsPush))
                {
                    DeviceGroup dg = new DeviceGroup
                    {
                        Name = controlDevice.Name,
                        ControlDevice = controlDevice,
                        SyncBack = SyncBack
                    };

                    dg.DeviceLeds = new ObservableCollection<DeviceLED>(controlDevice.LEDs.Select(x => new DeviceLED(x, SelectedSyncGroup.Leds.Any(y=>y.SLSLEDUID == controlDevice.GetLedUID(x)))
                    {
                        ParentalRollUp = dg.RollUpCheckBoxes
                    }));

                    Devices.Add(dg);
                }

                List<IRGBDevice> devices = RGBSurface.Instance.Leds.Select(x => x.Device).Distinct().ToList();
               // Devices = new ObservableCollection<DeviceGroup>();
                foreach (var d in devices)
                {

                    var dg = new DeviceGroup
                    {
                        Name = d.GetDeviceName(),
                        RGBDevice = d,
                        SyncBack = SyncBack
                    };

                    dg.DeviceLeds = new ObservableCollection<DeviceLED>(RGBSurface.Instance.Leds
                        .Where(x => x.Device == d)
                        .Select(y => new DeviceLED(y, SelectedSyncGroup.Leds.Any(x => x.LedId == y.Id))
                        {
                            ParentalRollUp = dg.RollUpCheckBoxes
                        }).ToList());

                    Devices.Add(dg);
                }

                SynchronizedLeds = GetGroupedLedList(SelectedSyncGroup.Leds);

                OnPropertyChanged(nameof(SynchronizedLeds));

                AvailableLeds = GetGroupedLedList(RGBSurface.Instance.Leds.Where(led => !SelectedSyncGroup.Leds.Any(sc => (sc.LedId == led.Id) && (sc.Device == led.Device.GetDeviceName()))));
                OnPropertyChanged(nameof(AvailableLeds));
                OnPropertyChanged(nameof(Devices));
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating LedLists." + ex);
            }
        }

        public void SyncBack()
        {
            List<SyncLed> leds = new List<SyncLed>();
            List<ControlDevice.LedUnit> slsleds = new List<ControlDevice.LedUnit>();
            foreach (DeviceGroup deviceGroup in Devices)
            {
                if (deviceGroup.DeviceLeds.Where(x => x.IsSelected).Select(y => y.Led).All(p => p != null))
                {
                    leds.AddRange(deviceGroup.DeviceLeds.Where(x => x.IsSelected).Select(y => y.Led).Select(p=>new SyncLed(p)));
                }
            }

            foreach (DeviceGroup deviceGroup in Devices)
            {
                if (deviceGroup.DeviceLeds.Where(x => x.IsSelected).Select(y => y.SLSLed).All(p => p != null))
                {
                    leds.AddRange(deviceGroup.DeviceLeds.Where(x => x.IsSelected).Select(y => new SyncLed(deviceGroup.ControlDevice, y.SLSLed)));
                }
            }
            
            SelectedSyncGroup.Leds = new ObservableCollection<SyncLed>(leds);

            SynchronizedLeds = GetGroupedLedList(SelectedSyncGroup.Leds);
            OnPropertyChanged(nameof(SynchronizedLeds));

            ApplicationManager.Instance.RemoveSyncGroup(SelectedSyncGroup);
            ApplicationManager.Instance.AddSyncGroup(SelectedSyncGroup);
            
        }

        private void OpenHomepage() => Process.Start("https://www.rgbsync.com");

        private void OpenExcludeTool() => Process.Start("ExcludeHelper.exe");

        private void Discord() => Process.Start("https://www.rgbsync.com/discord");

        private void Attribs() => Process.Start("https://www.rgbsync.com?attribution");

        private void checkUpdate()
        {
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
            catch (Exception ex)
            {
                Logger.Error("Unable to check for updates. Download failed with exception: " + ex);
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
            if (ApplicationManager.Instance.AppSettings.MinimizeToTray == true)
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

        private void SetAdmin()
        {
            if (ApplicationManager.Instance.AppSettings.RunAsAdmin == true)
            {
                RunAsAdmin = true;
            }
            else if (ApplicationManager.Instance.AppSettings.RunAsAdmin == false)
            {
                RunAsAdmin = false;
            }
        }

        public void ToggleServer()
        {
            if (EnableServer == true)
            {
                EnableServer = true;
                EnableClient = false;
            }
            else
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
            if (pname.Length == 0)
            {
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
            try
            {
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
            catch (Exception ex)
            {
                GenericErrorDialog infoDialog = new GenericErrorDialog("Access is denied", "Error", ex.GetFullMessage());
                infoDialog.Show();
            }

        }

        private void ExportCloud()
        {
            if (ApplicationManager.Instance.AppSettings.ApiKey != null)
            {
                try
                {
                    WebClient client = new WebClient();
                    string profileData = Base64Encode(JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
                    string response = client.DownloadString("https://rgbsync.com/api/saveData.php?token=zQlszc7d1l9t8cv734nmte8ui4o3s8d15pcz&key=" + ApplicationManager.Instance.AppSettings.ApiKey + "&profile=" + profileData);
                    if (response == "success")
                    {
                        GenericInfoDialog infoDialog = new GenericInfoDialog("Profile has successfully been uploaded to the cloud.", "Success!");
                        infoDialog.Show();
                    }
                    else
                    {
                        GenericErrorDialog errorDialog = new GenericErrorDialog("Error uploading profile.", "Error!", "Error uploading profile. Api response: " + response);
                        errorDialog.Show();
                    }
                }
                catch (Exception ex)
                {
                    GenericErrorDialog errorDialog = new GenericErrorDialog("Error uploading profile.", "Error!", "Error uploading profile. " + ex);
                    errorDialog.Show();
                }
            }
            else
            {
                EnterKeyDialog enterKeyDialog = new EnterKeyDialog();
                enterKeyDialog.Show();
            }
        }

        private void ImportCloud()
        {
            if (ApplicationManager.Instance.AppSettings.ApiKey != null)
            {
                try
                {
                    WebClient client = new WebClient();
                    string response = client.DownloadString("https://rgbsync.com/api/fetchData.php?token=zQlszc7d1l9t8cv734nmte8ui4o3s8d15pcz&key=" + ApplicationManager.Instance.AppSettings.ApiKey);
                    ApplicationManager.Instance.Settings = JsonConvert.DeserializeObject<Settings>(Base64Decode(response));
                    App.SaveSettings();
                    ApplicationManager.Instance.RestartApp();
                }
                catch (Exception ex)
                {
                    GenericErrorDialog errorDialog = new GenericErrorDialog("Error downloading profile.", "Error!", "Error downloading profile. " + ex);
                    errorDialog.Show();
                }
            }
            else
            {
                EnterKeyDialog enterKeyDialog = new EnterKeyDialog();
                enterKeyDialog.Show();
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

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        #endregion
    }
}
