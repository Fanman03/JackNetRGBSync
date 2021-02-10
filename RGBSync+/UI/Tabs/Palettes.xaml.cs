using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RGBSyncPlus.Helper;
using SimpleLed;

namespace RGBSyncPlus.UI.Tabs
{
    /// <summary>
    /// Interaction logic for Palettes.xaml
    /// </summary>
    public partial class Palettes : UserControl
    {
        PalettesViewModel vm => (PalettesViewModel)DataContext;
        public Palettes()
        {
            InitializeComponent();
        }

        private void AddColor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ColorBank bank = button.DataContext as ColorBank;

            if (bank.Colors.Count <= 20)
            {
                bank.Colors.Add(new ColorObject
                {
                    ColorString = "#ff0000"
                });

                vm.SaveProfile();
            }
            else
            {
                ServiceManager.Instance.ModalService.ShowSimpleModal("Color banks are limited to 20 colors");
            }
        }

        private void debugclick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            ColorModel cm = b.DataContext as ColorModel;
            Debug.WriteLine(cm);
            cm = new ColorModel(80, 80, 80);
        }

        private void DeleteColor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ColorObject colorObject = button.DataContext as ColorObject;

            var parent = button.FindParent<ListView>();

            ColorBank parentContext =
                parent.DataContext as ColorBank;

            if (parentContext.Colors.Count > 2)
            {
                parentContext.Colors.Remove(colorObject);
                vm.SaveProfile();
            }
            else
            {
                ServiceManager.Instance.ModalService.ShowSimpleModal("Color banks require at least 2 colors");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.SaveProfile();
        }

        private void AddProfile(object sender, RoutedEventArgs e)
        {
            string profileName = "New Profile";

            if (vm.ColorProfiles.Any(x => x.ProfileName == profileName))
            {
                int ct = 1;

                while (vm.ColorProfiles.Any(x => x.ProfileName == profileName + " " + ct))
                {
                    ct++;
                }

                profileName = profileName + " " + ct;

            }

            ColorProfile cp = new ColorProfile
            {
                ProfileName = profileName,
                ColorBanks = new ObservableCollection<ColorBank>
                {
                    new ColorBank
                    {
                        BankName = "Primary",
                        Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#ff0000" } , new ColorObject { ColorString = "#000000" } }
                    },
                    new ColorBank
                    {
                        BankName = "Secondary",
                        Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#00ff00" } , new ColorObject { ColorString = "#000000" } }
                    },new ColorBank
                    {
                        BankName = "Tertiary",
                        Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#0000ff" } , new ColorObject { ColorString = "#000000" } }
                    },new ColorBank
                    {
                        BankName = "Auxilary",
                        Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#ff00ff" } , new ColorObject { ColorString = "#000000" } }
                    }
                }
            };

            vm.ColorProfiles.Add(cp);
            vm.CurrentProfile = cp;
        }

        private void DeleteProfile(object sender, RoutedEventArgs e)
        {
            if (vm.ColorProfiles.Count > 1)
            {
                vm.ColorProfiles.Remove(vm.CurrentProfile);
                File.Delete("ColorProfiles\\"+vm.CurrentProfile.Id+".json");
                vm.CurrentProfile = vm.ColorProfiles.First();
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var thingy = sender as ComboBox;
            ColorProfile cp = thingy.SelectedItem as ColorProfile;
            vm.CurrentProfile = cp;
        }
    }
}
