using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SyncStudio.Domain;

namespace SyncStudio.WPF.UI.Tabs
{
    public class DeviceViewModel : BaseViewModel
    {
        public DeviceViewModel(Device device)
        {
            this.Name = device.Name;
            this.ControlDevice = device.ControlDevice;
            this.DriverProps = device.DriverProps;
            this.Image = device.Image;
            this.Overrides = device.Overrides;
            this.ProviderName = device.ProviderName;
            this.Selected = device.Selected;
            this.SunkTo = device.SunkTo;
            this.SupportsPull = device.SupportsPull;
            this.UID = device.UID;
            this.SupportsPush = device.SupportsPush;
            this.Title = device.Title;
        }
        private DeviceOverrides overrides;

        [JsonIgnore]
        public DeviceOverrides Overrides
        {
            get => overrides;
            set => SetProperty(ref overrides, value);
        }

        private bool selected;

        public bool Selected
        {
            get => selected;
            set => SetProperty(ref selected, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }
        
        private string uid;
        public string UID
        {
            get => uid;
            set => SetProperty(ref uid, value);
        }

        private string title;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private string providerName;
        public string ProviderName
        {
            get => providerName;
            set => SetProperty(ref providerName, value);
        }

        private BitmapImage image;

        public BitmapImage Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }

        private InterfaceControlDevice controlDevice;

        public InterfaceControlDevice ControlDevice
        {
            get => controlDevice;
            set => SetProperty(ref controlDevice, value);
        }

        private bool supportsPush;

        public bool SupportsPush
        {
            get => supportsPush;
            set => SetProperty(ref supportsPush, value);
        }


        private bool supportsPull;

        public bool SupportsPull
        {
            get => supportsPull;
            set => SetProperty(ref supportsPull, value);
        }

        private InterfaceDriverProperties driverProps;

        public InterfaceDriverProperties DriverProps
        {
            get => driverProps;
            set => SetProperty(ref driverProps, value);
        }

        private string sunkTo;

        public string SunkTo
        {
            get => sunkTo;
            set => SetProperty(ref sunkTo, value);
        }
    }
}
