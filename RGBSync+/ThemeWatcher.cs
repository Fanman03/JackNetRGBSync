using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SyncStudio.WPF
{
    public class ThemeWatcher
    {
        public class ThemeChangeEventArgs
        {
            public WindowsTheme CurrentTheme { get; set; }
            public Color AccentColor { get; set; }

        }

        public delegate void ThemeChangedEventHandler(object sender, ThemeChangeEventArgs e);
        public event ThemeChangedEventHandler OnThemeChanged;

        public WindowsTheme CurrentTheme = GetWindowsTheme();
        public Color CurrentAccent;
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string RegistryValueName = "AppsUseLightTheme";


        public ThemeWatcher()
        {
            var acnt = (SolidColorBrush)SystemParameters.WindowGlassBrush;
            acnt.Freeze();
            var clr = acnt.Color;

            CurrentAccent = clr;
        }
        public enum WindowsTheme
        {
            Light,
            Dark
        }

        public void WatchTheme()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            string query = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                RegistryValueName);

            try
            {
                var watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, args) =>
                {
                    WindowsTheme newWindowsTheme = GetWindowsTheme();
                    if (newWindowsTheme != CurrentTheme)
                    {
                        var acnt = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                        acnt.Freeze();
                        OnThemeChanged?.Invoke(this, new ThemeChangeEventArgs
                        {
                            CurrentTheme = newWindowsTheme,
                            AccentColor = acnt.Color
                        });
                        // React to new theme
                        CurrentTheme = newWindowsTheme;
                    }
                };

                // Start listening for events
                watcher.Start();

                DispatcherTimer accentWatcher = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5),
                };

                accentWatcher.Tick += AccentWatcher_Tick;

                accentWatcher.Start();
            }
            catch (Exception)
            {
                // This can fail on Windows 7
            }

            WindowsTheme initialTheme = GetWindowsTheme();
        }

        private void AccentWatcher_Tick(object sender, EventArgs e)
        {
            var acnt = (SolidColorBrush)SystemParameters.WindowGlassBrush;
            acnt.Freeze();
            var clr = acnt.Color;
            if (clr != CurrentAccent)
            {
                CurrentAccent = clr;

                OnThemeChanged?.Invoke(this, new ThemeChangeEventArgs
                {
                    CurrentTheme = CurrentTheme,
                    AccentColor = CurrentAccent
                });


            }
        }

        public static WindowsTheme GetWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return WindowsTheme.Light;
                }

                int registryValue = (int)registryValueObject;
                var theme = registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
                return theme;
            }
        }

        public static Color GetAccent()
        {
            var acnt = (SolidColorBrush)SystemParameters.WindowGlassBrush;
            acnt.Freeze();
            var clr = acnt.Color;

            return clr;
        }
    }

}
