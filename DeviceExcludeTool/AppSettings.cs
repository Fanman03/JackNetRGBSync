using System;
using System.Collections.Generic;

namespace DeviceExcludeTool
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

        public int StartDelay { get; set; } = 0;

        public string ApiKey { get; set; } = null;

        public bool MinimizeToTray { get; set; } = false;

        public bool EnableDiscordRPC { get; set; } = true;
        public bool RunAsAdmin { get; set; } = false;
        public int DeviceTypes { get; set; } = -1;

        public bool EnableClient { get; set; } = false;

        public bool EnableServer { get; set; } = false;

        public string BackgroundImg { get; set; } = "default";

        public string versionURL { get; set; } = "https://fanman03.com/inc/version.json";

        public string updateURL { get; set; } = "http://fanman03.com/dlmgr/click.php?id=jnsync_latest";

        public bool ShowHelperConsole { get; set; } = false;

        #endregion
    }
}
