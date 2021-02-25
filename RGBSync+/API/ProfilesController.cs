using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SyncStudio.WPF.API
{
    public class ProfilesController : ApiController
    {
        public List<string> GetProfiles()
        {
            return ServiceManager.Instance.ConfigService.Settings.ProfileNames.ToList();
        }

        public void SetProfile(string profileName)
        {
            ServiceManager.Instance.ProfileService.LoadProfileFromName(profileName);
        }
    }
}
