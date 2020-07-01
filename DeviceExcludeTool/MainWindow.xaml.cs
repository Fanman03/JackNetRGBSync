using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DeviceExcludeTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int finalValue;

        private const string PATH_APPSETTINGS = "AppSettings.json";

        public bool restartMainExe;

        AppSettings appsettings;

        public MainWindow()
        {
            InitializeComponent();

            try { appsettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(PATH_APPSETTINGS)); }
            catch (Exception ex)
            {
                appsettings = new AppSettings();
                Console.WriteLine(ex.Message);
                /* File doesn't exist or is corrupt - just create a new one. */
            }

            Process[] processes = Process.GetProcessesByName("RGBSync+");
            if (processes.Length == 0)
            {
                restartMainExe = false;
            }
            else
            {
                foreach (var process in processes)
                {
                    process.Kill();
                }
                restartMainExe = true;
            }

            resultBox.Text = appsettings.DeviceTypes.ToString();
        }

        private void WriteBtn_Click(object sender, RoutedEventArgs e)
        {
            appsettings.DeviceTypes = finalValue;
            File.WriteAllText(PATH_APPSETTINGS, JsonConvert.SerializeObject(appsettings));
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            finalValue = 0;
            if (keyboard.IsChecked == true)
            {
                finalValue = finalValue | 1 << 0;
            }
            if (mouse.IsChecked == true)
            {
                finalValue = finalValue | 1 << 1;
            }
            if (headset.IsChecked == true)
            {
                finalValue = finalValue | 1 << 2;
            }
            if (mousepad.IsChecked == true)
            {
                finalValue = finalValue | 1 << 3;
            }
            if (ledstrip.IsChecked == true)
            {
                finalValue = finalValue | 1 << 4;
            }
            if (ledmatrix.IsChecked == true)
            {
                finalValue = finalValue | 1 << 5;
            }
            if (mainboard.IsChecked == true)
            {
                finalValue = finalValue | 1 << 6;
            }
            if (gpu.IsChecked == true)
            {
                finalValue = finalValue | 1 << 7;
            }
            if (dram.IsChecked == true)
            {
                finalValue = finalValue | 1 << 8;
            }
            if (headsetstand.IsChecked == true)
            {
                finalValue = finalValue | 1 << 9;
            }
            if (keypad.IsChecked == true)
            {
                finalValue = finalValue | 1 << 10;
            }
            if (fan.IsChecked == true)
            {
                finalValue = finalValue | 1 << 11;
            }
            if (speaker.IsChecked == true)
            {
                finalValue = finalValue | 1 << 12;
            }
            if (cooler.IsChecked == true)
            {
                finalValue = finalValue | 1 << 13;
            }
            if (unknown.IsChecked == true)
            {
                finalValue = finalValue | 1 << 31;
            }
            if (all.IsChecked == true)
            {
                finalValue = -1;
            }

            resultBox.Text = finalValue.ToString();
        }

        private void all_Checked(object sender, RoutedEventArgs e)
        {
            keyboard.IsChecked = true;
            keyboard.IsEnabled = false;

            mouse.IsChecked = true;
            mouse.IsEnabled = false;

            headset.IsChecked = true;
            headset.IsEnabled = false;

            mousepad.IsChecked = true;
            mousepad.IsEnabled = false;

            ledstrip.IsChecked = true;
            ledstrip.IsEnabled = false;

            ledmatrix.IsChecked = true;
            ledmatrix.IsEnabled = false;

            mainboard.IsChecked = true;
            mainboard.IsEnabled = false;

            gpu.IsChecked = true;
            gpu.IsEnabled = false;

            headsetstand.IsChecked = true;
            headsetstand.IsEnabled = false;

            keypad.IsChecked = true;
            keypad.IsEnabled = false;

            speaker.IsChecked = true;
            speaker.IsEnabled = false;

            unknown.IsChecked = true;
            unknown.IsEnabled = false;

            dram.IsChecked = true;
            dram.IsEnabled = false;

            fan.IsChecked = true;
            fan.IsEnabled = false;

            cooler.IsChecked = true;
            cooler.IsEnabled = false;
        }

        private void all_Unchecked(object sender, RoutedEventArgs e)
        {
            keyboard.IsChecked = false;
            keyboard.IsEnabled = true;

            mouse.IsChecked = false;
            mouse.IsEnabled = true;

            headset.IsChecked = false;
            headset.IsEnabled = true;

            mousepad.IsChecked = false;
            mousepad.IsEnabled = true;

            ledstrip.IsChecked = false;
            ledstrip.IsEnabled = true;

            ledmatrix.IsChecked = false;
            ledmatrix.IsEnabled = true;

            mainboard.IsChecked = false;
            mainboard.IsEnabled = true;

            gpu.IsChecked = false;
            gpu.IsEnabled = true;

            headsetstand.IsChecked = false;
            headsetstand.IsEnabled = true;

            speaker.IsChecked = false;
            speaker.IsEnabled = true;

            keypad.IsChecked = false;
            keypad.IsEnabled = true;

            unknown.IsChecked = false;
            unknown.IsEnabled = true;

            dram.IsChecked = false;
            dram.IsEnabled = true;

            fan.IsChecked = false;
            fan.IsEnabled = true;

            cooler.IsChecked = false;
            cooler.IsEnabled = true;
        }
        private void AppWindow_Closed(object sender, EventArgs e)
        {
            if (restartMainExe)
            {
                Process[] processes = Process.GetProcessesByName("RGBSync+");
                if (processes.Length == 0)
                {
                    string ExeName = "RGBSync+.exe";
                    Process.Start(ExeName);
                }
            }
        }

        private void AppWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (restartMainExe)
            {
                Process[] processes = Process.GetProcessesByName("RGBSync+");
                if (processes.Length == 0)
                {
                    string ExeName = "RGBSync+.exe";
                    Process.Start(ExeName);
                }
            }
        }
    }
}
