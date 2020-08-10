using RGB.NET.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBSyncPlus.Model
{
    public class DeviceGroup : AbstractBindable
    {
        //ohgod so hacky :(
        public Action SyncBack;
        private bool suspendRollUp=false;
        private IRGBDevice rgbDevice;
        public IRGBDevice RGBDevice
        {
            get => rgbDevice;
            set => SetProperty(ref rgbDevice, value);
        }
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private bool expanded = true;
        public bool Expanded
        {
            get => expanded;
            set => SetProperty(ref expanded, value);
        }

        private bool allSelectedIndeterminate = false;
        public bool AllSelectedIndeterminate
        {
            get => expanded;
            set => SetProperty(ref allSelectedIndeterminate, value);
        }


        private bool allSelected;

        public bool AllSelected
        {
            get => DeviceLeds.All(x => x.IsSelected);
            set
            {
                suspendRollUp = true;
                foreach (var deviceLed in DeviceLeds)
                {
                    deviceLed.IsSelected = value;
                }

                AllSelectedIndeterminate = false;
                OnPropertyChanged();
                suspendRollUp = false;
            }
        }

        public void RollUpCheckBoxes()
        {
            SyncBack?.Invoke();
            if (!suspendRollUp)
            {
                AllSelectedIndeterminate = true;
                if (DeviceLeds.All(x => x.IsSelected))
                {
                    this.AllSelected = true;
                    AllSelectedIndeterminate = false;
                }

                if (DeviceLeds.All(x => !x.IsSelected))
                {
                    this.AllSelected = false;
                    AllSelectedIndeterminate = false;
                }
            }
        }

        public ObservableCollection<DeviceLED> DeviceLeds { get; set; } = new ObservableCollection<DeviceLED>();
    }

    public class DeviceLED : AbstractBindable
    {
        public Action ParentalRollUp;
        public DeviceLED(Led led, bool isSynced)
        {
            Led = led;
            IsSelected = isSynced;
        }

        private Led led;
        public Led Led
        {
            get => led;
            set => SetProperty(ref led, value);
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                SetProperty(ref isSelected, value);
                ParentalRollUp?.Invoke();
            }
        }
    }
}
