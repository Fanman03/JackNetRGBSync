using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class SourceModel
    {
        public override string ToString()
        {
            return Device?.DeviceType + " - " + Device?.Name;
        }

        [JsonIgnore]
        public string TitleOverride { get; set; }
        [JsonIgnore]
        public string DeviceType { get; set; }
        [JsonIgnore]
        public string SubTitleOverride { get; set; }

        [JsonIgnore]
        public string ChannelOverride { get; set; }

        public bool IsControllingSomething { get; set; }

        public bool IsHidden { get; set; }

        public bool HasConfig { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }

        [JsonIgnore]
        public string Controlling { get; set; }


        public string UID { get; set; }

        [JsonIgnore]
        public int ControllingModelsCount { get; set; }

        [JsonIgnore]
        public ObservableCollection<SourceControllingModel> ControllingModels { get; set; }

        public bool Enabled { get; set; }

        public BitmapImage Image { get; set; }
        public InterfaceControlDevice Device { get; set; }
        
        [JsonIgnore]
        public bool Hovered { get; set; }
    }
}