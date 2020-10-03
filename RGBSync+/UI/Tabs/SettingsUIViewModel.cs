using System;
using System.Collections.Generic;
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
    }
}
