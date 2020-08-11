using Microsoft.Win32;
using Newtonsoft.Json;
using RGBSyncPlus.Controls;
using RGBSyncPlus.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RGB.NET.Core;
using RGBSyncPlus.Configuration;
using System.Net;
using System.Windows.Input;

namespace RGBSyncPlus.UI
{
    public partial class ConfigurationWindow : BlurredDecorationWindow
    {
        private bool isDesign = Application.Current?.MainWindow != null && DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow);

        public bool DoNotRestart;
        public bool promptForFile = false;
        public bool customRestart = false;
        public string imageFileName;
        public string localFilePath = Directory.GetCurrentDirectory() + "\\CustomTheme\\base.png";
        public string localTemplatePath = Directory.GetCurrentDirectory() + "\\CustomTheme\\presets\\";
        public ConfigurationWindow()
        {
            InitializeComponent();
            Debug.WriteLine("starting config View");
            if (isDesign)
            {
                ApplicationManager.Instance.AppSettings = new Configuration.AppSettings
                {
                    EnableClient = false,

                };

                Debug.WriteLine("mocking data");
                var vm = ((this.DataContext) as ConfigurationViewModel);
                vm.Devices = new ObservableCollection<DeviceGroup>
                {
                    new DeviceGroup
                    {
                        Name="Device Group",
                        DeviceLeds= new ObservableCollection<DeviceLED>
                        {

                        }
                    }
                };
                vm.SyncGroups = new ObservableCollection<SyncGroup>(JsonConvert.DeserializeObject<List<SyncGroup>>("\"SyncGroups\":[{\"DisplayName\":\"Jeff\",\"Name\":\"Jeff\",\"SyncLed\":null,\"Leds\":[{\"Device\":\"MSI GraphicsCard (GraphicsCard)\",\"LedId\":7340033}]}]"));
                vm.SelectedSyncGroup = vm.SyncGroups.First();
                vm.AvailableLeds = new ListCollectionView(vm.SelectedSyncGroup.Leds);
                vm.AvailableSyncLeds = new ListCollectionView(vm.SelectedSyncGroup.Leds);
                //return;
            }

            try
            {
                WebClient client = new WebClient();

                string bannerJson = client.DownloadString("https://www.rgbsync.com/api/banner.json");
                BannerSchema schema = JsonConvert.DeserializeObject<BannerSchema>(bannerJson);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(schema.imgUrl); ;
                bitmapImage.EndInit();

                BannerImage.Source = bitmapImage;
            }
            catch
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri("pack://application:,,,/RGBSync+;component/Resources/DefaultBanner.png", UriKind.Absolute);
                bitmapImage.EndInit();
                BannerImage.Source = bitmapImage;
            }

            ApplyButton.Visibility = Visibility.Hidden;
            if (ConfigurationViewModel.PremiumStatus == "Visible")
            {
                customItem.Visibility = Visibility.Visible;
            }
            DoNotRestart = true;
            if (ApplicationManager.Instance.AppSettings.BackgroundImg == "custom")
            {
                try
                {
                    customItem.IsSelected = true;
                    Uri LocalFile = new Uri(localFilePath, UriKind.Absolute);
                    ImageSource imgSource = new BitmapImage(LocalFile);
                    this.BackgroundImage = imgSource;
                }
                catch
                {
                    darkItem.IsSelected = true;
                    BitmapImage bimage = new BitmapImage();
                    bimage.BeginInit();
                    bimage.UriSource = new Uri("pack://application:,,,/Resources/base.png", UriKind.RelativeOrAbsolute);
                    bimage.EndInit();
                    this.BackgroundImage = bimage;
                }
            }
            else if (ApplicationManager.Instance.AppSettings.BackgroundImg == "rgb")
            {
                rgbItem.IsSelected = true;
                BitmapImage bimage = new BitmapImage();
                bimage.BeginInit();
                bimage.UriSource = new Uri("pack://application:,,,/Resources/background.png", UriKind.RelativeOrAbsolute);
                bimage.EndInit();
                this.BackgroundImage = bimage;
            }
            else
            {
                darkItem.IsSelected = true;
                BitmapImage bimage = new BitmapImage();
                bimage.BeginInit();
                bimage.UriSource = new Uri("pack://application:,,,/Resources/base.png", UriKind.RelativeOrAbsolute);
                bimage.EndInit();
                this.BackgroundImage = bimage;
            }

            if (ApplicationManager.Instance.AppSettings.Lang == "en")
            {
                englishItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "es")
            {
                spanishItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "de")
            {
                germanItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "fr")
            {
                frenchItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "ru")
            {
                russianItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "nl")
            {
                dutchItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "pt")
            {
                portugeseItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "it")
            {
                italianItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "te")
            {
                testItem.Visibility = System.Windows.Visibility.Visible;
                testItem.IsSelected = true;
            }
            else
            {
                LangBox.Text = "Unknown";
            }
            DoNotRestart = false;
            //ScrollViewer.SetVerticalScrollBarVisibility(AvailableLEDsListbox, ScrollBarVisibility.Visible);
            ScrollViewer.SetVerticalScrollBarVisibility(LEDGroupsListbox, ScrollBarVisibility.Visible);
        }

        //DarthAffe 07.02.2018: This prevents the applicaiton from not shutting down and crashing afterwards if 'close' is selected in the taskbar-context-menu
        private void ConfigurationWindow_OnClosed(object sender, EventArgs e)
        {
            ApplicationManager.Instance.ExitCommand.Execute(null);
        }

        private void EnglishSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "en";
        }
        private void SpanishSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "es";
        }
        private void GermanSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "de";
        }
        private void FrenchSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "fr";
        }
        private void RussianSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "ru";
        }
        private void DutchSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "nl";
        }
        private void PortugeseSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "pt";
        }
        private void ItalianSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "it";
        }
        private void TestSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "te";
        }

        private void rgbSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.BackgroundImg = "rgb";
            if (ThemeBox.IsVisible)
            {
                ApplyButton.Visibility = Visibility.Visible;
            }

        }

        private void darkSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.BackgroundImg = "dark";
            if (ThemeBox.IsVisible)
            {
                ApplyButton.Visibility = Visibility.Visible;
            }

        }


        public void PromptForFile(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UI.ConfigurationViewModel.PremiumStatus == "Visible")
            {
                ApplicationManager.Instance.AppSettings.BackgroundImg = "custom";
                Process.Start("BackgroundUploadHelper.exe");
            }
            else
            {
                if (ApplicationManager.Instance.AppSettings.BackgroundImg == "rgb")
                {
                    rgbItem.IsSelected = true;
                }
                else
                {
                    darkItem.IsSelected = true;
                }
            }
        }

        private void Lang_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RestartViaHelper();
        }

        public void RestartViaHelper()
        {
            if (!DoNotRestart)
            {
                try
                {
                    Process.Start("RestartHelper.exe");
                    System.Windows.Application.Current.Shutdown();
                }
                catch
                {
                    ApplicationManager.Instance.RestartApp();
                }


            }
        }

        private void customSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UI.ConfigurationViewModel.PremiumStatus == "Hidden")
            {

                PremiumMessageBox pMsgBox = new PremiumMessageBox();
                pMsgBox.Show();
                if (ApplicationManager.Instance.AppSettings.BackgroundImg == "rgb")
                {
                    rgbItem.IsSelected = true;
                }
                else
                {
                    darkItem.IsSelected = true;
                }
            }
            else
            {
                customRestart = true;
            }

            if (ThemeBox.IsVisible)
            {
                ApplyButton.Visibility = Visibility.Visible;
            }

        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("https://patreon.com/fanman03");
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (customRestart == true)
            {
                ApplicationManager.Instance.AppSettings.BackgroundImg = "custom";
                Process.Start("BackgroundUploadHelper.exe");
                App.SaveSettings();
                ApplicationManager.Instance.Exit();
            }
            else
            {
                RestartViaHelper();
            }
        }

        private void ManagePlugin(object sender, RoutedEventArgs e)
        {
            Process.Start("PluginManager.exe");
        }


        private void ToggleExpanded(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBlock)?.DataContext is DeviceGroup devGroup)
            {
                devGroup.Expanded = !devGroup.Expanded;

            }
        }

        private void BannerImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                WebClient client = new WebClient();

                string bannerJson = client.DownloadString("https://www.rgbsync.com/api/banner.json");
                BannerSchema schema = JsonConvert.DeserializeObject<BannerSchema>(bannerJson);

                Process.Start(schema.clickUrl);
            }
            catch
            {
                Process.Start("https://rgbsync.com");
            }
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
