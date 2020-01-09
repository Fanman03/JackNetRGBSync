﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RGBSyncPlus.Converter
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value as bool?) == true ? Visibility.Visible
            : (string.Equals(parameter?.ToString(), "true", StringComparison.OrdinalIgnoreCase) ? Visibility.Hidden : Visibility.Collapsed);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value as Visibility? == Visibility.Visible;

        #endregion
    }
}
