using System.Security.AccessControl;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceProfileSettings : BaseViewModel
    {
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