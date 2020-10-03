using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RGBSyncPlus.Model;
using SimpleLed;

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
                bool zeroDevicesPreSelected = vm.SLSDevices.All(x => x.Selected == false);
                foreach (var item in DeviceConfigList.Items)
                {
                    var cItem = (DeviceMappingModels.Device)item;

                    cItem.Selected = false;
                    if (DeviceConfigList.SelectedItems.Contains(item))
                    {
                        cItem.Selected = true;
                    }
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
                }
            }
            catch
            {
            }
        }


        public void UpdateDeviceConfigUi(ControlDevice cd)
        {
            ConfigHere.Children.Clear();
            if (cd != null)
            {
                if (cd.Driver is ISimpleLedWithConfig drv)
                {
                    var cfgUI = drv.GetCustomConfig(cd);
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

            var derp = senderButton.DataContext;
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
            var items = (System.Windows.Controls.ListView)sender;
            
            var selected = (DeviceMappingModels.SourceModel)items.SelectedItem;
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

                if (castItem.Enabled && shouldEnable)
                {
                    castItem.Enabled = false;
                }
                else
                {
                    castItem.Enabled = shouldEnable;
                }


            }

            var vm = this.DataContext as DevicesViewModel;
            var selectedParents = vm.SLSDevices.Where(x => x.Selected == true);

            foreach (var parentDevice in selectedParents)
            {
                {
                    var profile = ApplicationManager.Instance.CurrentProfile;
                    if (profile.DeviceProfileSettings == null)
                    {
                        profile.DeviceProfileSettings =
                            new ObservableCollection<DeviceMappingModels.NGDeviceProfileSettings>();
                    }

                    var configDevice = profile.DeviceProfileSettings.FirstOrDefault(x =>
                        x.Name == parentDevice.Name && x.ProviderName == parentDevice.ProviderName);

                    if (configDevice == null)
                    {
                        configDevice = new DeviceMappingModels.NGDeviceProfileSettings
                        {
                            Name = parentDevice.Name,
                            ProviderName = parentDevice.ProviderName,
                        };

                        profile.DeviceProfileSettings.Add(configDevice);
                    }


                    if (selected != null)
                    {
                        configDevice.SourceName = selected?.Name;
                        configDevice.SourceProviderName = selected?.ProviderName;
                    }
                    else
                    {
                        configDevice.SourceName = null;
                        configDevice.SourceProviderName = null;

                        foreach (var controlDeviceLeD in parentDevice.ControlDevice.LEDs)
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
                foreach (var parentDevice in selectedParents)
                {
                    foreach (var controlDeviceLeD in parentDevice.ControlDevice.LEDs)
                    {
                        controlDeviceLeD.Color = new LEDColor(0, 0, 0);
                    }

                    if (parentDevice.SupportsPush)
                    {
                        parentDevice.ControlDevice.Push();
                    }
                }
            }
        }


        private void DevicesListCondensedSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = this.DataContext as DevicesViewModel;
            bool zeroDevicesPreSelected = vm.SLSDevices.All(x => x.Selected == false);

            foreach (var item in DeviceConfigCondensedList.Items)
            {
                var cItem = (DeviceMappingModels.Device)item;

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


     
    }
}
