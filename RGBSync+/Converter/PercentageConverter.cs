using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace RGBSyncPlus.Converter
{
    public class PercentageConverter : MarkupExtension, IValueConverter
    {
        private static PercentageConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
            }
            catch
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new PercentageConverter());
        }
    }


    public class SubtractorConverter : MarkupExtension, IValueConverter
    {
        private static SubtractorConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
            }
            catch
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new SubtractorConverter());
        }
    }

    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;

            
            string param = (string) parameter;
            if (param.StartsWith("-"))
            {
                param = param.Substring(1);
                v = -v;
            }

            var parts = param.Split('|');
            param = parts.First();

            if (parts.Length > 1)
            {
                double amount = double.Parse(parts[1]);

                v = v * amount;
            }

            switch (param.ToLower())
            {
                case "left": return new Thickness(v, 0, 0, 0);
                case "top": return new Thickness(0, v, 0, 0);
                case "right": return new Thickness(0, 0, v, 0);
                case "bottom": return new Thickness(0, 0, 0, v);

                case "topleft": return new Thickness(v, v, 0, 0);
                case "topright": return new Thickness(0, v, v, 0);
                case "bottomleft": return new Thickness(v, 0, 0, v);
                case "bottomright": return new Thickness(0, 0, v, v);

            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
