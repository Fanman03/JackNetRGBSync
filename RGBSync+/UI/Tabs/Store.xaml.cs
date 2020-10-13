using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;
using SharpCompress.Archives;
using SimpleLed;
using Path = System.IO.Path;

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
        }

        private StoreViewModel vm => (StoreViewModel) DataContext;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.PluginSearch = this.PluginSearchBox.Text;
        }

        private void ToggleExperimental(object sender, RoutedEventArgs e)
        {
            vm.ShowPreRelease = !vm.ShowPreRelease;
        }

        private void InstallPlugin(object sender, RoutedEventArgs e)
        {
            
            using (new SimpleModal(mainVm, "Installing..."))
            {
                ApplicationManager.Instance.PauseSyncing = true;
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                ApplicationManager.Instance.UnloadSLSProviders();


                if (((Button)sender).DataContext is PositionalAssignment.PluginDetailsViewModel bdc)
                {
                    var newest = bdc.Versions.First(x => x.Version == bdc.Version);
                    //if (!vm.ShowPreRelease)
                    //{
                    //    newest = bdc.Versions.Where(x=>x.PluginDetails.DriverProperties.IsPublicRelease).OrderByDescending(x => x.PluginDetails.Version).First();
                    //}

                    string url = $"https://github.com/SimpleLed/Store/blob/master/{newest.Id}.7z?raw=true";

                    WebClient webClient = new WebClient();
                    string destPath = Path.GetTempPath() + bdc.PluginDetails.Id + ".7z";
                    webClient.DownloadFile(url, destPath);

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

                    using (Stream stream = File.OpenRead(destPath))
                    {
                        var thingy = SharpCompress.Archives.ArchiveFactory.Open(stream);

                        foreach (var archiveEntry in thingy.Entries)
                        {

                            archiveEntry.WriteToDirectory(pluginPath);
                        }

                        try
                        {
                            File.Delete(pluginPath + "\\SimpleLed.dll");
                        }
                        catch
                        {
                        }

                    }
                }

                ApplicationManager.Instance.LoadSLSProviders();
                vm.LoadStoreAndPlugins();
            }

        }

        private MainWindowViewModel mainVm =>
            (MainWindowViewModel) ApplicationManager.Instance.ConfigurationWindow.DataContext;

        private void ReloadAllPlugins(object sender, RoutedEventArgs e)
        {
            StoreViewModel vm = (StoreViewModel)this.DataContext;
            using (new SimpleModal(mainVm, "Reloading Plugins"))
            {
                ContainingGrid.Refresh();
                ApplicationManager.Instance.UnloadSLSProviders();


                ApplicationManager.Instance.LoadSLSProviders();
                vm.LoadStoreAndPlugins();
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
            throw new NotImplementedException();
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
                        var cfgUI = drv.GetCustomConfig(null);
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
            vm.ShowStore = !vm.ShowStore;
        }

        private void ToggleUpdates(object sender, RoutedEventArgs e)
        {
            vm.ShowUpdates = !vm.ShowUpdates;
        }

        private void PluginSearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            PluginSearchBox.Width = 350;
        }

        private void PluginSearchBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            PluginSearchBox.Width = 100;
        }
    }
}
