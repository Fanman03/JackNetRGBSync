using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.Model;
using RGBSyncPlus.UI;
using SharpCompress.Archives;
using SimpleLed;

namespace RGBSyncPlus.Services
{
    public class StoreService
    {
        private SimpleModal installingModal;

        private async void InstallPlugin()
        {

            using (installingModal = new SimpleModal(mainVm, "Installing..."))
            {
                ServiceManager.Instance.LedService.PauseSyncing = true;
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                ServiceManager.Instance.LedService.UnloadSLSProviders();


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

                    ServiceManager.Instance.LedService.LoadPlungFolder(pluginPath);
                }
            }

        }
    }
}
