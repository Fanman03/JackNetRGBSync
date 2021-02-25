using SimpleLed;

namespace SyncStudio.WPF.UI
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        public LanguageAwareBaseViewModel()
        {
            try
            {
                if (ServiceManager.Instance.ApplicationManager != null)
                {
                    ServiceManager.Instance.ApplicationManager.LanguageChangedEvent += (sender, args) =>
                    {
                        if (ServiceManager.Instance?.ConfigService?.Settings != null)
                        {
                            Language = ServiceManager.Instance.ConfigService.Settings.Lang;
                        }
                    };
                }
            }
            catch
            {
            }
        }
        private string language;

        public string Language
        {
            get => language;
            set => SetProperty(ref language, value);
        }

    }
}
