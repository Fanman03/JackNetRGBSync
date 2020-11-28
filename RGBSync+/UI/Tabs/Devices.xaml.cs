using RGBSyncPlus.Model;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private void SubInfo(object sender, RoutedEventArgs e)
        {
            vm.SubViewMode = "Info";
        }

        private void SyncTo(object sender, RoutedEventArgs e)
        {
            vm.SubViewMode = "SyncTo";
        }

        private void Config(object sender, RoutedEventArgs e)
        {
            vm.SubViewMode = "Config";
        }

        private void Alignment(object sender, RoutedEventArgs e)
        {
            vm.SubViewMode = "Alignment";
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
            catch
            {
            }

            vm.SinkThing();
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

        private void ClickSource(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(sender);
            ListView_SelectionChanged(this.SourceList, null);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView items = (System.Windows.Controls.ListView)sender;

            DeviceMappingModels.SourceModel selected = (DeviceMappingModels.SourceModel)items.SelectedItem;
            if (selected == null)
            {
                return;
            }

            if (selected.Enabled)
            {
                selected = null;
            }

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


                    if (selected != null && selected.Name.Trim()!="None")
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

            if (selected == null)
            {
                foreach (DeviceMappingModels.Device parentDevice in selectedParents)
                {
                    foreach (ControlDevice.LedUnit controlDeviceLeD in parentDevice.ControlDevice.LEDs)
                    {
                        controlDeviceLeD.Color = new LEDColor(0, 0, 0);
                    }

                    if (parentDevice.SupportsPush)
                    {
                        parentDevice.ControlDevice.Push();
                    }
                }
            }

            vm.SetupSourceDevices();
            vm.SinkThing();
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
            ZoomIn(this, new RoutedEventArgs());
        }
    }
}
