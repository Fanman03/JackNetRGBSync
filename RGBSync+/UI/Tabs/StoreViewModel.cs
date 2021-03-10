using SyncStudio.WPF.Model;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autofac;
using SyncStudio.ClientService;
using SyncStudio.WPF.Helper;

namespace SyncStudio.WPF.UI.Tabs
{
    public class StoreViewModel : TabViewModel
    {
        private ClientService.Settings settings = new Settings();
        private StoreHandler storeHandler;
        private ClientService.Devices _devices;
        public StoreViewModel(ClientService.Devices devices)
        {
            _devices = devices;
        }

        public StoreViewModel()
        {
            _devices = ServiceManager.Container.Resolve<ClientService.Devices>();
        }

        public async Task InitAsync()
        {
            ShowPreRelease = settings.Experimental;

            storeHandler = new StoreHandler();

            await LoadStoreAndPlugins();

            ShowInstalled = true;
            ShowStore = false;
            ShowUpdates = false;
            if (_devices == null)
            {
                return;
            }

            var devices = await _devices.GetDevicesAsync();
            if (devices != null && devices.Count(x => x.InterfaceDriverProperties != null && !x.InterfaceDriverProperties.ProductId.ToString().StartsWith("1111")) == 0)
            {
                ShowInstalled = false;
                ShowStore = true;
                ShowUpdates = false;
            }

        }

        private MainWindowViewModel mainVm => (ServiceManager.Instance.ApplicationManager.MainWindow?.DataContext) as MainWindowViewModel;

        private bool showPreRelease = false;

        public bool ShowPreRelease
        {
            get => showPreRelease;
            set
            {
                SetProperty(ref showPreRelease, value);
                FilterPlugins();
                OnPropertyChanged("FilteredPlugins");
            }
        }

        private bool showConfig = false;

        public bool ShowConfig
        {
            get => showConfig;
            set => SetProperty(ref showConfig, value);
        }

        private bool showUpdates = false;

        public bool ShowUpdates
        {
            get => showUpdates;
            set
            {
                SetProperty(ref showUpdates, value);
                FilterPlugins();
                OnPropertyChanged("FilteredPlugins");
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
                OnPropertyChanged("FilteredPlugins");
            }
        }


        private ObservableCollection<PositionalAssignment.PluginDetailsViewModel> plugins;
        public ObservableCollection<PositionalAssignment.PluginDetailsViewModel> Plugins
        {
            get => plugins;
            set
            {
                SetProperty(ref plugins, value);
                OnPropertyChanged("FilteredPlugins");
            }
        }

        private readonly ObservableCollection<PositionalAssignment.PluginDetailsViewModel> filteredPlugins;
        public ObservableCollection<PositionalAssignment.PluginDetailsViewModel> FilteredPlugins
        {
            get
            {
                if (Plugins == null)
                {
                    return null;
                }

                if (ShowUpdates)
                {
                    return new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>(Plugins.Where(x => x.InstalledButOutdated && !x.Id.ToString().StartsWith("11111")));
                }
                else
                {
                    if (ShowStore)
                    {
                        return new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>(Plugins.Where(x => !x.Id.ToString().StartsWith("11111")));
                    }
                    else
                    {
                        return new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>(Plugins.Where(x => x.Installed && !x.Id.ToString().StartsWith("11111")));
                    }
                }
            }
        }


        private ObservableCollection<PositionalAssignment.PluginDetailsViewModel> installedPlugins;
        
        private bool showStore;

        public bool ShowStore
        {
            get => showStore;
            set
            {
                SetProperty(ref showStore, value);
                OnPropertyChanged("FilteredPlugins");
            }
        }

        private bool showInstalled;

        public bool ShowInstalled
        {
            get => showInstalled;
            set
            {
                SetProperty(ref showInstalled, value);
                OnPropertyChanged("FilteredPlugins");
            }
        }

        public async Task LoadStoreAndPlugins()
        {
            //todo
            return;
            //using (new SimpleModal(mainVm, "Refreshing store...."))
            //{
            //    Plugins = new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>();


            //    var storeProviders = (await SyncStudio.Core.ServiceManager.Store.GetStoreProviders()).ToList();
            //    var installedProviders = SyncStudio.Core.ServiceManager.Store.GetInstalledProviders().ToList();


            //    var uniqueStoreProviders = storeProviders.GroupBy(x => x.ProviderId).Select(y => y.OrderByDescending(p => p.Version).First()).ToList();

            //    Plugins = new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>(uniqueStoreProviders.Select(slsManagerDriver =>
            //        new PositionalAssignment.PluginDetailsViewModel
            //        {
            //            Author = slsManagerDriver.Author,
            //            Blurb = slsManagerDriver.Blurb,
            //            Id = slsManagerDriver.InstanceId.ToString(),
            //            Image = slsManagerDriver.Image,
            //            Installed = false,
            //            Name = slsManagerDriver.Name,
            //            PluginId = slsManagerDriver.ProviderId,
            //            Version = slsManagerDriver.Version.ToString(),
            //            Visible = true,
            //            PluginDetails = new PositionalAssignment.PluginDetails
            //            {
            //                Version = slsManagerDriver.Version,
            //                Id = slsManagerDriver.InstanceId.ToString(),
            //                PluginId = slsManagerDriver.ProviderId,
            //                // DriverProperties = pid,
            //                Author = slsManagerDriver.Author,
            //                Name = slsManagerDriver.Name,
            //                //Repo = pid.GitHubLink
            //            },
            //            VersionsAvailable = new ObservableCollection<PositionalAssignment.PluginVersionDetails>(
            //            storeProviders
            //                .Where(x => x.ProviderId == slsManagerDriver.ProviderId)
            //                .Select(g =>
            //                    new PositionalAssignment.PluginVersionDetails
            //                    {
            //                        IsExperimental = !slsManagerDriver.IsPublicRelease,
            //                        IsInstalled = installedProviders.Any(i => i.ProviderId == slsManagerDriver.ProviderId),
            //                        ReleaseNumber = g.Version
            //                    }
            //                ).OrderByDescending(o => o.ReleaseNumber).ToList())

            //        }));
                
            //    var localOnly = installedProviders.Where(x => Plugins.All(p => p.PluginId != x.ProviderId));

            //    foreach (ProviderInfo slsManagerDriver in localOnly)
            //    {
            //        Plugins.Add(new PositionalAssignment.PluginDetailsViewModel
            //        {
            //            Author = slsManagerDriver.Author,
            //            Blurb = slsManagerDriver.Blurb,
            //            Id = slsManagerDriver.InstanceId.ToString(),
            //            Installed = true,
            //            Name = slsManagerDriver.Name,
            //            PluginId = slsManagerDriver.ProviderId,
            //            Version = slsManagerDriver.Version.ToString(),
            //            Visible = true,
            //            PluginDetails = new PositionalAssignment.PluginDetails
            //            {
            //                Version = slsManagerDriver.Version,
            //                Id = slsManagerDriver.InstanceId.ToString(),
            //                PluginId = slsManagerDriver.ProviderId,
            //                // DriverProperties = pid,
            //                Author = slsManagerDriver.Author,
            //                Name = slsManagerDriver.Name,
            //                //Repo = pid.GitHubLink
            //            }
            //        });
            //    }

            //    foreach (ProviderInfo installedProvider in installedProviders)
            //    {
            //        var existing = Plugins.First(x => x.PluginId == installedProvider.ProviderId);
            //        existing.Version = installedProvider.Version.ToString();
            //        existing.Installed = true;
            //        existing.InstalledButOutdated = existing.VersionsAvailable.Any(x => x.ReleaseNumber > installedProvider.Version);
            //        existing.InstalledVersionModel = existing.VersionsAvailable.FirstOrDefault(x => x.ReleaseNumber.ToString() == installedProvider.Version.ToString());
            //        if (existing.InstalledVersionModel != null)
            //        {
            //            existing.InstalledVersionModel.IsInstalled = true;
            //        }
            //    }
                
            //    FilterPlugins();

            //}

            //OnPropertyChanged("FilteredPlugins");
        }

        public void FilterPlugins()
        {
            if (Plugins == null) return;

            IEnumerable<IGrouping<Guid, PositionalAssignment.PluginDetailsViewModel>> groupedPlugins = Plugins.GroupBy(x => x.PluginId);


            foreach (PositionalAssignment.PluginDetailsViewModel pluginDetailsViewModel in Plugins)
            {
                ReleaseNumber newestPublicFound = pluginDetailsViewModel.VersionsAvailable.Where(x => !x.IsExperimental).OrderByDescending(t => t.ReleaseNumber).FirstOrDefault()?.ReleaseNumber;
                ReleaseNumber newestExperimentalFound = pluginDetailsViewModel.VersionsAvailable.Where(x => x.IsExperimental).OrderByDescending(t => t.ReleaseNumber).FirstOrDefault()?.ReleaseNumber;

                ReleaseNumber versionToShow = newestPublicFound;
                if (ShowPreRelease)
                {
                    versionToShow = newestExperimentalFound;
                }

                if (pluginDetailsViewModel.Installed)
                {
                    versionToShow = pluginDetailsViewModel.PluginDetails.Version;
                }
                
                if (versionToShow != null)
                {
                    pluginDetailsViewModel.Version = versionToShow.ToString();
                }
            }

            OnPropertyChanged("Plugins");
            OnPropertyChanged("FilteredPlugins");
        }


        public void RefreshPlungs()
        {
            OnPropertyChanged("FilteredPlugins");
            OnPropertyChanged("Plugins");
        }
    }
}
