﻿using System.Windows;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for CrashWindow.xaml
    /// </summary>
    public partial class CrashWindow : Window
    {
        public CrashWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Send_Report(object sender, RoutedEventArgs e)
        {
            string text = ApplicationManager.Logger.Log;

            
        }
    }
}
