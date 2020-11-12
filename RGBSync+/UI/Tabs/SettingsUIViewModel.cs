using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RGBSyncPlus.Languages;
using RGBSyncPlus.Model;
using SharedCode;
using SimpleLed;

namespace RGBSyncPlus.UI.Tabs
{
    public class SettingsUIViewModel : LanguageAwareBaseViewModel
    {
        private void SaveLauncherSettings()
        {
            string json = JsonConvert.SerializeObject(ApplicationManager.Instance.LauncherPrefs);
            File.WriteAllText("launcherPrefs.json",json);
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
            get => minimizeToTray;
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
        }

        public ObservableCollection<LanguageOption> Languages { get; set; } =
            new ObservableCollection<LanguageOption>(
                LanguageManager.Languages.Select(x => new LanguageOption { Name = x.NativeName, Code = x.Code }));


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
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
