using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class SourceModel : BaseViewModel
    {
        public override string ToString()
        {
            return Device?.DeviceType + " - " + Device?.Name;
        }

        private string titleOverride;
        private string subTitleOverride;
        private string channelOverride;
        private string deviceType;
        [JsonIgnore]
        public string TitleOverride
        {
            get => titleOverride;
            set => SetProperty(ref titleOverride, value);
        }

        [JsonIgnore]
        public string DeviceType
        {
            get => deviceType;
            set => SetProperty(ref deviceType, value);
        }

        [JsonIgnore]
        public string SubTitleOverride
        {
            get => subTitleOverride;
            set => SetProperty(ref subTitleOverride, value);
        }

        [JsonIgnore]
        public string ChannelOverride
        {
            get => channelOverride;
            set => SetProperty(ref channelOverride, value);
        }

        private bool isControllingSomething;

        public bool IsControllingSomething
        {
            get => isControllingSomething;
            set => SetProperty(ref isControllingSomething, value);
        }


        private bool isHidden;

        public bool IsHidden
        {
            get => isHidden;
            set => SetProperty(ref isHidden, value);
        }


        private bool hasConfig;

        public bool HasConfig
        {
            get => hasConfig;
            set => SetProperty(ref hasConfig, value);
        }


        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }


        private string providerName;
        public string ProviderName
        {
            get => providerName;
            set => SetProperty(ref providerName, value);
        }

        //private string connectedTo;

        //public string ConnectedTo
        //{
        //    get => connectedTo;
        //    set => SetProperty(ref connectedTo, value);
        //}

        private string controlling;

        [JsonIgnore]
        public string Controlling
        {
            get => controlling;
            set => SetProperty(ref controlling, value);
        }

        private string uid;

        
        public string UID
        {
            get => uid;
            set => SetProperty(ref uid, value);
        }

        private int controllingModelsCount;

        [JsonIgnore]
        public int ControllingModelsCount
        {
            get => controllingModelsCount;
            set => SetProperty(ref controllingModelsCount, value);
        }

        private ObservableCollection<SourceControllingModel> controllingModels;

        [JsonIgnore]
        public ObservableCollection<SourceControllingModel> ControllingModels
        {
            get => controllingModels;
            set
            {
                SetProperty(ref controllingModels, value);
                //     ControllingModelsCount = controllingModels.Count;
            }
        }

        private bool enabled;

        public bool Enabled
        {
            get => enabled;
            set
            {
                SetProperty(ref enabled, value);
            }
        }

        private BitmapImage image;

        public BitmapImage Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }

        public ControlDevice Device { get; set; }

        private bool hovered;
        [JsonIgnore]
        public bool Hovered { get => hovered; set => SetProperty(ref hovered, value); }
    }
}