namespace SharedCode
{
    public class LauncherPrefs
    {
        public enum ReleaseType
        {
            Release,
            Beta,
            CI
        }

        public ReleaseType ReleaseBranch { get; set; } = ReleaseType.CI;
        public bool MinimizeOnStartUp { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool RunAsAdmin { get; set; }

        public int ReleaseInstalled { get; set; } = 0;
        public ReleaseType ReleaseTypeInstalled { get; set; } = ReleaseType.Release;
    }
}
