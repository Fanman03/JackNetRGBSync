using System;
using SimpleLed;

namespace SyncStudio.Domain
{
    //todo - will this survive?
    public class DeviceMappingItemViewModel
    {
        public Action<object> SyncBack;
        public ControlDevice DestinationDevice { get; set; }
        private bool enabled;

        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                SyncBack?.Invoke(this);
            }
        }

        public Guid ParentId { get; set; }
    }
}