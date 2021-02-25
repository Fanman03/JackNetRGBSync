using SyncStudio.WPF.Helper;
using SyncStudio.WPF.Model;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using SyncStudio.Domain;

namespace SyncStudio.WPF.UI.Tabs
{

    public class DevicesViewModel : LanguageAwareBaseViewModel
    {
        private bool showConfig = false;

        public bool ShowConfig
        {
            get => showConfig;
            set => SetProperty(ref showConfig, value);
        }

        public DevicesViewModel()
        {
            this.ZoomLevel = 4;
            SetUpDeviceMapViewModel();

            ServiceManager.Instance.ApplicationManager.LanguageChangedEvent += Instance_LanguageChangedEvent;
            SyncStudio.Core.ServiceManager.Devices.DeviceAdded += Devices_DeviceAdded;
            SyncStudio.Core.ServiceManager.Devices.DeviceRemoved += Devices_DeviceRemoved;

            SLSDevices = new ObservableCollection<Device>(SyncStudio.Core.ServiceManager.Devices.GetDevices().Select(GetDevice));
        }

        private void Devices_DeviceRemoved(object sender, Events.DeviceChangeEventArgs e)
        {
            var rm = SLSDevices.FirstOrDefault(x => x.UID == e.ControlDevice.UniqueIdentifier);

            if (rm != null)
            {
                SLSDevices.Remove(rm);
                this.OnPropertyChanged("SLSDevicesFiltered");
                this.OnPropertyChanged("SLSDevices");
            }
        }

        private Device GetDevice(ControlDevice p)
        {
            var pp = p.Driver.GetProperties();

            return new Device()
            {
                ControlDevice = p,
                Name = p.Name,
                UID = p.UniqueIdentifier,
                DriverProps = p.Driver.GetProperties(),
                Image = p.ProductImage.ToBitmapImage(),
                Overrides = SyncStudio.Core.ServiceManager.Devices.GetOverride(p),
                ProviderName = p.Driver.Name(),
                Title = p.TitleOverride,
                SupportsPull = pp.SupportsPull,
                SupportsPush = pp.SupportsPush,

            };
        }

        private void Devices_DeviceAdded(object sender, Events.DeviceChangeEventArgs e)
        {
            SLSDevices.Add(GetDevice(e.ControlDevice));
            this.OnPropertyChanged("SLSDevicesFiltered");
            this.OnPropertyChanged("SLSDevices");
        }

        private void Instance_LanguageChangedEvent(object sender, EventArgs e)
        {
            this.OnPropertyChanged("SLSDevicesFiltered");
            this.OnPropertyChanged("SLSDevices");
        }

        private ObservableCollection<Device> slsDevices;
        public ObservableCollection<Device> SLSDevices
        {
            get => slsDevices;
            set
            {
                SetProperty(ref slsDevices, value);
                this.OnPropertyChanged("SLSDevicesFiltered");
            }
        }

        public ObservableCollection<Device> SLSDevicesFiltered
        {
            get
            {
                if (SLSDevices == null)
                {
                    return null;
                }
                return new ObservableCollection<Device>(SLSDevices.Where(x => x.SupportsPush || ShowSources));
            }
        }


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

                AnyDevicesSelected = value != 0;
                SingledDeviceSelected = value == 1;
                MultipleDeviceSelected = value > 1;

                ShowConfigTab = true;
                if (MultipleDeviceSelected && (SubViewMode == "Config" || SubViewMode == "Alignment"))
                {
                    SubViewMode = "Info";
                    ShowConfigTab = false;
                }
                else
                {
                    if (SubViewMode == "Config")
                    {
                        SubViewMode = "Info";
                    }

                }

                if (!(SLSDevices.FirstOrDefault(x => x.Selected)?.ControlDevice.Driver is ISimpleLedWithConfig))
                {
                    ShowConfigTab = false;
                }

            }
        }

        private bool showConfigTab;

        public bool ShowConfigTab
        {
            get => showConfigTab;
            set => SetProperty(ref showConfigTab, value);
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

        private string subViewMode = "SyncTo";

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

        private ObservableCollection<SourceModel> sourceDevices;

        public ObservableCollection<SourceModel> SourceDevices
        {
            get => sourceDevices;
            set => SetProperty(ref sourceDevices, value);
        }

        private ObservableCollection<SourceGroup> sourceGroups;

        public ObservableCollection<SourceGroup> SourceGroups
        {
            get => sourceGroups;
            set => SetProperty(ref sourceGroups, value);
        }

        private SourceGroup selectedSourceGroup;

        public SourceGroup SelectedSourceGroup
        {
            get => selectedSourceGroup;
            set => SetProperty(ref selectedSourceGroup, value);
        }

        public class SourceGroup : BaseViewModel
        {
            private string name;

            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }

            private string subName;

            public string SubName
            {
                get => subName;
                set => SetProperty(ref subName, value);
            }

            private BitmapImage image;
            public BitmapImage Image
            {
                get => image;
                set => SetProperty(ref image, value);
            }

            private int plugins;

            public int Plugins
            {
                get => plugins;
                set => SetProperty(ref plugins, value);
            }

            private bool selected;

            public bool Selected
            {
                get => selected;
                set => SetProperty(ref selected, value);
            }


            private ObservableCollection<SourceModel> filteredSourceDevices;

            public ObservableCollection<SourceModel> FilteredSourceDevices
            {
                get => filteredSourceDevices;
                set => SetProperty(ref filteredSourceDevices, value);
            }
        }

        public void FilterSourceDevices()
        {

            IEnumerable<SourceModel> showSourceDevices = SourceDevices.ToList();

            if (selectedSourceGroup != null && selectedSourceGroup.Name.ToLower() != "show all")
            {
                showSourceDevices = showSourceDevices.Where(sourceDevice =>
                    sourceDevice.ProviderName == SelectedSourceGroup.Name &&
                    sourceDevice.Device.DeviceType == SelectedSourceGroup.SubName);
            }

            FilteredSourceDevices = new ObservableCollection<SourceModel>(showSourceDevices);

        }

        private ObservableCollection<SourceModel> filteredSourceDevices;

        public ObservableCollection<SourceModel> FilteredSourceDevices
        {
            get => filteredSourceDevices;
            set => SetProperty(ref filteredSourceDevices, value);
        }

        private ObservableCollection<CustomDeviceSpecification> overrideSpecs = new ObservableCollection<CustomDeviceSpecification>();

        public ObservableCollection<CustomDeviceSpecification> OverrideSpecs
        {
            get => overrideSpecs;
            set => SetProperty(ref overrideSpecs, value);
        }

        private bool showOverrides = false;

        public bool ShowOverrides
        {
            get => showOverrides;
            set => SetProperty(ref showOverrides, value);
        }
        public void SetupSourceDevices(ControlDevice controlDevice)
        {
            if (controlDevice == null) return;

            DriverProperties props = controlDevice.Driver.GetProperties();

            ShowOverrides = controlDevice.OverrideSupport != OverrideSupport.None;

            if (controlDevice.OverrideSupport != OverrideSupport.None)
            {
                if (controlDevice.OverrideSupport == OverrideSupport.All)
                {
                    OverrideSpecs = new ObservableCollection<CustomDeviceSpecification>(ServiceManager.Instance.SLSManager.GetCustomDeviceSpecifications());
                }

                if (controlDevice.OverrideSupport == OverrideSupport.Self && props.GetCustomDeviceSpecifications != null)
                {
                    OverrideSpecs = new ObservableCollection<CustomDeviceSpecification>(props.GetCustomDeviceSpecifications());
                }

                List<Type> mappers = ServiceManager.Instance.SLSManager.GetMappers(controlDevice.Driver);
                AvailableMappers = new ObservableCollection<string>();
                AvailableMappers.Add("");
                foreach (Type mapper in mappers)
                {
                    Mapper mapperInstance = (Mapper)Activator.CreateInstance(mapper);
                    AvailableMappers.Add(mapperInstance.GetName());
                }
            }

            IEnumerable<ControlDevice> sources = SyncStudio.Core.ServiceManager.Devices.GetDevices().Where(x => x.Driver.GetProperties().IsSource || x.Driver.GetProperties().SupportsPull);

            ObservableCollection<DeviceProfileSettings> temp = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile()?.DeviceProfileSettings;
            DeviceProfileSettings current = null;

            if (controlDevice != null)
            {
                temp?.FirstOrDefault(x => x.UID == controlDevice.UniqueIdentifier);
            }

            SourceDevices = new ObservableCollection<SourceModel>();

            SourceDevices.Add(new SourceModel
            {
                ProviderName = "",
                Device = null,
                Name = "None",
                Enabled = false,

                
                Controlling = "",
            });

            SourceGroups = new ObservableCollection<SourceGroup>();
            SourceGroups.Add(new SourceGroup
            {
                Plugins = 1,
                Name = "Show All",

            });

            if (SelectedSourceGroup == null)
            {
                SelectedSourceGroup = SourceGroups.First();
            }

            foreach (ControlDevice source in sources)
            {
                IEnumerable<DeviceProfileSettings> things = temp.Where(x => x.SourceUID == source.UniqueIdentifier);

                var DeviceProfileSettingsEnumerable = things as DeviceProfileSettings[] ?? things.ToArray();
                current = DeviceProfileSettingsEnumerable.FirstOrDefault(x => x.SourceUID == source.UniqueIdentifier);
                bool enabled = current != null &&  current.SourceUID == source.UniqueIdentifier;
                //  enabled = things.Any();
                SourceDevices.Add(new SourceModel
                {
                    HasConfig = source.Driver is ISimpleLedWithConfig,
                    DeviceType = source.DeviceType,
                    ProviderName = source.Driver.Name(),
                    Device = source,
                    Name = source.Name,
                    Enabled = enabled,
                    Image = (source.ProductImage.ToBitmapImage()),
                    
                    //Controlling = string.Join(", ", DeviceProfileSettingsEnumerable.Select(x => x.Name)),
                    ControllingModels = new ObservableCollection<SourceControllingModel>(DeviceProfileSettingsEnumerable.Select(x => new SourceControllingModel
                    {
                        ProviderName = SLSDevices.First(p=>p.UID==x.UID).ProviderName,
                        
                        Name = SLSDevices.First(p => p.UID == x.UID).Name,
                        IsCurrent = SLSDevicesFiltered.Any(y => y.Selected && y.UID == x.UID )
                    }).ToList()),
                    ControllingModelsCount = DeviceProfileSettingsEnumerable.Count()
                });

                if (SourceGroups.Any(x => source.Driver.Name() == x.Name && x.SubName == source.DeviceType))
                {
                    SourceGroup existing = SourceGroups.First(x => source.Driver.Name() == x.Name && x.SubName == source.DeviceType);
                    existing.Plugins++;
                }
                else
                {
                    SourceGroups.Add(new SourceGroup
                    {
                        Plugins = 1,
                        Name = source.Driver.Name(),
                        SubName = source.DeviceType,
                        Image = (source.ProductImage.ToBitmapImage())
                    });
                }
            }

            if (SelectedSourceGroup == null)
            {
                SelectedSourceGroup = SourceGroups.First(x => x.Name.ToLower() == "show all");
            }

            FilterSourceDevices();

            foreach (DevicesViewModel.SourceGroup vmSourceGroup in SourceGroups)
            {
                vmSourceGroup.Selected = vmSourceGroup == SelectedSourceGroup;
            }
        }

        public void UpdateFilteredSourceDevices()
        {

            if (FilteredSourceDevices != null)
            {
                foreach (SourceModel sd in FilteredSourceDevices)
                {
                    UpdateSourceDevice(sd);
                }

                OnPropertyChanged("SourceDevices");
                OnPropertyChanged("FilteredSourceDevices");
            }
        }

        public void UpdateSourceDevices()
        {


            foreach (SourceModel sd in SourceDevices)
            {
                UpdateSourceDevice(sd);
            }

            OnPropertyChanged("SourceDevices");
            OnPropertyChanged("FilteredSourceDevices");

        }

        public void UpdateSourceDevice(SourceModel sd)
        {
            ObservableCollection<DeviceProfileSettings> temp = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile()?.DeviceProfileSettings;

            IEnumerable<DeviceProfileSettings> things = temp.Where(x => x.SourceUID == sd.UID);

            if (sd.UID != null)
            {
                sd.ControllingModels = new ObservableCollection<SourceControllingModel>(things
                    .Select(x => new SourceControllingModel
                    {
                        ProviderName = SLSDevices.First(p => p.UID == x.UID).ProviderName,

                        Name = SLSDevices.First(p => p.UID == x.UID).Name,
                        IsCurrent = SLSDevicesFiltered.Any(y =>
                            y.Selected && y.UID == x.UID)
                    }).ToList());
                sd.ControllingModelsCount = sd.ControllingModels.Count;
                sd.IsControllingSomething = sd.ControllingModels.Any(x => x.IsCurrent);
            }
        }

        public void SetupSourceDevices()
        {


            IEnumerable<ControlDevice> sources = SyncStudio.Core.ServiceManager.Devices.GetDevices().Where(x => x.Driver.GetProperties().IsSource || x.Driver.GetProperties().SupportsPull);

            ObservableCollection<DeviceProfileSettings> temp = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile()?.DeviceProfileSettings;
            DeviceProfileSettings current = null;

            SourceDevices = new ObservableCollection<SourceModel>();

            SourceDevices.Add(new SourceModel
            {
                ProviderName = "",
                Device = null,
                Name = "None",
                Enabled = false,
                
                Controlling = "",
            });


            foreach (ControlDevice source in sources)
            {
                IEnumerable<DeviceProfileSettings> things = temp.Where(x => x.SourceUID == source.UniqueIdentifier);



                bool enabled = current != null && current.SourceUID == source.UniqueIdentifier;
                SourceDevices.Add(new SourceModel
                {
                    ProviderName = source.Driver.Name(),
                    Device = source,
                    Name = source.Name,
                    Enabled = enabled,
                    Image = (source.ProductImage.ToBitmapImage()),
                    
                    ControllingModels = new ObservableCollection<SourceControllingModel>(things.Select(x => new SourceControllingModel
                    {
                        ProviderName = SLSDevices.First(p => p.UID == x.UID).ProviderName,
                        Name = SLSDevices.First(p => p.UID == x.UID).Name,
                        IsCurrent = SLSDevicesFiltered.Any(y => y.Selected && y.UID == x.UID)
                    }).ToList())
                });
            }

            OnPropertyChanged("SourceDevices");
            //SinkThing();
        }






        private int thumbWidth = 192;

        public int ThumbWidth
        {
            get => thumbWidth;
            set
            {
                SetProperty(ref thumbWidth, value);
            }
        }

        private int thumbHeight = 144;

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
                if (ZoomLevel > 5)
                {
                    ThumbHeight = (int)((new[] { 16, 32, 64, 128, 192, 256, 385, 512, 768, 1024, 2048, 4096 }[ZoomLevel - 1]) / 1.3333333333333333f);
                }

                ShowFullThumb = ZoomLevel > 4;
            }
        }

        private bool showFullThumb;

        public bool ShowFullThumb
        {
            get => showFullThumb;
            set => SetProperty(ref showFullThumb, value);
        }



        private bool alignDevice;

        public bool AlignDevice
        {
            get => alignDevice;
            set => SetProperty(ref alignDevice, value);
        }

        private Device singleSelectedSourceControlDevice;

        public Device SingleSelectedSourceControlDevice
        {
            get => singleSelectedSourceControlDevice;
            set => SetProperty(ref singleSelectedSourceControlDevice, value);
        }

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

        private bool anyDevicesSelected;

        public bool AnyDevicesSelected
        {
            get => anyDevicesSelected;
            set => SetProperty(ref anyDevicesSelected, value);
        }

        private bool showSources;

        public bool ShowSources
        {
            get => showSources;
            set
            {
                SetProperty(ref showSources, value);
                this.OnPropertyChanged("SLSDevicesFiltered");
            }
        }

        private ObservableCollection<string> availableMappers = new ObservableCollection<string>();

        public ObservableCollection<string> AvailableMappers
        {
            get => availableMappers;
            set => SetProperty(ref availableMappers, value);
        }

        public void SinkThing()
        {
            if (SourceDevices != null)
            {
                ObservableCollection<DeviceProfileSettings> temp = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile()?.DeviceProfileSettings;
                IEnumerable<Device> selected = SLSDevices.Where(x => x.Selected);
                List<DeviceProfileSettings> s2 = temp.Where(x => selected.Any(y => y.UID == x.UID)).ToList();

                foreach (SourceModel sourceDevice in SourceDevices)
                {
                    sourceDevice.IsControllingSomething = s2.Any(x =>
                        x.SourceUID == sourceDevice.UID);
                }

                OnPropertyChanged("SourceDevices");
            }
        }

        public void SetUpDeviceMapViewModel()
        {

            if (SyncStudio.Core.ServiceManager.Devices.GetDevices() != null && SyncStudio.Core.ServiceManager.Devices.GetDevices().Count(x => x.Driver != null && x.Driver.GetProperties().ProductId != Guid.Parse("11111111-1111-1111-1111-111111111111")) == 0)
            {
                ServiceManager.Instance.ApplicationManager.NavigateToTab("store");
            }

            if (SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile() == null)
            {
                return;
            }

            SLSDevices = new ObservableCollection<Device>(SyncStudio.Core.ServiceManager.Devices.GetDevices().Select(p => new Device()
            {
                UID = p.UniqueIdentifier,
                ControlDevice = p,
                Name = p.Name,
                DriverProps = p.Driver.GetProperties(),
                Image = p.ProductImage.ToBitmapImage(),
                Overrides = SyncStudio.Core.ServiceManager.Devices.GetOverride(p),
                ProviderName = p.Driver.Name(),
                Title = p.TitleOverride
            }));

            SinkThing();
            this.OnPropertyChanged("SLSDevicesFiltered");

        }

        public void RefreshDevicesUI()
        {
            OnPropertyChanged("SLSDevices");
        }
    }
}
