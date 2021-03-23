using SimpleLed;
using SyncStudio.ClientService;

namespace SyncStudio.WPF.UI
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        private ClientService.Settings settings = new Settings();
        public LanguageAwareBaseViewModel()
        {
            try
            {
                if (ServiceManager.Instance.ApplicationManager != null)
                {
                    ServiceManager.Instance.ApplicationManager.LanguageChangedEvent += (sender, args) =>
                    {
                        if (settings != null)
                        {
                            Language = settings.Lang;
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
