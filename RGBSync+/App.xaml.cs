using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Configuration.Legacy;
using RGBSyncPlus.Helper;
using RGBSyncPlus.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using RGBSyncPlus.UI.Tabs;
using SimpleLed;

namespace RGBSyncPlus
{
    public partial class App : Application
    {
        #region Constants

        private const string PATH_SETTINGS = "Profile.json";

        private const string PATH_APPSETTINGS = "AppSettings.json";

        #endregion

        #region Properties & Fields

        private TaskbarIcon _taskbarIcon;

        public AppBVM appBvm { get; set; } = new AppBVM();

        #endregion

        #region Methods
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ServiceManager.Instance.Logger.CrashWindow(e.Exception);

            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!Debugger.IsAttached)
            {
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            }

            ApplicationManager.Instance.Initialize();

            ServiceManager.Instance.ProfileService.OnProfilesChanged += (object sender, EventArgs ev) => appBvm.RefreshProfiles();

            try
            {
                ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

                _taskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
                _taskbarIcon.DoubleClickCommand = ApplicationManager.Instance.OpenConfigurationCommand;

                //ApplicationManager.Instance.OpenConfigurationCommand.Execute(null);
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", $"[{DateTime.Now:G}] Exception!\r\n\r\nMessage:\r\n{ex.GetFullMessage()}\r\n\r\nStackTrace:\r\n{ex.StackTrace}\r\n\r\n");

                try { ApplicationManager.Instance.ExitCommand.Execute(null); }
                catch { Environment.Exit(0); }
            }

           
        }

        protected override void OnExit(ExitEventArgs e)
        {
            
            base.OnExit(e);

            //File.WriteAllText(PATH_SETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
            //File.WriteAllText(PATH_APPSETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.AppSettings, new ColorSerializer()));
        }


        public static void SaveSettings()
        {
            //File.WriteAllText(PATH_SETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
            //File.WriteAllText(PATH_APPSETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.AppSettings, new ColorSerializer()));
        }

        #endregion

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            //  throw new NotImplementedException();
        }


        private void ToggleProfilesPopup(object sender, RoutedEventArgs e)
        {
            if (appBvm.PopupVisibility == Visibility.Collapsed)
            {
                appBvm.PopupVisibility = Visibility.Visible;
                appBvm.Arrow = "";
                appBvm.ProfilesBackground = SystemParameters.WindowGlassBrush;
            }
            else
            {
                appBvm.PopupVisibility = Visibility.Collapsed;
                appBvm.Arrow = "";
                appBvm.ProfilesBackground = new SolidColorBrush(Color.FromRgb(64, 64, 64));
            }
        }

        private void SwitchProfile(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ServiceManager.Instance.ProfileService.LoadProfileFromName(btn.Content.ToString());
            appBvm.Profiles = AppBVM.GetProfiles();
        }

    }

    public class AppBVM : BaseViewModel
    {
        public void RefreshProfiles()
        {
            Profiles = GetProfiles();
        }

        public static ObservableCollection<ProfileObject> GetProfiles()
        {
            if (ServiceManager.Instance?.ConfigService?.NGSettings?.ProfileNames != null)
            {
                ObservableCollection<ProfileObject> prfs = new ObservableCollection<ProfileObject>();
                foreach (string name in ServiceManager.Instance.ConfigService.NGSettings.ProfileNames)
                {
                    ProfileObject prf = new ProfileObject();
                    prf.Name = name;
                    prfs.Add(prf);

                    if (prf.Name == ServiceManager.Instance.ConfigService.NGSettings.CurrentProfile)
                    {
                        prf.IsSelected = true;
                    }
                    else
                    {
                        prf.IsSelected = false;
                    }
                }
                return prfs;
            } else
            {
                return new ObservableCollection<ProfileObject>();
            }
           
        }
        private Visibility popupVisibility = Visibility.Collapsed;
        public Visibility PopupVisibility
        {
            get
            {
                return popupVisibility;
            }
            set
            {
                SetProperty(ref popupVisibility, value);
            }
        }

        private string arrow = "";

        public string Arrow
        {
            get
            {
                return arrow;
            }
            set
            {
                SetProperty(ref arrow, value);
            }
        }

        private Brush profilesBackground = new SolidColorBrush(Color.FromRgb(64, 64, 64));
        public Brush ProfilesBackground
        {
            get
            {
                return profilesBackground;
            }
            set
            {
                SetProperty(ref profilesBackground, value);
            }
        }

        private ObservableCollection<ProfileObject> profiles = GetProfiles();
        public ObservableCollection<ProfileObject> Profiles
        {
            get
            {
                return profiles;
            }
            set
            {
                SetProperty(ref profiles, value);
            }
        }
        public class ProfileObject
        {
            public string Name { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
