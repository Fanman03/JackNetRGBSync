using SimpleLed;

namespace RGBSyncPlus.UI
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        public LanguageAwareBaseViewModel()
        {
            try
            {
                if (ApplicationManager.Instance != null)
                {
                    ApplicationManager.Instance.LanguageChangedEvent += (sender, args) =>
                    {
                        if (ApplicationManager.Instance?.NGSettings != null)
                        {
                            Language = ApplicationManager.Instance.NGSettings.Lang;
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
