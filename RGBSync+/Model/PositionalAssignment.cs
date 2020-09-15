using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SimpleLed;

namespace RGBSyncPlus.Model
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

            public DriverProperties DriverProperties { get; set; }
            public ReleaseNumber Version { get; set; }
            public string Author { get; set; }
            public string Repo { get; set; }

            
        }

        public class PluginDetailsViewModel : BaseViewModel
        {
            private BitmapImage image;

            public BitmapImage Image
            {
                get => image;
                set => SetProperty(ref image, value);
            }


            private string name;
            public string Name { get => name; set => SetProperty(ref name, value); }

            private string author;
            public string Author
            {
                get => author;
                set => SetProperty(ref author, value);
            }

            private string version;
            public string Version
            {
                get => version;
                set => SetProperty(ref version, value);
            }

            private string newestPublicVersion;
            public string NewestPublicVersion
            {
                get => newestPublicVersion;
                set => SetProperty(ref newestPublicVersion, value);
            }

            private string newestPreReleaseVersion;
            public string NewestPreReleaseVersion
            {
                get => newestPreReleaseVersion;
                set => SetProperty(ref newestPreReleaseVersion, value);
            }

            private string installedVersion;
            public string InstalledVersion
            {
                get => installedVersion;
                set => SetProperty(ref installedVersion, value);
            }


            private string blurb;
            public string Blurb
            {
                get => blurb;
                set => SetProperty(ref blurb, value);
            }

            private Guid pluginId;

            public Guid PluginId
            {
                get => pluginId;
                set => SetProperty(ref pluginId, value);
            }

            private string id;

            public string Id
            {
                get => id;
                set => SetProperty(ref id, value);
            }

            public PluginDetails PluginDetails;

            private int releases;
            public int Releases
            {
                get => releases;
                set => SetProperty(ref releases, value);
            }

            private bool preRelease;

            public bool PreRelease
            {
                get => preRelease;
                set => SetProperty(ref preRelease, value);
            }

            private bool installed;

            public bool Installed
            {
                get => installed;
                set => SetProperty(ref installed, value);
            }

            private bool installedButOutdated;

            public bool InstalledButOutdated
            {
                get => installedButOutdated;
                set => SetProperty(ref installedButOutdated, value);
            }


            private bool visible;

            public bool Visible
            {
                get => visible;
                set => SetProperty(ref visible, value);
            }


            public ObservableCollection<PluginDetailsViewModel> Versions { get; set; } = new ObservableCollection<PluginDetailsViewModel>();

            public PluginDetailsViewModel(PluginDetails inp, bool dontChild=false)
            {
                string versionAsString = inp.Version!=null ? inp.Version.ToString() : "0.0.0.0";

                Name = inp.Name;
                Author = inp.Author;
                Version = versionAsString;
                Blurb = inp.DriverProperties.Blurb;
                PluginDetails = inp;
                PluginId = inp.PluginId;
                Id = inp.Id;
                if (!dontChild)
                {
                    Versions.Add(new PluginDetailsViewModel(inp, true));
                }

                Releases = 1;
            }
        }
    }
}
