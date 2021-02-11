using RGBSyncStudio.Model;
using SimpleLed;
using System.Collections.ObjectModel;

namespace RGBSyncStudio
{
    public class DriverService : BaseViewModel
    {

        private ObservableCollection<DeviceMappingModels.Device> slsDevices;
        public ObservableCollection<DeviceMappingModels.Device> SLSDevices
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
