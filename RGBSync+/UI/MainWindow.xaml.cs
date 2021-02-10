using System;
using System.Linq;
using System.Threading;
using RGBSyncPlus.UI.Tabs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

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
            ApplicationManager.Instance.ConfigurationWindow = this;
            InitializeComponent();

            this.Title = "RGB Sync Studio " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

            this.SourceInitialized += new EventHandler(OnSourceInitialized); //this makes minimize to tray work


            ServiceManager.Instance.LedService.RssBackgroundDevice.ColourChange += (sender, args) =>
            {

            };


        }

        //This is a semi-hacky way to create custom minimize actions
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(new HwndSourceHook(HandleMessages));
        }

        private IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // 0x0112 == WM_SYSCOMMAND, 'Window' command message.
            // 0xF020 == SC_MINIMIZE, command to minimize the window.
            if (msg == 0x0112 && ((int)wParam & 0xFFF0) == 0xF020)
            {
                if (ServiceManager.Instance.ConfigService.NGSettings.MinimizeToTray == true)
                {
                    // Cancel the minimize.
                    handled = true;

                    //hide window instead
                    this.Hide();

                }
                else
                {
                    // Let system handle minimization
                    handled = false;
                }

            }

            return IntPtr.Zero;
        }
        //end minimization stuff

        private void SubmitModalText(object sender, RoutedEventArgs e)
        {
            //vm.ShowModal = false;
            //vm.SubmitModalTextBox(ModalTextBox.Text);
        }

        private void CloseModal(object sender, RoutedEventArgs e)
        {
            vm.ShowModal = false;
        }

        private void ToggleHamburger(object sender, RoutedEventArgs e)
        {
            vm.HamburgerExtended = !vm.HamburgerExtended;
        }

        public void SetTab(string tab)
        {
            vm.CurrentTab = tab;
        }
        public void SetTab(object sender, RoutedEventArgs e)
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

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((sender as Grid).DataContext is MainWindowViewModel.TabItem lvm)
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

        private void Grid_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            vm.CurrentTab = "settings";
        }
    }
}
