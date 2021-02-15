using RGBSyncStudio.Helper;
using RGBSyncStudio.Model;
using SharpCompress.Archives;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RGBSyncStudio.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Store.xaml
    /// </summary>
    public partial class Store : UserControl
    {
        public Store()
        {
            InitializeComponent();
            ServiceManager.Instance.SLSManager.RescanRequired += SLSManagerOnRescanRequired;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                (this.DataContext as StoreViewModel)?.InitAsync();
            }
            else
            {
                vm.Plugins = new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>(new List<PositionalAssignment.PluginDetailsViewModel>
                {
                    new PositionalAssignment.PluginDetailsViewModel{Author = "Author", Blurb = "Blurb", Id = "xxx", Installed = true}
                });

                vm.ShowInstalled = true;
            }
        }

        private void SLSManagerOnRescanRequired(object sender, EventArgs e)
        {
            //  ReloadAllPlugins(sender, new RoutedEventArgs());
        }

        private StoreViewModel vm => (StoreViewModel)DataContext;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.PluginSearch = this.PluginSearchBox.Text;
        }

        private void ToggleExperimental(object sender, RoutedEventArgs e)
        {
            vm.ShowPreRelease = !vm.ShowPreRelease;
        }

        
        private void RefreshStore(object sender, RoutedEventArgs e)
        {
            vm.LoadStoreAndPlugins();
        }

        private void DisablePlugin(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DeletePlugin(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.DataContext is PositionalAssignment.PluginDetailsViewModel tvm)
                {
                    PositionalAssignment.PluginDetailsViewModel vmplugin = vm.Plugins.First(x => x.PluginId == tvm.PluginId);

                    ISimpleLed existingPlugin = ServiceManager.Instance.SLSManager.Drivers.First(x => x.GetProperties().Id == tvm.PluginId);

                    ServiceManager.Instance.StoreService.DeletePlugin(existingPlugin);

                    vm.Plugins.Remove(vmplugin);

                    vm.RefreshPlungs();
                }
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                if (grid.DataContext is PositionalAssignment.PluginDetailsViewModel tvm)
                {
                    tvm.IsHovered = true;
                }
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                if (grid.DataContext is PositionalAssignment.PluginDetailsViewModel tvm)
                {
                    tvm.IsHovered = false;
                }
            }
        }

        private void ConfigurePlugin(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.DataContext is PositionalAssignment.PluginDetailsViewModel tvm)
                {
                    ServiceManager.Instance.StoreService.ShowPlugInUI(tvm.PluginId, ConfigHere);
                        vm.ShowConfig = true;
                        double i = 1 / 20d;
                    
                }
            }
        }

        private void CloseConfigure(object sender, RoutedEventArgs e)
        {
            vm.ShowConfig = false;
        }

        private void ToggleStore(object sender, RoutedEventArgs e)
        {
            vm.ShowUpdates = false;
            vm.ShowStore = true;
            vm.ShowInstalled = false;
        }

        private void ToggleUpdates(object sender, RoutedEventArgs e)
        {
            vm.ShowUpdates = true;
            vm.ShowStore = false;
            vm.ShowInstalled = false;
        }

        private void ToggleInstalled(object sender, RoutedEventArgs e)
        {
            vm.ShowUpdates = false;
            vm.ShowStore = false;
            vm.ShowInstalled = true;
        }

        private void PluginSearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            PluginSearchBox.Width = 350;
        }

        private void PluginSearchBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            PluginSearchBox.Width = 100;
        }

        private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
            {
                return;
            }

            if (e.RoutedEvent.Name == "SelectionChanged")
            {
                ComboBox parent = e.Source as ComboBox;
                PositionalAssignment.PluginDetailsViewModel parentDC = parent.DataContext as PositionalAssignment.PluginDetailsViewModel;
                bool anyFail = false;


                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    PositionalAssignment.PluginVersionDetails bdc = e.AddedItems[0] as PositionalAssignment.PluginVersionDetails;

                   bool success= await ServiceManager.Instance.StoreService.InstallPlugin(parentDC.PluginId, bdc);
                   if (success)
                   {
                       vm.LoadStoreAndPlugins();
                    }

                }

            }
            //  throw new NotImplementedException();
        }
    }
}
