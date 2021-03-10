namespace SyncStudio.WPF.UI.Tabs
{
    public class AboutViewModel : LanguageAwareBaseViewModel
    {
        private string appName = "TODO";//ServiceManager.Instance.SLSManager.AppName;
        public string AppName
        {
            get => appName;
            set => SetProperty(ref appName, value);
        }
    }
}
