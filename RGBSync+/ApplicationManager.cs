using RGBSyncStudio.UI;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace RGBSyncStudio
{
    public class ApplicationManager
    {
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;
        public MainWindowViewModel MainViewModel => MainWindow.DataContext as MainWindowViewModel;

        public const string SLSPROVIDER_DIRECTORY = "SLSProvider";
        private const string ProfileS_DIRECTORY = "Profiles";
        private const string SLSCONFIGS_DIRECTORY = "SLSConfigs";

        public MainWindow MainWindow;

        public void FireLanguageChangedEvent()
        {
            LanguageChangedEvent?.Invoke(this, new EventArgs());
        }

        public event EventHandler LanguageChangedEvent;

        public ApplicationManager()
        {
            try
            {
                if (!Directory.Exists(ProfileS_DIRECTORY))
                {
                    Directory.CreateDirectory(ProfileS_DIRECTORY);
                    ServiceManager.Instance.ProfileService.GenerateNewProfile("Default", false);
                    ServiceManager.Instance.ConfigService.isHotLoading = false;
                }
            }
            catch
            {
            }
        }

        public void NavigateToTab(string tab) => MainWindow?.SetTab(tab);

        public SplashLoader LoadingSplash;
        public void Initialize()
        {
            LoadingSplash = new SplashLoader();
            LoadingSplash.Show();

            LoadingSplash.LoadingText.Text = "Initializing";

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            LoadingSplash.Activate();

            ServiceManager.Instance.Logger.Debug("============ JackNet RGB Sync is Starting ============");

            CultureInfo ci = CultureInfo.InstalledUICulture;
            if (ServiceManager.Instance.ConfigService.Settings.Lang == null)
            {
                ServiceManager.Instance.Logger.Debug("Language is not set, inferring language from system culture. Lang=" + ci.TwoLetterISOLanguageName);
                ServiceManager.Instance.ConfigService.Settings.Lang = ci.TwoLetterISOLanguageName;
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(ServiceManager.Instance.ConfigService.Settings.Lang);

            LoadingSplash.LoadingText.Text = "Starting Discord";
            if (ServiceManager.Instance.ConfigService.Settings.EnableDiscordRPC)
            {
                ServiceManager.Instance.DiscordService.ConnectDiscord();
            }

            ServiceManager.Instance.LedService.LoadOverrides();
            ServiceManager.Instance.LedService.LoadSLSProviders();

            LoadingSplash.LoadingText.Text = "Mapping from config";
            ServiceManager.Instance.ConfigService.SetUpMappedDevicesFromConfig();

            LoadingSplash.LoadingText.Text = "Loading Settings";
            ServiceManager.Instance.ConfigService.LoadSettings();
            LoadingSplash.LoadingText.Text = "All done";

            OpenConfiguration();

            DispatcherTimer closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromSeconds(1);
            closeTimer.Tick += (sender, args) =>
            {
                LoadingSplash.Close();
                closeTimer.Stop();
            };

            closeTimer.Start();

        }

        public void HideConfiguration()
        {
            if (ServiceManager.Instance.ConfigService.Settings.EnableDiscordRPC == true)
            {
                ServiceManager.Instance.DiscordService.Stop();
            }
            if (ServiceManager.Instance.ConfigService.Settings.MinimizeToTray)
            {
                if (MainWindow.IsVisible)
                    MainWindow.Hide();
            }
            else
                MainWindow.WindowState = WindowState.Minimized;
        }

        public void OpenConfiguration()
        {
            if (MainWindow == null) MainWindow = new MainWindow();
            if (!MainWindow.IsVisible)
            {
                MainWindow.Show();
            }

            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }
        }

        public void RestartApp()
        {
            ServiceManager.Instance.Logger.Info("App is restarting.");
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            ServiceManager.Shutdown();
        }

        public void Exit()
        {
            ServiceManager.Instance.Logger.Info("App is Shutting Down");

            ServiceManager.Shutdown();
        }
    }
}
