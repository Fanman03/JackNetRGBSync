using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace RGBSyncPlus.API
{
    public class ProfilesController : ApiController
    {
        public List<string> GetProfiles()
        {
            return ApplicationManager.Instance.NGSettings.ProfileNames.ToList();
        }

        public void SetProfile(string profileName)
        {
            ApplicationManager.Instance.LoadProfileFromName(profileName);
        }
    }
}
