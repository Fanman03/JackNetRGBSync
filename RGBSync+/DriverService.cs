using SyncStudio.WPF.Model;
using SimpleLed;
using System.Collections.ObjectModel;
using SyncStudio.Domain;
using SyncStudio.WPF.UI;

namespace SyncStudio.WPF
{
    public class DriverService : BaseViewModel
    {

        private ObservableCollection<Device> slsDevices;
        public ObservableCollection<Device> SLSDevices
        {
            get => slsDevices;
            set
            {
                SetProperty(ref slsDevices, value);
                this.OnPropertyChanged("SLSDevicesFiltered");
            }
        }

    }
}
