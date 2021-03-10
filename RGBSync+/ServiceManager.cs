using SimpleLed;
using System.Windows;
using Autofac;
using Autofac.Core;
using Nan0SyncStudio.Branding;
using RGBSyncStudio.Branding;
using SyncStudio.Branding;

using SyncStudio.WPF.Services;

namespace SyncStudio.WPF
{
    public class ServiceManager
    {
        public static ServiceManager Instance;

        public ApplicationManager ApplicationManager;
        //  public SLSManager SLSManager;

        public ClientService.Profiles ProfilesService;

        public ApiServerService ApiServerService;
        public SimpleLogger Logger;
      //  public ConfigService ConfigService;
        
        public DiscordService DiscordService;
        public SLSAuthService SLSAuthService;
        public ModalService ModalService;
        public ProfileTriggerManager ProfileTriggerManager;
        public StoreService StoreService;
        public IBranding Branding;

        public static IContainer Container { get; set; }
        public static void Initialize(string slsConfigsDirectory, string ProfileDir)
        {

            var builder = new ContainerBuilder();
            builder.RegisterType<ClientService.Devices>().InstancePerLifetimeScope();
            builder.RegisterType<ClientService.Profiles>().InstancePerLifetimeScope();
            Container = builder.Build();
            ClientService.Devices devices;
            using (var scope = Container.BeginLifetimeScope())
            {
                devices = scope.Resolve<ClientService.Devices>();
            }

            Instance = new ServiceManager();
            Instance.ApplicationManager = new ApplicationManager();
            
            Instance.ApiServerService = new ApiServerService();
            Instance.Logger = new SimpleLogger();
          //  Instance.ConfigService = new ConfigService(ProfileDir, slsConfigsDirectory, devices);
            
            Instance.DiscordService = new DiscordService();
            Instance.SLSAuthService = new SLSAuthService();
            Instance.ModalService = new ModalService();
            Instance.ProfileTriggerManager = new ProfileTriggerManager();
            Instance.StoreService = new StoreService();

            Instance.Branding = new RGBSyncStudioBranding();
            
        }

        public static void Shutdown()
        {


            Instance.DiscordService.Stop();
            Application.Current.Shutdown();
        }
    }
}
