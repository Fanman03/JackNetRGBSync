using Microsoft.Win32;
using SyncStudio.WPF.Helper;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MarkdownUI.WPF;
using SyncStudio.Domain;
using CustomDeviceSpecification = SimpleLed.CustomDeviceSpecification;

namespace SyncStudio.WPF.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Devices.xaml
    /// </summary>
    public partial class Devices : UserControl
    {
        private DevicesViewModel vm => (DevicesViewModel)DataContext;
        public Devices()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.ConfigPanelRow.Height = new GridLength(400);
                this.vm.SubViewMode = "Alignment";
            }
        }

        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            vm.ZoomLevel++;
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            vm.ZoomLevel--;
        }
        private void ToggleDevicesView(object sender, RoutedEventArgs e)
        {
            vm.DevicesCondenseView = !vm.DevicesCondenseView;
        }

        private void ToggleShowSources(object sender, RoutedEventArgs e)
        {
            vm.ShowSources = !vm.ShowSources;
        }

        private void CloseConfigure(object sender, RoutedEventArgs e)
        {
            vm.ShowConfig = false;
        }


        private async void SubInfo(object sender, RoutedEventArgs e)
        {
            vm.SubViewMode = "Info";
            await Task.Delay(TimeSpan.FromSeconds(1));
            await SetMaxHeight();
        }

        private async void SyncTo(object sender, RoutedEventArgs e)
        {
            vm.SubViewMode = "SyncTo";

            //  ContainerGrid.RowDefinitions[2].Height = new GridLength(ContainerGrid.ActualHeight / 2);


            ConfigPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);



            Splitter.UpdateLayout();
            await Task.Delay(TimeSpan.FromSeconds(0.01f));
            await SetMaxHeight();

            ContainerGrid.RowDefinitions[1].Height = new GridLength(56);
            ContainerGrid.RowDefinitions[2].Height = new GridLength(1d, GridUnitType.Star);
            ContainerGrid.RowDefinitions[0].Height = new GridLength(1d, GridUnitType.Star);

        }

        private async void Config(object sender, RoutedEventArgs e)
        {
            vm.SubViewMode = "Config";
            await Task.Delay(TimeSpan.FromSeconds(1));
            await SetMaxHeight();
        }

        private async void Alignment(object sender, RoutedEventArgs e)
        {
            vm.SubViewMode = "Alignment";
            await Task.Delay(TimeSpan.FromSeconds(1));
            await SetMaxHeight();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned != null)
            {
                int vl = SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned.LedShift;

                vl--;
                if (vl < 0) vl = SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned.LEDs.Length - 1;

                SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned.LedShift = vl;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned != null)
            {
                int vl = SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned.LedShift;

                vl++;
                if (vl >= SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned.LEDs.Length) vl = 0;

                SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned.LedShift = vl;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned != null)
            {
                SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned.Reverse = !SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned.Reverse;

            }
        }

        private void DevicesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Profile profile = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile();
                if (profile?.DeviceProfileSettings == null)
                {
                    profile.DeviceProfileSettings = new ObservableCollection<DeviceProfileSettings>();
                }


                bool zeroDevicesPreSelected = vm.SLSDevices.All(x => x.Selected == false);



                foreach (object item in DeviceConfigList.Items)
                {
                    Device cItem = (Device)item;

                    cItem.Selected = false;
                    if (DeviceConfigList.SelectedItems.Contains(item))
                    {
                        cItem.Selected = true;
                    }
                }

                vm.DevicesSelectedCount = DeviceConfigList.SelectedItems.Count;

                if (DeviceConfigList.SelectedItems.Count > 1)
                {
                    vm.MultipleDeviceSelected = true;
                    vm.AnyDevicesSelected = true;
                    vm.SingledDeviceSelected = false;
                }

                if (DeviceConfigList.SelectedItems.Count == 0)
                {
                    vm.MultipleDeviceSelected = false;
                    vm.AnyDevicesSelected = false;
                    vm.SingledDeviceSelected = false;
                }

                if (DeviceConfigList.SelectedItems.Count == 1)
                {
                    vm.MultipleDeviceSelected = false;
                    vm.AnyDevicesSelected = true;
                    vm.SingledDeviceSelected = true;
                    vm.DevicesSelectedCount = 1;

                    foreach (object item in DeviceConfigList.SelectedItems)
                    {
                        if (item is Device cd)
                        {
                            ServiceManager.Instance.StoreService.ShowPlugInUI(cd.ControlDevice.Driver.GetProperties().ProductId, this.ConfigHere);
                            vm.SingleSelectedSourceControlDevice = cd;
                        }
                    }
                }
                else
                {
                    vm.SingleSelectedSourceControlDevice = null;
                }

                ConfigPanel.DataContext = DeviceConfigList.SelectedItem;
                ConfigPanelBar.DataContext = DeviceConfigList.SelectedItem;
                AddBottomPanel();

                ControlDevice selectedDevice = null;
                if (ConfigPanel != null && ((Device)ConfigPanel.DataContext) != null)
                {
                    SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned =
                        ((Device)ConfigPanel.DataContext).ControlDevice;
                    ((DevicesViewModel)this.DataContext).SetupSourceDevices(
                        ((Device)ConfigPanel.DataContext).ControlDevice);
                }
                else
                {
                    SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned = null;
                    ((DevicesViewModel)this.DataContext).SetupSourceDevices(null);
                }

                vm.DevicesSelectedCount = vm.SLSDevices.Count(x => x.Selected);

                vm.RefreshDevicesUI();
            }
            catch (Exception eee)
            {
                Debug.WriteLine(eee.Message);
            }

            vm.SinkThing();

            vm.UpdateFilteredSourceDevices();
        }

        private async void AddBottomPanel()
        {
            if (ConfigBarRow.Height.IsAbsolute && ConfigBarRow.Height.Value != null && ConfigBarRow.Height.Value == 0)
            {
                ConfigBarRow.Height = new GridLength(56);

                ConfigBarRow.Height = new GridLength(56);

                ConfigPanelRow.Height = GridLength.Auto;


                Thread.Sleep(100);
                SetMaxHeight();
                ContainerGrid_OnSizeChanged(this, null);
            }
        }

        private async Task SetMaxHeight()
        {
            DevicesPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);
            ConfigPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);

            ContainerGrid.Refresh();

            DevicesPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);
            ConfigPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);
        }

        public void UpdateDeviceConfigUi(ControlDevice cd)
        {
            ConfigHere.Children.Clear();
            if (cd != null)
            {
                if (cd.Driver is ISimpleLedWithConfig drv)
                {
                    MarkdownUIBundle cfgUI = drv.GetCustomConfig(cd);
                    ConfigHere.Children.Clear();
                    StackPanel sp = new StackPanel();
                    ConfigHere.Children.Add(sp);

                    var primary = Colors.White;
                    var secondary = Colors.Black;
                    if (Core.ServiceManager.SLSManager.GetTheme() == ThemeWatcher.WindowsTheme.Dark)
                    {
                        primary = Colors.Black;
                        secondary = Colors.White;
                    }

                    ((Style)App.Current.Resources["MarkdownHeader1"]).Setters.Add(new Setter(System.Windows.Controls.Button.ForegroundProperty, primary));
                    ((Style)App.Current.Resources["MarkdownHeader2"]).Setters.Add(new Setter(System.Windows.Controls.Button.ForegroundProperty, primary));
                    ((Style)App.Current.Resources["MarkdownHeader3"]).Setters.Add(new Setter(System.Windows.Controls.Button.ForegroundProperty, primary));

                    ResourceDictionary style = new ResourceDictionary();
                    
                    MarkdownUIHandler handler = new MarkdownUIHandler(cfgUI.Markdown, cfgUI.ViewModel, Application.Current.Resources);
                    handler.RenderToUI(sp);

                    //todo
                    //ConfigHere.Children.Add(cfgUI);

                    //ConfigHere.HorizontalAlignment = HorizontalAlignment.Stretch;
                    //ConfigHere.VerticalAlignment = VerticalAlignment.Stretch;

                    //cfgUI.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    //cfgUI.HorizontalAlignment = HorizontalAlignment.Stretch;
                    //cfgUI.VerticalAlignment = VerticalAlignment.Stretch;
                    //cfgUI.VerticalContentAlignment = VerticalAlignment.Stretch;

                    //cfgUI.Foreground = new SolidColorBrush(Colors.White);

                }
            }
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Button senderButton = (Button)sender;

            object derp = senderButton.DataContext;
            Debug.WriteLine(derp);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Device dvc = ConfigHere.DataContext as Device;
            ControlDevice cd = null;
            if (dvc != null)
            {
                cd = dvc.ControlDevice;
            }

            UpdateDeviceConfigUi(cd);
        }

        private void ClickSource(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine(sender);
            Button but = sender as Button;
            SourceModel dc = but.DataContext as SourceModel;

            vm.SyncTo(dc);
            //ListView_SelectionChanged(this.SourceList, null, dc);

            //vm.UpdateFilteredSourceDevices();

            //vm.UpdateSourceDevice(dc);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e, SourceModel selected)
        {
            //ListView items = (System.Windows.Controls.ListView)sender;

            ////SourceModel selected = (SourceModel)items.SelectedItem;
            //if (selected == null)
            //{
            //    return;
            //}

            //List<SourceModel> selectedItems = new List<SourceModel>();
            //foreach (object itemsSelectedItem in items.SelectedItems)
            //{
            //    SourceModel castItem = (SourceModel)itemsSelectedItem;
            //    selectedItems.Add(castItem);
            //}

            //foreach (object item in items.Items)
            //{
            //    SourceModel castItem = (SourceModel)item;


            //    bool shouldEnable = selectedItems.Contains(item);
            //    //castItem == selected;

            //    if (castItem.Enabled)
            //    {
            //        castItem.Enabled = false;
            //    }
            //    else
            //    {
            //        castItem.Enabled = shouldEnable;
            //    }


            //}

            //DevicesViewModel vm = this.DataContext as DevicesViewModel;
            //IEnumerable<Device> selectedParents = vm.SLSDevices.Where(x => x.Selected == true);

            //foreach (Device parentDevice in selectedParents)
            //{
            //    {
            //        Profile profile = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile();
            //        if (profile?.DeviceProfileSettings == null)
            //        {
            //            profile.DeviceProfileSettings = new ObservableCollection<DeviceProfileSettings>();
            //        }

            //        DeviceProfileSettings configDevice = profile.DeviceProfileSettings.FirstOrDefault(x => x.DestinationUID == parentDevice.UID);

            //        if (configDevice == null)
            //        {
            //            configDevice = new DeviceProfileSettings
            //            {
            //                DestinationUID = parentDevice.UID,
            //            };

            //            profile.DeviceProfileSettings.Add(configDevice);
            //        }


            //        if (selected.Name.Trim() != "None")
            //        {
            //            configDevice.SourceUID= selected?.UID;

            //            //parentDevice.SunkTo = configDevice.SourceName;
            //        }
            //        else
            //        {
            //            configDevice.SourceUID = null;

            //            parentDevice.SunkTo = "";

            //            foreach (ControlDevice.LedUnit controlDeviceLeD in parentDevice.ControlDevice.LEDs)
            //            {
            //                controlDeviceLeD.Color = new LEDColor(0, 0, 0);
            //            }

            //            if (parentDevice.SupportsPush)
            //            {
            //                parentDevice.ControlDevice.Push();
            //            }
            //        }

            //        profile.IsProfileStale = true;
            //    }
            //}

            //vm.SetupSourceDevices();
            //// vm.SinkThing();
        }


        private void DevicesListCondensedSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DevicesViewModel vm = this.DataContext as DevicesViewModel;
            bool zeroDevicesPreSelected = vm.SLSDevices.All(x => x.Selected == false);

            foreach (object item in DeviceConfigCondensedList.Items)
            {
                Device cItem = (Device)item;

                cItem.Selected = false;
                if (DeviceConfigCondensedList.SelectedItems.Contains(item))
                {
                    cItem.Selected = true;
                }
            }

            ConfigPanel.DataContext = DeviceConfigCondensedList.SelectedItem;
            ConfigPanelBar.DataContext = DeviceConfigCondensedList.SelectedItem;

            AddBottomPanel();

            if (ConfigPanel != null && ((Device)ConfigPanel.DataContext) != null)
            {
                SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned = ((Device)ConfigPanel.DataContext).ControlDevice;
                ((DevicesViewModel)this.DataContext).SetupSourceDevices(((Device)ConfigPanel.DataContext).ControlDevice);
            }
            else
            {
                SyncStudio.Core.ServiceManager.LedService.DeviceBeingAligned = null;
                ((DevicesViewModel)this.DataContext).SetupSourceDevices(null);
            }

            Device dvc = ConfigHere.DataContext as Device;
            ControlDevice cd = dvc.ControlDevice;

            vm.DevicesSelectedCount = vm.SLSDevices.Count(x => x.Selected);


            UpdateDeviceConfigUi(cd);

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm.ZoomLevel = 6;


            ConfigPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);
        }

        private readonly bool blockUpdates = false;

        private bool EnforceMinSize = false;
        private async void ContainerGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (EnforceMinSize)
            {
                if (vm.AnyDevicesSelected)
                {
                    DevicesPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);

                }

                ConfigPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);
            }

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Grid button = sender as Grid;
            DevicesViewModel.SourceGroup sg = button.DataContext as DevicesViewModel.SourceGroup;

            foreach (DevicesViewModel.SourceGroup vmSourceGroup in vm.SourceGroups)
            {
                vmSourceGroup.Selected = vmSourceGroup == sg;
            }

            vm.SelectedSourceGroup = sg;

            vm.FilterSourceDevices();
        }

        private void MouseEnterMapper(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            SourceModel dc = grid.DataContext as SourceModel;

            foreach (SourceModel vmFilteredSourceDevice in vm.FilteredSourceDevices)
            {
                vmFilteredSourceDevice.Hovered = vmFilteredSourceDevice == dc;
            }
        }

        private void MouseLeaveMapper(object sender, MouseEventArgs e)
        {

        }

        private async void ConfigSource(object sender, RoutedEventArgs e)
        {
            Button grid = sender as Button;
            SourceModel dc = grid.DataContext as SourceModel;

            UpdateDeviceConfigUi(dc.Device);
            vm.SubViewMode = "Config";
            await Task.Delay(TimeSpan.FromSeconds(1));
            await SetMaxHeight();
        }

        private void TopPanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid grid = sender as Grid;

            Debug.WriteLine(grid.ActualHeight);
            Debug.WriteLine("---");
            Debug.WriteLine(ContainerGrid.RowDefinitions[0].Height);
            Debug.WriteLine(ContainerGrid.RowDefinitions[1].Height);
            Debug.WriteLine(ContainerGrid.RowDefinitions[2].Height);
            Debug.WriteLine("---");
            Debug.WriteLine(ContainerGrid.RowDefinitions[0].ActualHeight);
            Debug.WriteLine(ContainerGrid.RowDefinitions[1].ActualHeight);
            Debug.WriteLine(ContainerGrid.RowDefinitions[2].ActualHeight);
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            StackPanel parent = cb.Parent as StackPanel;

            if (cb.SelectedItem == null)
            {
                //dunno
            }
            else
            {
                CustomDeviceSpecification cds = cb.SelectedItem as CustomDeviceSpecification;
                // CustomDeviceSpecification cds = cbi.DataContext as CustomDeviceSpecification;

                Debug.WriteLine(cds);

                Device parentContext = parent.DataContext as Device;

                if (parentContext.Overrides == null)
                {
                    parentContext.Overrides =
                        SyncStudio.Core.ServiceManager.Devices.GetOverride(parentContext.ControlDevice);
                }

                parentContext.Overrides.CustomDeviceSpecification = cds;

                if (cds.Bitmap != null)
                {
                    parentContext.Image = (cds.Bitmap.ToBitmapImage());
                }
            }
        }

        public void PickFile(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            StackPanel parent = button.Parent as StackPanel;
            Device device = parent.DataContext as Device;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Bitmap bim = new Bitmap(openFileDialog.FileName);

                    if ((bim.Width != 192 && bim.Width != 256) || bim.Height != 192)
                    {
                        MessageBox.Show("Image needs to be 192x192 or 256x192\r\n(" + bim.Width + ", " + bim.Height + ")", "Incorrect Dimensions");
                    }
                    else
                    {
                        using (FileStream s = File.OpenRead(openFileDialog.FileName))
                        {
                            byte[] tmp = new byte[s.Length];
                            s.Read(tmp, 0, tmp.Length);

                            if (device.Overrides.CustomDeviceSpecification == null)
                            {
                                device.Overrides = SyncStudio.Core.ServiceManager.Devices.GetOverride(device.ControlDevice);
                                if (device.Overrides.CustomDeviceSpecification == null)
                                {
                                    device.Overrides.CustomDeviceSpecification = new CustomDeviceSpecification();
                                }
                            }

                            device.Overrides.CustomDeviceSpecification.PngData = tmp;
                            device.Overrides.CustomDeviceSpecification.Bitmap = bim;
                            BitmapOverrideImageHolder.Source = (bim.ToBitmapImage());
                            device.Image = (bim.ToBitmapImage());
                        }
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }


        }

        private void RGBOrderOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            object derp = cb.SelectedValue;

            Debug.WriteLine(derp);
            object si = cb.SelectedItem;

            ComboBoxItem sis = si as ComboBoxItem;


            Debug.WriteLine(sis);
            Debug.WriteLine(sis.Content);
            string tt = si.ToString();

            RGBOrder order = (RGBOrder)Enum.Parse(typeof(RGBOrder), (string)sis.Content);

            Device pp = cb.DataContext as Device;
            pp.Overrides.CustomDeviceSpecification.RGBOrder = order;

            Debug.WriteLine(cb);
        }

        private void LedCountChanged(object sender, TextChangedEventArgs e)
        {
            //var dc = this.DataContext as Device;
            //var cd = (this.DataContext as Device).ControlDevice;
            //    var props = cd.Driver.GetProperties();
            //    if (props.SetDeviceOverride != null)
            //    {
            //        props.SetDeviceOverride(cd, cd.CustomDeviceSpecification);
            //    }
        }

        private void WriteCDS(object sender, RoutedEventArgs e)
        {
            Button sb = sender as Button;
            Device dave = sb.DataContext as Device;

            ControlDevice cd = dave.ControlDevice;
            DriverProperties props = cd.Driver.GetProperties();
            if (props.SetDeviceOverride != null)
            {
                props.SetDeviceOverride(cd, dave.Overrides.CustomDeviceSpecification);
                cd.CustomDeviceSpecification = dave.Overrides.CustomDeviceSpecification;
            }

            Debug.WriteLine(sb);
        }

        private void ShowConfig(object sender, RoutedEventArgs e)
        {
            //ServiceManager.Instance.StoreService.ShowPlugInUI();
            vm.ShowConfig = true;
        }
    }
}
