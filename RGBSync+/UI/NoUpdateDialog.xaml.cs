﻿using System.Windows;

namespace RGBSyncPlus.UI
{
    /// <summary>
    /// Interaction logic for NoUpdateDialog.xaml
    /// </summary>
    public partial class NoUpdateDialog : Window
    {
        public NoUpdateDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
