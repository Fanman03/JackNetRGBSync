
using System.Diagnostics;

namespace RGBSyncPlus.Configuration
{
    public class AppSettings
    {
        #region Constants

        public const int CURRENT_VERSION = 1;

        #endregion

        #region Properties & Fields
        public string Lang { get; set; } = "en";
        public int Version { get; set; } = 0;
        public double UpdateRate { get; set; } = 30.0;
        public bool RunOnStartup { get; set; } = false;
        public int StartDelay { get; set; } = 15;
        public string ApiKey { get; set; } = null;
        private bool minimizeToTray = false;
        public bool MinimizeToTray
        {
            get
            {
                if (Debugger.IsAttached) return false;
                return minimizeToTray;
            }
            set => minimizeToTray = value;
        }
        public bool EnableDiscordRPC { get; set; } = true;
        public bool RunAsAdmin { get; set; } = false;
        //public RGBDeviceType DeviceTypes { get; set; } = RGBDeviceType.All;
        public bool EnableClient { get; set; } = false;
        public bool EnableServer { get; set; } = false;
        public string BackgroundImg { get; set; } = "default";
        public string versionURL { get; set; } = "https://fanman03.com/inc/version.json";
        public string updateURL { get; set; } = "http://fanman03.com/dlmgr/click.php?id=jnsync_latest";
        public bool ShowHelperConsole { get; set; } = false;
        #endregion
    }
}
