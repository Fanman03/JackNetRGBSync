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
