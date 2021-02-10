using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.Model;
using SimpleLed;

namespace RGBSyncPlus
{
   public  class DriverService : BaseViewModel
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
