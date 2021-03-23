using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Autofac.Core;
using SyncStudio.ClientService;

namespace SyncStudio.WPF.UI.Tabs
{
    public class ProfileTabViewModel : LanguageAwareBaseViewModel
    {
        private ClientService.Settings settings = new Settings();
        private ClientService.Profiles _profiles = ServiceManager.Container.Resolve<ClientService.Profiles>();
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
                    if (_profiles.GetCurrentProfileSync().Name != newProfileName)
                    {
                        //tood
                        Debug.WriteLine("todo");

                        //_profiles.LoadProfileFromName(newProfileName);

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
        public bool ShowTriggers
        {
            get => showTriggers;
            set => SetProperty(ref showTriggers, value);
        }

        public ProfileTabViewModel()
        {
            ProfileNames = new ObservableCollection<string>(_profiles.GetAvailableProfilesSync().Select(x => x.Name).ToList());
            SetUpProfileModels();

            EnsureCorrectProfileIndex();

            OnPropertyChanged(nameof(ProfileTriggerTypeNames));

            //todo
            //ServiceManager.Instance.ConfigService.Settings.ProfileChange += delegate (object sender, EventArgs args) { CheckCurrentProfile(); };
        }

        public void SetUpProfileModels(bool setActive = true)
        {
            try
            {
                if (ProfileNames != null)
                {
                    ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry> triggers = ServiceManager.Instance.ProfileTriggerManager.ProfileTriggers;
                    ProfileItems.Clear();
                    foreach (string profileName in ProfileNames)
                    {
                        IEnumerable<ProfileTriggerManager.ProfileTriggerEntry> relevantTriggers = triggers.Where(x => x.ProfileName?.ToLower() == profileName.ToLower());

                        ProfileItems.Add(new ProfileItemViewModel
                        {
                            OriginalName = profileName,
                            Name = profileName,
                            Triggers =
                                new ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry>(relevantTriggers),
                            IsActiveProfile = ActiveProfile == profileName,

                        });
                    }

                    if (setActive)
                    {
                        //todo
                        //ActiveProfile = ServiceManager.Instance.ConfigService.Settings.CurrentProfile;
                    }
                }
            }
            catch
            {
            }
        }

        public void CheckCurrentProfile()
        {
            //todo
            //if (ActiveProfile != ServiceManager.Instance.ConfigService.Settings.CurrentProfile)
            //{
            //    ActiveProfile = ServiceManager.Instance.ConfigService.Settings.CurrentProfile;
            //}


            //foreach (ProfileItemViewModel profileItemViewModel in ProfileItems)
            //{
            //    profileItemViewModel.IsActiveProfile = ActiveProfile == profileItemViewModel.Name;
            //}
        }

        public void EnsureCorrectProfileIndex()
        {
            //todo?
            return;
            //if (profileItems != null && SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile() != null)
            //{
            //    if (profileNames != null)
            //    {
            //        SelectedProfileIndex = profileNames.IndexOf(SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile().Name);
            //        SelectedProfileItem = SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile().Name;
            //    }
            //}
        }

        private readonly Action<string> modalSubmitAction;

        public void CreateNewProfileUI()
        {
            ShowEditProfile = true;
            CurrentProfile = new ProfileItemViewModel { };

            CurrentProfile.Name = "Untitled";
            CurrentProfile.ProfileId = Guid.NewGuid();
            IsCreateButton = true;

        }

        public void CreateProfile()
        {
            //ServiceManager.Instance.ProfileService.GenerateNewProfile(CurrentProfile.Name);
            RefreshProfiles();
        }

        public void DeleteProfile(ProfileItemViewModel dc)
        {
           // ServiceManager.Instance.ProfileService.DeleteProfile(dc.Name);
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

        public void RefreshProfiles(bool setActive = true)
        {
            ProfileNames = new ObservableCollection<string>(_profiles.GetAvailableProfilesSync().Select(x => x.Name).ToList());
            SetUpProfileModels(setActive);
            ShowEditProfile = false;
            OnPropertyChanged("ProfileNames");
            OnPropertyChanged("ProfileItems");
        }

        public void SaveProfile()
        {
           // ServiceManager.Instance.ProfileService.RenameProfile(CurrentProfile.OriginalName, CurrentProfile.Name);
            RefreshProfiles();
        }

        public void SwitchToProfile(ProfileItemViewModel dc)
        {
          //  ServiceManager.Instance.ProfileService.LoadProfileFromName(dc.Name);
            RefreshProfiles();
            AppBVM appBVM = new AppBVM();
            appBVM.RefreshProfiles();
            appBVM.PopupVisibility = System.Windows.Visibility.Collapsed;
        }


        public void CreateNewTrigger()
        {
            ServiceManager.Instance.ProfileTriggerManager.ProfileTriggers.Add(new ProfileTriggerManager.ProfileTriggerEntry
            {
                Name = "No name",
                TriggerType = ProfileTriggerManager.ProfileTriggerTypes.RunningProccess,
                ProfileName = CurrentProfile.OriginalName,
                ProfileId = CurrentProfile.ProfileId
            });

            CurrentProfile.Triggers = new ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry>(
                ServiceManager.Instance.ProfileTriggerManager.ProfileTriggers.Where(x =>
                    x.ProfileName == CurrentProfile.OriginalName));

            OnPropertyChanged("ProfileNames");
            OnPropertyChanged("ProfileItems");
            OnPropertyChanged("CurrentProfile");
        }

        public void DeleteTrigger(ProfileTriggerManager.ProfileTriggerEntry entry)
        {
            ProfileTriggerManager.ProfileTriggerEntry killMe = ServiceManager.Instance.ProfileTriggerManager.ProfileTriggers.First(x => x.Id == entry.Id);
            ServiceManager.Instance.ProfileTriggerManager.ProfileTriggers.Remove(killMe);

            RefreshProfiles();

            CurrentProfile.Triggers = new ObservableCollection<ProfileTriggerManager.ProfileTriggerEntry>(
                ServiceManager.Instance.ProfileTriggerManager.ProfileTriggers.Where(x =>
                    x.ProfileName == CurrentProfile.OriginalName));

            OnPropertyChanged("ProfileNames");
            OnPropertyChanged("ProfileItems");
            OnPropertyChanged("CurrentProfile");
        }
    }
}
