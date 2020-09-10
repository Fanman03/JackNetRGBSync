﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RGBSyncPlus.Converter
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringMatchToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine(value?.ToString() + " vs "+parameter?.ToString());
            if (value?.ToString() == parameter?.ToString())
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value as Visibility? == Visibility.Visible;

    }
}