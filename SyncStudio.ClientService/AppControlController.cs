
using System.Web.Http;

namespace SyncStudio.ClientService
{
    public class AppControlController : ApiController
    {
        public void PauseSyncing(bool pause)
        {
            SyncStudio.Core.ServiceManager.LedService.PauseSyncing = pause;
        }
    }
}
