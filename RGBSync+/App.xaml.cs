using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Resources;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Configuration.Legacy;
using RGBSyncPlus.Helper;
using RGBSyncPlus.UI;

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

        #endregion

        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

                _taskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
                _taskbarIcon.DoubleClickCommand = ApplicationManager.Instance.OpenConfigurationCommand;

                Settings settings = null;
                try { settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(PATH_SETTINGS), new ColorSerializer()); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    /* File doesn't exist or is corrupt - just create a new one. */
                }

                AppSettings appsettings = null;
                try { appsettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(PATH_APPSETTINGS), new ColorSerializer()); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    /* File doesn't exist or is corrupt - just create a new one. */
                }

                if (settings == null)
                {
                    settings = new Settings { Version = Settings.CURRENT_VERSION };
                    _taskbarIcon.ShowBalloonTip("JackNet RGB Sync is starting in the tray!", "Click on the icon to open the configuration.", BalloonIcon.Info);
                }

                else if (settings.Version != Settings.CURRENT_VERSION)
                    ConfigurationUpdates.PerformOn(settings);

                if (appsettings == null)
                {
                    appsettings = new AppSettings { Version = AppSettings.CURRENT_VERSION };
                }


                ApplicationManager.Instance.Settings = settings;
                ApplicationManager.Instance.AppSettings = appsettings;
                ApplicationManager.Instance.Initialize();
                if (!appsettings.MinimizeToTray) //HACK Fanman03 05.12.2019: Workaround to create the window
                {
                    ApplicationManager.Instance.OpenConfigurationCommand.Execute(null);
                    ApplicationManager.Instance.HideConfigurationCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", $"[{DateTime.Now:G}] Exception!\r\n\r\nMessage:\r\n{ex.GetFullMessage()}\r\n\r\nStackTrace:\r\n{ex.StackTrace}\r\n\r\n");
                GenericErrorDialog dialog = new GenericErrorDialog("An error occured while starting JackNet RGB Sync.\r\nMore information can be found in the error.log file in the application directory.", "Can't start JackNet RGB Sync.", $"[{DateTime.Now:G}] Exception!\r\n\r\nMessage:\r\n{ex.GetFullMessage()}\r\n\r\nStackTrace:\r\n{ex.StackTrace}\r\n\r\n");
                dialog.Show();

                try { ApplicationManager.Instance.ExitCommand.Execute(null); }
                catch { Environment.Exit(0); }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            File.WriteAllText(PATH_SETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
            File.WriteAllText(PATH_APPSETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.AppSettings, new ColorSerializer()));
        }

        public static void SaveSettings()
        {
            File.WriteAllText(PATH_SETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
            File.WriteAllText(PATH_APPSETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.AppSettings, new ColorSerializer()));
        }

        #endregion
    }
}
