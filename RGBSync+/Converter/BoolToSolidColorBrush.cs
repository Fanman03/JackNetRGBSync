using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

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
}
