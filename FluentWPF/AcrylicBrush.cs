﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SourceChord.FluentWPF
{
    public class AcrylicBrushExtension : MarkupExtension
    {
        public string TargetName { get; set; }

        public Color TintColor { get; set; } = Colors.White;

        public double TintOpacity { get; set; } = 0.0;

        public double NoiseOpacity { get; set; } = 0.03;


        public AcrylicBrushExtension()
        {

        }

        public AcrylicBrushExtension(string target)
        {
            this.TargetName = target;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var target = pvt.TargetObject as FrameworkElement;

            var acrylicPanel = new AcrylicPanel()
            {
                TintColor = this.TintColor,
                TintOpacity = this.TintOpacity,
                NoiseOpacity = this.NoiseOpacity,
                Width = target.Width,
                Height = target.Height
            };
            BindingOperations.SetBinding(acrylicPanel, AcrylicPanel.TargetProperty, new Binding() { ElementName = this.TargetName });
            BindingOperations.SetBinding(acrylicPanel, AcrylicPanel.SourceProperty, new Binding() { Source = target });

            var brush = new VisualBrush(acrylicPanel)
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                ViewboxUnits = BrushMappingMode.Absolute,
            };

            return brush;
        }
    }

    public class BrushTranslationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(o => o == DependencyProperty.UnsetValue || o == null)) return new Point(0, 0);

            var parent = values[0] as UIElement;
            var ctrl = values[1] as UIElement;
            //var pointerPos = (Point)values[2];
            var relativePos = parent.TranslatePoint(new Point(0, 0), ctrl);

            return new TranslateTransform(relativePos.X, relativePos.Y);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
