using System;
using System.Collections.ObjectModel;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceMappingViewModel
        {
            public DeviceMappingViewModel()
            {
                DestinationDevices.CollectionChanged += DestinationDevices_CollectionChanged;
            }

            private void DestinationDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {

                SyncBack?.Invoke(this);
            }

            public Action<object> SyncBack;

            public string ProviderName { get; set; }
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
            public ObservableCollection<DeviceMappingItemViewModel> DestinationDevices { get; set; } = new ObservableCollection<DeviceMappingItemViewModel>();
            public bool expanded;
            public bool Expanded
            {
                get => expanded;
                set
                {
                    expanded = value;
                    SyncBack?.Invoke(this);
                }
            }

            public Guid Id { get; set; }
        }
}

