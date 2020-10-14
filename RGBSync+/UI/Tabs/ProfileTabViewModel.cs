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
            set
            {
                SetProperty(ref activeProfile, value); 
                RefreshProfiles(false);
            }
        }

        private ProfileItemViewModel currentProfile;

        public ProfileItemViewModel CurrentProfile
        {
            get => currentProfile;
            set => SetProperty(ref currentProfile, value);
        }

        public class ProfileItemViewModel : BaseViewModel
        {
            private Guid profileId;

            public Guid ProfileId
            {
                get => profileId;
                set => SetProperty(ref profileId, value);
            }

            private bool isActiveProfile = false;
            public bool IsActiveProfile
            {
                get => isActiveProfile;
                set => SetProperty(ref isActiveProfile, value);
            }

            private string name;
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }

            public ObservableCollection<string> HoursList
            {
                get
                {
                    ObservableCollection<string> result = new ObservableCollection<string>();
                    for (int i = 0; i < 24; i++)
                    {
                        result.Add(i.ToString("00"));
                    }

                    return result;
                }
            }

            public ObservableCollection<string> MinutesList
            {
                get
                {
                    ObservableCollection<string> result = new ObservableCollection<string>();
                    for (int i = 0; i < 60; i++)
                    {
                        result.Add(i.ToString("00"));
                    }

                    return result;
                }
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

            ApplicationManager.Instance.NGSettings.ProfileChange+= delegate(object sender, EventArgs args) { CheckCurrentProfile(); };
        }

        public void SetUpProfileModels(bool setActive = true)
        {
            var triggers = ApplicationManager.Instance.ProfileTriggerManager.ProfileTriggers;
            ProfileItems.Clear();
            foreach (string profileName in ProfileNames)
            {
                var relevantTriggers = triggers.Where(x => x.ProfileName?.ToLower() == profileName.ToLower());

                ProfileItems.Add(new ProfileItemViewModel
                {
                    OriginalName = profileName,
                    Name = profileName,
                    Triggers = new ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry>(relevantTriggers),
                    IsActiveProfile = ActiveProfile == profileName,
                
                });
            }

            if (setActive)
            {
                ActiveProfile = ApplicationManager.Instance.NGSettings.CurrentProfile;
            }
        }

        public void CheckCurrentProfile()
        {
            if (ActiveProfile != ApplicationManager.Instance.NGSettings.CurrentProfile)
            {
                ActiveProfile = ApplicationManager.Instance.NGSettings.CurrentProfile;
            }


            foreach (ProfileItemViewModel profileItemViewModel in ProfileItems)
            {
                profileItemViewModel.IsActiveProfile = ActiveProfile == profileItemViewModel.Name;
            }
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
            CurrentProfile.ProfileId = Guid.NewGuid();
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

        private void RefreshProfiles(bool setActive = true)
        {
            ProfileNames = ApplicationManager.Instance.NGSettings.ProfileNames;
            SetUpProfileModels(setActive);
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

        public void CreateNewTrigger()
        {
            ApplicationManager.Instance.ProfileTriggerManager.ProfileTriggers.Add(new ProfileTriggerManager.ProfileTriggerEntry
            {
                Name = "No name",
                TriggerType = ProfileTriggerManager.ProfileTriggerTypes.RunningProccess,
                ProfileName = CurrentProfile.OriginalName,
                ProfileId = CurrentProfile.ProfileId
            });

            CurrentProfile.Triggers = new ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry>(
                ApplicationManager.Instance.ProfileTriggerManager.ProfileTriggers.Where(x =>
                    x.ProfileName == CurrentProfile.OriginalName));

            OnPropertyChanged("ProfileNames");
            OnPropertyChanged("ProfileItems");
            OnPropertyChanged("CurrentProfile");
        }
    }
}
