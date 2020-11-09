using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using RGBSyncPlus.Languages;

namespace RGBSyncPlus.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class LocConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace((string) value))
                {
                    return LanguageManager.GetValue((string) parameter?.ToString(), (string) value?.ToString());
                }
            }
            catch
            {
            }

            return LanguageManager.GetValue((string)parameter?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class ValueLocConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LanguageManager.GetValue((string)value?.ToString().Replace(" ",""));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
