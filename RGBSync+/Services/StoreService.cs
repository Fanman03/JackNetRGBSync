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
using MarkdownUI.WPF;
using SyncStudio.WPF.Model;
using SyncStudio.WPF.UI.Tabs;
using SharpCompress.Archives;
using SimpleLed;
using SyncStudio.Core.Models;
using SyncStudio.WPF.UI;

namespace SyncStudio.WPF.Services
{
    public class StoreService
    {
        private SimpleModal installingModal;


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

        public void ShowPlugInUI(Guid pluginID, Grid designationGrid)
        {
            var herp = SyncStudio.Core.ServiceManager.SLSManager.Drivers.ToList();
            var derp = ServiceManager.Instance.SLSManager.Drivers.ToList();
            List<ProviderInfo> berks = SyncStudio.Core.ServiceManager.Store.GetInstalledProviders().ToList();

            Dictionary<Guid, ISimpleLed> lookup = herp.ToDictionary(simpleLed => simpleLed.GetProperties().ProductId);


            ISimpleLed existingPlugin = lookup[pluginID];
            // berks.First(x =>  x.GetProperties().ProductId == pluginID);

            ISimpleLedWithConfig testDrv = existingPlugin as ISimpleLedWithConfig;

            if (existingPlugin is ISimpleLedWithConfig drv)
            {


                MarkdownUIBundle cfgUI = drv.GetCustomConfig(null);
                designationGrid.Children.Clear();
                StackPanel sp = new StackPanel();
                designationGrid.Children.Add(sp);

                ResourceDictionary theme = new ResourceDictionary { Source = new Uri(@"\UI\MarkdownDark.xaml", UriKind.Relative) };
                if (Core.ServiceManager.SLSManager.GetTheme() == ThemeWatcher.WindowsTheme.Dark)
                {
                    ((SolidColorBrush)theme["Primary"]).Color = Colors.Black;
                    ((SolidColorBrush)theme["Secondary"]).Color = Colors.White;
                }
                else
                {
                    ((SolidColorBrush)theme["Primary"]).Color = Colors.White;
                    ((SolidColorBrush)theme["Secondary"]).Color = Colors.Black;
                }
                ((SolidColorBrush)theme["AccentColor"]).Color = Core.ServiceManager.SLSManager.GetAccent();


                MarkdownUIHandler handler = new MarkdownUIHandler(cfgUI.Markdown, cfgUI.ViewModel, theme);
                handler.RenderToUI(sp);



                //todo
                //designationGrid.Children.Add(cfgUI);

                //designationGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                //designationGrid.VerticalAlignment = VerticalAlignment.Stretch;

                //cfgUI.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                //cfgUI.HorizontalAlignment = HorizontalAlignment.Stretch;
                //cfgUI.VerticalAlignment = VerticalAlignment.Stretch;
                //cfgUI.VerticalContentAlignment = VerticalAlignment.Stretch;

                //cfgUI.Foreground = new SolidColorBrush(Colors.Black); //Make theme aware
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
