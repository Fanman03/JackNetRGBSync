using Newtonsoft.Json;
using SimpleLed;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Threading;

namespace SyncStudio.WPF.UI.Tabs
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
            SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile().ColorProfileId = CurrentProfile.Id;
            SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile().IsProfileStale = true;

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
            CurrentProfile = SyncStudio.Core.ServiceManager.ColorPallets.GetActiveColorPallet();

            ColorProfiles = new ObservableCollection<ColorProfile>(SyncStudio.Core.ServiceManager.ColorPallets.GetAllColorPallets());
            
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
