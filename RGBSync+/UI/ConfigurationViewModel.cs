using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
using System.Windows.Media.Imaging;
using SimpleLed;

namespace RGBSyncPlus.UI
{
    public sealed class ConfigurationViewModel : AbstractBindable
    {
        private bool showManageProfiles;

        public bool ShowManageProfiles
        {
            get => showManageProfiles;
            set => SetProperty(ref showManageProfiles, value);
        }

        private int selectedProfileIndex = 0;

        public int SelectedProfileIndex
        {
            get => selectedProfileIndex;
            set
            {
                selectedProfileIndex = value;
                if (value > -1 && value < profileNames.Count)
                {
                    string newProfileName = ProfileNames[value];
                    if (ApplicationManager.Instance.CurrentProfile.Name != newProfileName)
                    {
                        ApplicationManager.Instance.LoadProfileFromName(newProfileName);

                    }
                }
            }
        }
        private bool isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());


        private ObservableCollection<string> profileTriggerTypeNames = new ObservableCollection<string>
        {
            ProfileTriggerManager.ProfileTriggerTypes.TimeBased,
            ProfileTriggerManager.ProfileTriggerTypes.RunningProccess
        };

        public ObservableCollection<string> ProfileTriggerTypeNames
        {
            get => profileTriggerTypeNames;
            set => SetProperty(ref profileTriggerTypeNames, value);
        }

        public class ProfileItemViewModel : BaseViewModel
        {
            private string name;
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }

            private ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry> triggers;

            public ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry> Triggers
            {
                get => triggers;
                set => SetProperty(ref triggers, value);
            }
        }

        private ObservableCollection<ProfileItemViewModel> profileItems = new ObservableCollection<ProfileItemViewModel>();

        public ObservableCollection<ProfileItemViewModel> ProfileItems
        {
            get => profileItems;
            set => SetProperty(ref profileItems, value);
        }

        private string selectedProfileItem;
        public string SelectedProfileItem
        {
            get => selectedProfileItem;
            set
            {
                SetProperty(ref selectedProfileItem, value);
            }
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        #region Properties & Fields

        private ObservableCollection<string> profileNames;

        public ObservableCollection<string> ProfileNames
        {
            get => profileNames;
            set => SetProperty(ref profileNames, value);
        }

        private ObservableCollection<PositionalAssignment.PluginDetailsViewModel> plugins;
        public ObservableCollection<PositionalAssignment.PluginDetailsViewModel> Plugins
        {
            get => plugins;
            set => SetProperty(ref plugins, value);
        }

        private ObservableCollection<DeviceMappingModels.Device> slsDevices;
        public ObservableCollection<DeviceMappingModels.Device> SLSDevices { get => slsDevices; set => SetProperty(ref slsDevices, value); }

        private ObservableCollection<DeviceMappingModels.DeviceMappingViewModel> deviceMappingViewModel;
        public ObservableCollection<DeviceMappingModels.DeviceMappingViewModel> DeviceMappingViewModel { get => deviceMappingViewModel; set => SetProperty(ref deviceMappingViewModel, value); }
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        private bool singleDeviceSelected;

        public bool SingledDeviceSelected
        {
            get => singleDeviceSelected;
            set => SetProperty(ref singleDeviceSelected, value);
        }

        private bool multipleDeviceSelected;

        public bool MultipleDeviceSelected
        {
            get => multipleDeviceSelected;
            set => SetProperty(ref multipleDeviceSelected, value);
        }



        private bool showModal;

        public bool ShowModal
        {
            get => showModal;
            set => SetProperty(ref showModal, value);
        }

        private string modalText = "Please Wait";

        public string ModalText
        {
            get => modalText;
            set => SetProperty(ref modalText, value);
        }

        private bool showModalTextBox = false;

        public bool ShowModalTextBox
        {
            get => showModalTextBox;
            set => SetProperty(ref showModalTextBox, value);
        }

        private bool showModalCloseButton = false;

        public bool ShowModalCloseButton
        {
            get => showModalCloseButton;
            set => SetProperty(ref showModalCloseButton, value);
        }


        private string syncToSearch = "";

        public string SyncToSearch
        {
            get => syncToSearch;
            set
            {
                SetProperty(ref syncToSearch, value);
                FilterSourceDevices();
            }
        }

        private string subViewMode = "Info";

        public string SubViewMode
        {
            get => subViewMode;
            set => SetProperty(ref subViewMode, value);
        }

        private bool devicesCondenseView = false;

        public bool DevicesCondenseView
        {
            get => devicesCondenseView;
            set => SetProperty(ref devicesCondenseView, value);
        }

        private ObservableCollection<DeviceMappingModels.SourceModel> sourceDevices;

        public ObservableCollection<DeviceMappingModels.SourceModel> SourceDevices
        {
            get => sourceDevices;
            set { SetProperty(ref sourceDevices, value); }
        }

        public void FilterSourceDevices()
        {

            var visibleDevices = SourceDevices.Where(sourceDevice => ((string.IsNullOrWhiteSpace(SyncToSearch) ||
                                                                       (sourceDevice.Name.ToLower() ==
                                                                        SyncToSearch.ToLower() ||
                                                                        sourceDevice.ProviderName.ToLower() ==
                                                                        SyncToSearch.ToLower()
                                                                       )
                    )
                ));

            Debug.WriteLine(visibleDevices.Count());
            foreach (var sourceDevice in SourceDevices)
            {
                sourceDevice.IsHidden = !
                    (sourceDevice.Enabled || (string.IsNullOrWhiteSpace(SyncToSearch) ||
                      (sourceDevice.Name.ToLower().Contains(SyncToSearch.ToLower()) ||
                       sourceDevice.ProviderName.ToLower().Contains(SyncToSearch.ToLower()))));
            }

            OnPropertyChanged(nameof(SourceDevices));
        }

        private ObservableCollection<DeviceMappingModels.SourceModel> filteredSourceDevices;

        public ObservableCollection<DeviceMappingModels.SourceModel> FilteredSourceDevices
        {
            get => filteredSourceDevices;
            set => SetProperty(ref filteredSourceDevices, value);
        }

        public void SetupSourceDevices(ControlDevice controlDevice)
        {
            if (controlDevice == null) return;

            var sources = ApplicationManager.Instance.SLSDevices.Where(x => x.Driver.GetProperties().IsSource || x.Driver.GetProperties().SupportsPull);

            ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings> temp = ApplicationManager.Instance.CurrentProfile?.DeviceProfileSettings;
            DeviceMappingModels.NGDeviceProfileSettings current = null;

            if (controlDevice != null)
            {
                temp?.FirstOrDefault(x =>
                    x != null && x.Name == controlDevice.Name && x.ProviderName == controlDevice.Driver?.Name());
            }

            SourceDevices = new ObservableCollection<DeviceMappingModels.SourceModel>();
            foreach (var source in sources)
            {
                var enabled = current != null && source.Driver.Name() == current.SourceProviderName && source.Name == current.SourceName && source.ConnectedTo == current.ConnectedTo;
                SourceDevices.Add(new DeviceMappingModels.SourceModel
                {
                    ProviderName = source.Driver.Name(),
                    ConnectedTo = source.ConnectedTo,
                    Device = source,
                    Name = source.Name,
                    Enabled = enabled,
                    Image = ToBitmapImage(source.ProductImage),
                }); ;
            }

        }

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

        private int thumbWidth = 64;

        public int ThumbWidth
        {
            get => thumbWidth;
            set
            {
                SetProperty(ref thumbWidth, value);
            }
        }

        private int thumbHeight = 48;

        public int ThumbHeight
        {
            get => thumbHeight;
            set
            {
                SetProperty(ref thumbHeight, value);
            }
        }

        private int zoomLevel = 5;

        public int ZoomLevel
        {
            get => zoomLevel;
            set
            {
                SetProperty(ref zoomLevel, value);
                if (ZoomLevel < 3) ZoomLevel = 3;
                if (ZoomLevel > 9) ZoomLevel = 9;
                ThumbWidth = new[] { 16, 32, 64, 128, 192, 256, 385, 512, 768, 1024, 2048, 4096 }[ZoomLevel];
                ThumbHeight = (int)(ThumbWidth / 1.3333333333333333f);

                ShowFullThumb = ZoomLevel > 3;
            }
        }

        private bool showFullThumb;

        public bool ShowFullThumb
        {
            get => showFullThumb;
            set => SetProperty(ref showFullThumb, value);
        }

        public bool EnableStartupDelay
        {
            get => ApplicationManager.Instance.AppSettings.RunOnStartup;
            set
            {
                ApplicationManager.Instance.AppSettings.RunOnStartup = value;
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

        public ObservableCollection<DeviceMappingModels.DeviceMapping> DeviceMaps { get; set; } = new ObservableCollection<DeviceMappingModels.DeviceMapping>();

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

        private double maxSubViewHeight;
        public double MaxSubViewHeight
        {
            get => maxSubViewHeight;
            set => SetProperty(ref maxSubViewHeight, value);
        }

        private bool showPreRelease = false;

        public bool ShowPreRelease
        {
            get => showPreRelease;
            set
            {
                SetProperty(ref showPreRelease, value);
                FilterPlugins();
            }
        }

        private string pluginSearch;

        public string PluginSearch
        {
            get => pluginSearch;
            set
            {
                SetProperty(ref pluginSearch, value);
                FilterPlugins();
            }
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

        private ActionCommand _setDelayCommand;
        public ActionCommand SetDelayCommand => _setDelayCommand ?? (_setDelayCommand = new ActionCommand(SetDelay));

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
            DeviceMappingViewModel = new ObservableCollection<DeviceMappingModels.DeviceMappingViewModel>();



            ProfileNames = ApplicationManager.Instance.NGSettings.ProfileNames;
            SetUpProfileModels();
            this.ZoomLevel = 4;

            storeHandler = new StoreHandler();

            LoadStoreAndPlugins();
            EnsureCorrectProfileIndex();

            OnPropertyChanged(nameof(ProfileTriggerTypeNames));
        }

        public void SetUpProfileModels()
        {
            foreach (string profileName in ProfileNames)
            {
                ProfileItems.Add(new ProfileItemViewModel
                {
                    Name = profileName,
                    Triggers = new ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry>(ApplicationManager.Instance.ProfileTriggerManager.ProfileTriggers.Where(x=>x.ProfileName==profileName))
                });
            }
        }

        public void LoadStoreAndPlugins()
        {
          //  using (new SimpleModal(this,"Refreshing store...."))
            {
                
                SetUpDeviceMapViewModel();

                List<PositionalAssignment.PluginDetails> pp = storeHandler.DownloadStoreManifest();

                var ppu = pp.GroupBy(x => x.PluginId);

                Plugins = new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>();

                foreach (IGrouping<Guid, PositionalAssignment.PluginDetails> pluginDetailses in ppu)
                {
                    var insertThis = pluginDetailses.First();

                    var tmp = new PositionalAssignment.PluginDetailsViewModel(insertThis);
                    tmp.Versions = new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>(pluginDetailses
                        .Select(x => new PositionalAssignment.PluginDetailsViewModel(x, true)).ToList());

                    try
                    {
                        if (!Directory.Exists("icons"))
                        {
                            try
                            {
                                Directory.CreateDirectory("icons");
                            }
                            catch
                            {
                            }
                        }

                        if (File.Exists("icons\\" + tmp.PluginDetails.PluginId + ".png"))
                        {
                            using (Bitmap bm = new Bitmap("icons\\" + tmp.PluginDetails.PluginId + ".png"))
                            {
                                tmp.Image = ToBitmapImage(bm);
                            }
                        }
                        else
                        {

                            string imageUrl = "https://github.com/SimpleLed/Store/raw/master/Icons/" +
                                              tmp.PluginDetails.PluginId + ".png";
                            Debug.WriteLine("Trying to fetch: " + imageUrl);
                            var webClient = new WebClient();
                            using (Stream stream = webClient.OpenRead(imageUrl))
                            {
                                // make a new bmp using the stream
                                using (Bitmap bitmap = new Bitmap(stream))
                                {
                                    //flush and close the stream
                                    stream.Flush();
                                    stream.Close();
                                    // write the bmp out to disk
                                    tmp.Image = ToBitmapImage(bitmap);
                                    bitmap.Save("icons\\" + tmp.PluginDetails.PluginId + ".png");
                                }
                            }
                        }
                    }
                    catch
                    {
                    }

                    Plugins.Add(tmp);

                }

                FilterPlugins();

            }
        }

        public void FilterPlugins()
        {
            foreach (var pluginDetailsViewModel in Plugins)
            {
                ReleaseNumber highestApplicable;

                ISimpleLed newestPublicModel;
                ISimpleLed newestExperimentalModel;

                ISimpleLed installedVersion=null;
                ReleaseNumber newestPublicFound = new ReleaseNumber(0, 0, 0, 0);
                ReleaseNumber newestExperimentalFound = new ReleaseNumber(0, 0, 0, 0);
                ReleaseNumber installed = new ReleaseNumber(0, 0, 0, 0);

                if (ApplicationManager.Instance.SLSManager.Drivers.Any(x => x.GetProperties().Id == pluginDetailsViewModel.PluginId))
                {
                    installedVersion = ApplicationManager.Instance.SLSManager.Drivers.First(x => x.GetProperties().Id == pluginDetailsViewModel.PluginId);
                    installed = installedVersion.GetProperties().CurrentVersion;
                    pluginDetailsViewModel.Installed = true;
                }
                else
                {
                    pluginDetailsViewModel.Installed = false;
                }

                foreach (var detailsViewModel in pluginDetailsViewModel.Versions)
                {
                    if (detailsViewModel.PluginDetails.DriverProperties.CurrentVersion != null &&
                        detailsViewModel.PluginDetails.DriverProperties.CurrentVersion > newestPublicFound &&
                        detailsViewModel.PluginDetails.DriverProperties.IsPublicRelease)
                    {
                        newestPublicFound = detailsViewModel.PluginDetails.DriverProperties.CurrentVersion;
                    }

                    if (detailsViewModel.PluginDetails.DriverProperties.CurrentVersion != null &&
                        detailsViewModel.PluginDetails.DriverProperties.CurrentVersion > newestExperimentalFound &&
                        !detailsViewModel.PluginDetails.DriverProperties.IsPublicRelease)
                    {
                        newestExperimentalFound = detailsViewModel.PluginDetails.DriverProperties.CurrentVersion;
                    }
                }

                pluginDetailsViewModel.NewestPreReleaseVersion = newestExperimentalFound.ToString();
                pluginDetailsViewModel.NewestPublicVersion = newestPublicFound.ToString();

                if (ShowPreRelease || (installedVersion!= null && !installedVersion.GetProperties().IsPublicRelease))
                {
                    highestApplicable = newestExperimentalFound;
                    if (highestApplicable < newestPublicFound)
                    {
                        highestApplicable = newestPublicFound;
                    }
                }
                else
                {
                    highestApplicable = newestPublicFound;
                }

                
                //p


                //var publicReleases = pluginDetailsViewModel.Versions.Where(t => t.PluginDetails.Version != null && t.PluginDetails.DriverProperties.IsPublicRelease);
                //if (publicReleases == null || publicReleases.Count()==0)
                //{
                //    pluginDetailsViewModel.Version = "0.0.0.0";
                //}
                //else
                //{
                //    pluginDetailsViewModel.Version = publicReleases.Max(p => p.PluginDetails.Version).ToString();
                //}

                //var versionsWithVersions = pluginDetailsViewModel.Versions.Where(x => x.PluginDetails?.Version != null);
                //pluginDetailsViewModel.PreReleaseVersion = versionsWithVersions.Max(p => p.PluginDetails.Version)?.ToString();
                //if (pluginDetailsViewModel.PreReleaseVersion == null)
                //{
                //    pluginDetailsViewModel.PreReleaseVersion = "0.0.0.0";
                //}

                //var existingInstalled = ApplicationManager.Instance.SLSManager.Drivers.FirstOrDefault(x => x.GetProperties().CurrentVersion?.ToString() == pluginDetailsViewModel.Version);

                //ReleaseNumber highestApplicable = new ReleaseNumber(pluginDetailsViewModel.Version);
                //if (ShowPreRelease||(existingInstalled!=null&&!existingInstalled.GetProperties().IsPublicRelease))
                //{
                //    highestApplicable = new ReleaseNumber(pluginDetailsViewModel.PreReleaseVersion);
                //}

                //bool isoutdated = false;
                //if (existingInstalled != null)
                //{
                //    isoutdated = existingInstalled.GetProperties().CurrentVersion < highestApplicable;
                //    pluginDetailsViewModel.InstalledButOutdated = isoutdated;
                //}

                
                PositionalAssignment.PluginDetailsViewModel newest = pluginDetailsViewModel.Versions.FirstOrDefault(x => x.Version == highestApplicable.ToString());

                if (newest == null && highestApplicable.ToString() == "0.0.0.0000")
                {
                    newest = pluginDetailsViewModel.Versions.FirstOrDefault(x => x.Version == null);
                }

                if (newest == null)
                {
                    pluginDetailsViewModel.Visible = false;
                }
                else
                {
                    pluginDetailsViewModel.Visible = true;
                    pluginDetailsViewModel.Version = highestApplicable.ToString();
                    pluginDetailsViewModel.Name = newest.Name;
                    pluginDetailsViewModel.Author = newest.Author;

                    pluginDetailsViewModel.PreRelease = !newest.PluginDetails.DriverProperties.IsPublicRelease;

                    pluginDetailsViewModel.InstalledButOutdated = pluginDetailsViewModel.Installed && highestApplicable > installedVersion.GetProperties().CurrentVersion;

                    pluginDetailsViewModel.Visible =
                        (ShowPreRelease || !pluginDetailsViewModel.PreRelease) &&
                        (string.IsNullOrWhiteSpace(PluginSearch) ||
                         pluginDetailsViewModel.Name.ToLower().Contains(PluginSearch.ToLower())
                         ||
                         pluginDetailsViewModel.Author.ToLower().Contains(PluginSearch.ToLower())
                         ||
                         (pluginDetailsViewModel.Blurb != null &&
                          pluginDetailsViewModel.Blurb.ToLower().Contains(PluginSearch.ToLower()))
                        );
                }


            }
        }

        private StoreHandler storeHandler;

        private void SetUpDeviceMapViewModel()
        {
            SLSDevices = new ObservableCollection<DeviceMappingModels.Device>();
            foreach (ControlDevice device in ApplicationManager.Instance.SLSDevices)
            {
                var props = device.Driver.GetProperties();

                SLSDevices.Add(new DeviceMappingModels.Device
                {
                    ControlDevice = device,
                    Image = ToBitmapImage(device.ProductImage),
                    Name = device.Name,
                    ProviderName = device.Driver.Name(),
                    SupportsPull = props.SupportsPull,
                    SupportsPush = props.SupportsPush,
                    DriverProps = props,
                    ConnectedTo = device.ConnectedTo,
                    Title = string.IsNullOrWhiteSpace(device.TitleOverride) ? device.Driver.Name() : device.TitleOverride
                });
            }

        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            try
            {
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Methods
        public ObservableCollection<DeviceGroup> Devices { get; set; } = new ObservableCollection<DeviceGroup>();
        private int devicesSelectedCount = 0;
        public int DevicesSelectedCount
        {
            get => devicesSelectedCount;
            set
            {
                int previousCount = devicesSelectedCount;
                SetProperty(ref devicesSelectedCount, value);
                if (previousCount == 0 && value > 0)
                {
                    SubViewMode = "Info";
                }

                SingledDeviceSelected = value == 1;
                MultipleDeviceSelected = value > 1;

                if (MultipleDeviceSelected && (SubViewMode == "Config" || SubViewMode == "Alignment"))
                {
                    SubViewMode = "Info";
                }
            }
        }

        

        private ListCollectionView GetGroupedLedList(IEnumerable<Led> leds)
        {
            var thing = GetGroupedLedList(leds.Select(led => new SyncLed(led)).ToList());

            return thing;
        }

        public List<SyncLed> GetSyncLeds()
        {
            List<SyncLed> leds = new List<SyncLed>();

            leds.AddRange(RGBSurface.Instance.Leds.Where(x => x.Device.DeviceInfo.SupportsSyncBack).Select(p => new SyncLed(p)));

            foreach (var cd in ApplicationManager.Instance.SLSDevices.Where(v => v.Driver.GetProperties().IsSource))
            {
                leds.AddRange(cd.LEDs.OrderBy(x => x.Data.LEDNumber).Select(l => new SyncLed(cd, l)));
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
                foreach (var controlDevice in slsDevices.Where(p => p.Driver.GetProperties().SupportsPush && p.LEDs != null))
                {
                    DeviceGroup dg = new DeviceGroup
                    {
                        Name = controlDevice.Name,
                        ControlDevice = controlDevice,
                        SyncBack = LedMappingSyncBack
                    };

                    dg.DeviceLeds = new ObservableCollection<DeviceLED>(controlDevice.LEDs.Select(x => new DeviceLED(x, SelectedSyncGroup.Leds.Any(y => y.SLSLEDUID == controlDevice.GetLedUID(x)))
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
                        SyncBack = LedMappingSyncBack
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

        public void LedMappingSyncBack()
        {
            List<SyncLed> leds = new List<SyncLed>();
            List<ControlDevice.LedUnit> slsleds = new List<ControlDevice.LedUnit>();
            foreach (DeviceGroup deviceGroup in Devices)
            {
                if (deviceGroup.DeviceLeds.Where(x => x.IsSelected).Select(y => y.Led).All(p => p != null))
                {
                    leds.AddRange(deviceGroup.DeviceLeds.Where(x => x.IsSelected).Select(y => y.Led).Select(p => new SyncLed(p)));
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
            //TODO fix update box. Until then, disable it because its annoying af. GetUpdateWindow getUpdateWindow = new GetUpdateWindow();
            //getUpdateWindow.Show();
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

        private void SetDelay()
        {
            if (ApplicationManager.Instance.AppSettings.RunOnStartup == true)
            {
                EnableStartupDelay = true;
            }
            else if (ApplicationManager.Instance.AppSettings.RunOnStartup == false)
            {
                EnableStartupDelay = false;
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

        public void SubmitModalTextBox(string text)
        {
            modalSubmitAction?.Invoke(text);
        }

        public void ShowCreateNewProfile()
        {
            ShowModal = true;
            ModalText = "Enter name for new profile";
            ShowModalTextBox = true;
            ShowModalCloseButton = true;
            modalSubmitAction = (text) =>
            {
                ApplicationManager.Instance.GenerateNewProfile(text);
                ProfileNames = ApplicationManager.Instance.NGSettings.ProfileNames;
                ApplicationManager.Instance.LoadProfileFromName(text);
                EnsureCorrectProfileIndex();
            };

        }

        public void EnsureCorrectProfileIndex()
        {
            SelectedProfileIndex = profileNames.IndexOf(ApplicationManager.Instance.CurrentProfile.Name);
            SelectedProfileItem = ApplicationManager.Instance.CurrentProfile.Name;
        }

        private Action<string> modalSubmitAction;
    }
}
