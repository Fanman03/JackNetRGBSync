using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncStudio.Core.Services.Profiles;
using SyncStudio.Domain;

namespace SyncStudio.ClientService
{
    public class Profiles
    {
        private Settings settings = new Settings();
        private readonly EasyRest easyRest = new EasyRest("http://localhost:59023/api/Profiles/");
        public Task<Profile> GetProfile(Guid id)
        {
            return easyRest.Get<Profile>($"GetProfile/{id}");
        }

        public Profile GetProfileSync(Guid id)
        {
            return easyRest.GetSync<Profile>($"GetProfile/{id}");
        }

        public Task<List<Profile>> GetAvailableProfiles()
        {
            return easyRest.Get<List<Profile>>("GetProfiles");
        }

        public List<Profile> GetAvailableProfilesSync()
        {
            return easyRest.GetSync<List<Profile>>("GetProfiles");
        }

        public void SaveProfile(Profile profile)
        {
            easyRest.Post<object, Profile>("SaveProfile", profile);
        }

        public Task<Profile> GenerateNewProfile(string name)
        {
            return easyRest.Get<Profile>($"GenerateNewProfile/{name}");
        }

        public Profile GenerateNewProfileSync(string name)
        {
            return easyRest.GetSync<Profile>($"GenerateNewProfile/{name}");
        }

        public void RemoveProfile(string profileName)
        {
            easyRest.Get<Profile>($"RemoveProfile/{profileName}");
        }

        public Task<Profile> GetCurrentProfile()
        {
            return GetProfile(settings.CurrentProfile);
        }

        public Profile GetCurrentProfileSync()
        {
            var profile = GetProfileSync(settings.CurrentProfile);
            if (profile == null)
            {
                try
                {
                    profile = GenerateNewProfileSync("default");
                    settings.CurrentProfile = profile.Id;
                }
                catch
                {
                    var profiles = GetAvailableProfilesSync();
                    if (profiles.Any(x=>x.Name.ToLower()=="default"))
                    {
                        profile = profiles.First(x => x.Name.ToLower() == "default");
                    }
                    else
                    {
                        profile = profiles.FirstOrDefault();
                    }
                }
            }

            return profile;
        }

        public void SetCurrentProfile(Profile profile)
        {
            settings.CurrentProfile = profile.Id;
        }
    }
}
