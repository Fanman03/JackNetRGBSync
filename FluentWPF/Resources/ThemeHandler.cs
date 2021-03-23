using System;
using System.Windows;
using System.Windows.Interop;

namespace SourceChord.FluentWPF
{
    public abstract class ThemeHandler
    {
        protected static ThemeHandler Instance { get; set; }

        static ThemeHandler()
        {
        }

        public ThemeHandler()
        {
            // 初期化メソッドを呼ぶ
            Window win = Application.Current.MainWindow;
            if (win != null)
            {
                this.Initialize(win);
            }
            else
            {
                EventHandler handler = null;
                handler = (e, args) =>
                {
                    this.Initialize(Application.Current.MainWindow);
                    Application.Current.Activated -= handler;
                };
                Application.Current.Activated += handler;
            }
        }

        private void Initialize(Window win)
        {
            if (win.IsLoaded)
            {
                InitializeCore(win);
            }
            else
            {
                win.Loaded += (_, __) =>
                {
                    InitializeCore(win);
                };
            }
        }

        protected virtual void InitializeCore(Window win)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(win).Handle);
            source.AddHook(this.WndProc);
        }

        protected abstract IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
    }
}
