using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncStudio.Domain;

namespace SyncStudio.Core.Services.Profiles
{
    public interface IProfiles
    {
        Profile GetCurrentProfile();
        void SetCurrentProfile(Profile profile);

        Profile GetProfile(string fileName);

        List<Profile> GetAvailableProfiles();

        void SaveProfile(Profile profile);

        Profile GenerateNewProfile(string name);
        void RemoveProfile(string profileName);
    }
}
