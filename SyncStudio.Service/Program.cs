using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace SyncStudio.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SyncStudio Windows Service");

            Console.WriteLine("Creating API server");
            var server = new Server();
            Console.WriteLine("Starting API Server");
            server.Start();
            Console.WriteLine("Loading Providers");

            SyncStudio.Core.ServiceManager.LedService.LoadSLSProviders();

            Console.WriteLine("Server started and ready.");

            Console.ReadLine();
        }
        public class Server
        {
            public HttpSelfHostServer server;
            public void Start()
            {
                server?.CloseAsync().Wait();
                //setup API
                //todo make this be able to be toggled:
                Debug.WriteLine("Setting up API");
                HttpSelfHostConfiguration apiconfig = new HttpSelfHostConfiguration("http://localhost:59023");

                apiconfig.MapHttpAttributeRoutes();
                
                server = new HttpSelfHostServer(apiconfig);
                apiconfig.EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "RGBSync API");
                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                }).EnableSwaggerUi();
                //server.OpenAsync();

                Task.Run(() => server.OpenAsync());
                Console.WriteLine("API Running");
                Console.WriteLine(apiconfig.BaseAddress);
            }

            public void Stop()
            {
                server?.CloseAsync().Wait();
            }

        }
    }
}