using System.Web.Http;

namespace SyncStudio.WPF.API
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
