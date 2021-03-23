using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using SimpleLed;

namespace SyncStudio.WPF.Converter
{
    public class DeviceTypeToLetterConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return default;
            }

            if (value is string stringValue)
            {
                Debug.WriteLine("Received: "+stringValue);

                switch (stringValue)
                {
                    case DeviceTypes.Keyboard:
                        return "H";
                    case DeviceTypes.Mouse:
                        return "K";
                    case DeviceTypes.Headset:
                        return "F";
                    case DeviceTypes.MousePad:
                        return "L";
                    case DeviceTypes.Fan:
                        return "D";
                    case DeviceTypes.LedStrip:
                        return "I";
                    case DeviceTypes.Memory:
                        return "B";
                    case DeviceTypes.MotherBoard:
                        return "J";
                    case DeviceTypes.GPU:
                        return "E";
                    case DeviceTypes.Cooler:
                        return "M";
                    case DeviceTypes.Speaker:
                        return "N";
                    case DeviceTypes.Effect:
                        return "C";
                    case DeviceTypes.Other:
                        return "G";
                    default:
                        return "G";
                }
            }

            return "Dunno";

        }
    }
}