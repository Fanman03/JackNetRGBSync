using System.Web.Http;

namespace RGBSyncStudio.API
{
    public class AppControlController : ApiController
    {
        public void PauseSyncing(bool pause)
        {
            ServiceManager.Instance.LedService.PauseSyncing = pause;
        }
    }
}
