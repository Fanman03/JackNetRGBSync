using Newtonsoft.Json;
using SyncStudio.WPF.Model;
using SharedCode;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using Autofac.Core;
using SyncStudio.Domain;
using SyncStudio.WPF.Languages;
using SyncStudio.WPF.Services;

namespace SyncStudio.WPF.UI.Tabs
{
    public class SettingsUIViewModel : LanguageAwareBaseViewModel
    {
        private ClientService.Settings settings = new ClientService.Settings();
        private LauncherPrefs launcherPrefs;

        public bool enableReleaseTypeModal = false;
        private void SaveLauncherSettings()
        {
            string json = JsonConvert.SerializeObject(launcherPrefs);
            File.WriteAllText("launcherPrefs.json", json);
        }

        private bool startAsAdmin;

        public bool StartAsAdmin
        {
            get => startAsAdmin;
            set
            {
                SetProperty(ref startAsAdmin, value);
                launcherPrefs.RunAsAdmin = value;
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
                launcherPrefs.ReleaseBranch = value;
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
                settings.MinimizeToTray = value;
                launcherPrefs.MinimizeToTray = value;
                SaveLauncherSettings();
            }
        }

        public bool MinimizeOnStart
        {
            get => minimizeOnStart;
            set
            {
                SetProperty(ref minimizeOnStart, value);
                launcherPrefs.MinimizeOnStartUp = value;
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
        public Settings Settings { get; set; }

        public SettingsUIViewModel()
        {
            launcherPrefs = JsonConvert.DeserializeObject<LauncherPrefs>(File.ReadAllText("launcherPrefs.json"));
        }

        public void Init()
        {
            
            CurrentLanguage = Languages.FirstOrDefault(x => x.Code == settings.Lang);

            ReleaseTypes = new ObservableCollection<LauncherPrefs.ReleaseType>();

            ReleaseTypes.Add(LauncherPrefs.ReleaseType.Release);
            ReleaseTypes.Add(LauncherPrefs.ReleaseType.Beta);
            ReleaseTypes.Add(LauncherPrefs.ReleaseType.CI);

            StartAsAdmin = launcherPrefs.MinimizeToTray;
            MinimizeOnStart = launcherPrefs.MinimizeOnStartUp;
            MinimizeToTray = launcherPrefs.MinimizeToTray;

            ReleaseType = launcherPrefs.ReleaseBranch;
            SimpleLedUserName = settings.SimpleLedUserName;

            OnPropertyChanged("ReleaseTypes");
            OnPropertyChanged("StartAsAdmin");
            OnPropertyChanged("MinimizeOnStart");
            OnPropertyChanged("ReleaseType");
            OnPropertyChanged("MinimizeToTray");

            
            DiscordEnabled = settings.EnableDiscordRPC;

            Background = settings.Background;
            BackgroundOpacity = settings.BackgroundOpacity * 100;
            DimBackgroundOpacity = settings.DimBackgroundOpacity * 100;
            BackgroundBlur = settings.BackgroundBlur * 5;
            ControllableBG = settings.ControllableBG;

            UpdateRate = settings.UpdateRate;

            enableReleaseTypeModal = true;
        }


        private string background = "";

        public string Background
        {
            get => background;
            set
            {
                SetProperty(ref background, value);
                settings.Background = value;
            }
        }


        private float bgopacity;

        public float BackgroundOpacity
        {
            get => bgopacity;
            set
            {
                SetProperty(ref bgopacity, (float)Math.Floor(value));
                settings.BackgroundOpacity = (float)Math.Floor(value) / 100f;
            }
        }

        private int updateRate;

        public int UpdateRate
        {
            get => updateRate;
            set
            {
                SetProperty(ref updateRate, value);
                settings.UpdateRate = value;
            }
        }

        private bool discordEnabled;
        public bool DiscordEnabled
        {
            get => discordEnabled;
            set
            {
                SetProperty(ref discordEnabled, value);
                settings.EnableDiscordRPC = value;
            }
        }

        private bool controllableBG;

        public bool ControllableBG
        {
            get => controllableBG;
            set
            {
                SetProperty(ref controllableBG, value);
                settings.ControllableBG = value;
            }
        }

        private bool rainbowTab;

        public bool RainbowTabBars
        {
            get => rainbowTab;
            set
            {
                SetProperty(ref rainbowTab, value);
                settings.RainbowTabBars = value;
            }
        }

        //private Color accentColor;

        //public Color AccentColor
        //{
        //    get => accentColor;
        //    set
        //    {
        //        SetProperty(ref accentColor, value);
        //        settings.AccentColor = value;
        //    }
        //}

        private float dimbgopacity;

        public float DimBackgroundOpacity
        {
            get => dimbgopacity;
            set
            {
                SetProperty(ref dimbgopacity, (float)Math.Floor(value));
                settings.DimBackgroundOpacity = (float)Math.Floor(value) / 100f;
            }
        }

        private float backgroundBlur;

        public float BackgroundBlur
        {
            get => backgroundBlur;
            set
            {
                SetProperty(ref backgroundBlur, (float)Math.Floor(value));
                settings.BackgroundBlur = (float)Math.Floor(value) / 5f;
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
                //    settings.BackgroundBlur = (float)Math.Floor(value) / 20f;
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
                    settings.Lang = value.Code;
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
