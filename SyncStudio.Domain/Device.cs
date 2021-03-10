using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class Device : BaseViewModel
    {
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

        //private string connectedTo;

        //public string ConnectedTo
        //{
        //    get => connectedTo;
        //    set => SetProperty(ref connectedTo, value);
        //}

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