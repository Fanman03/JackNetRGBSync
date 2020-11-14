using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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


        private void DeleteProfile(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTabViewModel.ProfileItemViewModel dc = button.DataContext as ProfileTabViewModel.ProfileItemViewModel;

            vm.DeleteProfile(dc);
        }

        private void ToggleShowTriggers(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTabViewModel.ProfileItemViewModel dc = button.DataContext as ProfileTabViewModel.ProfileItemViewModel;
            vm.ShowTriggers = !vm.ShowTriggers;
            vm.CurrentProfile = dc;

            Model.DeviceMappingModels.NGProfile tempProfile = ApplicationManager.Instance.GetProfileFromName(dc.Name);
            vm.CurrentProfile.ProfileId = tempProfile.Id;
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

        private void SetTrigger(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTriggerManager.ProfileTriggerEntry dc = button.DataContext as ProfileTriggerManager.ProfileTriggerEntry;
            dc.TriggerType = (string)button.Tag;
            Debug.WriteLine(dc);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CloseTriggerList(object sender, RoutedEventArgs e)
        {
            vm.ShowTriggers = false;
        }

        private void ToggleTriggerWhenNotFound(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTriggerManager.ProfileTriggerEntry dc = button.DataContext as ProfileTriggerManager.ProfileTriggerEntry;
            dc.TriggerWhenNotFound = !dc.TriggerWhenNotFound;
        }

        private void AddNewTrigger(object sender, RoutedEventArgs e)
        {
            vm.CreateNewTrigger();
        }

        private void ToggleExpanded(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTriggerManager.ProfileTriggerEntry dc = button.DataContext as ProfileTriggerManager.ProfileTriggerEntry;

            dc.Expanded = !dc.Expanded;
        }

        private void DeleteTrigger(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ProfileTriggerManager.ProfileTriggerEntry entry =
                button.DataContext as ProfileTriggerManager.ProfileTriggerEntry;

            vm.DeleteTrigger(entry);
        }
    }
}
