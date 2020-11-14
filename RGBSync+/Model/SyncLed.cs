//using Newtonsoft.Json;
//using RGB.NET.Core;
//using RGBSyncPlus.Helper;
//using SimpleLed;

//namespace RGBSyncPlus.Model
//{
//    //public class SyncLed : AbstractBindable
//    {
//        #region Properties & Fields

//        private string _device;
//        public string Device
//        {
//            get => _device;
//            set => SetProperty(ref _device, value);
//        }

//        private LedId _ledId;
//        public LedId LedId
//        {
//            get => _ledId;
//            set => SetProperty(ref _ledId, value);
//        }

//        public int Index
//        {
//            get
//            {
//                if (SLSLedUnit != null)
//                {
//                    return SLSLedUnit.Data.LEDNumber;
//                }

//                return (int)LedId;
//            }
//        }

//        private Led _led;
//        public string SLSLEDUID { get; set; }

//        [JsonIgnore]
//        public Led Led
//        {
//            get => _led;
//            set => SetProperty(ref _led, value);
//        }

//        private ControlDevice.LedUnit slsLedUnit;
//        [JsonIgnore]
//        public ControlDevice.LedUnit SLSLedUnit
//        {
//            get => slsLedUnit;
//            set => SetProperty(ref slsLedUnit, value);
//        }

//        private ControlDevice controlDevice;

//        [JsonIgnore]
//        public ControlDevice ControlDevice
//        {
//            get => controlDevice;
//            set => SetProperty(ref controlDevice, value);
//        }
//        #endregion

//        #region Constructors

//        public SyncLed()
//        { }

//        public SyncLed(string device, LedId ledId)
//        {
//            this.Device = device;
//            this.LedId = ledId;
//        }

//        public SyncLed(Led led)
//        {
//            this.Device = led.Device.GetDeviceName();
//            this.LedId = led.Id;
//            this.Led = led;
//            this.AutoName = led.Id.ToString();
//        }

//        public SyncLed(ControlDevice controlDevice, ControlDevice.LedUnit ledUnit)
//        {
//            this.SLSLedUnit = ledUnit;
//            this.SLSLEDUID = controlDevice.GetLedUID(ledUnit);
//            this.Device = controlDevice.Name;
//            this.ControlDevice = controlDevice;
//            this.AutoName = ledUnit.LEDName;
//        }

//        private string autoName;

//        public string AutoName
//        {
//            get => autoName;
//            set => SetProperty(ref autoName, value);
//        }

//        #endregion

//        #region Methods

//        protected bool Equals(SyncLed other) => string.Equals(_device, other._device) && (_ledId == other._ledId);

//        public override bool Equals(object obj)
//        {
//            if (ReferenceEquals(null, obj)) return false;
//            if (ReferenceEquals(this, obj)) return true;
//            if (obj.GetType() != this.GetType()) return false;
//            return Equals((SyncLed)obj);
//        }

//        public override int GetHashCode()
//        {
//            unchecked
//            {
//                return ((_device != null ? _device.GetHashCode() : 0) * 397) ^ (int)_ledId;
//            }
//        }

//        public static bool operator ==(SyncLed left, SyncLed right) => Equals(left, right);
//        public static bool operator !=(SyncLed left, SyncLed right) => !Equals(left, right);

//        #endregion
//    }
//}
