using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Swashbuckle.Application;

namespace RGBSyncStudio.Services
{
    public class ApiServerService
    {
        public HttpSelfHostServer server;
        public void Start()
        {
            server?.CloseAsync().Wait();
            //setup API
            //todo make this be able to be toggled:
            Debug.WriteLine("Setting up API");
            HttpSelfHostConfiguration apiconfig = new HttpSelfHostConfiguration("http://localhost:59022");


            apiconfig.Routes.MapHttpRoute("API Default", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            server = new HttpSelfHostServer(apiconfig);
            apiconfig.EnableSwagger(c => c.SingleApiVersion("v1", "RGBSync API")).EnableSwaggerUi();
            //server.OpenAsync();

            Task.Run(() => server.OpenAsync());
            Debug.WriteLine("API Running");
        }

        public void Stop()
        {
            server?.CloseAsync().Wait();
        }


    }
}
