using Newtonsoft.Json;
using RGBSyncStudio.Languages;
using RGBSyncStudio.Model;
using SharedCode;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using RGBSyncStudio.Services;

namespace RGBSyncStudio.UI.Tabs
{
    public class SettingsUIViewModel : LanguageAwareBaseViewModel
    {
        public bool enableReleaseTypeModal = false;
        private void SaveLauncherSettings()
        {
            string json = JsonConvert.SerializeObject(ServiceManager.Instance.ConfigService.LauncherPrefs);
            File.WriteAllText("launcherPrefs.json", json);
        }

        private bool startAsAdmin;

        public bool StartAsAdmin
        {
            get => startAsAdmin;
            set
            {
                SetProperty(ref startAsAdmin, value);
                ServiceManager.Instance.ConfigService.LauncherPrefs.RunAsAdmin = value;
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
                ServiceManager.Instance.ConfigService.LauncherPrefs.ReleaseBranch = value;
                SaveLauncherSettings();
                if (value == LauncherPrefs.ReleaseType.CI && enableReleaseTypeModal)
                {
                    ModalModel modal = new ModalModel();
                    modal.ModalText =
                        "Warning! CI builds contain bleeding-edge updates and features. If you value stability, switching to a different release type is highly advised.";
                    modal.ShowModalTextBox = false;
                    ModalService ms = new ModalService();
                    ms.ShowModal(modal);
                }
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
                ServiceManager.Instance.ConfigService.Settings.MinimizeToTray = value;
                ServiceManager.Instance.ConfigService.LauncherPrefs.MinimizeToTray = value;
                SaveLauncherSettings();
            }
        }

        public bool MinimizeOnStart
        {
            get => minimizeOnStart;
            set
            {
                SetProperty(ref minimizeOnStart, value);
                ServiceManager.Instance.ConfigService.LauncherPrefs.MinimizeOnStartUp = value;
                SaveLauncherSettings();
            }
        }

        private string simpleLedUserName;

        public string SimpleLedUserName
        {
            get => simpleLedUserName;
            set
            {
                SetProperty(ref simpleLedUserName, value);

            }
        }

        public ObservableCollection<LauncherPrefs.ReleaseType> ReleaseTypes { get; set; }
        public DeviceMappingModels.Settings Settings { get; set; }

        public SettingsUIViewModel()
        {

        }

        public void Init()
        {
            Settings = ServiceManager.Instance.ConfigService.Settings;
            CurrentLanguage = Languages.FirstOrDefault(x => x.Code == ServiceManager.Instance.ConfigService.Settings.Lang);

            ReleaseTypes = new ObservableCollection<LauncherPrefs.ReleaseType>();

            ReleaseTypes.Add(LauncherPrefs.ReleaseType.Release);
            ReleaseTypes.Add(LauncherPrefs.ReleaseType.Beta);
            ReleaseTypes.Add(LauncherPrefs.ReleaseType.CI);

            StartAsAdmin = ServiceManager.Instance.ConfigService.LauncherPrefs.MinimizeToTray;
            MinimizeOnStart = ServiceManager.Instance.ConfigService.LauncherPrefs.MinimizeOnStartUp;
            MinimizeToTray = ServiceManager.Instance.ConfigService.LauncherPrefs.MinimizeToTray;

            ReleaseType = ServiceManager.Instance.ConfigService.LauncherPrefs.ReleaseBranch;
            SimpleLedUserName = ServiceManager.Instance.ConfigService.Settings.SimpleLedUserName;

            OnPropertyChanged("ReleaseTypes");
            OnPropertyChanged("StartAsAdmin");
            OnPropertyChanged("MinimizeOnStart");
            OnPropertyChanged("ReleaseType");
            OnPropertyChanged("MinimizeToTray");

            Background = ServiceManager.Instance.ConfigService.Settings.Background;
            BackgroundOpacity = ServiceManager.Instance.ConfigService.Settings.BackgroundOpacity * 100;
            DimBackgroundOpacity = ServiceManager.Instance.ConfigService.Settings.DimBackgroundOpacity * 100;
            BackgroundBlur = ServiceManager.Instance.ConfigService.Settings.BackgroundBlur * 5;
            ControllableBG = ServiceManager.Instance.ConfigService.Settings.ControllableBG;

            UpdateRate = ServiceManager.Instance.ConfigService.Settings.UpdateRate;

            enableReleaseTypeModal = true;
        }


        private string background = "";

        public string Background
        {
            get => background;
            set
            {
                SetProperty(ref background, value);
                ServiceManager.Instance.ConfigService.Settings.Background = value;
            }
        }


        private float bgopacity;

        public float BackgroundOpacity
        {
            get => bgopacity;
            set
            {
                SetProperty(ref bgopacity, (float)Math.Floor(value));
                ServiceManager.Instance.ConfigService.Settings.BackgroundOpacity = (float)Math.Floor(value) / 100f;
            }
        }

        private int updateRate;

        public int UpdateRate
        {
            get => updateRate;
            set
            {
                SetProperty(ref updateRate, value);
                ServiceManager.Instance.ConfigService.Settings.UpdateRate = value;
            }
        }

        private bool controllableBG;

        public bool ControllableBG
        {
            get => controllableBG;
            set
            {
                SetProperty(ref controllableBG, value);
                ServiceManager.Instance.ConfigService.Settings.ControllableBG = value;
            }
        }

        private bool rainbowTab;

        public bool RainbowTabBars
        {
            get => rainbowTab;
            set
            {
                SetProperty(ref rainbowTab, value);
                ServiceManager.Instance.ConfigService.Settings.RainbowTabBars = value;
            }
        }

        private Color accentColor;

        public Color AccentColor
        {
            get => accentColor;
            set
            {
                SetProperty(ref accentColor, value);
                ServiceManager.Instance.ConfigService.Settings.AccentColor = value;
            }
        }

        private float dimbgopacity;

        public float DimBackgroundOpacity
        {
            get => dimbgopacity;
            set
            {
                SetProperty(ref dimbgopacity, (float)Math.Floor(value));
                ServiceManager.Instance.ConfigService.Settings.DimBackgroundOpacity = (float)Math.Floor(value) / 100f;
            }
        }

        private float backgroundBlur;

        public float BackgroundBlur
        {
            get => backgroundBlur;
            set
            {
                SetProperty(ref backgroundBlur, (float)Math.Floor(value));
                ServiceManager.Instance.ConfigService.Settings.BackgroundBlur = (float)Math.Floor(value) / 5f;
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
                //    ServiceManager.Instance.ConfigService.Settings.BackgroundBlur = (float)Math.Floor(value) / 20f;
                //};

                //blurTimer.Start();
            }
        }

        private void BlurTimer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private readonly DispatcherTimer blurTimer;

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
                    Settings.Lang = value.Code;
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
