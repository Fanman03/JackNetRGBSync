using System.Collections.Generic;

namespace StartupHelper.Configuration
{
    public class AppSettings
    {
        #region Constants

        public const int CURRENT_VERSION = 1;

        #endregion

        #region Properties & Fields
        public int Version { get; set; } = 0;
        public int StartDelay { get; set; } = 0;
        public bool RunOnStartup { get; set; } = false;
        public bool ShowHelperConsole { get; set; } = false;

        #endregion
    }
}
