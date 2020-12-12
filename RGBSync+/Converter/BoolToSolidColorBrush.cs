﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using RGBSyncPlus.UI.Tabs;
using SimpleLed;

namespace RGBSyncPlus.Converter
{
    public class BoolToSolidColorBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string options = parameter.ToString();

            var optionsList = options.Split('|').ToList();

            string selectedOption;
            if ((value as bool?) == true)
            {
                selectedOption = optionsList.First();
            }
            else
            {
                selectedOption = optionsList.Last();
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedOption));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringMatchToSolidColorBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string options = parameter.ToString();

            var optionsList = options.Split('|').ToList();

            string selectedOption;
            if ((value as string).ToLower() == optionsList[0].ToLower())
            {
                selectedOption = optionsList[1];
            }
            else
            {
                selectedOption = optionsList.Last();
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedOption));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LEDColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LEDColor))
            {
                return null;
            }

            LEDColor ledColor = (LEDColor)value;

            return new SolidColorBrush(new Color()
            {
                R = (byte)ledColor.Red,
                G = (byte)ledColor.Green,
                B = (byte)ledColor.Blue,
                A = 255
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LEDColorToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LEDColor))
            {
                return null;
            }

            LEDColor ledColor = (LEDColor)value;

            return new Color
            {
                R = (byte)ledColor.Red,
                G = (byte)ledColor.Green,
                B = (byte)ledColor.Blue,
                A = 255
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorModelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ColorModel))
            {
                return null;
            }

            ColorModel ledColor = (ColorModel)value;

            return new Color
            {
                R = (byte)ledColor.Red,
                G = (byte)ledColor.Green,
                B = (byte)ledColor.Blue,
                A = 255
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Color))
            {
                return null;
            }

            Color ledColor = (Color)value;

            return new ColorModel(ledColor.R, ledColor.G, ledColor.B);
        }
    }

    public class ColorModelToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ColorModel))
            {
                return null;
            }

            ColorModel ledColor = (ColorModel)value;

            return new SolidColorBrush(new Color
            {
                R = (byte) ledColor.Red,
                G = (byte) ledColor.Green,
                B = (byte) ledColor.Blue,
                A = 255
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SolidColorBrush))
            {
                return null;
            }

            Color ledColor = ((SolidColorBrush)value).Color;

            return new ColorModel(ledColor.R, ledColor.G, ledColor.B);
        }
    }

    public class ColorModelToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ColorModel))
            {
                return null;
            }

            ColorModel ledColor = (ColorModel)value;

            return $"#{ledColor.Red:X2}{ledColor.Green:X2}{ledColor.Blue:X2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string))
            {
                return null;
            }

            string ledColors = (string) value;
            Color ledColor = (Color)ColorConverter.ConvertFromString(ledColors);
            return new ColorModel(ledColor.R, ledColor.G, ledColor.B);
        }
    }
}