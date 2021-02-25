using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SyncStudio.WPF.Converter
{
    public class ProfileSelectedToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                return null;
            }

            SolidColorBrush output = new SolidColorBrush();

            bool isSelected = (bool)value;

            if (isSelected)
            {
                output = (SolidColorBrush)SystemParameters.WindowGlassBrush;
            }
            else
            {
                output.Color = Color.FromRgb(64, 64, 64);
            }

            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
