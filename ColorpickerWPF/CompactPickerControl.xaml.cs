using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorPickerWPF.Controls
{
    /// <summary>
    /// Interaction logic for CompactPickerControl.xaml
    /// </summary>
    public partial class CompactPickerControl : UserControl
    {
        public Color Color { get; set; }
        public CompactPickerControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        private void ColorBox_OnChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox colorBox = sender as TextBox;
                Color = HexToColor(colorBox.Text.ToString());
                ColorIcon.Foreground = new SolidColorBrush(Color);
                ColorBox.Text = Color.ToString();
            }
            catch
            {

            }
        }

        private void ColorBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ColorBox.Text = Color.ToString();
        }

        private void EditBtn_OnClick(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow picker = new ColorPickerWindow();
            Color color;
            bool showWindow = ColorPickerWindow.ShowDialog(out color, HexToColor(ColorBox.Text));
            Color = color;
            ColorIcon.Foreground = new SolidColorBrush(Color);
            ColorBox.Text = Color.ToString();
        }

        public static System.Windows.Media.Color HexToColor(String hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

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
