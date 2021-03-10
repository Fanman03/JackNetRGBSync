using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SharpCompress.Archives;
using SimpleLed;
using SyncStudio.Core.Helpers;
using SyncStudio.Core.Models;
using SyncStudio.Domain;

namespace SyncStudio.Core.Services.Store
{
    public class Store : IStore
    {
        private readonly SimpleLedApiClient apiClient;
        public Store()
        {
            apiClient = new SimpleLedApiClient();
        }

        public async Task<IEnumerable<ProviderInfo>> GetStoreProviders()
        {
            var products = await apiClient.GetProducts();
            return products.Select(driverProperties => new ProviderInfo
            {
                Author = driverProperties.Author,
                AuthorId = driverProperties.AuthorId,
                Blurb = driverProperties.Blurb,
                Image = GetIcon(driverProperties.ProductId, driverProperties.Name),
                IsPublicRelease = driverProperties.IsPublicRelease,
                Name = driverProperties.Name,
                ProviderId = driverProperties.ProductId,
                Version = driverProperties.CurrentVersion,
                InstanceId = driverProperties.InstanceId
            });
        }

        public IEnumerable<ProviderInfo> GetInstalledProviders() =>
            ServiceManager.SLSManager.Drivers.Select(x => x.GetProperties()).Select(x => new ProviderInfo
            {
                Author = x.Author,
                AuthorId = x.AuthorId,
                Blurb = x.Blurb,
                InstanceId = x.InstanceId,
                Name = x.Name,
                IsPublicRelease = x.IsPublicRelease,
                ProviderId = x.ProductId,
                Version = x.CurrentVersion,
                Image = GetIcon(x.ProductId, x.Name)
            });


        public async Task<bool> InstallProvider(Guid providerId, ReleaseNumber version, Action<string> setInstallingMessage, Action<int> setInstallingPercentage)
        {
            RemoveProvider(providerId, setInstallingMessage, setInstallingPercentage);

            bool anyFail = false;
            string pluginPath = ServiceManager.SLSPROVIDER_DIRECTORY + "\\" + providerId;

            byte[] buffer = await apiClient.GetProduct(providerId, version);

            try
            {
                Directory.CreateDirectory(pluginPath);
            }
            catch
            {
            }

            using (Stream stream = new MemoryStream(buffer))
            {
                IArchive thingy = ArchiveFactory.Open(stream);

                float mx = thingy.Entries.Count();
                int ct = 0;

                foreach (IArchiveEntry archiveEntry in thingy.Entries)
                {
                    bool success = false;
                    int attempt = 0;

                    while (attempt < 10 && !success
)
                    {
                        try
                        {
                            archiveEntry.WriteToDirectory(pluginPath);
                            success = true;
                        }
                        catch
                        {
                            attempt++;
                            Thread.Sleep(100);
                        }
                    }

                    if (!success
)
                    {
                        anyFail = true;
                    }

                    ct++;

                    setInstallingPercentage?.Invoke((int)(ct / mx) * 100);
                }

                try
                {
                    File.Delete(pluginPath + "\\SimpleLed.dll");
                }
                catch
                {
                }

            }

            LoadPluginFolder(pluginPath);

            return anyFail;
        }

        public bool RemoveProvider(Guid providerId, Action<string> setInstallingMessage, Action<int> setInstallingPercentage)
        {
            var exist = ServiceManager.SLSManager.Drivers.FirstOrDefault(x => x.GetProperties().ProductId == providerId);

            if (exist != null)
            {
                setInstallingMessage?.Invoke("Stopping Existing Driver");
                ServiceManager.SLSManager.Drivers.Remove(exist);
                Thread.Sleep(100);
                exist.Dispose();
                Thread.Sleep(1000);
            }

            string pluginPath = ServiceManager.SLSPROVIDER_DIRECTORY + "\\" + providerId;
            if (Directory.Exists(pluginPath))
            {
                try
                {
                    setInstallingMessage?.Invoke("Removing Existing Folder");
                    Directory.Delete(pluginPath, true);
                }
                catch
                {
                }
            }

            return true;
        }


        public bool UnloadProvider(ISimpleLed exist)
        {

            if (exist != null)
            {
                ServiceManager.SLSManager.Drivers.Remove(exist);
                Thread.Sleep(100);
                exist.Dispose();
                Thread.Sleep(1000);
            }

            return true;
        }


        private BitmapImage GetIcon(Guid id, string name)
        {
            try
            {
                if (!Directory.Exists("icons"))
                {
                    try
                    {
                        Directory.CreateDirectory("icons");
                    }
                    catch
                    {
                    }
                }

                string fileName = name + "-" + id + ".png";

                if (File.Exists("icons\\" + fileName))
                {
                    using (Bitmap bm = new Bitmap("icons\\" + fileName))
                    {
                        return (bm.ToBitmapImage());
                    }
                }
                else
                {

                    string imageUrl = "http://cdn.ezmuze.co.uk/SimpleLedIcons/" + id + ".png";
                    Debug.WriteLine("Trying to fetch: " + imageUrl);
                    WebClient webClient = new WebClient();
                    try
                    {
                        using (Stream stream = webClient.OpenRead(imageUrl))
                        {
                            // make a new bmp using the stream
                            using (Bitmap bitmap = new Bitmap(stream))
                            {
                                //flush and close the stream
                                stream.Flush();
                                stream.Close();
                                // write the bmp out to disk
                                BitmapImage image = bitmap.ToBitmapImage();
                                bitmap.Save("icons\\" + fileName);
                                return image;
                            }
                        }
                    }
                    catch
                    {
                        using (Stream stream = webClient.OpenRead("http://cdn.ezmuze.co.uk/SimpleLedIcons/Unknown.png"))
                        {
                            // make a new bmp using the stream
                            using (Bitmap bitmap = new Bitmap(stream))
                            {
                                //flush and close the stream
                                stream.Flush();
                                stream.Close();
                                // write the bmp out to disk
                                BitmapImage image = bitmap.ToBitmapImage();
                                bitmap.Save("icons\\" + fileName);
                                return image;
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private void LoadPlugin(string file)
        {
            string filename = file.Split('\\').Last();
            string justPath = file.Substring(0, file.Length - filename.Length);
            if (filename.ToLower().StartsWith("driver") || filename.ToLower().StartsWith("source") || filename.ToLower().StartsWith("gameintegration"))
            {
                try
                {
                    ISimpleLed slsDriver = TypeLoaderExtensions.LoadDll(justPath, filename);

                    if (slsDriver != null)
                    {
                        Console.Write(slsDriver.Name().PadRight(40,'.'));
                        try
                        {
                            if (ServiceManager.SLSManager.Drivers.All(p => p.GetProperties().ProductId != slsDriver.GetProperties().ProductId))
                            {
                                slsDriver.DeviceAdded += SlsDriver_DeviceAdded;
                                slsDriver.DeviceRemoved += SlsDriver_DeviceRemoved;
                                ServiceManager.SLSManager.Drivers.Add(slsDriver);
                                string pad = "";
                                try
                                {
                                    slsDriver.Configure(new DriverDetails() { HomeFolder = justPath });
                                    Console.Write("Configured");
                                    pad = "..........";
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }

                                Console.Write(pad+"Initialized");

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        Console.WriteLine("");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void LoadPluginFolder(string pluginFolder)
        {
            string[] files = Directory.GetFiles(pluginFolder, "*.dll");

            List<Guid> driversAdded = new List<Guid>();
            foreach (string file in files)
            {
                //Console.WriteLine("Checking " + file);
                LoadPlugin(file);
            }
        }

        public void UnloadSLSProviders()
        {
            foreach (ISimpleLed slsManagerDriver in ServiceManager.SLSManager.Drivers)
            {
                UnloadProvider(slsManagerDriver);
            }
        }

        private void SlsDriver_DeviceRemoved(object sender, Events.DeviceChangeEventArgs e)
        {
            ServiceManager.Devices.RemoveDevice(e.ControlDevice);
        }

        private void SlsDriver_DeviceAdded(object sender, Events.DeviceChangeEventArgs e)
        {
            Debug.WriteLine("Adding device: " + e.ControlDevice.Name);

            //   t.Stop();
            DeviceOverrides overriden = ServiceManager.Devices.GetOverride(e.ControlDevice);
            if (overriden != null)
            {
                ServiceManager.Devices.SetOverride(e.ControlDevice, overriden);
            }

            ServiceManager.Devices.AddDevice(e.ControlDevice);
        }
    }
}
