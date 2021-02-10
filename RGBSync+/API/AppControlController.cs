using System.Web.Http;

namespace RGBSyncPlus.API
{
    public class AppControlController : ApiController
    {
        public void PauseSyncing(bool pause)
        {
            ServiceManager.Instance.LedService.PauseSyncing = pause;
        }
    }
}
