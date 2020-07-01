using NLog;
using RGB.NET.Brushes;
using RGB.NET.Brushes.Gradients;
using RGB.NET.Core;
using RGB.NET.Groups;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDK_Debug
{
    static class Program
    {

        private const string DEVICEPROVIDER_DIRECTORY = "DeviceProvider";
        public static TimerUpdateTrigger UpdateTrigger { get; private set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "debugtool.log" };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            RGBSurface surface = RGBSurface.Instance;
            LoadDeviceProviders();
            surface.AlignDevices();

            foreach (IRGBDevice device in surface.Devices)
                device.UpdateMode = DeviceUpdateMode.Sync | DeviceUpdateMode.SyncBack;

            UpdateTrigger = new TimerUpdateTrigger { UpdateFrequency = 1.0 / 60 };
            surface.RegisterUpdateTrigger(UpdateTrigger);

            ILedGroup group = new ListLedGroup(surface.Leds);


            // Create new gradient
            IGradient gradient = new RainbowGradient();

            // Add a MoveGradientDecorator to the gradient
            gradient.AddDecorator(new RGB.NET.Decorators.Gradient.MoveGradientDecorator());

            // Make new LinearGradientBrush from gradient
            LinearGradientBrush nBrush = new LinearGradientBrush(gradient);

            // Apply LinearGradientBrush to led group
            group.Brush = nBrush;

            // Start UpdateTrigger, without this, the gradient will be drawn, but will not move.
            UpdateTrigger.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }

        private static void LoadDeviceProviders()
        {
            string deviceProvierDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty, DEVICEPROVIDER_DIRECTORY);
            if (!Directory.Exists(deviceProvierDir)) return;

            foreach (string file in Directory.GetFiles(deviceProvierDir, "*.dll"))
            {
                try
                {
                    Logger.Debug("Loading provider " + file);
                    Assembly assembly = Assembly.LoadFrom(file);
                    foreach (Type loaderType in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass
                                                                               && typeof(IRGBDeviceProviderLoader).IsAssignableFrom(t)))
                    {
                        if (Activator.CreateInstance(loaderType) is IRGBDeviceProviderLoader deviceProviderLoader)
                        {
                            //TODO DarthAffe 03.06.2018: Support Initialization
                            if (deviceProviderLoader.RequiresInitialization) continue;

                            RGBSurface.Instance.LoadDevices(deviceProviderLoader, RGBDeviceType.All, false, true);
                            Logger.Debug(file + " has been loaded");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error loading " + file);
                    Logger.Error(ex);
                }
            }
        }
    }
}
