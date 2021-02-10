using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RGBSyncStudio.Converter
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

            bool isSelected = (bool) value;

            if (isSelected)
            {
                output = (SolidColorBrush) SystemParameters.WindowGlassBrush;
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
