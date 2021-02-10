using System.Web.Http;

namespace RGBSyncStudio.API
{
    public class TriggerController : ApiController
    {
        [Route("{key/{value}")]
        public void SetValue(string key, string value)
        {
            ServiceManager.Instance.ProfileTriggerManager.APIValueSet(key, value);
        }
    }
}
