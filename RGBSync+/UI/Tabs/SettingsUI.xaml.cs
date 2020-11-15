using System.ComponentModel;
using System.Windows.Controls;

namespace RGBSyncPlus.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsUI : UserControl
    {
        public SettingsUI()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                (this.DataContext as SettingsUIViewModel)?.Init();
            }
        }
    }
}
