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
using System.Windows.Shapes;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm => ((MainWindowViewModel)DataContext);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SubmitModalText(object sender, RoutedEventArgs e)
        {
            //vm.ShowModal = false;
            //vm.SubmitModalTextBox(ModalTextBox.Text);
        }

        private void CloseModal(object sender, RoutedEventArgs e)
        {
            //vm.ShowModal = false;
        }

        private void ToggleHamburger(object sender, RoutedEventArgs e)
        {
            vm.HamburgerExtended = !vm.HamburgerExtended;
        }

        private void SetTab(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is MainWindowViewModel.TabItem lvm)
            {
                if (string.IsNullOrWhiteSpace(lvm.Key))
                {
                    vm.HamburgerExtended = !vm.HamburgerExtended;
                }
                else
                {
                    vm.CurrentTab = lvm.Key;
                }
            }
        }
    }
}
