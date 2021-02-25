using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class Profile : BaseViewModel
    {

        private ColorProfile colorProfile;

        [JsonIgnore]
        public ColorProfile LoadedColorProfile
        {
            get => colorProfile;
            set { SetProperty(ref colorProfile, value); }
        }
        private Guid? colorProfileId;

        public Guid? ColorProfileId
        {
            get => colorProfileId;
            set
            {
                SetProperty(ref colorProfileId, value);
                IsProfileStale = true;
            }
        }

        [JsonIgnore]
        public bool IsProfileStale { get; set; }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
                IsProfileStale = true;
            }
        }

        private ObservableCollection<DeviceProfileSettings> deviceProfileSettings = new ObservableCollection<DeviceProfileSettings>();

        public ObservableCollection<DeviceProfileSettings> DeviceProfileSettings
        {
            get => deviceProfileSettings;
            set
            {
                SetProperty(ref deviceProfileSettings, value);
                IsProfileStale = true;
            }
        }

        public Guid Id { get; set; }
    }
}
