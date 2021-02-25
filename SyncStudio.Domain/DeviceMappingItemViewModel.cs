using System;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceMappingItemViewModel : BaseViewModel
    {
        public Action<object> SyncBack;
        public ControlDevice DestinationDevice { get; set; }
        private bool enabled;

        public bool Enabled
        {
            get => enabled;
            set
            {
                SetProperty(ref enabled, value);
                SyncBack?.Invoke(this);
            }
        }

        public Guid ParentId { get; set; }
    }
}