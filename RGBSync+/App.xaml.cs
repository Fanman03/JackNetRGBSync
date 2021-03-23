using Hardcodet.Wpf.TaskbarNotification;
using SimpleLed;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SyncStudio.WPF.Helper;
using SyncStudio.WPF.UI;

namespace SyncStudio.WPF
{
    public partial class App : Application
    {
        
        public const string SLSPROVIDER_DIRECTORY = "Providers";
        private const string ProfileS_DIRECTORY = "Profiles";
        private const string SLSCONFIGS_DIRECTORY = "SLSConfigs";

        #region Constants

        private const string PATH_SETTINGS = "Profile.json";

        private const string PATH_APPSETTINGS = "AppSettings.json";

        #endregion

        #region Properties & Fields

        private TaskbarIcon _taskbarIcon;
        

        public AppBVM appBvm { get; set; } = new AppBVM();

        #endregion

        #region Methods
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
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

            ServiceManager.Initialize(SLSCONFIGS_DIRECTORY, ProfileS_DIRECTORY);
            ServiceManager.Instance.ApplicationManager.Initialize();

//            ServiceManager.Instance.ProfileService.OnProfilesChanged += (object sender, EventArgs ev) => appBvm.RefreshProfiles();

            try
            {
                ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

                _taskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
                _taskbarIcon.DoubleClickCommand = new ActionCommand(() => ServiceManager.Instance.ApplicationManager.OpenConfiguration());

                //ServiceManager.Instance.ApplicationManager.OpenConfigurationCommand.Execute(null);
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", $"[{DateTime.Now:G}] Exception!\r\n\r\nMessage:\r\n{ex.GetFullMessage()}\r\n\r\nStackTrace:\r\n{ex.StackTrace}\r\n\r\n");

                try { ServiceManager.Instance.ApplicationManager.Exit(); }
                catch { Environment.Exit(0); }
            }

            
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
            ServiceManager.Instance.ProfilesService.GetProfile(Guid.NewGuid());
                //.Get.ProfileService.LoadProfileFromName(btn.Content.ToString());
            appBvm.Profiles = AppBVM.GetProfiles();
        }

        private void TechSupportClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://rgbsync.com/discord");
        }

        private void RestartAppClick(object sender, RoutedEventArgs e)
        {
            ServiceManager.Instance.ApplicationManager.RestartApp();
        }

        private void ExitClicked(object sender, RoutedEventArgs e)
        {
            ServiceManager.Instance.ApplicationManager.Exit();
        }

        private void HideClicked(object sender, RoutedEventArgs e)
        {
            ServiceManager.Instance.ApplicationManager.HideConfiguration();
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
            return null;
            //todo wtf why here?
            //if (ServiceManager.Instance?.ConfigService?.Settings?.ProfileNames != null)
            //{
            //    ObservableCollection<ProfileObject> prfs = new ObservableCollection<ProfileObject>();
            //    foreach (string name in settings.ProfileNames)
            //    {
            //        ProfileObject prf = new ProfileObject();
            //        prf.Name = name;
            //        prfs.Add(prf);

            //        if (prf.Name == settings.CurrentProfile)
            //        {
            //            prf.IsSelected = true;
            //        }
            //        else
            //        {
            //            prf.IsSelected = false;
            //        }
            //    }
            //    return prfs;
            //}
            //else
            //{
            //    return new ObservableCollection<ProfileObject>();
            //}

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
