using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace RGBSyncPlus.API
{
    public class AppControlController : ApiController
    {
        public void PauseSyncing(bool pause)
        {
            ApplicationManager.Instance.PauseSyncing = pause;
        }
    }
}
