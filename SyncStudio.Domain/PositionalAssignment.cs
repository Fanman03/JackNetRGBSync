using Newtonsoft.Json;
using SimpleLed;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace SyncStudio.WPF.Model
{
    public class PositionalAssignment
    {
        public enum DevicePosition
        {
            Front,
            Back,
            Top,
            Bottom,
            PCI,
            Mouse,
            Keyboard,
        }

        public class PluginDetails
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public Guid PluginId { get; set; }

            //public DriverProperties DriverProperties { get; set; }
            public ReleaseNumber Version { get; set; }
            public string Author { get; set; }
            public string Repo { get; set; }

            public PluginDetails()
            {

            }

        }

        public class PluginVersionDetails
        {
            public bool IsExperimental { get; set; }
            public ReleaseNumber ReleaseNumber { get; set; }
            public bool IsInstalled { get; set; }
        }

        public class PluginDetailsViewModel
        {
            public ObservableCollection<PluginVersionDetails> VersionsAvailable { get; set; }
            
            public BitmapImage Image { get; set; }
            public string Name { get; set; }
            public string Author { get; set; }
            public string Version { get; set; }
            public string NewestPublicVersion { get; set; }
            public string NewestPreReleaseVersion { get; set; }
            public string InstalledVersion { get; set; }
            public string Blurb { get; set; }
            public Guid PluginId { get; set; }
            public string Id { get; set; }

            public PluginDetails PluginDetails;
            
            public int Releases { get; set; }
            public bool PreRelease { get; set; }
            public bool Installed { get; set; }
            public bool InstalledButOutdated { get; set; }

            public bool Visible { get; set; }
            public ObservableCollection<PluginDetailsViewModel> Versions { get; set; } = new ObservableCollection<PluginDetailsViewModel>();

            [JsonIgnore]
            public bool IsHovered { get; set; }
            public PluginVersionDetails InstalledVersionModel { get; set; }

            public PluginDetailsViewModel()
            {
            }

            public PluginDetailsViewModel(PluginDetails inp, bool dontChild = false)
            {
                string versionAsString = inp.Version != null ? inp.Version.ToString() : "0.0.0.0";

                Name = inp.Name;
                Author = inp.Author;
                Version = versionAsString;
                
                PluginDetails = inp;
                PluginId = inp.PluginId;
                Id = inp.Id;
                if (!dontChild)
                {
                    Versions.Add(new PluginDetailsViewModel(inp, true));
                }

                Releases = 1;
            }

            //public PluginDetailsViewModel(DriverProperties inp, bool dontChild = false)
            //{
            //    string versionAsString = inp.CurrentVersion != null ? inp.CurrentVersion.ToString() : "0.0.0.0";

            //    Name = inp.Name;
            //    Author = inp.Author;
            //    Version = versionAsString;
            //    Blurb = inp.Blurb;
            //    PluginDetails = inp;
            //    PluginId = inp.ProductId;
            //    Id = inp.Id.ToString();
            //    if (!dontChild)
            //    {
            //        Versions.Add(new PluginDetailsViewModel(inp, true));
            //    }

            //    Releases = 1;
            //}
        }
    }
}
