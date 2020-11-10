using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.Languages;
using SimpleLed;

namespace RGBSyncPlus.UI
{
    public class MainWindowViewModel : LanguageAwareBaseViewModel
    {
        public MainWindowViewModel()
        {
            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem("","",""),
                new TabItem("","Devices","devices"),
                new TabItem("", "Profiles","profiles"),
                new TabItem("","Drivers","store"),
                new TabItem("","About","about"),
                new TabItem("","Crash","crashme")
            };

            foreach (TabItem tabItem in TabItems.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
            {
                tabItem.Name = LanguageManager.GetValue("Main." + tabItem.Key);
            }
            OnPropertyChanged(nameof(TabItems));

            ApplicationManager.Instance.LanguageChangedEvent += delegate (object sender, EventArgs args)
            {
                var ti= new ObservableCollection<TabItem>
                {
                    new TabItem("","",""),
                    new TabItem("","Devices","devices"),
                    new TabItem("", "Profiles","profiles"),
                    new TabItem("","Drivers","store"),
                    new TabItem("","About","about"),
                    new TabItem("","Crash","crashme")
                };

                foreach (TabItem tabItem in ti.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
                {
                    tabItem.Name = LanguageManager.GetValue("Main." + tabItem.Key, Language);
                }

                TabItems = ti;
            };
        }

        private bool hamburgerExtended;

        public bool HamburgerExtended
        {
            get => hamburgerExtended;
            set => SetProperty(ref hamburgerExtended, value);
        }

        private bool showModal;

        public bool ShowModal
        {
            get => showModal;
            set => SetProperty(ref showModal, value);
        }

        private string modalText = "Please Wait";

        public string ModalText
        {
            get => modalText;
            set => SetProperty(ref modalText, value);
        }

        private bool showModalTextBox = false;

        public bool ShowModalTextBox
        {
            get => showModalTextBox;
            set => SetProperty(ref showModalTextBox, value);
        }

        private bool showModalCloseButton = false;

        public bool ShowModalCloseButton
        {
            get => showModalCloseButton;
            set => SetProperty(ref showModalCloseButton, value);
        }

        private string currentTab = "devices";

        public string CurrentTab
        {
            get => currentTab;
            set
            {
                SetProperty(ref currentTab, value);
                if (value == "crashme")
                {
                    int i = 11;
                    i = i - 11;
                    Debug.WriteLine(11 / i);
                }
            }
        }

        private ObservableCollection<TabItem> tabItems = new ObservableCollection<TabItem>();

        public ObservableCollection<TabItem> TabItems
        {
            get => tabItems;
            set => SetProperty(ref tabItems, value);
        }

        private int modalPercentage = 0;
        public int ModalPercentage { get => modalPercentage; set => SetProperty(ref modalPercentage, value); }

        private bool modalShowPercentage;

        public bool ModalShowPercentage
        {
            get => modalShowPercentage;
            set => SetProperty(ref modalShowPercentage, value);
        }

        public class TabItem
        {
            public string Name { get; set; }
            public string Key { get; set; }
            public string Icon { get; set; }

            public TabItem(string icon, string name, string key)
            {
                Icon = icon;
                Name = name;
                Key = key;

            }
        }
    }
}
