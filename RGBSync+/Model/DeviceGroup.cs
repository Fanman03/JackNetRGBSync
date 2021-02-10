//using RGB.NET.Core;
//using SimpleLed;
//using System;
//using System.Collections.ObjectModel;
//using System.Linq;

//namespace RGBSyncStudio.Model
//{
//    public class DeviceGroup : AbstractBindable
//    {
//        //ohgod so hacky :(
//        public Action SyncBack;
//        private bool suspendRollUp = false;
//        private IRGBDevice rgbDevice;
//        public IRGBDevice RGBDevice
//        {
//            get => rgbDevice;
//            set => SetProperty(ref rgbDevice, value);
//        }
//        private string name;
//        public string Name
//        {
//            get => name;
//            set => SetProperty(ref name, value);
//        }

//        private bool expanded = false;
//        public bool Expanded
//        {
//            get => expanded;
//            set => SetProperty(ref expanded, value);
//        }

//        private bool allSelectedIndeterminate = false;
//        public bool AllSelectedIndeterminate
//        {
//            get => expanded;
//            set => SetProperty(ref allSelectedIndeterminate, value);
//        }


//        private readonly bool allSelected;

//        public bool AllSelected
//        {
//            get => DeviceLeds.All(x => x.IsSelected);
//            set
//            {
//                suspendRollUp = true;
//                foreach (DeviceLED deviceLed in DeviceLeds)
//                {
//                    deviceLed.IsSelected = value;
//                }

//                AllSelectedIndeterminate = false;
//                OnPropertyChanged();
//                suspendRollUp = false;
//            }
//        }

//        public void RollUpCheckBoxes()
//        {
//            SyncBack?.Invoke();
//            if (!suspendRollUp)
//            {
//                AllSelectedIndeterminate = true;
//                if (DeviceLeds.All(x => x.IsSelected))
//                {
//                    this.AllSelected = true;
//                    AllSelectedIndeterminate = false;
//                }

//                if (DeviceLeds.All(x => !x.IsSelected))
//                {
//                    this.AllSelected = false;
//                    AllSelectedIndeterminate = false;
//                }
//            }
//        }

//        public ObservableCollection<DeviceLED> DeviceLeds { get; set; } = new ObservableCollection<DeviceLED>();
//        public ControlDevice ControlDevice { get; set; }
//    }

//    public class DeviceLED : AbstractBindable
//    {
//        public Action ParentalRollUp;
//        public DeviceLED(ControlDevice.LedUnit led, bool isSynced)
//        {
//            SLSLed = led;
//            IsSelected = isSynced;
//        }


//        public DeviceLED(Led led, bool isSynced)
//        {
//            Led = led;
//            IsSelected = isSynced;
//        }

//        private Led led;
//        public Led Led
//        {
//            get => led;
//            set
//            {
//                SetProperty(ref led, value);
//                AutoName = value.Id.ToString();
//            }
//        }

//        private string autoName;

//        public string AutoName
//        {
//            get => autoName;
//            set => SetProperty(ref autoName, value);
//        }

//        private ControlDevice.LedUnit slsLed;

//        public ControlDevice.LedUnit SLSLed
//        {
//            get => slsLed;
//            set
//            {
//                SetProperty(ref slsLed, value);
//                AutoName = value.LEDName;
//            }
//        }

//        private bool isSelected;
//        public bool IsSelected
//        {
//            get => isSelected;
//            set
//            {
//                SetProperty(ref isSelected, value);
//                ParentalRollUp?.Invoke();
//            }
//        }
//    }
//}
