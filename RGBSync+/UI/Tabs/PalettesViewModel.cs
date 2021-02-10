using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using SimpleLed;

namespace RGBSyncPlus.UI.Tabs
{
    public class PalettesViewModel : LanguageAwareBaseViewModel
    {
        public void SaveProfile()
        {
            Unsaved = true;
            ColorProfiles.First(x => x.Id == CurrentProfile.Id).ProfileName = CurrentProfile.ProfileName;

            if (saveTimer != null)
            {
                saveTimer.Stop();
            }
            else
            {
                saveTimer = new Timer
                {
                    AutoReset = false,
                    Interval = 50
                };

                saveTimer.Elapsed += SaveTimer_Elapsed;
            }

            saveTimer.Start();
        }

        private void SaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ActuallySaveProfile();
        }

        private Timer saveTimer = null;
        private void ActuallySaveProfile()
        {
            string path = "ColorProfiles\\" + CurrentProfile.Id + ".json";

            if (!Directory.Exists("ColorProfiles"))
            {
                Directory.CreateDirectory("ColorProfiles");
            }

            string json = JsonConvert.SerializeObject(CurrentProfile);
            File.WriteAllText(path, json);

            ServiceManager.Instance.SLSManager.ColorProfile = CurrentProfile;
            ServiceManager.Instance.ProfileService.CurrentProfile.ColorProfileId = CurrentProfile.Id;
            ServiceManager.Instance.ProfileService.CurrentProfile.IsProfileStale = true;

            Dispatcher.CurrentDispatcher.Invoke(() => Unsaved = false);
        }

        private bool unsaved;

        public bool Unsaved
        {
            get => unsaved;
            set => SetProperty(ref unsaved, value);
        }

        public PalettesViewModel()
        {
            CurrentProfile = new ColorProfile()
            {
                ProfileName = "Default",
                ColorBanks = new ObservableCollection<ColorBank>()
                {
                    new ColorBank()
                    {
                        BankName = "Basic colors",
                        Colors = new ObservableCollection<ColorObject>
                        {
                            new ColorObject {Color = new ColorModel(255,0,0)},
                            new ColorObject {Color = new ColorModel(0,255,0)},
                            new ColorObject {Color = new ColorModel(0,0,255)},
                            new ColorObject {Color = new ColorModel(255,255,255)},
                        }
                    },
                    new ColorBank()
                    {
                        BankName = "Rainbow",
                        Colors = new ObservableCollection<ColorObject>
                        {
                            new ColorObject {Color = new ColorModel(255,0,0)},
                            new ColorObject {Color = new ColorModel(255,153,0)},
                            new ColorObject {Color = new ColorModel(204,255,0)},
                            new ColorObject {Color = new ColorModel(51, 255, 0)},
                            new ColorObject {Color = new ColorModel(0, 255, 102)},
                            new ColorObject {Color = new ColorModel(0, 255, 255)},
                            new ColorObject {Color = new ColorModel(0, 102, 255)},
                            new ColorObject {Color = new ColorModel(51,0,255)},
                            new ColorObject {Color = new ColorModel(204, 0, 255)},
                            new ColorObject {Color = new ColorModel(255, 0, 153)},
                        }
                    }
                    ,
                    new ColorBank()
                    {
                        BankName = "Simple",
                        Colors = new ObservableCollection<ColorObject>
                        {
                            new ColorObject {Color = new ColorModel(255,0,255)},
                            new ColorObject {Color = new ColorModel(0,0,0)},
                        }
                    }
                    ,
                    new ColorBank()
                    {
                        BankName = "Complicated",
                        Colors = new ObservableCollection<ColorObject>
                        {
                            new ColorObject {Color = new ColorModel(255,0,0)},
                            new ColorObject {Color = new ColorModel(0,255,0)},
                            new ColorObject {Color = new ColorModel(0,0,255)},
                            new ColorObject {Color = new ColorModel(255,0,255)},
                            new ColorObject {Color = new ColorModel(255,0,0)},
                            new ColorObject {Color = new ColorModel(0,255,0)}
                        }
                    }
                }
            };


            ColorProfiles = new ObservableCollection<ColorProfile>(ApplicationManager.Instance.GetColorProfiles());

            if (ServiceManager.Instance.ProfileService.CurrentProfile.ColorProfileId != null)
            {
                CurrentProfile = ColorProfiles.FirstOrDefault(x=>x.Id == ServiceManager.Instance.ProfileService.CurrentProfile.ColorProfileId);
            }
        }

        private ColorProfile currentProfile;

        public ColorProfile CurrentProfile
        {
            get => currentProfile;
            set
            {
                SetProperty(ref currentProfile, value);
                SetUpWatchers();
                try
                {
                    ServiceManager.Instance.SLSManager.ColorProfile = value;
                }
                catch
                {
                }
            }
        }

        private ObservableCollection<ColorProfile> colorProfiles = new ObservableCollection<ColorProfile>();
        public ObservableCollection<ColorProfile> ColorProfiles
        {
            get => colorProfiles;
            set => SetProperty(ref colorProfiles, value);
        }

        public void SetUpWatchers()
        {
            foreach (ColorBank currentProfileColorBank in CurrentProfile.ColorBanks)
            {
                currentProfileColorBank.Colors.CollectionChanged += Colors_CollectionChanged;

            }
        }

        private void Colors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveProfile();
        }
    }
}
