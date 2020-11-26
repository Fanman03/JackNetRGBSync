using System;
using Newtonsoft.Json;
using RGBSyncPlus.Languages;
using RGBSyncPlus.Model;
using SharedCode;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Threading;

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

            Background = ApplicationManager.Instance.NGSettings.Background;
            BackgroundOpacity = ApplicationManager.Instance.NGSettings.BackgroundOpacity*100;
            DimBackgroundOpacity = ApplicationManager.Instance.NGSettings.DimBackgroundOpacity * 100;
            BackgroundBlur = ApplicationManager.Instance.NGSettings.BackgroundBlur * 5;
            ControllableBG = ApplicationManager.Instance.NGSettings.ControllableBG;
        }


        private string background = "";

        public string Background
        {
            get => background;
            set
            {
                SetProperty(ref background, value);
                ApplicationManager.Instance.NGSettings.Background = value;
            }
        }


        private float bgopacity;

        public float BackgroundOpacity
        {
            get => bgopacity;
            set
            {
                SetProperty(ref bgopacity, (float)Math.Floor(value));
                ApplicationManager.Instance.NGSettings.BackgroundOpacity = (float)Math.Floor(value) / 100f;
            }
        }

        private bool controllableBG;

        public bool ControllableBG
        {
            get => controllableBG;
            set
            {
                SetProperty(ref controllableBG, value);
                ApplicationManager.Instance.NGSettings.ControllableBG = value;
            }
        }

        private bool rainbowTab;

        public bool RainbowTabBars
        {
            get => rainbowTab;
            set
            {
                SetProperty(ref rainbowTab, value);
                ApplicationManager.Instance.NGSettings.RainbowTabBars = value;
            }
        }

        private float dimbgopacity;

        public float DimBackgroundOpacity
        {
            get => dimbgopacity;
            set
            {
                SetProperty(ref dimbgopacity, (float)Math.Floor(value));
                ApplicationManager.Instance.NGSettings.DimBackgroundOpacity = (float)Math.Floor(value) / 100f;
            }
        }

        private float backgroundBlur;

        public float BackgroundBlur
        {
            get => backgroundBlur;
            set
            {
                SetProperty(ref backgroundBlur, (float)Math.Floor(value));
                ApplicationManager.Instance.NGSettings.BackgroundBlur = (float)Math.Floor(value) / 5f;
                //if (blurTimer != null)
                //{
                //    blurTimer.Stop();
                //    blurTimer = null;
                //}

                //blurTimer = new DispatcherTimer()
                //{
                //    Interval = TimeSpan.FromSeconds(3),
                //};

                //blurTimer.Tick += (sender, args) =>
                //{
                //    blurTimer.Stop();
                //    ApplicationManager.Instance.NGSettings.BackgroundBlur = (float)Math.Floor(value) / 20f;
                //};
                
                //blurTimer.Start();
            }
        }

        private void BlurTimer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private DispatcherTimer blurTimer;

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
