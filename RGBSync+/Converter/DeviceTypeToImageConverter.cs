using RGB.NET.Core;
using RGBSyncPlus.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RGBSyncPlus.Converter
{
    public class DeviceTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var type = (RGBDeviceType)value;
            switch (type)
            {
                case RGBDeviceType.Keyboard:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Keyboard.png", UriKind.Absolute));
                case RGBDeviceType.Mouse:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Mouse.png", UriKind.Absolute));
                case RGBDeviceType.Headset:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Headphones.png", UriKind.Absolute));
                case RGBDeviceType.Mousepad:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Mousepad.png", UriKind.Absolute));
                case RGBDeviceType.Fan:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Fan.png", UriKind.Absolute));
                case RGBDeviceType.LedStripe:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/LedStrip.png", UriKind.Absolute));
                case RGBDeviceType.DRAM:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/DRAM.png", UriKind.Absolute));
                case RGBDeviceType.Mainboard:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Motherboard.png", UriKind.Absolute));
                case RGBDeviceType.GraphicsCard:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/GPU.png", UriKind.Absolute));
                case RGBDeviceType.Cooler:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Pump.png", UriKind.Absolute));
                case RGBDeviceType.Speaker:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Speaker.png", UriKind.Absolute));
                case RGBDeviceType.Unknown:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Invalid.png", UriKind.Absolute));
                default:
                    return new BitmapImage(new Uri("pack://application:,,,/RGBSync+;component/Resources/DevImg/Default.png", UriKind.Absolute));
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
