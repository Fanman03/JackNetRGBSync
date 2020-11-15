using Newtonsoft.Json;
using RGBSyncPlus.Languages;
using RGBSyncPlus.Model;
using SharedCode;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RGBSyncPlus.UI.Tabs
{
    public class SettingsUIViewModel : LanguageAwareBaseViewModel
    {
        private void SaveLauncherSettings()
        {
            string json = JsonConvert.SerializeObject(ApplicationManager.Instance.LauncherPrefs);
            File.WriteAllText("launcherPrefs.json", json);
        }

        private bool startAsAdmin;

        public bool StartAsAdmin
        {
            get => startAsAdmin;
            set
            {
                SetProperty(ref startAsAdmin, value);
                ApplicationManager.Instance.LauncherPrefs.RunAsAdmin = value;
                SaveLauncherSettings();
            }
        }

        private LauncherPrefs.ReleaseType releaseType;

        public LauncherPrefs.ReleaseType ReleaseType
        {
            get => releaseType;
            set

            {
                SetProperty(ref releaseType, value);
                ApplicationManager.Instance.LauncherPrefs.ReleaseBranch = value;
                SaveLauncherSettings();
            }
        }

        private bool minimizeToTray;
        private bool minimizeOnStart;

        public bool MinimizeToTray
        {
            get => minimizeToTray;
            set
            {
                SetProperty(ref minimizeToTray, value);
                ApplicationManager.Instance.LauncherPrefs.MinimizeToTray = value;
                SaveLauncherSettings();
            }
        }

        public bool MinimizeOnStart
        {
            get => minimizeOnStart;
            set
            {
                SetProperty(ref minimizeOnStart, value);
                ApplicationManager.Instance.LauncherPrefs.MinimizeOnStartUp = value;
                SaveLauncherSettings();
            }
        }

        public ObservableCollection<LauncherPrefs.ReleaseType> ReleaseTypes { get; set; }
        public DeviceMappingModels.NGSettings NGSettings { get; set; }

        public SettingsUIViewModel()
        {
          
        }

        public void Init()
        {
            NGSettings = ApplicationManager.Instance.NGSettings;
            CurrentLanguage = Languages.FirstOrDefault(x => x.Code == ApplicationManager.Instance.NGSettings.Lang);

            ReleaseTypes = new ObservableCollection<LauncherPrefs.ReleaseType>();

            ReleaseTypes.Add(LauncherPrefs.ReleaseType.Release);
            ReleaseTypes.Add(LauncherPrefs.ReleaseType.Beta);
            ReleaseTypes.Add(LauncherPrefs.ReleaseType.CI);

            StartAsAdmin = ApplicationManager.Instance.LauncherPrefs.MinimizeToTray;
            MinimizeOnStart = ApplicationManager.Instance.LauncherPrefs.MinimizeOnStartUp;
            MinimizeToTray = ApplicationManager.Instance.LauncherPrefs.MinimizeToTray;

            ReleaseType = ApplicationManager.Instance.LauncherPrefs.ReleaseBranch;

            OnPropertyChanged("ReleaseTypes");
            OnPropertyChanged("StartAsAdmin");
            OnPropertyChanged("MinimizeOnStart");
            OnPropertyChanged("ReleaseType");
            OnPropertyChanged("MinimizeToTray");
        }

        public ObservableCollection<LanguageOption> Languages { get; set; } =
            new ObservableCollection<LanguageOption>(
                LanguageManager.Languages.Select(x => new LanguageOption { Name = x.NativeName, Code = x.Code, Emoji = x.Emoji }));


        private LanguageOption currentLanguage;

        public LanguageOption CurrentLanguage
        {
            get => currentLanguage;
            set
            {
                currentLanguage = value;
                if (value != null)
                {
                    NGSettings.Lang = value.Code;
                }

            }
        }
        public class LanguageOption
        {
            public string Emoji { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
