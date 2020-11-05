using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.Model;

namespace RGBSyncPlus.UI.Tabs
{
    public class SettingsUIViewModel
    {
        public DeviceMappingModels.NGSettings NGSettings { get; set; }

        public SettingsUIViewModel()
        {
            NGSettings = ApplicationManager.Instance.NGSettings;
        }

        public ObservableCollection<LanguageOption> Languages { get; set; } = new ObservableCollection<LanguageOption>
        {
            new LanguageOption{ Name = "English", Code="en"},
            new LanguageOption{ Name = "Americanish", Code="en"},
            new LanguageOption{ Name = "Spaghett", Code="it"},

        };

        public class LanguageOption
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
