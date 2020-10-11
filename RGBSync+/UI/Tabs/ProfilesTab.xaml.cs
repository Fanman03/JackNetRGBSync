using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace RGBSyncPlus.UI.Tabs
{
    /// <summary>
    /// Interaction logic for ProfilesTab.xaml
    /// </summary>
    public partial class ProfilesTab : UserControl
    {

        ProfileTabViewModel vm => (ProfileTabViewModel)DataContext;
        public ProfilesTab()
        {
            InitializeComponent();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ConfigurationViewModel vm = (ConfigurationViewModel)this.DataContext;
            vm.ZoomLevel++;
        }

        private void DeleteProfile(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTabViewModel.ProfileItemViewModel dc = button.DataContext as ProfileTabViewModel.ProfileItemViewModel;
            vm.DeleteProfile(dc);
        }

        private void ToggleShowTriggers(object sender, RoutedEventArgs e)
        {
            vm.ShowTriggers = !vm.ShowTriggers;
        }

        private void AddNewProfile(object sender, RoutedEventArgs e)
        {
            vm.CreateNewProfileUI();
            }

        private void CreateProfile(object sender, RoutedEventArgs e)
        {
            vm.CreateProfile();
        }

        private void EditProfile(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTabViewModel.ProfileItemViewModel dc = button.DataContext as ProfileTabViewModel.ProfileItemViewModel;
            vm.EditProfile(dc);
        }

        private void ToggleShowProfile(object sender, RoutedEventArgs e)
        {
            vm.ShowEditProfile = !vm.ShowEditProfile;
        }

        private void CloseShowProfile(object sender, RoutedEventArgs e)
        {
            vm.ShowEditProfile = false;
            vm.SaveProfile();
        }


        private void SetProfile(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTabViewModel.ProfileItemViewModel dc = button.DataContext as ProfileTabViewModel.ProfileItemViewModel;
            vm.SwitchToProfile(dc);
        }
    }
}
