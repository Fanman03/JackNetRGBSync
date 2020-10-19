using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using RGBSyncPlus.Model;
using SimpleLed;

namespace RGBSyncPlus.UI.Tabs
{
    public class DevicesViewModel : BaseViewModel
    {
        public DevicesViewModel()
        {
            ApplicationManager.Instance.SLSDevices.CollectionChanged += (sender, args) =>
            {
                SetUpDeviceMapViewModel();
            };

            this.ZoomLevel = 4;
            SetUpDeviceMapViewModel();

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
                return new ObservableCollection<DeviceMappingModels.Device>(SLSDevices.Where(x => x.SupportsPush || ShowSources));
            }
        }

        private ObservableCollection<DeviceMappingModels.DeviceMappingViewModel> deviceMappingViewModel;
        public ObservableCollection<DeviceMappingModels.DeviceMappingViewModel> DeviceMappingViewModel { get => deviceMappingViewModel; set => SetProperty(ref deviceMappingViewModel, value); }

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
                var enabled = current != null && source.Driver.Name() == current.SourceProviderName && source.Name == current.SourceName && source.ConnectedTo == current.SourceConnectedTo;
                SourceDevices.Add(new DeviceMappingModels.SourceModel
                {
                    ProviderName = source.Driver.Name(),
                    Device = source,
                    Name = source.Name,
                    Enabled = enabled,
                    Image = ToBitmapImage(source.ProductImage),
                    ConnectedTo = source.ConnectedTo,
                }); ;
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

                ShowFullThumb = ZoomLevel > 3;
            }
        }

        private bool showFullThumb;

        public bool ShowFullThumb
        {
            get => showFullThumb;
            set => SetProperty(ref showFullThumb, value);
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

        public void SetUpDeviceMapViewModel()
        {
            SLSDevices = new ObservableCollection<DeviceMappingModels.Device>();
            var devices = ApplicationManager.Instance.SLSDevices;
            foreach (ControlDevice device in devices)
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
                    Title = string.IsNullOrWhiteSpace(device.TitleOverride) ? device.Driver.Name() : device.TitleOverride,
                    ConnectedTo = device.ConnectedTo
                });
            }

            this.OnPropertyChanged("SLSDevicesFiltered");

        }

    }
}
