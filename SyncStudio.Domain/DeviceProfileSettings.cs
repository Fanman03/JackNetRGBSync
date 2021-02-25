using System.Security.AccessControl;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceProfileSettings : BaseViewModel
    {
        [JsonIgnore]
        public ControlDevice Device { get; set; }

        [JsonIgnore]
        public ControlDevice SourceDevice { get; set; }

        //private string name;
        //public string Name
        //{
        //    get => name;
        //    set => SetProperty(ref name, value);
        //}


        //private string providerName;
        //public string ProviderName
        //{
        //    get => providerName;
        //    set => SetProperty(ref providerName, value);
        //}

        //private string connectedTo;

        //public string ConnectedTo
        //{
        //    get => connectedTo;
        //    set => SetProperty(ref connectedTo, value);
        //}




        //private string sourceName;
        //public string SourceName
        //{
        //    get => sourceName;
        //    set => SetProperty(ref sourceName, value);
        //}


        //private string sourceProviderName;
        //public string SourceProviderName
        //{
        //    get => sourceProviderName;
        //    set => SetProperty(ref sourceProviderName, value);
        //}

        //private string sourceConnectedTo;

        //public string SourceConnectedTo
        //{
        //    get => sourceConnectedTo;
        //    set => SetProperty(ref sourceConnectedTo, value);
        //}

        private string sourceUID;
        private string uid;

        public string SourceUID
        {
            get => sourceUID;
            set => SetProperty(ref sourceUID, value);
        }

        public string UID
        {
            get => uid;
            set => SetProperty(ref uid, value);
        }


    }
}