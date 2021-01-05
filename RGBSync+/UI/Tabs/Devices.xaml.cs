using RGBSyncPlus.Model;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RGBSyncPlus.Helper;

namespace RGBSyncPlus.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Devices.xaml
    /// </summary>
    public partial class Devices : UserControl
    {

        DevicesViewModel vm => (DevicesViewModel)DataContext;
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

        private void DevicesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DeviceMappingModels.NGProfile profile = ApplicationManager.Instance.CurrentProfile;
                if (profile?.DeviceProfileSettings == null)
                {
                    profile.DeviceProfileSettings = new ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings>();
                }


                bool zeroDevicesPreSelected = vm.SLSDevices.All(x => x.Selected == false);

              

                foreach (object item in DeviceConfigList.Items)
                {
                    DeviceMappingModels.Device cItem = (DeviceMappingModels.Device)item;

                    cItem.Selected = false;
                    if (DeviceConfigList.SelectedItems.Contains(item))
                    {
                        cItem.Selected = true;
                    }


                    //if (DeviceConfigList.Items.Count == 1)
                    //{
                    //    if (vm.SourceDevices == null && cItem.ControlDevice != null)
                    //    {
                    //        vm.SetupSourceDevices(cItem.ControlDevice);
                    //    }

                    //    if (vm.SourceDevices != null)
                    //    {

                    //var thingy = profile.DeviceProfileSettings.Where(x =>
                    //        x.Name == cItem.Name && x.ProviderName == cItem.Name &&
                    //        x.ConnectedTo == cItem.ConnectedTo)
                    //    .ToList();


                    //        foreach (DeviceMappingModels.SourceModel sd in vm.SourceDevices)
                    //        {
                    //            bool selected = (thingy.Any(x =>
                    //                x.SourceName == sd.Name && x.SourceConnectedTo == sd.ConnectedTo &&
                    //                x.SourceProviderName == sd.ProviderName));

                    //            sd.Enabled = selected;
                    //        }
                    //    }
                    //}
                }


                ConfigPanel.DataContext = DeviceConfigList.SelectedItem;
                ConfigPanelBar.DataContext = DeviceConfigList.SelectedItem;
                AddBottomPanel();

                ControlDevice selectedDevice = null;
                if (ConfigPanel != null && ((DeviceMappingModels.Device)ConfigPanel.DataContext) != null)
                {
                    ApplicationManager.Instance.DeviceBeingAligned =
                        ((DeviceMappingModels.Device)ConfigPanel.DataContext).ControlDevice;
                    ((DevicesViewModel)this.DataContext).SetupSourceDevices(
                        ((DeviceMappingModels.Device)ConfigPanel.DataContext).ControlDevice);
                }
                else
                {
                    ApplicationManager.Instance.DeviceBeingAligned = null;
                    ((DevicesViewModel)this.DataContext).SetupSourceDevices(null);
                }



                DeviceMappingModels.Device dvc = ConfigHere.DataContext as DeviceMappingModels.Device;
                if (dvc != null)
                {
                    ControlDevice cd = dvc.ControlDevice;

                    vm.DevicesSelectedCount = vm.SLSDevices.Count(x => x.Selected);

                    UpdateDeviceConfigUi(cd);

                    ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings> temp = ApplicationManager.Instance.CurrentProfile?.DeviceProfileSettings;
                    var thingy = temp.Where(x => x.Name == cd.Name && x.ConnectedTo == cd.ConnectedTo && x.ProviderName == cd.Driver.Name()).ToList();

                }
                  
                vm.RefreshDevicesUI();
            }
            catch(Exception eee)
            {
                Debug.WriteLine(eee.Message);
            }

            vm.SinkThing();

            vm.UpdateFilteredSourceDevices();
        }

        private async void AddBottomPanel()
        {
            if (ConfigBarRow.Height.IsAbsolute && ConfigBarRow.Height.Value!= null && ConfigBarRow.Height.Value == 0)
            {
                ConfigBarRow.Height = new GridLength(56);
                //ConfigPanelRow.Height = GridLength.Auto;

                //var r0 = ContainerGrid.RowDefinitions[0].Height;

                //var r2 = ContainerGrid.RowDefinitions[2].Height;

                //var ra0 = ContainerGrid.RowDefinitions[0].ActualHeight;

                //var ra2 = ContainerGrid.RowDefinitions[2].ActualHeight;

                //ContainerGrid.RowDefinitions[0].Height = new GridLength(ra0 + 1);
                //ContainerGrid.RowDefinitions[2].Height = new GridLength(ra2 + 1);

                //ContainerGrid.RowDefinitions[0].Height = new GridLength(ra0);
                //ContainerGrid.RowDefinitions[2].Height = new GridLength(ra2);

                ConfigBarRow.Height = new GridLength(56);
               // ContainerGrid.RowDefinitions[0].Height = new GridLength(ContainerGrid.ActualHeight/2);
              //  ContainerGrid.RowDefinitions[2].Height = new GridLength(ContainerGrid.ActualHeight / 2);

               ConfigPanelRow.Height = GridLength.Auto;


                Thread.Sleep(100);
               // ConfigPanelRow.Height = GridLength.Auto;
                SetMaxHeight();
                //ConfigPanelRow.Height = GridLength.Auto;
                ContainerGrid_OnSizeChanged(this,null);

            }
        }

        private async Task SetMaxHeight()
        {
            DevicesPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);
            ConfigPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);

            //await Task.Delay(TimeSpan.FromMilliseconds(20));
            //ConfigPanelRow.Height = new GridLength(ConfigPanelRow.Height.Value + 1);
            //await Task.Delay(TimeSpan.FromMilliseconds(20));
            //ConfigPanelRow.Height = new GridLength(ConfigPanelRow.Height.Value - 1);
            //await Task.Delay(TimeSpan.FromMilliseconds(20));
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
                    UserControl cfgUI = drv.GetCustomConfig(cd);
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
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Button senderButton = (Button)sender;

            object derp = senderButton.DataContext;
            Debug.WriteLine(derp);
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

        private void ClickSource(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine(sender);
            Button but = sender as Button;
            DeviceMappingModels.SourceModel dc = but.DataContext as DeviceMappingModels.SourceModel;
            ListView_SelectionChanged(this.SourceList, null,dc);

            vm.UpdateFilteredSourceDevices();

            vm.UpdateSourceDevice(dc);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e, DeviceMappingModels.SourceModel selected)
        {
            ListView items = (System.Windows.Controls.ListView)sender;

            //DeviceMappingModels.SourceModel selected = (DeviceMappingModels.SourceModel)items.SelectedItem;
            if (selected == null)
            {
                return;
            }

            //if (selected.Enabled)
            //{
            //    selected = null;
            //}

            List<DeviceMappingModels.SourceModel> selectedItems = new List<DeviceMappingModels.SourceModel>();
            foreach (object itemsSelectedItem in items.SelectedItems)
            {
                DeviceMappingModels.SourceModel castItem = (DeviceMappingModels.SourceModel)itemsSelectedItem;
                selectedItems.Add(castItem);
            }

            foreach (object item in items.Items)
            {
                DeviceMappingModels.SourceModel castItem = (DeviceMappingModels.SourceModel)item;


                bool shouldEnable = selectedItems.Contains(item);
                //castItem == selected;

                if (castItem.Enabled)
                {
                    castItem.Enabled = false;
                }
                else
                {
                    castItem.Enabled = shouldEnable;
                }


            }

            DevicesViewModel vm = this.DataContext as DevicesViewModel;
            IEnumerable<DeviceMappingModels.Device> selectedParents = vm.SLSDevices.Where(x => x.Selected == true);

            foreach (DeviceMappingModels.Device parentDevice in selectedParents)
            {
                {
                    DeviceMappingModels.NGProfile profile = ApplicationManager.Instance.CurrentProfile;
                    if (profile?.DeviceProfileSettings == null)
                    {
                        profile.DeviceProfileSettings = new ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings>();
                    }

                    DeviceMappingModels.NGDeviceProfileSettings configDevice = profile.DeviceProfileSettings.FirstOrDefault(x => x.Name == parentDevice.Name && x.ProviderName == parentDevice.ProviderName && x.ConnectedTo == parentDevice.ConnectedTo);

                    if (configDevice == null)
                    {
                        configDevice = new DeviceMappingModels.NGDeviceProfileSettings
                        {
                            Name = parentDevice.Name,
                            ProviderName = parentDevice.ProviderName,
                            ConnectedTo = parentDevice.ConnectedTo
                        };

                        profile.DeviceProfileSettings.Add(configDevice);
                    }


                    if (selected.Name.Trim()!="None")
                    {
                        configDevice.SourceName = selected?.Name;
                        configDevice.SourceProviderName = selected?.ProviderName;
                        configDevice.SourceConnectedTo = selected?.ConnectedTo;

                        parentDevice.SunkTo = configDevice.SourceName;
                    }
                    else
                    {
                        configDevice.SourceName = null;
                        configDevice.SourceProviderName = null;
                        configDevice.SourceConnectedTo = null;
                        parentDevice.SunkTo = "";

                        foreach (ControlDevice.LedUnit controlDeviceLeD in parentDevice.ControlDevice.LEDs)
                        {
                            controlDeviceLeD.Color = new LEDColor(0, 0, 0);
                        }

                        if (parentDevice.SupportsPush)
                        {
                            parentDevice.ControlDevice.Push();
                        }
                    }

                    profile.IsProfileStale = true;
                }
            }

            vm.SetupSourceDevices();
           // vm.SinkThing();
        }


        private void DevicesListCondensedSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DevicesViewModel vm = this.DataContext as DevicesViewModel;
            bool zeroDevicesPreSelected = vm.SLSDevices.All(x => x.Selected == false);

            foreach (object item in DeviceConfigCondensedList.Items)
            {
                DeviceMappingModels.Device cItem = (DeviceMappingModels.Device)item;

                cItem.Selected = false;
                if (DeviceConfigCondensedList.SelectedItems.Contains(item))
                {
                    cItem.Selected = true;
                }
            }

            ConfigPanel.DataContext = DeviceConfigCondensedList.SelectedItem;
            ConfigPanelBar.DataContext = DeviceConfigCondensedList.SelectedItem;

            AddBottomPanel();

            if (ConfigPanel != null && ((DeviceMappingModels.Device)ConfigPanel.DataContext) != null)
            {
                ApplicationManager.Instance.DeviceBeingAligned = ((DeviceMappingModels.Device)ConfigPanel.DataContext).ControlDevice;
                ((DevicesViewModel)this.DataContext).SetupSourceDevices(((DeviceMappingModels.Device)ConfigPanel.DataContext).ControlDevice);
            }
            else
            {
                ApplicationManager.Instance.DeviceBeingAligned = null;
                ((DevicesViewModel)this.DataContext).SetupSourceDevices(null);
            }

            DeviceMappingModels.Device dvc = ConfigHere.DataContext as DeviceMappingModels.Device;
            ControlDevice cd = dvc.ControlDevice;

            vm.DevicesSelectedCount = vm.SLSDevices.Count(x => x.Selected);
            

            UpdateDeviceConfigUi(cd);

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm.ZoomLevel = 6;


            ConfigPanelRow.MaxHeight = Math.Max(0,ContainerGrid.ActualHeight - 200);
        }

        private bool blockUpdates = false;

        private async void ContainerGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (vm.AnyDevicesSelected)
            {
                DevicesPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);
                //if (!blockUpdates)
                //{
                //    ContainerGrid.RowDefinitions[2].Height =
                //        new GridLength(ContainerGrid.RowDefinitions[2].ActualHeight);
                //    blockUpdates = true;
                //}
                //else
                //{
                //    ContainerGrid.RowDefinitions[2].Height = GridLength.Auto;
                //}
            }

           // Splitter.Height = Splitter.ActualHeight - 1;

            ConfigPanelRow.MaxHeight = Math.Max(0, ContainerGrid.ActualHeight - 200);

            
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
            DeviceMappingModels.SourceModel dc = grid.DataContext as DeviceMappingModels.SourceModel;

            foreach (DeviceMappingModels.SourceModel vmFilteredSourceDevice in vm.FilteredSourceDevices)
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
            DeviceMappingModels.SourceModel dc = grid.DataContext as DeviceMappingModels.SourceModel;

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
    }
}
