using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Configuration.Legacy;
using RGBSyncPlus.Helper;
using RGBSyncPlus.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            //CrashWindow crashWindow = new CrashWindow();
            //crashWindow.errorName.Text = e.Exception.GetType().ToString();
            //crashWindow.message.Text = e.Exception.Message;

            //crashWindow.stackTrace.Text = e.Exception.StackTrace;
            //crashWindow.Show();
            ApplicationManager.Logger.CrashWindow(e.Exception);

            // Prevent default unhandled exception processing
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
    }
}
