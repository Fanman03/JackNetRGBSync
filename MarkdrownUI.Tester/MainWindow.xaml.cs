using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using MarkdownUI.WPF;

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

            var vm = new DemoVM();

            var vvm = vm.GetVirtualViewModel();


            Debug.WriteLine(vvm);
        }

        public class VirtualViewModel
        {
            public string Name { get; set; }
            public List<string> Props { get; set; } = new List<string>();
            public List<VirtualViewModel> Collections { get; set; } = new List<VirtualViewModel>();
        }

        private void textChanged(object sender, TextChangedEventArgs e)
        {
            var vm = new DemoVM();

            MarkDownViewModel vvm = new MarkDownViewModel();
            var props = vm.GetType().GetProperties();
            foreach (var prop in props)
            {
                Console.WriteLine("{0}={1}", prop.Name, prop.GetValue(vm, null));
                if (prop.GetValue(vm, null) is string vl)
                {
                    vvm.bindings.Add(prop.Name, vl);
                }
            }

            string markdown = TextEntry.Text;

            ResourceDictionary theme = new ResourceDictionary { Source = new Uri(@"\MarkdownDark.xaml", UriKind.Relative) };
            MarkdownUI.WPF.MarkdownUIHandler handler = new MarkdownUI.WPF.MarkdownUIHandler(markdown, vvm, theme);
            try
            {
                handler.RenderToUI(this.MDHere);
            }
            catch (Exception ee) { }

        }
    }
}
