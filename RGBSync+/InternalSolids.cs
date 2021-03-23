using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SimpleLed;

namespace SyncStudio.WPF
{
    internal static class InternalSolids
    {
        
        internal static ThemeWatcher themeWatcher = new ThemeWatcher();
        internal static ThemeWatcher.WindowsTheme WindowsTheme;
        internal static Color Accent;
        static InternalSolids()
        {
            WindowsTheme = ThemeWatcher.GetWindowsTheme();
            themeWatcher.WatchTheme();
            themeWatcher.OnThemeChanged += ThemeWatcher_OnThemeChanged;
        }

        private static void ThemeWatcher_OnThemeChanged(object sender, ThemeWatcher.ThemeChangeEventArgs e)
        {
            WindowsTheme = e.CurrentTheme;
            Accent = e.AccentColor;
        }
    }
}
