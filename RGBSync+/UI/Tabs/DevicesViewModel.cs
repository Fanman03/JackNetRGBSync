using RGBSyncPlus.Model;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using RGBSyncPlus.Helper;

namespace RGBSyncPlus.UI.Tabs
{
    
    public class DevicesViewModel : LanguageAwareBaseViewModel
    {
        public DevicesViewModel()
        {
            ServiceManager.Instance.LedService.SLSDevices.CollectionChanged += (sender, args) =>
            {
                SetUpDeviceMapViewModel();
            };

            this.ZoomLevel = 4;
            SetUpDeviceMapViewModel();

            ServiceManager.Instance.ApplicationManager.LanguageChangedEvent += Instance_LanguageChangedEvent;
        }

        private void Instance_LanguageChangedEvent(object sender, EventArgs e)
        {
            this.OnPropertyChanged("SLSDevicesFiltered");
            this.OnPropertyChanged("SLSDevices");
        }

        private ObservableCollection<DeviceMappingModels.Device> slsDevices;
        public ObservableCollection<DeviceMappingModels.Device> SLSDevices
        {
            get => slsDevices;
            set
            {
                SetProperty(ref slsDevices, value);
                this.OnPropertyChanged("SLSDevicesFiltered");
            }
        }

        public ObservableCollection<DeviceMappingModels.Device> SLSDevicesFiltered
        {
            get
            {
                if (SLSDevices == null)
                {
                    return null;
                }
                return new ObservableCollection<DeviceMappingModels.Device>(SLSDevices.Where(x => x.SupportsPush || ShowSources));
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

                if (!(SLSDevices.First(x => x.Selected).ControlDevice.Driver is ISimpleLedWithConfig))
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

        private ObservableCollection<DeviceMappingModels.SourceModel> sourceDevices;

        public ObservableCollection<DeviceMappingModels.SourceModel> SourceDevices
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


            private ObservableCollection<DeviceMappingModels.SourceModel> filteredSourceDevices;

            public ObservableCollection<DeviceMappingModels.SourceModel> FilteredSourceDevices
            {
                get => filteredSourceDevices;
                set => SetProperty(ref filteredSourceDevices, value);
            }
        }

        public void FilterSourceDevices()
        {

            IEnumerable<DeviceMappingModels.SourceModel> showSourceDevices = SourceDevices.ToList();

            if (selectedSourceGroup != null && selectedSourceGroup.Name.ToLower() != "show all")
            {
                showSourceDevices = showSourceDevices.Where(sourceDevice =>
                    sourceDevice.ProviderName == SelectedSourceGroup.Name &&
                    sourceDevice.Device.DeviceType == SelectedSourceGroup.SubName);
                //FilteredSourceDevices = new ObservableCollection<DeviceMappingModels.SourceModel>(SourceDevices.Where(sourceDevice => sourceDevice.Name == SelectedSourceGroup.Name && sourceDevice.Device.DeviceType == SelectedSourceGroup.SubName));
            }


            FilteredSourceDevices = new ObservableCollection<DeviceMappingModels.SourceModel>(showSourceDevices);

        }

        private ObservableCollection<DeviceMappingModels.SourceModel> filteredSourceDevices;

        public ObservableCollection<DeviceMappingModels.SourceModel> FilteredSourceDevices
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

            var props = controlDevice.Driver.GetProperties();

            ShowOverrides = controlDevice.OverrideSupport!= null && controlDevice.OverrideSupport != OverrideSupport.None;

            if (controlDevice.OverrideSupport != null && controlDevice.OverrideSupport != OverrideSupport.None)
            {
                if (controlDevice.OverrideSupport == OverrideSupport.All)
                {
                    OverrideSpecs = new ObservableCollection<CustomDeviceSpecification>(ServiceManager.Instance.SLSManager.GetCustomDeviceSpecifications());
                }

                if (controlDevice.OverrideSupport == OverrideSupport.Self && props.GetCustomDeviceSpecifications!=null)
                {
                    OverrideSpecs = new ObservableCollection<CustomDeviceSpecification>(props.GetCustomDeviceSpecifications());
                }

                var mappers = ServiceManager.Instance.SLSManager.GetMappers(controlDevice.Driver);
                AvailableMappers = new ObservableCollection<string>();
                AvailableMappers.Add("");
                foreach (var mapper in mappers)
                {
                    Mapper mapperInstance = (Mapper)Activator.CreateInstance(mapper);
                    AvailableMappers.Add(mapperInstance.GetName());
                }
                //AvailableMappers = new ObservableCollection<string>(.Select(x=>));
            }

            IEnumerable<ControlDevice> sources = ServiceManager.Instance.LedService.SLSDevices.Where(x => x.Driver.GetProperties().IsSource || x.Driver.GetProperties().SupportsPull);

            ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings> temp = ServiceManager.Instance.ProfileService.CurrentProfile?.DeviceProfileSettings;
            DeviceMappingModels.NGDeviceProfileSettings current = null;

            if (controlDevice != null)
            {
                temp?.FirstOrDefault(x =>
                    x != null && x.Name == controlDevice.Name && x.ProviderName == controlDevice.Driver?.Name());
            }

            SourceDevices = new ObservableCollection<DeviceMappingModels.SourceModel>();

            SourceDevices.Add(new DeviceMappingModels.SourceModel
            {
                ProviderName = "",
                Device = null,
                Name = "None",
                Enabled = false,

                ConnectedTo = "",
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
                IEnumerable<DeviceMappingModels.NGDeviceProfileSettings> things = temp.Where(x =>
                    x.SourceName == source.Name && x.SourceProviderName == source.Driver.Name() &&
                    x.SourceConnectedTo == source.ConnectedTo);

                current = things.FirstOrDefault(x => x.SourceName == source.Driver.Name() && x.Name == source.Name && x.ConnectedTo == source.ConnectedTo);
                bool enabled = current != null && source.Driver.Name() == current.SourceProviderName && source.Name == current.SourceName && source.ConnectedTo == current.SourceConnectedTo;
                //  enabled = things.Any();
                SourceDevices.Add(new DeviceMappingModels.SourceModel
                {
                    HasConfig = source.Driver != null && source.Driver is ISimpleLedWithConfig,
                    DeviceType = source.DeviceType,
                    ProviderName = source.Driver.Name(),
                    Device = source,
                    Name = source.Name,
                    Enabled = enabled,
                    Image = (source.ProductImage.ToBitmapImage()),
                    ConnectedTo = source.ConnectedTo,
                    Controlling = string.Join(", ", things.Select(x => x.Name)),
                    ControllingModels = new ObservableCollection<DeviceMappingModels.SourceControllingModel>(things.Select(x => new DeviceMappingModels.SourceControllingModel
                    {
                        ProviderName = x.ProviderName,
                        ConnectedTo = x.ConnectedTo,
                        Name = x.Name,
                        IsCurrent = SLSDevicesFiltered.Any(y => y.Selected && y.Name == x.Name && y.ProviderName == x.ProviderName && x.ConnectedTo == y.ConnectedTo)
                    }).ToList()),
                    ControllingModelsCount = things.Count()
                });

                if (SourceGroups.Any(x => source.Driver.Name() == x.Name && x.SubName == source.DeviceType))
                {
                    var existing = SourceGroups.First(x => source.Driver.Name() == x.Name && x.SubName == source.DeviceType);
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
                foreach (DeviceMappingModels.SourceModel sd in FilteredSourceDevices)
                {
                    UpdateSourceDevice(sd);
                }

                OnPropertyChanged("SourceDevices");
                OnPropertyChanged("FilteredSourceDevices");
            }
        }

        public void UpdateSourceDevices()
        {
            

            foreach (DeviceMappingModels.SourceModel sd in SourceDevices)
            {
                UpdateSourceDevice(sd);
            }

            OnPropertyChanged("SourceDevices");
            OnPropertyChanged("FilteredSourceDevices");

        }

        public void UpdateSourceDevice(DeviceMappingModels.SourceModel sd)
        {
            ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings> temp = ServiceManager.Instance.ProfileService.CurrentProfile?.DeviceProfileSettings;

            IEnumerable<DeviceMappingModels.NGDeviceProfileSettings> things = temp.Where(x =>
                x.SourceName == sd.Name && x.SourceProviderName == sd.ProviderName &&
                x.SourceConnectedTo == sd.ConnectedTo);

            sd.Controlling = string.Join(", ", things.Select(x => x.Name));
            sd.ControllingModels = new ObservableCollection<DeviceMappingModels.SourceControllingModel>(things
                .Select(x => new DeviceMappingModels.SourceControllingModel
                {
                    ProviderName = x.ProviderName,
                    ConnectedTo = x.ConnectedTo,
                    Name = x.Name,
                    IsCurrent = SLSDevicesFiltered.Any(y =>
                        y.Selected && y.Name == x.Name && y.ProviderName == x.ProviderName &&
                        x.ConnectedTo == y.ConnectedTo)
                }).ToList());
            sd.ControllingModelsCount = sd.ControllingModels.Count;
            sd.IsControllingSomething = sd.ControllingModels.Any(x => x.IsCurrent);

        }

        public void SetupSourceDevices()
        {


            IEnumerable<ControlDevice> sources = ServiceManager.Instance.LedService.SLSDevices.Where(x => x.Driver.GetProperties().IsSource || x.Driver.GetProperties().SupportsPull);

            ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings> temp = ServiceManager.Instance.ProfileService.CurrentProfile?.DeviceProfileSettings;
            DeviceMappingModels.NGDeviceProfileSettings current = null;

            SourceDevices = new ObservableCollection<DeviceMappingModels.SourceModel>();

            SourceDevices.Add(new DeviceMappingModels.SourceModel
            {
                ProviderName = "",
                Device = null,
                Name = "None",
                Enabled = false,

                ConnectedTo = "",
                Controlling = "",
            });


            foreach (ControlDevice source in sources)
            {
                IEnumerable<DeviceMappingModels.NGDeviceProfileSettings> things = temp.Where(x =>
                    x.SourceName == source.Name && x.SourceProviderName == source.Driver.Name() &&
                    x.SourceConnectedTo == source.ConnectedTo);


                bool enabled = current != null && source.Driver.Name() == current.SourceProviderName && source.Name == current.SourceName && source.ConnectedTo == current.SourceConnectedTo;
                SourceDevices.Add(new DeviceMappingModels.SourceModel
                {
                    ProviderName = source.Driver.Name(),
                    Device = source,
                    Name = source.Name,
                    Enabled = enabled,
                    Image = (source.ProductImage.ToBitmapImage()),
                    ConnectedTo = source.ConnectedTo,
                    Controlling = string.Join(", ", things.Select(x => x.Name)),
                    ControllingModels = new ObservableCollection<DeviceMappingModels.SourceControllingModel>(things.Select(x => new DeviceMappingModels.SourceControllingModel
                    {
                        ProviderName = x.ProviderName,
                        ConnectedTo = x.ConnectedTo,
                        Name = x.Name,
                        IsCurrent = SLSDevicesFiltered.Any(y => y.Selected && y.Name == x.Name && y.ProviderName == x.ProviderName && x.ConnectedTo == y.ConnectedTo)
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
                ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings> temp =
                    ServiceManager.Instance.ProfileService.CurrentProfile?.DeviceProfileSettings;
                var selected = SLSDevices.Where(x => x.Selected);
                var s2 = temp.Where(x => selected.Any(y =>
                    y.ConnectedTo == x.ConnectedTo && y.Name == x.Name && y.ProviderName == x.ProviderName)).ToList();
                foreach (DeviceMappingModels.SourceModel sourceDevice in SourceDevices)
                {
                    sourceDevice.IsControllingSomething = s2.Any(x =>
                        x.SourceName == sourceDevice.Name &&
                        x.SourceProviderName == sourceDevice.ProviderName &&
                        x.SourceConnectedTo == sourceDevice.ConnectedTo);
                }

                OnPropertyChanged("SourceDevices");
            }
        }

        public void SetUpDeviceMapViewModel()
        {

            if (ServiceManager.Instance.LedService.SLSDevices != null && ServiceManager.Instance.LedService.SLSDevices.Count(x => x.Driver != null && x.Driver.GetProperties().Id != Guid.Parse("11111111-1111-1111-1111-111111111111")) == 0)
            {
                ServiceManager.Instance.ApplicationManager.NavigateToTab("store");
            }

            if (ServiceManager.Instance.ProfileService.CurrentProfile == null)
            {
                return;
            }

            ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings> temp = ServiceManager.Instance.ProfileService.CurrentProfile?.DeviceProfileSettings;

            SLSDevices = new ObservableCollection<DeviceMappingModels.Device>();
            ObservableCollection<ControlDevice> devices = ServiceManager.Instance.LedService.SLSDevices;
            foreach (ControlDevice device in devices.ToList())
            {
                try
                {
                    DeviceMappingModels.NGDeviceProfileSettings thingy = temp.FirstOrDefault(x =>
                        x.Name == device.Name && x.ConnectedTo == device.ConnectedTo &&
                        x.ProviderName == device.Driver.Name());

                    DriverProperties props = device.Driver.GetProperties();

                    var overrides = ServiceManager.Instance.LedService.GetOverride(device);

                    BitmapImage bmp = null;

                    try
                    {
                        if (overrides?.CustomDeviceSpecification?.Bitmap != null)
                        {
                            bmp = (overrides.CustomDeviceSpecification.Bitmap.ToBitmapImage());
                        }
                        else
                        {
                            bmp = (device.ProductImage.ToBitmapImage());
                        }
                    }
                    catch
                    {
                    }

                    var tmp = new DeviceMappingModels.Device
                    {
                        SunkTo = thingy?.SourceName ?? "",
                        ControlDevice = device,
                        Image = bmp,
                        Name = device.Name,
                        ProviderName = device.Driver.Name(),
                        SupportsPull = props.SupportsPull,
                        SupportsPush = props.SupportsPush,
                        DriverProps = props,
                        Title = string.IsNullOrWhiteSpace(device.TitleOverride)
                            ? device.Driver.Name()
                            : device.TitleOverride,
                        ConnectedTo = device.ConnectedTo,
                        Overrides = ServiceManager.Instance.LedService.GetOverride(device)

                    };

                    SLSDevices.Add(tmp);
                }
                catch (Exception e)
                {
                    ServiceManager.Instance.Logger.CrashWindow(e);
                }
            }

            SinkThing();
            this.OnPropertyChanged("SLSDevicesFiltered");

        }

        public void RefreshDevicesUI()
        {
            OnPropertyChanged("SLSDevices");
        }
    }
}
