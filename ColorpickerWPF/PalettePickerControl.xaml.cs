using ColorPickerWPF.Code;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPickerWPF.Controls
{
    /// <summary>
    /// Interaction logic for PalettePickerControl.xaml
    /// </summary>
    public partial class PalettePickerControl : UserControl
    {
        public static readonly DependencyProperty PickedColorProperty = DependencyProperty.Register("PickedColor", typeof(string), typeof(PalettePickerControl), new FrameworkPropertyMetadata("#000000", FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PalettePickerControl input = (PalettePickerControl)d;

            if (e.NewValue is string col)
            {

                input.PickedColor = col;
                if (input.ColorBox.Text != input.PickedColor)
                {
                    input.ColorBox.Text = input.PickedColor;
                }
                if (input.PickedColor.Replace("#", "").Length == 6)
                {
                    try
                    {
                        input.Color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(input.PickedColor);

                        input.ColorIcon.Background = new SolidColorBrush(input.Color);

                        input.ColorBox.Background = new SolidColorBrush(input.Color);

                        float lightness = input.Color.GetBrightness();

                        if (lightness > 0.75)
                        {
                            input.ColorBox.Foreground = new SolidColorBrush(Colors.Black);
                            input.PenIcon.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            input.ColorBox.Foreground = new SolidColorBrush(Colors.White);
                            input.PenIcon.Foreground = new SolidColorBrush(Colors.White);
                        }

                    }
                    catch (Exception ee)
                    {
                        Debug.WriteLine(ee.Message);
                    }
                }

            }
        }

        private void SetColor()
        {
            if (PickedColor.Replace("#", "").Length == 6)
            {
                try
                {
                    Color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(PickedColor);
                    ColorIcon.Background = new SolidColorBrush(Color);
                    ColorBox.Background = new SolidColorBrush(Color);

                    float lightness = Color.GetBrightness();

                    if (lightness > 0.75)
                    {
                        ColorBox.Foreground = new SolidColorBrush(Colors.Black);
                        PenIcon.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        ColorBox.Foreground = new SolidColorBrush(Colors.White);
                        PenIcon.Foreground = new SolidColorBrush(Colors.White);
                    }

                    Debug.WriteLine(PickedColor);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        public PalettePickerControl()
        {
            InitializeComponent();
            //this.DataContext = this;
        }
        private void ColorBox_OnChanged(object sender, TextChangedEventArgs e)
        {
            TextBox colorBox = sender as TextBox;
            PickedColor = colorBox.Text;
            SetColor();

        }

        private void ColorBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ColorBox.Text = PickedColor;
        }

        public string PickedColor
        {
            get => (string)GetValue(PickedColorProperty);
            set => SetValue(PickedColorProperty, value);
        }

        private Color color;

        public Color Color
        {
            get => color;
            set
            {
                color = value;
            }
        }

        private void EditBtn_OnClick(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow picker = new ColorPickerWindow();
            Color color;
            bool showWindow = ColorPickerWindow.ShowDialog(out color, HexToColor(ColorBox.Text));
            string ccc = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            PickedColor = ccc;
            ColorBox.Text = PickedColor;
            SetColor();
        }

        public static System.Windows.Media.Color HexToColor(String hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            if (string.IsNullOrWhiteSpace(hex))
            {
                return Colors.Transparent;
            }

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }
    }
}
