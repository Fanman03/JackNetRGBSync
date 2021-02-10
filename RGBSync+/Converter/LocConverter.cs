﻿using RGBSyncStudio.Languages;
using System;
using System.Globalization;
using System.Windows.Data;

namespace RGBSyncStudio.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class LocConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace((string)value))
                {
                    return LanguageManager.GetValue(parameter?.ToString(), value?.ToString());
                }
            }
            catch
            {
            }

            return LanguageManager.GetValue(parameter?.ToString());
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
            return LanguageManager.GetValue(value?.ToString().Replace(" ", ""));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
