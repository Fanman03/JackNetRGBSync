using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class Device
    {
        [JsonIgnore]
        public DeviceOverrides Overrides{ get; set; }

        public bool Selected { get; set; }
        public string Name { get; set; }
        public string UID { get; set; }
        public string Title { get; set; }
        public string ProviderName { get; set; }
        public BitmapImage Image { get; set; }
        
        public InterfaceControlDevice ControlDevice { get; set; }

        public bool SupportsPush { get; set; }

        public bool SupportsPull { get; set; }

        public InterfaceDriverProperties DriverProps { get; set; }

        public string SunkTo { get; set; }
    }
}