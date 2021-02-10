using System.Web.Http;

namespace RGBSyncPlus.API
{
    public class TriggerController : ApiController
    {
        [RouteAttribute("{key/{value}")]
        public void SetValue(string key, string value)
        {
            ServiceManager.Instance.ProfileTriggerManager.APIValueSet(key, value);
        }
    }
}
