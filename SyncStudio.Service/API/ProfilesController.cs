using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SyncStudio.Domain;

namespace SyncStudio.Service.API
{
    [RoutePrefix("api/Profiles")]
    public class ProfilesController : ApiController
    {
        [HttpGet]
        [Route("GetProfile/{id}")]
        public async Task<Profile> GetProfile(Guid id)
        {
            return Core.ServiceManager.Profiles.GetAvailableProfiles().FirstOrDefault(x=>x.Id == id);
        }

        [HttpGet]
        [Route("GetProfiles")]
        public List<Profile> GetProfiles()
        {
            return Core.ServiceManager.Profiles.GetAvailableProfiles();
        }

        [HttpPost]
        [Route("SaveProfile")]
        public void SaveProfile(Profile profile)
        {
            Core.ServiceManager.Profiles.SaveProfile(profile);
        }

        [HttpGet]
        [Route("GenerateNewProfile/{name}")]
        public Profile GenerateNewProfile(string name)
        {
            return Core.ServiceManager.Profiles.GenerateNewProfile(name);
        }

        [HttpGet]
        [Route("RemoveProfile/{profileName}")]
        public void RemoveProfile(string profileName)
        {
            Core.ServiceManager.Profiles.RemoveProfile(profileName);
        }
    }
}
