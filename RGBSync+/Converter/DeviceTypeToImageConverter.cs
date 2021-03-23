
using SimpleLed;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SyncStudio.WPF.Converter
{
    public class DeviceTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return default;
            }

            if (value is string stringValue)
            {
                switch (stringValue)
                {
                    case DeviceTypes.Keyboard:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Keyboard.png",
                            UriKind.Absolute));
                    case DeviceTypes.Mouse:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Mouse.png", UriKind.Absolute));
                    case DeviceTypes.Headset:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Headphones.png",
                            UriKind.Absolute));
                    case DeviceTypes.MousePad:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Mousepad.png",
                            UriKind.Absolute));
                    case DeviceTypes.Fan:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Fan.png",
                            UriKind.Absolute));
                    case DeviceTypes.LedStrip:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/LedStrip.png",
                            UriKind.Absolute));
                    case DeviceTypes.Memory:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/DRAM.png", UriKind.Absolute));
                    case DeviceTypes.MotherBoard:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Motherboard.png",
                            UriKind.Absolute));
                    case DeviceTypes.GPU:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/GPU.png",
                            UriKind.Absolute));
                    case DeviceTypes.Cooler:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Pump.png", UriKind.Absolute));
                    case DeviceTypes.Speaker:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Speaker.png",
                            UriKind.Absolute));
                    case DeviceTypes.Effect:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Effect.png", UriKind.Absolute));
                    case DeviceTypes.Other:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Invalid.png",
                            UriKind.Absolute));
                    default:
                        return new BitmapImage(new Uri(
                            "pack://application:,,,/RGBSync+;component/Resources/DevImg/Default.png",
                            UriKind.Absolute));
                }
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
