using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SyncStudio.Domain;

namespace SyncStudio.Service.API
{
    [RoutePrefix("api/Events")]
    public class EventsController : ApiController
    {
        [HttpGet]
        [Route("GetEvents")]
        public List<SerializableEvent> GetEvents()
        {
            var result= Core.ServiceManager.PullEvents();

            return result;
        }
    }


}
