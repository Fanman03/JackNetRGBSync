using Newtonsoft.Json;
using RGBSyncPlus.UI;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace RGBSyncPlus
{
    public class ProfileTriggerManager : LanguageAwareBaseViewModel
    {
        private ObservableCollection<ProfileTriggerEntry> profileTriggers = new ObservableCollection<ProfileTriggerEntry>();

        public ObservableCollection<ProfileTriggerEntry> ProfileTriggers
        {
            get => profileTriggers;
            set => SetProperty(ref profileTriggers, value);
        }

        private readonly List<Guid> blockedTriggers = new List<Guid>();

        public ProfileTriggerManager()
        {
            if (File.Exists("NGProfileTriggers.json"))
            {
                try
                {
                    string json = File.ReadAllText("NGProfileTriggers.json");
                    List<ProfileTriggerEntry> temp = JsonConvert.DeserializeObject<List<ProfileTriggerEntry>>(json);
                    ProfileTriggers = new ObservableCollection<ProfileTriggerEntry>(temp);
                    IsDirty = false;

                }
                catch
                {
                }

                if (ProfileTriggers == null)
                {
                    ProfileTriggers = new ObservableCollection<ProfileTriggerEntry>();
                }
            }

            ProfileTriggers.CollectionChanged += ProfileTriggers_CollectionChanged;

        

        profileTriggerTimer = new DispatcherTimer();
        profileTriggerTimer.Interval = TimeSpan.FromSeconds(1);
        profileTriggerTimer.Tick += (sender, args) => ServiceManager.Instance.ProfileTriggerManager.CheckTriggers();
        profileTriggerTimer.Start();
        }

        public DispatcherTimer profileTriggerTimer;


        [JsonIgnore] public bool IsDirty = false;

        private void ProfileTriggers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsDirty = true;
        }

        private void CheckDirty()
        {
            if (IsDirty)
            {
                Debug.WriteLine("Writing profile Triggers");
                string json = JsonConvert.SerializeObject(ProfileTriggers.ToList());
                try
                {
                    File.WriteAllText("NGProfileTriggers.json", json);
                }
                catch
                {
                }

                IsDirty = false;

            }
        }

        public void CheckTriggers()
        {
            CheckDirty();

            Process[] processlist = Process.GetProcesses();
            if (ProfileTriggers != null)
            {
                foreach (ProfileTriggerEntry profileTriggerEntry in ProfileTriggers)
                {
                    bool doit = false;
                    switch (profileTriggerEntry?.TriggerType)
                    {
                        case ProfileTriggerTypes.RunningProccess:
                            {
                                bool foundProcess = processlist.Any(x =>
                                    x.ProcessName.ToLower() == profileTriggerEntry?.ProcessName?.ToLower());

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
                        ServiceManager.Instance.ProfileService.LoadProfileFromName(profileTriggerEntry.ProfileName);
                        blockedTriggers.Add(profileTriggerEntry.Id);
                    }

                    if (!doit && blockedTriggers.Any(x => x == profileTriggerEntry.Id))
                    {
                        blockedTriggers.Remove(profileTriggerEntry.Id);
                    }
                }
            }
        }
        public class ProfileTriggerEntry : BaseViewModel
        {
            private void Dirty()
            {
                ServiceManager.Instance.ProfileTriggerManager.IsDirty = true;

            }

            private bool expanded = false;

            [JsonIgnore]
            public bool Expanded
            {
                get => expanded;
                set
                {

                    SetProperty(ref expanded, value);
                    Dirty();
                }
            }
            private Guid id;

            public Guid Id
            {
                get => id;
                set
                {

                    SetProperty(ref id, value);
                    Dirty();
                }
            }

            private string name;
            public string Name
            {
                get => name;
                set
                {

                    SetProperty(ref name, value);
                    Dirty();
                }
            }

            private string profileName;
            public string ProfileName
            {
                get => profileName;
                set
                {
                    SetProperty(ref profileName, value);

                    Dirty();
                }
            }

            private Guid profileId;

            public Guid ProfileId
            {
                get => profileId;
                set
                {
                    SetProperty(ref profileId, value);

                    Dirty();
                }
            }

            private string triggerType;

            public string TriggerType
            {
                get => triggerType;
                set
                {

                    SetProperty(ref triggerType, value);

                    Dirty();
                }
            }

            //RunningProccess

            private string processName;

            public string ProcessName
            {
                get => processName;
                set
                {
                    SetProperty(ref processName, value);

                    Dirty();
                }
            }

            private bool triggerWhenNotFound;

            public bool TriggerWhenNotFound
            {
                get => triggerWhenNotFound;
                set
                {
                    SetProperty(ref triggerWhenNotFound, value);

                    Dirty();
                }
            }

            //TimeBased

            private int hour = 0;

            public int Hour
            {
                get => hour;
                set
                {
                    SetProperty(ref hour, value);

                    Dirty();
                }
            }

            private int minute = 0;

            public int Minute
            {
                get => minute;
                set
                {
                    SetProperty(ref minute, value);

                    Dirty();
                }
            }
        }

        public static class ProfileTriggerTypes
        {
            public const string RunningProccess = "Running Process";
            public const string TimeBased = "Time Based";
            public const string DiscordTrigger = "Discord Trigger";
            public const string APITrigger = "API Trigger";

        }

        public void APIValueSet(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}
