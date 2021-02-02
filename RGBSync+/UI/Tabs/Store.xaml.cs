using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;
using SharpCompress.Archives;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

namespace RGBSyncPlus.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Store.xaml
    /// </summary>
    public partial class Store : UserControl
    {
        public Store()
        {
            InitializeComponent();
            ApplicationManager.Instance.SLSManager.RescanRequired += SLSManagerOnRescanRequired;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                (this.DataContext as StoreViewModel)?.Init();
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

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString(), CultureInfo.InvariantCulture);
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString(), CultureInfo.InvariantCulture);
            double percentage = bytesIn / totalBytes * 100;

            installingModal?.UpdateModalPercentage(mainVm, (int)percentage);
        }

        private SimpleModal installingModal;

        private async void InstallPlugin(object sender, RoutedEventArgs e)
        {

            using (installingModal = new SimpleModal(mainVm, "Installing..."))
            {
                ApplicationManager.Instance.PauseSyncing = true;
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                ApplicationManager.Instance.UnloadSLSProviders();


                if (((Button)sender).DataContext is PositionalAssignment.PluginDetailsViewModel bdc)
                {
                    PositionalAssignment.PluginDetailsViewModel newest = bdc.Versions.First(x => x.Version == bdc.Version);

                    SimpleLedApiClient apiClient = new SimpleLedApiClient();
                    byte[] drver = await apiClient.GetProduct(newest.PluginId, new ReleaseNumber(bdc.Version));

                    string pluginPath = ApplicationManager.SLSPROVIDER_DIRECTORY + "\\" + bdc.PluginId;
                    if (Directory.Exists(pluginPath))
                    {
                        try
                        {
                            Directory.Delete(pluginPath, true);
                        }
                        catch
                        {
                        }
                    }

                    try
                    {
                        Directory.CreateDirectory(pluginPath);
                    }
                    catch
                    {
                    }

                    using (Stream stream = new MemoryStream(drver))
                    {
                        IArchive thingy = SharpCompress.Archives.ArchiveFactory.Open(stream);

                        float mx = thingy.Entries.Count();
                        int ct = 0;
                        foreach (IArchiveEntry archiveEntry in thingy.Entries)
                        {

                            archiveEntry.WriteToDirectory(pluginPath);
                            ct++;

                            installingModal?.UpdateModalPercentage(mainVm, (int)(ct / mx) * 100);
                        }

                        try
                        {
                            File.Delete(pluginPath + "\\SimpleLed.dll");
                        }
                        catch
                        {
                        }

                    }

                    ApplicationManager.Instance.LoadPlungFolder(pluginPath);
                }
            }

        }

        private MainWindowViewModel mainVm =>
            (MainWindowViewModel)ApplicationManager.Instance.ConfigurationWindow.DataContext;

        private void ReloadAllPlugins(object sender, RoutedEventArgs e)
        {
            StoreViewModel vm = (StoreViewModel)this.DataContext;
            using (new SimpleModal(mainVm, "Reloading Plugins"))
            {
                ContainingGrid.Refresh();
                ApplicationManager.Instance.UnloadSLSProviders();


                ApplicationManager.Instance.LoadSLSProviders();
                vm.LoadStoreAndPlugins();
                ApplicationManager.Instance.Rescan(this, new EventArgs());
            }

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
                    var vmplugin = vm.Plugins.First(x => x.PluginId == tvm.PluginId);

                    ISimpleLed existingPlugin = ApplicationManager.Instance.SLSManager.Drivers.First(x => x.GetProperties().Id == tvm.PluginId);

                    if (existingPlugin != null)
                    {
                        var removeList = ApplicationManager.Instance.SLSDevices.Where(x =>
                            x.Driver.GetProperties().Id == existingPlugin.GetProperties().Id).ToList();

                        while (ApplicationManager.Instance.SLSDevices.Any(x => x.Driver.GetProperties().Id == existingPlugin.GetProperties().Id) && removeList.Any())
                        {
                            try
                            {
                                var pp = removeList.First();
                                ApplicationManager.Instance.SLSDevices.Remove(pp);
                                removeList.Remove(pp);
                            }
                            catch
                            {
                            }

                        }
                    }


                    try
                    {
                        existingPlugin?.Dispose();
                    }
                    catch
                    {
                    }

                    Thread.Sleep(1000);

                    if (ApplicationManager.Instance.SLSManager.Drivers.Contains(existingPlugin))
                    {
                        ApplicationManager.Instance.SLSManager.Drivers.Remove(existingPlugin);
                    }

                    try
                    {
                        Directory.Delete("SLSProvider\\" + tvm.PluginId, true);
                    }
                    catch
                    {
                    }

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
                    ISimpleLed existingPlugin = ApplicationManager.Instance.SLSManager.Drivers.First(x => x.GetProperties().Id == tvm.PluginId);

                    ISimpleLedWithConfig testDrv = existingPlugin as ISimpleLedWithConfig;

                    if (existingPlugin is ISimpleLedWithConfig drv)
                    {
                        UserControl cfgUI = drv.GetCustomConfig(null);
                        ConfigHere.Children.Clear();
                        ConfigHere.Children.Add(cfgUI);

                        ConfigHere.HorizontalAlignment = HorizontalAlignment.Stretch;
                        ConfigHere.VerticalAlignment = VerticalAlignment.Stretch;

                        cfgUI.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        cfgUI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        cfgUI.VerticalAlignment = VerticalAlignment.Stretch;
                        cfgUI.VerticalContentAlignment = VerticalAlignment.Stretch;

                        cfgUI.Foreground = new SolidColorBrush(Colors.Black); //Make theme aware

                        vm.ShowConfig = true;
                        double i = 1 / 20d;
                    }
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
                using (installingModal = new SimpleModal(mainVm, "Installing..."))
                {
                    ApplicationManager.Instance.PauseSyncing = true;
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                    //ApplicationManager.Instance.UnloadSLSProviders();




                    if (e.AddedItems != null && e.AddedItems.Count > 0)
                    {
                        PositionalAssignment.PluginVersionDetails bdc = e.AddedItems[0] as PositionalAssignment.PluginVersionDetails;
                        //var newest = bdc.Versions.First(x => x.Version == bdc.Version);

                        SimpleLedApiClient apiClient = new SimpleLedApiClient();
                        byte[] drver = await apiClient.GetProduct(parentDC.PluginId, bdc.ReleaseNumber);

                        var exist = ApplicationManager.Instance.SLSManager.Drivers.FirstOrDefault(x =>
                            x.GetProperties().Id == parentDC.PluginId);
                        if (exist != null)
                        {
                         
                            ApplicationManager.Instance.SLSManager.Drivers.Remove(exist);
                            Thread.Sleep(100);
                            exist.Dispose();
                            Thread.Sleep(1000);
                        }



                        string pluginPath = ApplicationManager.SLSPROVIDER_DIRECTORY + "\\" + parentDC.PluginId;
                        if (Directory.Exists(pluginPath))
                        {
                            try
                            {
                                Directory.Delete(pluginPath, true);
                            }
                            catch
                            {
                            }
                        }

                        try
                        {
                            Directory.CreateDirectory(pluginPath);
                        }
                        catch
                        {
                        }
                        
                        using (Stream stream = new MemoryStream(drver))
                        {
                            IArchive thingy = SharpCompress.Archives.ArchiveFactory.Open(stream);

                            float mx = thingy.Entries.Count();
                            int ct = 0;
                            List<string> pluginPaths = new List<string>();
                            
                            foreach (IArchiveEntry archiveEntry in thingy.Entries)
                            {
                                bool suc = false;
                                int attemp = 0;

                                while (attemp < 10 && !suc)
                                {
                                    try
                                    {
                                        archiveEntry.WriteToDirectory(pluginPath);
                                        suc = true;
                                    }
                                    catch
                                    {
                                        attemp++;
                                        Thread.Sleep(100);
                                    }
                                }

                                if (!suc)
                                {
                                    anyFail = true;
                                }

                                ct++;

                                installingModal?.UpdateModalPercentage(mainVm, (int)(ct / mx) * 100);
                            }

                            try
                            {
                                File.Delete(pluginPath + "\\SimpleLed.dll");
                            }
                            catch
                            {
                            }

                        }



                        try
                        {
                            ApplicationManager.Instance.LoadPlungFolder(pluginPath);
                            vm.LoadStoreAndPlugins();
                        }
                        catch { }
                    }


                    //vm.ReloadStoreAndPlugins();
                    ApplicationManager.Instance.Rescan(this, new EventArgs());
                }

                if (anyFail)
                {

                    var error = new SimpleModal(mainVm, "One or more files failed to upgrade.");
                    


                }

            }
            //  throw new NotImplementedException();
        }
    }
}
