using Newtonsoft.Json;
using SyncStudio.WPF.Model;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using SyncStudio.Domain;
using SyncStudio.WPF.Helper;

namespace SyncStudio.WPF.Services
{
    public class ProfileService
    {
        public event EventHandler OnProfilesChanged;
        public void OnProfilesChangedInvoke(object sender, EventArgs e)
        {
            OnProfilesChanged?.Invoke(sender, e);
        }
        public readonly Dictionary<string, string> profilePathMapping = new Dictionary<string, string>();
        private readonly SimpleLogger Logger = ServiceManager.Instance.Logger;
        
        public ProfileService(string profilesDir)
        {
            
        }

        private Profile cp => new Profile
        {
            Name = "Default",
            Id = Guid.NewGuid()
        };

        //todo
        private Profile CurrentProfile => cp;// SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile();

        public bool ProfilesRequiresSave()
        {
            if (CurrentProfile == null) return false;
            return (CurrentProfile.IsProfileStale);
        }

        public void SaveCurrentProfile()
        {
            SyncStudio.Core.ServiceManager.Profiles.SaveProfile(CurrentProfile);
        }

        public void GenerateNewProfile(string name, bool hotLoad = true)
        {
            var newProfile = SyncStudio.Core.ServiceManager.Profiles.GenerateNewProfile(name);
            //todo updateConfig

        }

        public void LoadProfileFromName(string profileName)
        {
            //todo
            return;

            var profile = Core.ServiceManager.Profiles.GetProfile(profileName);
            Core.ServiceManager.Profiles.SetCurrentProfile(profile);
        }

        public Profile GetProfileFromName(string profileName)
        {
            return Core.ServiceManager.Profiles.GetProfile(profileName);
        }

        public Profile GetProfileFromPath(string path)
        {
            return TryReallyHard.ToRun(thisCode: () =>
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Profile>(json);
            });
        }
        
        public void DeleteProfile(string dcName)
        {
            Core.ServiceManager.Profiles.RemoveProfile(dcName);

            string path = profilePathMapping[dcName];
            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.Delete(path);
            ServiceManager.Instance.ConfigService.HotLoadSettings();
        }

        public void RenameProfile(string currentProfileOriginalName, string currentProfileName)
        {
            var profile = Core.ServiceManager.Profiles.GetProfile(currentProfileName);
            profile.Name = currentProfileName;
            Core.ServiceManager.Profiles.SaveProfile(profile);
        }


        public void SaveProfile(Profile profile)
        {
            string json = JsonConvert.SerializeObject(profile);
            string path = profilePathMapping[profile.Name];
            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

        public void SaveProfile(Profile profile, string path)
        {
            string json = JsonConvert.SerializeObject(profile);
            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

    }
}
