using RGBSyncPlus.UI.Tabs;
using System.Windows;
using System.Windows.Controls;

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

            this.Title = "RGB Sync Studio " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
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

        public DevicesViewModel DevicesViewModel => DevicesUserControl.DataContext as DevicesViewModel;

        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            vm.CurrentTab = "settings";
        }
    }
}
