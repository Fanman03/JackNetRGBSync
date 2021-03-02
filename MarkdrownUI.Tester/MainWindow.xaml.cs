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

namespace MarkdrownUI.Tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void textChanged(object sender, TextChangedEventArgs e)
        {
            string markdown = TextEntry.Text;

            ResourceDictionary theme = new ResourceDictionary { Source = new Uri(@"\MarkdownDark.xaml", UriKind.Relative) };
            MarkdownUI.WPF.MarkdownUIHandler handler = new MarkdownUI.WPF.MarkdownUIHandler(markdown, new DemoVM(), theme);
            try
            {
                handler.RenderToUI(this.MDHere);
            }
            catch (Exception ee) { }

        }
    }
}
