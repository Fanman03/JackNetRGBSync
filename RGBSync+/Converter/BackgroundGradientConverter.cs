using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RGBSyncStudio.Converter
{
    public class BackgroundGradientConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            SolidColorBrush item = (SolidColorBrush)value;

            GradientStop start = new GradientStop();
            start.Offset = 0;
            start.Color = item.Color;

            GradientStop stop = new GradientStop();
            stop.Offset = 1;
            stop.Color = Colors.Transparent;

            RadialGradientBrush result = new RadialGradientBrush();
            result.GradientOrigin = new Point(0.20, 0.5);
            result.Center = new Point(0.25, 0.5);
            result.RadiusX = 0.75;
            result.RadiusY = 0.5;
            result.GradientStops = new GradientStopCollection();
            result.GradientStops.Add(start);
            result.GradientStops.Add(stop);

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
