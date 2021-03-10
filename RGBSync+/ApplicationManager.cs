using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using SyncStudio.ClientService;
using SyncStudio.WPF.UI;


namespace SyncStudio.WPF
{
    public class ApplicationManager
    {
        private ClientService.Settings settings = new Settings();
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;
        public MainWindowViewModel MainViewModel => MainWindow.DataContext as MainWindowViewModel;

        public const string SLSPROVIDER_DIRECTORY = "Providers";
        private const string ProfileS_DIRECTORY = "Profiles";
        private const string SLSCONFIGS_DIRECTORY = "SLSConfigs";

        public MainWindow MainWindow;

        public void FireLanguageChangedEvent()
        {
            LanguageChangedEvent?.Invoke(this, new EventArgs());
        }

        public event EventHandler LanguageChangedEvent;

        public void NavigateToTab(string tab) => MainWindow?.SetTab(tab);

        public SplashLoader LoadingSplash;
        
        public void Initialize()
        {
          
            LoadingSplash = new SplashLoader();
            LoadingSplash.Show();

            LoadingSplash.LoadingText.Text = "Initializing";

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            LoadingSplash.Activate();

            ServiceManager.Instance.Logger.Debug("============ "+ ServiceManager.Instance.Branding.GetAppName()+" is Starting ============");


            //SyncStudio.Core.ServiceManager.LoadingMessage = (s => LoadingSplash.LoadingText.Text = s);
            //SyncStudio.Core.ServiceManager.LoadingMax = f => LoadingSplash.ProgressBar.Maximum = f;
            //SyncStudio.Core.ServiceManager.LoadingAmount = f => LoadingSplash.ProgressBar.Value = f;


            //SyncStudio.Core.ServiceManager.LedService.LoadSLSProviders();

            LoadingSplash.LoadingText.Text = "Mapping from config";
            //ServiceManager.Instance.ConfigService.SetUpMappedDevicesFromConfig();

            LoadingSplash.LoadingText.Text = "Loading Settings";
            //ServiceManager.Instance.ConfigService.LoadSettings();
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
            if (settings.EnableDiscordRPC == true)
            {
                ServiceManager.Instance.DiscordService.Stop();
            }
            if (settings.MinimizeToTray)
            {
                if (MainWindow.IsVisible)
                    MainWindow.Hide();
            }
            else
                MainWindow.WindowState = WindowState.Minimized;
        }

        public void OpenConfiguration()
        {
            Debug.WriteLine("Opening Main Window");
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
