using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.Languages;
using RGBSyncPlus.Model;
using SimpleLed;

namespace RGBSyncPlus.UI.Tabs
{
    public class SettingsUIViewModel : BaseViewModel
    {
        public DeviceMappingModels.NGSettings NGSettings { get; set; }

        public SettingsUIViewModel()
        {
            NGSettings = ApplicationManager.Instance.NGSettings;
        }

        public ObservableCollection<LanguageOption> Languages { get; set; } =
            new ObservableCollection<LanguageOption>(
                LanguageManager.Languages.Select(x => new LanguageOption { Name = x.NativeName, Code = x.Code }));


        private LanguageOption currentLanguage;

        public LanguageOption CurrentLanguage
        {
            get => currentLanguage;
            set
            {
                currentLanguage = value;
                NGSettings.Lang = value.Code;
                
            }
        }
        public class LanguageOption
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
