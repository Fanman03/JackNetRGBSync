using RGBSyncPlus.Languages;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using RGBSyncPlus.Model;
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
                new TabItem("","Devices","devices"),
                new TabItem("", "Profiles","profiles"),
                new TabItem("", "Palettes","palettes"),
                new TabItem("","Store","store"),
                new TabItem("","About","about"),
                new TabItem("","News","news"),
              //  new TabItem("","Crash","crashme")
            };

            foreach (TabItem tabItem in TabItems.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
            {
                tabItem.Name = LanguageManager.GetValue("Main." + tabItem.Key);
            }
            OnPropertyChanged(nameof(TabItems));

            ApplicationManager.Instance.LanguageChangedEvent += delegate (object sender, EventArgs args)
            {
                ObservableCollection<TabItem> ti = new ObservableCollection<TabItem>
                {
                    new TabItem("","",""),
                    new TabItem("","Devices","devices",true),
                    new TabItem("", "Profiles","profiles"),
                    new TabItem("", "Palettes","palettes"),
                    new TabItem("","Store","store"),
                    new TabItem("","About","about"),
                    new TabItem("","News","news"),
                //    new TabItem("","Crash","crashme")
                };

                foreach (TabItem tabItem in ti.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Key)))
                {
                    tabItem.Name = LanguageManager.GetValue("Main." + tabItem.Key, Language);
                }

                TabItems = ti;
            };

            CurrentTab = "devices";

            ApplicationManager.Instance.NGSettings.BGChangedEvent += (sender, args) =>
            {
                this.BackGround = ApplicationManager.Instance.NGSettings.Background;
                this.BackgroundOpacity = ApplicationManager.Instance.NGSettings.BackgroundOpacity;
                this.DimBackgroundOpacity = ApplicationManager.Instance.NGSettings.DimBackgroundOpacity;
                this.BackgroundBlur = ApplicationManager.Instance.NGSettings.BackgroundBlur;
                this.ControllableBG = ApplicationManager.Instance.NGSettings.ControllableBG;
                this.RainbowTabBars = ApplicationManager.Instance.NGSettings.RainbowTabBars;
            };

            this.BackGround = ApplicationManager.Instance.NGSettings.Background;
            this.BackgroundOpacity = ApplicationManager.Instance.NGSettings.BackgroundOpacity;
            this.DimBackgroundOpacity = ApplicationManager.Instance.NGSettings.DimBackgroundOpacity;
            this.BackgroundBlur = ApplicationManager.Instance.NGSettings.BackgroundBlur;
            this.ControllableBG = ApplicationManager.Instance.NGSettings.ControllableBG;
            this.RainbowTabBars = ApplicationManager.Instance.NGSettings.RainbowTabBars;

            DispatcherTimer update = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            update.Tick += (sender, args) =>
            {
                if (ControllableBG)
                {
                    var rbd = ApplicationManager.Instance.RssBackgroundDevice;
                    SCTop = GetBrush(rbd.Leds[0]);
                    SCTopRight = GetBrush(rbd.Leds[1]);
                    SCRight = GetBrush(rbd.Leds[2]);
                    SCBottomRight = GetBrush(rbd.Leds[3]);
                    SCBottom = GetBrush(rbd.Leds[4]);
                    SCBottomLeft = GetBrush(rbd.Leds[5]);
                    SCLeft = GetBrush(rbd.Leds[6]);
                    SCTopLeft = GetBrush(rbd.Leds[7]);
                }
            };

            update.Start();
        }

        RadialGradientBrush GetBrush(LEDColor clr)
        {
            return new RadialGradientBrush(new Color()
            {
                R = (byte)clr.Red,
                G = (byte)clr.Green,
                B = (byte)clr.Blue,
                A = 255
            }, Colors.Transparent);
        }

        private RadialGradientBrush scleft = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public RadialGradientBrush SCLeft
        {
            get => scleft;
            set => SetProperty(ref scleft, value);
        }

        private RadialGradientBrush sctop = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public RadialGradientBrush SCTop
        {
            get => sctop;
            set => SetProperty(ref sctop, value);
        }

        private RadialGradientBrush scright = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public RadialGradientBrush SCRight
        {
            get => scright;
            set => SetProperty(ref scright, value);
        }

        private RadialGradientBrush scbottom = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public RadialGradientBrush SCBottom
        {
            get => scbottom;
            set => SetProperty(ref scbottom, value);
        }



        private RadialGradientBrush sctopleft = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public RadialGradientBrush SCTopLeft
        {
            get => sctopleft;
            set => SetProperty(ref sctopleft, value);
        }


        private RadialGradientBrush sctopright = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public RadialGradientBrush SCTopRight
        {
            get => sctopright;
            set => SetProperty(ref sctopright, value);
        }





        private RadialGradientBrush scbottomleft = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public RadialGradientBrush SCBottomLeft
        {
            get => scbottomleft;
            set => SetProperty(ref scbottomleft, value);
        }


        private RadialGradientBrush scbottomright = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public RadialGradientBrush SCBottomRight
        {
            get => scbottomright;
            set => SetProperty(ref scbottomright, value);
        }




        private bool controllableBG;

        public bool ControllableBG
        {
            get => controllableBG;
            set => SetProperty(ref controllableBG, value);
        }


        private bool rainbowTabBars;

        public bool RainbowTabBars
        {
            get => rainbowTabBars;
            set => SetProperty(ref rainbowTabBars, value);
        }



        private float backgroundOpacity;

        public float BackgroundOpacity
        {
            get => backgroundOpacity;
            set => SetProperty(ref backgroundOpacity, value);
        }


        private float backgroundBlur;

        public float BackgroundBlur
        {
            get => backgroundBlur;
            set => SetProperty(ref backgroundBlur, value);
        }

        private float dimbackgroundOpacity;

        public float DimBackgroundOpacity
        {
            get => dimbackgroundOpacity;
            set => SetProperty(ref dimbackgroundOpacity, value);
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

        private string background;

        public string BackGround
        {
            get => background;
            set => SetProperty(ref background, value);
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

                foreach (TabItem tabItem in TabItems)
                {
                    tabItem.IsCurrent = tabItem.Key.ToLower() == value.ToLower();
                }

                //  OnPropertyChanged("TabItems");
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


        public class TabItem : LanguageAwareBaseViewModel
        {
            private string name;
            public string Name { get => name; set => SetProperty(ref name, value); }

            private string key;
            public string Key { get => key; set => SetProperty(ref key, value); }

            private string icon;
            public string Icon { get => icon; set => SetProperty(ref icon, value); }

            private bool isCurrent;
            public bool IsCurrent { get => isCurrent; set => SetProperty(ref isCurrent, value); }

            public TabItem(string icon, string name, string key, bool current = false)
            {
                Icon = icon;
                Name = name;
                Key = key;
                IsCurrent = current;

            }
        }
    }
}
