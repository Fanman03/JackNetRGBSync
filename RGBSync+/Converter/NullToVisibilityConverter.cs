using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SyncStudio.WPF.Converter
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value == null) == (string.Equals(parameter?.ToString(), "true", StringComparison.OrdinalIgnoreCase)) ? Visibility.Visible : Visibility.Hidden;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NotNullToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value != null) == (string.Equals(parameter?.ToString(), "true", StringComparison.OrdinalIgnoreCase)) ? Visibility.Visible : Visibility.Hidden;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }


    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NonEmptyStringToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool trig = value == null || string.IsNullOrWhiteSpace(value.ToString());

            if (trig)
            {
                if (parameter != null && (parameter.ToString() == "hidden"))
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            else
            {
                return Visibility.Visible;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool trig = value == null || string.IsNullOrWhiteSpace(value.ToString());

            if (!trig)
            {
                if (parameter != null && (parameter.ToString() == "hidden"))
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            else
            {
                return Visibility.Visible;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        #endregion
    }
}
