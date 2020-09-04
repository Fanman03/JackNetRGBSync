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
using System.Runtime.InteropServices;
using System.Windows.Input;
using MadLedFrameworkSDK;
using Color = RGB.NET.Core.Color;
using ListView = System.Windows.Forms.ListView;

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

            //todo where does banner image go?
            //try
            //{
            //    WebClient client = new WebClient();

            //    string bannerJson = client.DownloadString("https://www.rgbsync.com/api/banner.json");
            //    BannerSchema schema = JsonConvert.DeserializeObject<BannerSchema>(bannerJson);
            //    if (schema.imgUrl != null)
            //    {
            //        var bitmapImage = new BitmapImage();
            //        bitmapImage.BeginInit();
            //        bitmapImage.UriSource = new Uri(schema.imgUrl);
            //        ;
            //        bitmapImage.EndInit();

            //        BannerImage.Source = bitmapImage;
            //    }
            //}
            //catch
            //{
            //    var bitmapImage = new BitmapImage();
            //    bitmapImage.BeginInit();
            //    bitmapImage.UriSource = new Uri("pack://application:,,,/RGBSync+;component/Resources/DefaultBanner.png", UriKind.Absolute);
            //    bitmapImage.EndInit();
            //    BannerImage.Source = bitmapImage;
            //}

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
            //ScrollViewer.SetVerticalScrollBarVisibility(LEDGroupsListbox, ScrollBarVisibility.Visible);
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
            if ((sender as Control)?.DataContext is DeviceGroup devGroup)
            {
                devGroup.Expanded = !devGroup.Expanded;

            }
        }

        private void ToggleDMExpanded(object sender, RoutedEventArgs e)
        {
            if ((sender as Control)?.DataContext is DeviceMappingModels.DeviceMappingViewModel dmvm)
            {
                bool test = dmvm.Expanded;
                bool newValue = !test;
                dmvm.Expanded = newValue;

            }
        }

        private void ToggleCheckBox(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBlock)?.DataContext is DeviceGroup devGroup)
            {
                devGroup.AllSelected = !devGroup.AllSelected;

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

        private void CloseBanner_Click(object sender, RoutedEventArgs e)
        {
            //BannerColumn.Visibility = Visibility.Collapsed;
            //SyncLedsColumn.SetValue(Grid.ColumnSpanProperty, 3);
        }

        private void DevicesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigPanel.DataContext = DeviceConfigList.SelectedItem;
            ApplicationManager.Instance.DeviceBeingAligned = ((DeviceMappingModels.Device)ConfigPanel.DataContext).ControlDevice;

            ((ConfigurationViewModel)this.DataContext).SetupSourceDevices(((DeviceMappingModels.Device)ConfigPanel.DataContext).ControlDevice);

            DeviceMappingModels.Device dvc = ConfigHere.DataContext as DeviceMappingModels.Device;
            ControlDevice cd = dvc.ControlDevice;

            UpdateDeviceConfigUi(cd);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationManager.Instance.DeviceBeingAligned != null)
            {
                int vl = ApplicationManager.Instance.DeviceBeingAligned.LedShift;

                vl--;
                if (vl < 0) vl = ApplicationManager.Instance.DeviceBeingAligned.LEDs.Length - 1;

                ApplicationManager.Instance.DeviceBeingAligned.LedShift = vl;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ApplicationManager.Instance.DeviceBeingAligned != null)
            {
                int vl = ApplicationManager.Instance.DeviceBeingAligned.LedShift;

                vl++;
                if (vl >= ApplicationManager.Instance.DeviceBeingAligned.LEDs.Length) vl = 0;

                ApplicationManager.Instance.DeviceBeingAligned.LedShift = vl;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (ApplicationManager.Instance.DeviceBeingAligned != null)
            {
                ApplicationManager.Instance.DeviceBeingAligned.Reverse = !ApplicationManager.Instance.DeviceBeingAligned.Reverse;

            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = (System.Windows.Controls.ListView)sender;
            var selected = (DeviceMappingModels.SourceModel)items.SelectedItem;
            foreach (var item in items.Items)
            {
                DeviceMappingModels.SourceModel castItem = (DeviceMappingModels.SourceModel)item;


                bool shouldEnable = castItem == selected;

                if (castItem.Enabled && shouldEnable)
                {
                    castItem.Enabled = false;
                }
                else
                {
                    castItem.Enabled = shouldEnable;
                }


            }

            var parentDevice = (DeviceMappingModels.Device)ConfigPanel.DataContext;

            if (selected != null)
            {
                var profile = ApplicationManager.Instance.CurrentProfile;
                if (profile.DeviceProfileSettings == null)
                {
                    profile.DeviceProfileSettings = new ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings>();
                }

                var configDevice = profile.DeviceProfileSettings.FirstOrDefault(x => x.Name == parentDevice.Name && x.ProviderName == parentDevice.ProviderName);

                if (configDevice == null)
                {
                    configDevice = new DeviceMappingModels.NGDeviceProfileSettings
                    {
                        Name = parentDevice.Name,
                        ProviderName = parentDevice.ProviderName,
                    };

                    profile.DeviceProfileSettings.Add(configDevice);
                }



                configDevice.SourceName = selected.Name;
                configDevice.SourceProviderName = selected.ProviderName;
                profile.IsProfileStale = true;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            DeviceMappingModels.Device dvc = ConfigHere.DataContext as DeviceMappingModels.Device;
            ControlDevice cd = dvc.ControlDevice;

            if (cd.Driver is ISimpleLEDDriverWithConfig drv)
            {
                var cfgUI = drv.GetCustomConfig(cd);
                ConfigHere.Children.Clear();
                ConfigHere.Children.Add(cfgUI);

                ConfigHere.HorizontalAlignment = HorizontalAlignment.Stretch;
                ConfigHere.VerticalAlignment = VerticalAlignment.Stretch;

                cfgUI.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                cfgUI.HorizontalAlignment = HorizontalAlignment.Stretch;
                cfgUI.VerticalAlignment = VerticalAlignment.Stretch;
                cfgUI.VerticalContentAlignment = VerticalAlignment.Stretch;

                cfgUI.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        public void UpdateDeviceConfigUi(ControlDevice cd)
        {
            ConfigHere.Children.Clear();
            if (cd != null)
            {
                if (cd.Driver is ISimpleLEDDriverWithConfig drv)
                {
                    var cfgUI = drv.GetCustomConfig(cd);
                    ConfigHere.Children.Add(cfgUI);
                    
                    ConfigHere.HorizontalAlignment = HorizontalAlignment.Stretch;
                    ConfigHere.VerticalAlignment = VerticalAlignment.Stretch;

                    cfgUI.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    cfgUI.HorizontalAlignment = HorizontalAlignment.Stretch;
                    cfgUI.VerticalAlignment = VerticalAlignment.Stretch;
                    cfgUI.VerticalContentAlignment = VerticalAlignment.Stretch;

                    cfgUI.Foreground= new SolidColorBrush(Colors.White);

                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceMappingModels.Device dvc = ConfigHere.DataContext as DeviceMappingModels.Device;
            ControlDevice cd = null;
            if (dvc != null)
            {
                cd = dvc.ControlDevice;
            }

            UpdateDeviceConfigUi(cd);
        }

        private void ClickSource(object sender, MouseButtonEventArgs e)
        {
         Debug.WriteLine(sender);
         ListView_SelectionChanged(this.SourceList, null);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            ConfigurationViewModel vm = (ConfigurationViewModel) this.DataContext;
            vm.SyncToSearch = SyncToSearchTextBox.Text;
            vm.FilterSourceDevices();
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ConfigurationViewModel vm = (ConfigurationViewModel)this.DataContext;
            vm.SyncToSearch = "";
            SyncToSearchTextBox.Text = "";
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ConfigurationViewModel vm = (ConfigurationViewModel)this.DataContext;
            vm.ZoomLevel++;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            ConfigurationViewModel vm = (ConfigurationViewModel)this.DataContext;
            vm.ZoomLevel--;
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Button senderButton = (Button)sender;

            var derp = senderButton.DataContext;
            Debug.WriteLine(derp);
        }
    }
}
