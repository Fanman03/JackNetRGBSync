using System.Security.AccessControl;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceProfileSettings : BaseViewModel
    {
        //[JsonIgnore]
        //public ControlDevice Device { get; set; }

        //[JsonIgnore]
        //public ControlDevice SourceDevice { get; set; }
        

        private string sourceUID;
        private string destinationDestinationUid;

        public string SourceUID
        {
            get => sourceUID;
            set => SetProperty(ref sourceUID, value);
        }

        public string DestinationUID
        {
            get => destinationDestinationUid;
            set => SetProperty(ref destinationDestinationUid, value);
        }


    }
}