using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleLed;

namespace RGBSyncPlus
{
    public class ProfileTriggerManager : BaseViewModel
    {
        private ObservableCollection<ProfileTriggerEntry> profileTriggers = new ObservableCollection<ProfileTriggerEntry>();

        public ObservableCollection<ProfileTriggerEntry> ProfileTriggers
        {
            get => profileTriggers;
            set => SetProperty(ref profileTriggers, value);
        }

        private List<Guid> blockedTriggers = new List<Guid>();

        public ProfileTriggerManager()
        {
            ProfileTriggers.Add(new ProfileTriggerEntry
            {
                Id = Guid.NewGuid(),
                ProfileName = "Default",
                TriggerType = ProfileTriggerTypes.RunningProccess,
                ProcessName = "Calculator",
                TriggerWhenNotFound = false,
                Name = "Calc.exe opened"
            });

            ProfileTriggers.Add(new ProfileTriggerEntry
            {
                Id = Guid.NewGuid(),
                ProfileName = "Default",
                TriggerType = ProfileTriggerTypes.TimeBased,
                Hour = 0,
                Minute = 31,
                Name = "31 mins past midnight"

            });


            ProfileTriggers.CollectionChanged += ProfileTriggers_CollectionChanged;
        }

        private void ProfileTriggers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("Profile Triggers Changed");
        }

        public void CheckTriggers()
        {
            Process[] processlist = Process.GetProcesses();

            foreach (ProfileTriggerEntry profileTriggerEntry in ProfileTriggers)
            {
                bool doit = false;
                switch (profileTriggerEntry.TriggerType)
                {
                    case ProfileTriggerTypes.RunningProccess:
                        {
                            bool foundProcess = processlist.Any(x => x.ProcessName == profileTriggerEntry.ProcessName);

                            doit = foundProcess;
                            if (profileTriggerEntry.TriggerWhenNotFound)
                            {
                                doit = !doit;
                            }



                            break;
                        }

                    case ProfileTriggerTypes.TimeBased:
                        {
                            doit = (DateTime.Now.Minute == profileTriggerEntry.Minute &&
                                    DateTime.Now.Hour == profileTriggerEntry.Hour);
                            break;
                        }
                }

                if (doit && blockedTriggers.All(x => x != profileTriggerEntry.Id))
                {
                    ApplicationManager.Instance.LoadProfileFromName(profileTriggerEntry.ProfileName);
                    blockedTriggers.Add(profileTriggerEntry.Id);
                }

                if (!doit && blockedTriggers.Any(x => x == profileTriggerEntry.Id))
                {
                    blockedTriggers.Remove(profileTriggerEntry.Id);
                }
            }
        }

        public class ProfileTriggerEntry : BaseViewModel
        {
            private bool expanded = false;

            [JsonIgnore]
            public bool Expanded
            {
                get => expanded;
                set => SetProperty(ref expanded, value);
            }
            private Guid id;

            public Guid Id
            {
                get => id;
                set => SetProperty(ref id, value);
            }

            private string name;
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }

            private string profileName;
            public string ProfileName
            {
                get => profileName;
                set => SetProperty(ref profileName, value);
            }

            private Guid profileId;

            public Guid ProfileId
            {
                get => profileId;
                set => SetProperty(ref profileId, value);
            }

            private string triggerType;

            public string TriggerType
            {
                get => triggerType;
                set => SetProperty(ref triggerType, value);
            }

            //RunningProccess

            private string processName;

            public string ProcessName
            {
                get => processName;
                set => SetProperty(ref processName, value);
            }

            private bool triggerWhenNotFound;

            public bool TriggerWhenNotFound
            {
                get => triggerWhenNotFound;
                set => SetProperty(ref triggerWhenNotFound, value);
            }

            //TimeBased

            private int hour = 0;

            public int Hour
            {
                get => hour;
                set => SetProperty(ref hour, value);
            }

            private int minute = 0;

            public int Minute
            {
                get => minute;
                set => SetProperty(ref minute, value);
            }
        }

        public static class ProfileTriggerTypes
        {
            public const string RunningProccess = "Running Process";
            public const string TimeBased = "Time Based";
            public const string DiscordTrigger = "Discord Trigger";

        }
    }
}
