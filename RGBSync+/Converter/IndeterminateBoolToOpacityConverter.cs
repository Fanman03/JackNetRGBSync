using System;
using System.Globalization;
using System.Windows.Data;

namespace RGBSyncStudio.Converter
{
    public class IndeterminateBoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value as bool?) == true ? 1d : (0.5d);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
