using System.Web.Http;

namespace SyncStudio.WPF.API
{
    public class AppControlController : ApiController
    {
        public void PauseSyncing(bool pause)
        {
            SyncStudio.Core.ServiceManager.LedService.PauseSyncing = pause;
        }
    }
}
