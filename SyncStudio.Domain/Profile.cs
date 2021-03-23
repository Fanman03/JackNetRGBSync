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
    public class Profile
    {
        [JsonIgnore]
        public ColorProfile LoadedColorProfile { get; set; }
        
        public Guid? ColorProfileId { get; set; }

        [JsonIgnore]
        public bool IsProfileStale { get; set; }
        
        public string Name { get; set; }

        public ObservableCollection<DeviceProfileSettings> DeviceProfileSettings { get; set; }

        public Guid Id { get; set; }
    }
}
