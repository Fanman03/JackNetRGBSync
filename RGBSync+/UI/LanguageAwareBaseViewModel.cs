﻿using SimpleLed;

namespace RGBSyncPlus.UI
{
    public class LanguageAwareBaseViewModel : BaseViewModel
    {
        private readonly bool hasSetEvent;

        public LanguageAwareBaseViewModel()
        {
            try
            {
                ApplicationManager.Instance.LanguageChangedEvent += (sender, args) =>
                {
                    Language = ApplicationManager.Instance.NGSettings.Lang;
                };
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
