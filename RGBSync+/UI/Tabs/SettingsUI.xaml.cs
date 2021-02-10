using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace RGBSyncPlus.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsUI : UserControl
    {
        public SettingsUI()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                (this.DataContext as SettingsUIViewModel)?.Init();
            }
        }

        public void PickFile(object sender, RoutedEventArgs e)
        {
            SettingsUIViewModel context = this.DataContext as SettingsUIViewModel;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                context.Background = openFileDialog.FileName;
            this.DataContext = context;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ServiceManager.Instance.SLSAuthService.SimpleLedLogIn(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    (this.DataContext as SettingsUIViewModel).SimpleLedUserName =
                        ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserName;
                });
            });
        }

        private void LogOut(object sender, RoutedEventArgs e)
        {
            ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserName = "";
            ServiceManager.Instance.SLSAuthService.SimpleLedAuth.AccessToken = "";
            ServiceManager.Instance.SLSAuthService.SimpleLedAuth.UserId = Guid.Empty;
            ServiceManager.Instance.SLSAuthService.SimpleLedAuth.Authenticated = false;
            ServiceManager.Instance.SLSAuthService.SimpleLedAuthenticated = false;
            (this.DataContext as SettingsUIViewModel).SimpleLedUserName = "";
        }
    }
}
