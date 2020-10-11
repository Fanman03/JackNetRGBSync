using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RGBSyncPlus.UI.Tabs
{
    public class ProfileTabViewModel : BaseViewModel
    {
        private string activeProfile;

        public string ActiveProfile
        {
            get => activeProfile;
            set => SetProperty(ref activeProfile, value);
        }

        private ProfileItemViewModel currentProfile;

        public ProfileItemViewModel CurrentProfile
        {
            get => currentProfile;
            set => SetProperty(ref currentProfile, value);
        }

        public class ProfileItemViewModel : BaseViewModel
        {
            private string name;
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }

            private ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry> triggers;

            public ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry> Triggers
            {
                get => triggers;
                set => SetProperty(ref triggers, value);
            }

            public string OriginalName { get; set; }
        }
        private ObservableCollection<string> profileTriggerTypeNames = new ObservableCollection<string>
        {
            ProfileTriggerManager.ProfileTriggerTypes.TimeBased,
            ProfileTriggerManager.ProfileTriggerTypes.RunningProccess
        };

        public ObservableCollection<string> ProfileTriggerTypeNames
        {
            get => profileTriggerTypeNames;
            set => SetProperty(ref profileTriggerTypeNames, value);
        }
        private ObservableCollection<ProfileItemViewModel> profileItems = new ObservableCollection<ProfileItemViewModel>();

        public ObservableCollection<ProfileItemViewModel> ProfileItems
        {
            get => profileItems;
            set => SetProperty(ref profileItems, value);
        }

        private string selectedProfileItem;
        public string SelectedProfileItem
        {
            get => selectedProfileItem;
            set
            {
                SetProperty(ref selectedProfileItem, value);
            }
        }

        private bool showManageProfiles;

        public bool ShowManageProfiles
        {
            get => showManageProfiles;
            set => SetProperty(ref showManageProfiles, value);
        }

        private int selectedProfileIndex = 0;

        public int SelectedProfileIndex
        {
            get => selectedProfileIndex;
            set
            {
                selectedProfileIndex = value;
                if (value > -1 && value < profileNames.Count)
                {
                    string newProfileName = ProfileNames[value];
                    if (ApplicationManager.Instance.CurrentProfile.Name != newProfileName)
                    {
                        ApplicationManager.Instance.LoadProfileFromName(newProfileName);

                    }
                }
            }
        }

        private ObservableCollection<string> profileNames;

        public ObservableCollection<string> ProfileNames
        {
            get => profileNames;
            set => SetProperty(ref profileNames, value);
        }

        private bool showEditProfile;
        public bool ShowEditProfile
        {
            get => showEditProfile;
            set => SetProperty(ref showEditProfile, value);
        }

        private bool showTriggers;
        public bool ShowTriggers { get=>showTriggers;
            set => SetProperty(ref showTriggers, value);
        }

        public ProfileTabViewModel()
        {
            ProfileNames = ApplicationManager.Instance.NGSettings.ProfileNames;
            SetUpProfileModels();

            EnsureCorrectProfileIndex();

            OnPropertyChanged(nameof(ProfileTriggerTypeNames));
        }

        public void SetUpProfileModels()
        {
            ProfileItems.Clear();
            foreach (string profileName in ProfileNames)
            {
                ProfileItems.Add(new ProfileItemViewModel
                {
                    OriginalName = profileName,
                    Name = profileName,
                    Triggers = new ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry>(ApplicationManager.Instance.ProfileTriggerManager.ProfileTriggers.Where(x => x.ProfileName == profileName))
                });
            }

            ActiveProfile = ApplicationManager.Instance.NGSettings.CurrentProfile;
        }

        public void SubmitModalTextBox(string text)
        {
            modalSubmitAction?.Invoke(text);
        }

        public void ShowCreateNewProfile()
        {
            ApplicationManager.Instance.ShowModal(new ModalModel
            {
                ModalText = "Enter name for new profile",
                ShowModalTextBox = true,
                ShowModalCloseButton = true,
                modalSubmitAction = (text) =>
                {
                    ApplicationManager.Instance.GenerateNewProfile(text);
                    ProfileNames = ApplicationManager.Instance.NGSettings.ProfileNames;
                    ApplicationManager.Instance.LoadProfileFromName(text);
                    EnsureCorrectProfileIndex();
                }
            });

        }

        public void EnsureCorrectProfileIndex()
        {
            SelectedProfileIndex = profileNames.IndexOf(ApplicationManager.Instance.CurrentProfile.Name);
            SelectedProfileItem = ApplicationManager.Instance.CurrentProfile.Name;
        }

        private Action<string> modalSubmitAction;

        public void CreateNewProfileUI()
        {
            ShowEditProfile = true;
            CurrentProfile = new ProfileItemViewModel{};

            CurrentProfile.Name = "Untitled";
            IsCreateButton = true;

        }

        public void CreateProfile()
        {
            ApplicationManager.Instance.GenerateNewProfile(CurrentProfile.Name);
            RefreshProfiles();
        }

        public void DeleteProfile(ProfileItemViewModel dc)
        {
            ApplicationManager.Instance.DeleteProfile(dc.Name);
            RefreshProfiles();
        }

        public void EditProfile(ProfileItemViewModel dc)
        {
            CurrentProfile = dc;
            ShowEditProfile = true;
            IsCreateButton = false;
        }

        private bool isCreateButton;

        public bool IsCreateButton
        {
            get => isCreateButton;
            set => SetProperty(ref isCreateButton, value);
        }

        private void RefreshProfiles()
        {
            ProfileNames = ApplicationManager.Instance.NGSettings.ProfileNames;
            SetUpProfileModels();
            ShowEditProfile = false;
            OnPropertyChanged("ProfileNames");
            OnPropertyChanged("ProfileItems");
        }

        public void SaveProfile()
        {
            ApplicationManager.Instance.RenameProfile(CurrentProfile.OriginalName, CurrentProfile.Name);
            RefreshProfiles();
        }

        public void SwitchToProfile(ProfileItemViewModel dc)
        {
            ApplicationManager.Instance.LoadProfileFromName(dc.Name);
            RefreshProfiles();
        }
    }
}
