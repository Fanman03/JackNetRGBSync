using System.Collections.Generic;
using RGBSyncPlus.Model;

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

        public int StartDelay { get; set; } = 0;

        public bool MinimizeToTray { get; set; } = false;

        public bool EnableDiscordRPC { get; set; } = true;

        public bool EnableClient { get; set; } = false;

        public bool EnableServer { get; set; } = false;

        public string versionURL { get; set; } = "https://fanman03.com/inc/version.json";

        public string updateURL { get; set; } = "http://fanman03.com/dlmgr/click.php?id=jnsync_latest";

        #endregion
    }
}
