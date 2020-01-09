using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using RGBSyncPlus.Controls;

namespace RGBSyncPlus.UI
{
    public partial class ConfigurationWindow : BlurredDecorationWindow
    {
        public bool DoNotRestart;
        public ConfigurationWindow()
        {
            InitializeComponent();
            DoNotRestart = true;
            if (ApplicationManager.Instance.AppSettings.Lang == "en")
            {
                englishItem.IsSelected = true;
            } else if (ApplicationManager.Instance.AppSettings.Lang == "es")
            {
                spanishItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "de")
            {
                germanItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "fr")
            {
                frenchItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "ru")
            {
                russianItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "nl")
            {
                dutchItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "pt")
            {
                portugeseItem.IsSelected = true;
            }
            else if (ApplicationManager.Instance.AppSettings.Lang == "te")
            {
                LangBox.Text = "UI Test Mode";
            }
            else
            {
                LangBox.Text = "Unknown";
            }
            DoNotRestart = false;
                ScrollViewer.SetVerticalScrollBarVisibility(AvailableLEDsListbox, ScrollBarVisibility.Visible);
                ScrollViewer.SetVerticalScrollBarVisibility(LEDGroupsListbox, ScrollBarVisibility.Visible);
        }

        //DarthAffe 07.02.2018: This prevents the applicaiton from not shutting down and crashing afterwards if 'close' is selected in the taskbar-context-menu
        private void ConfigurationWindow_OnClosed(object sender, EventArgs e)
        {
            ApplicationManager.Instance.ExitCommand.Execute(null);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void EnglishSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "en";
        }
        private void SpanishSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "es";
        }
        private void GermanSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "de";
        }
        private void FrenchSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "fr";
        }
        private void RussianSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "ru";
        }
        private void DutchSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "nl";
        }
        private void PortugeseSelected(object sender, System.Windows.RoutedEventArgs e)
        {
            ApplicationManager.Instance.AppSettings.Lang = "pt";
        }
        private void Lang_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!DoNotRestart)
            { 
                try
                {
                    Process.Start("RestartHelper.bat");
                    System.Windows.Application.Current.Shutdown();
                }
                catch
                {
                    ApplicationManager.Instance.RestartApp();
                }
                

            }
        }
    }
}
