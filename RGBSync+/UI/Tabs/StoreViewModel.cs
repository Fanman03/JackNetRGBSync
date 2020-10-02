﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using RGBSyncPlus.Model;
using SimpleLed;

namespace RGBSyncPlus.UI.Tabs
{
    public class StoreViewModel : BaseViewModel
    {
        public StoreViewModel()
        {
            storeHandler = new StoreHandler();

            LoadStoreAndPlugins();
        }

        private MainWindowViewModel mainVm
        {
            get
            {
                var cfgWindow = ApplicationManager.Instance.ConfigurationWindow;
                if (cfgWindow != null)
                {
                    var cfgVm = cfgWindow.DataContext;
                    return cfgVm as MainWindowViewModel;
                }

                return null;

            }
        }

        private bool showPreRelease = false;

        public bool ShowPreRelease
        {
            get => showPreRelease;
            set
            {
                SetProperty(ref showPreRelease, value);
                FilterPlugins();
            }
        }

        private bool showConfig = false;

        public bool ShowConfig
        {
            get => showConfig;
            set => SetProperty(ref showConfig, value);
        }

        private string pluginSearch;

        public string PluginSearch
        {
            get => pluginSearch;
            set
            {
                SetProperty(ref pluginSearch, value);
                FilterPlugins();
            }
        }


        private ObservableCollection<PositionalAssignment.PluginDetailsViewModel> plugins;
        public ObservableCollection<PositionalAssignment.PluginDetailsViewModel> Plugins
        {
            get => plugins;
            set => SetProperty(ref plugins, value);
        }


        private ObservableCollection<PositionalAssignment.PluginDetailsViewModel> installedPlugins;
        public ObservableCollection<PositionalAssignment.PluginDetailsViewModel> StoreOnly
        {
            get => installedPlugins;
            set => SetProperty(ref installedPlugins, value);
        }


        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            try
            {
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }
            catch
            {
                return null;
            }
        }

        public void LoadStoreAndPlugins()
        {
            using (new SimpleModal(mainVm, "Refreshing store...."))
            {
                Plugins = new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>();
                StoreOnly = new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>();

                foreach (ISimpleLed slsManagerDriver in ApplicationManager.Instance.SLSManager.Drivers)
                {
                    DriverProperties pid = slsManagerDriver.GetProperties();

                    PositionalAssignment.PluginDetailsViewModel pvm = new PositionalAssignment.PluginDetailsViewModel
                    {
                       Author = pid.Author,
                       Blurb = pid.Blurb,
                       Id = pid.Id.ToString(),
                       Installed = true,
                       Name = slsManagerDriver.Name(),
                       PluginId = pid.Id,
                       Version = pid.CurrentVersion.ToString(),
                       Visible = true,
                       PluginDetails = new PositionalAssignment.PluginDetails
                       {
                           Version = pid.CurrentVersion,
                           Id = pid.Id.ToString(),
                           PluginId = pid.Id,
                           DriverProperties = pid,
                           Author = pid.Author,
                           Name = slsManagerDriver.Name(),
                           Repo = pid.GitHubLink
                       }
                       
                    };

                    Plugins.Add(pvm);
                }

                //SetUpDeviceMapViewModel();

                List<PositionalAssignment.PluginDetails> pp = storeHandler.DownloadStoreManifest();

                var ppu = pp.GroupBy(x => x.PluginId);

                

                foreach (IGrouping<Guid, PositionalAssignment.PluginDetails> pluginDetailses in ppu)
                {
                    var insertThis = pluginDetailses.First();

                    PositionalAssignment.PluginDetailsViewModel installedThingy = Plugins.FirstOrDefault(p => p.PluginId == insertThis.PluginId);
                    if (installedThingy!=null)
                    {
                        Plugins.Remove(installedThingy);
                    }

                    var tmp = new PositionalAssignment.PluginDetailsViewModel(insertThis);
                    tmp.Versions = new ObservableCollection<PositionalAssignment.PluginDetailsViewModel>(pluginDetailses
                        .Select(x => new PositionalAssignment.PluginDetailsViewModel(x, true)).ToList());

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

                        if (File.Exists("icons\\" + tmp.PluginDetails.PluginId + ".png"))
                        {
                            using (Bitmap bm = new Bitmap("icons\\" + tmp.PluginDetails.PluginId + ".png"))
                            {
                                tmp.Image = ToBitmapImage(bm);
                            }
                        }
                        else
                        {

                            string imageUrl = "https://github.com/SimpleLed/Store/raw/master/Icons/" +
                                              tmp.PluginDetails.PluginId + ".png";
                            Debug.WriteLine("Trying to fetch: " + imageUrl);
                            var webClient = new WebClient();
                            using (Stream stream = webClient.OpenRead(imageUrl))
                            {
                                // make a new bmp using the stream
                                using (Bitmap bitmap = new Bitmap(stream))
                                {
                                    //flush and close the stream
                                    stream.Flush();
                                    stream.Close();
                                    // write the bmp out to disk
                                    tmp.Image = ToBitmapImage(bitmap);
                                    bitmap.Save("icons\\" + tmp.PluginDetails.PluginId + ".png");
                                }
                            }
                        }
                    }
                    catch
                    {
                    }

                    Plugins.Add(tmp);
                    StoreOnly.Add(tmp);

                }

                FilterPlugins();

            }
        }

        public void FilterPlugins()
        {
            foreach (var pluginDetailsViewModel in Plugins)
            {
                ReleaseNumber highestApplicable;

                ISimpleLed newestPublicModel;
                ISimpleLed newestExperimentalModel;

                ISimpleLed installedVersion = null;
                ReleaseNumber newestPublicFound = new ReleaseNumber(0, 0, 0, 0);
                ReleaseNumber newestExperimentalFound = new ReleaseNumber(0, 0, 0, 0);
                ReleaseNumber installed = new ReleaseNumber(0, 0, 0, 0);

                if (ApplicationManager.Instance.SLSManager.Drivers.Any(x => x.GetProperties().Id == pluginDetailsViewModel.PluginId))
                {
                    installedVersion = ApplicationManager.Instance.SLSManager.Drivers.First(x => x.GetProperties().Id == pluginDetailsViewModel.PluginId);
                    installed = installedVersion.GetProperties().CurrentVersion;
                    pluginDetailsViewModel.Installed = true;
                }
                else
                {
                    pluginDetailsViewModel.Installed = false;
                }

                foreach (var detailsViewModel in pluginDetailsViewModel.Versions)
                {
                    if (detailsViewModel.PluginDetails.DriverProperties.CurrentVersion != null &&
                        detailsViewModel.PluginDetails.DriverProperties.CurrentVersion > newestPublicFound &&
                        detailsViewModel.PluginDetails.DriverProperties.IsPublicRelease)
                    {
                        newestPublicFound = detailsViewModel.PluginDetails.DriverProperties.CurrentVersion;
                    }

                    if (detailsViewModel.PluginDetails.DriverProperties.CurrentVersion != null &&
                        detailsViewModel.PluginDetails.DriverProperties.CurrentVersion > newestExperimentalFound &&
                        !detailsViewModel.PluginDetails.DriverProperties.IsPublicRelease)
                    {
                        newestExperimentalFound = detailsViewModel.PluginDetails.DriverProperties.CurrentVersion;
                    }
                }

                pluginDetailsViewModel.NewestPreReleaseVersion = newestExperimentalFound.ToString();
                pluginDetailsViewModel.NewestPublicVersion = newestPublicFound.ToString();

                if (ShowPreRelease || (installedVersion != null && !installedVersion.GetProperties().IsPublicRelease))
                {
                    highestApplicable = newestExperimentalFound;
                    if (highestApplicable < newestPublicFound)
                    {
                        highestApplicable = newestPublicFound;
                    }
                }
                else
                {
                    highestApplicable = newestPublicFound;
                }

                PositionalAssignment.PluginDetailsViewModel newest = pluginDetailsViewModel.Versions.FirstOrDefault(x => x.Version == highestApplicable.ToString());

                if (newest == null && highestApplicable.ToString() == "0.0.0.0000")
                {
                    newest = pluginDetailsViewModel.Versions.FirstOrDefault(x => x.Version == null);
                }

                if (StoreOnly.All(x => x.PluginId != pluginDetailsViewModel.PluginId))
                {
                    newest = pluginDetailsViewModel;
                }

                if (newest == null)
                {
                    pluginDetailsViewModel.Visible = false;
                }
                else
                {
                    pluginDetailsViewModel.Visible = true;
                    pluginDetailsViewModel.Version = highestApplicable.ToString();
                    pluginDetailsViewModel.Name = newest.Name;
                    pluginDetailsViewModel.Author = newest.Author;

                    pluginDetailsViewModel.PreRelease = !newest.PluginDetails.DriverProperties.IsPublicRelease;

                    pluginDetailsViewModel.InstalledButOutdated = pluginDetailsViewModel.Installed && highestApplicable > installedVersion.GetProperties().CurrentVersion;

                    pluginDetailsViewModel.Visible =
                        (ShowPreRelease || !pluginDetailsViewModel.PreRelease) &&
                        (string.IsNullOrWhiteSpace(PluginSearch) ||
                         pluginDetailsViewModel.Name.ToLower().Contains(PluginSearch.ToLower())
                         ||
                         pluginDetailsViewModel.Author.ToLower().Contains(PluginSearch.ToLower())
                         ||
                         (pluginDetailsViewModel.Blurb != null &&
                          pluginDetailsViewModel.Blurb.ToLower().Contains(PluginSearch.ToLower()))
                        );
                }


            }
        }

        private StoreHandler storeHandler;
    }
}