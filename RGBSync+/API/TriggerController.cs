using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace RGBSyncPlus.API
{
    public class TriggerController : ApiController
    {
        [RouteAttribute("{key/{value}")]
        public void SetValue(string key, string value)
        {
            ApplicationManager.Instance.ProfileTriggerManager.APIValueSet(key, value);
        }
    }
}
