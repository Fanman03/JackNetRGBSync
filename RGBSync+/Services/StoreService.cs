using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RGBSyncStudio.Model;
using RGBSyncStudio.UI;
using RGBSyncStudio.UI.Tabs;
using SharpCompress.Archives;
using SimpleLed;

namespace RGBSyncStudio.Services
{
    public class StoreService
    {
        private SimpleModal installingModal;

        public async void InstallPlugin(PositionalAssignment.PluginDetailsViewModel bdc)
        {

            using (installingModal = new SimpleModal(ServiceManager.Instance.ApplicationManager.MainViewModel, "Installing..."))
            {
                ServiceManager.Instance.LedService.PauseSyncing = true;
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                ServiceManager.Instance.LedService.UnloadSLSProviders();


                //if (((Button)sender).DataContext is PositionalAssignment.PluginDetailsViewModel bdc)
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

                            installingModal?.UpdateModalPercentage(ServiceManager.Instance.ApplicationManager.MainViewModel, (int)(ct / mx) * 100);
                        }

                        try
                        {
                            File.Delete(pluginPath + "\\SimpleLed.dll");
                        }
                        catch
                        {
                        }

                    }

                    ServiceManager.Instance.LedService.LoadPlungFolder(pluginPath);
                }
            }

        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString(), CultureInfo.InvariantCulture);
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString(), CultureInfo.InvariantCulture);
            double percentage = bytesIn / totalBytes * 100;

            installingModal?.UpdateModalPercentage(ServiceManager.Instance.ApplicationManager.MainViewModel, (int)percentage);
        }


        private void ReloadAllPlugins(object sender, RoutedEventArgs e)
        {

            using (new SimpleModal(ServiceManager.Instance.ApplicationManager.MainViewModel, "Reloading Plugins"))
            {
                ServiceManager.Instance.LedService.UnloadSLSProviders();

                ServiceManager.Instance.LedService.LoadSLSProviders();
                //LoadStoreAndPlugins();
                //ServiceManager.Instance.LedService.Rescan(this, new EventArgs());
            }

        }

        public void DeletePlugin(ISimpleLed existingPlugin)
        {
            Guid pluginId = existingPlugin.GetProperties().Id;
            if (existingPlugin != null)
            {
                List<ControlDevice> removeList = ServiceManager.Instance.LedService.SLSDevices.Where(x => x.Driver.GetProperties().Id == existingPlugin.GetProperties().Id).ToList();

                while (ServiceManager.Instance.LedService.SLSDevices.Any(x => x.Driver.GetProperties().Id == existingPlugin.GetProperties().Id) && removeList.Any())
                {
                    try
                    {
                        ControlDevice pp = removeList.First();
                        ServiceManager.Instance.LedService.SLSDevices.Remove(pp);
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

            if (ServiceManager.Instance.SLSManager.Drivers.Contains(existingPlugin))
            {
                ServiceManager.Instance.SLSManager.Drivers.Remove(existingPlugin);
            }

            try
            {
                Directory.Delete("SLSProvider\\" + pluginId, true);
            }
            catch
            {
            }

        }

        public void RemoveInstalledPlugin(ISimpleLed exist)
        {
            Guid id = exist.GetProperties().ProductId;

            ServiceManager.Instance.SLSManager.Drivers.Remove(exist);
            Thread.Sleep(100);
            exist.Dispose();
            Thread.Sleep(1000);


            string pluginPath = ApplicationManager.SLSPROVIDER_DIRECTORY + "\\" + id;
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
        }

        public async Task<bool> InstallPlugin(Guid pluginId, ReleaseNumber releaseNumber)
        {
            bool anyFail = false;
            string pluginPath = ApplicationManager.SLSPROVIDER_DIRECTORY + "\\" + pluginId;
            SimpleLedApiClient apiClient = new SimpleLedApiClient();
            byte[] drver = await apiClient.GetProduct(pluginId, releaseNumber);

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

                    installingModal?.UpdateModalPercentage(ServiceManager.Instance.ApplicationManager.MainViewModel,
                        (int)(ct / mx) * 100);
                }

                try
                {
                    File.Delete(pluginPath + "\\SimpleLed.dll");
                }
                catch
                {
                }

            }

            return anyFail;
        }
        public async Task<bool> InstallPlugin(Guid parentDCPluginId, PositionalAssignment.PluginVersionDetails bdc)
        {
            string pluginPath = ApplicationManager.SLSPROVIDER_DIRECTORY + "\\" + parentDCPluginId;
            bool anyFail = false;

            using (installingModal = new SimpleModal(ServiceManager.Instance.ApplicationManager.MainViewModel, "Installing..."))
            {

                ISimpleLed exist = ServiceManager.Instance.SLSManager.Drivers.FirstOrDefault(x => x.GetProperties().Id == parentDCPluginId);
                if (exist != null)
                {
                    RemoveInstalledPlugin(exist);
                }

                anyFail = await InstallPlugin(parentDCPluginId, bdc.ReleaseNumber);

                try
                {
                    ServiceManager.Instance.LedService.LoadPlungFolder(pluginPath);
                }
                catch
                {
                }

            }

            //vm.ReloadStoreAndPlugins();
            // ServiceManager.Instance.ApplicationManager.Rescan(this, new EventArgs());

            if (anyFail)
            {
                SimpleModal error = new SimpleModal(ServiceManager.Instance.ApplicationManager.MainViewModel,
                    "One or more files failed to upgrade.");
            }

            return !anyFail;
        }

        public void ShowPlugInUI(Guid pluginID, Grid designationGrid)
        {
            ISimpleLed existingPlugin = ServiceManager.Instance.SLSManager.Drivers.First(x => x.GetProperties().Id == pluginID);

            ISimpleLedWithConfig testDrv = existingPlugin as ISimpleLedWithConfig;

            if (existingPlugin is ISimpleLedWithConfig drv)
            {
                UserControl cfgUI = drv.GetCustomConfig(null);
                designationGrid.Children.Clear();
                designationGrid.Children.Add(cfgUI);

                designationGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                designationGrid.VerticalAlignment = VerticalAlignment.Stretch;

                cfgUI.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                cfgUI.HorizontalAlignment = HorizontalAlignment.Stretch;
                cfgUI.VerticalAlignment = VerticalAlignment.Stretch;
                cfgUI.VerticalContentAlignment = VerticalAlignment.Stretch;

                cfgUI.Foreground = new SolidColorBrush(Colors.Black); //Make theme aware
            }
        }

        public async Task<List<DriverProperties>> GetDriversFromStore()
        {
            SimpleLedApiClient apiClient = new SimpleLedApiClient();
            List<DriverProperties> storePlugins = await apiClient.GetProducts();
            return storePlugins;
        }

    }
}
