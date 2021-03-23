using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using SyncStudio.WPF.UI.Tabs;

namespace SyncStudio.WPF.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm => ((MainWindowViewModel)DataContext);

        public MainWindow()
        {
            Debug.WriteLine("Started Opening Main Window");
            ServiceManager.Instance.ApplicationManager.MainWindow = this;
            InitializeComponent();
            Debug.WriteLine("Component Initialized");
            this.Title = ServiceManager.Instance.Branding.GetAppName()+" " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            Debug.WriteLine("Set Title");
            this.Icon = ServiceManager.Instance.Branding.GetIcon();
            Debug.WriteLine("Set Icon");
            Debug.WriteLine("Main Initialized");
        }

       
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

        //public DevicesViewModel DevicesViewModel => DevicesUserControl.DataContext as DevicesViewModel;

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
                    SetTab(lvm.Key);
                }
            }
        }

        private void Grid_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            vm.CurrentTab = "settings";
        }
    }
}
