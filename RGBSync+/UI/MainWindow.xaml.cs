using System;
using System.Linq;
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
            InitializeComponent();

            this.Title = "RGB Sync Studio " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

            this.SourceInitialized += new EventHandler(OnSourceInitialized);


            ApplicationManager.Instance.RssBackgroundDevice.ColourChange += (sender, args) =>
            {
               
                    //var rbd = ApplicationManager.Instance.RssBackgroundDevice;
                    //vm.SCTop.GradientStops.First().Color = new Color()
                    //{
                    //    R = (byte) rbd.Leds[0].Red,
                    //    G = (byte) rbd.Leds[0].Green,
                    //    B = (byte) rbd.Leds[0].Blue
                    //};

                    //vm.SCRight.GradientStops.First().Color = new Color()
                    //{
                    //    R = (byte) rbd.Leds[1].Red,
                    //    G = (byte) rbd.Leds[1].Green,
                    //    B = (byte) rbd.Leds[1].Blue
                    //};


                    //vm.SCBottom.GradientStops.First().Color = new Color()
                    //{
                    //    R = (byte) rbd.Leds[2].Red,
                    //    G = (byte) rbd.Leds[2].Green,
                    //    B = (byte) rbd.Leds[2].Blue
                    //};


                    //vm.SCLeft.GradientStops.First().Color = new Color()
                    //{
                    //    R = (byte) rbd.Leds[3].Red,
                    //    G = (byte) rbd.Leds[3].Green,
                    //    B = (byte) rbd.Leds[3].Blue
                    //};

            };


          //  DispatcherTimer update = new DispatcherTimer(TimeSpan.FromMilliseconds(30), DispatcherPriority.Render, Callback, Application.Current.Dispatcher);
        }

        //private void Callback(object sender, EventArgs e)
        //{
        //    vm.SCTop = top;
        //    vm.SCRight = right;
        //    vm.SCBottom = bottom;
        //    vm.SCLeft = left;
        //    //scbottomtest.Fill = bottom;
        //    //scrighttest.Fill = right;
        //    //sctoptest.Fill = top;
        //    //sclefttest.Fill = left;

        //}

        //private RadialGradientBrush top=new RadialGradientBrush(Colors.Red,Colors.Transparent);
        //private RadialGradientBrush left = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        //private RadialGradientBrush right = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        //private RadialGradientBrush bottom = new RadialGradientBrush(Colors.Red, Colors.Transparent);

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
                if (ApplicationManager.Instance.NGSettings.MinimizeToTray == true)
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
