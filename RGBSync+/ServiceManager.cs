using SimpleLed;
using System.Windows;
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
        public SLSManager SLSManager;
        
        public ApiServerService ApiServerService;
        public SimpleLogger Logger;
        public ConfigService ConfigService;
        public ProfileService ProfileService;
        public DiscordService DiscordService;
        public SLSAuthService SLSAuthService;
        public ModalService ModalService;
        public ProfileTriggerManager ProfileTriggerManager;
        public StoreService StoreService;
        public IBranding Branding;
        public static void Initialize(string slsConfigsDirectory, string ProfileDir)
        {
            Instance = new ServiceManager();
            Instance.ApplicationManager = new ApplicationManager();
        
            Instance.SLSManager = SyncStudio.Core.ServiceManager.SLSManager;
            Instance.ApiServerService = new ApiServerService();
            Instance.Logger = new SimpleLogger();
            Instance.ConfigService = new ConfigService(ProfileDir, slsConfigsDirectory);
            Instance.ProfileService = new ProfileService(ProfileDir);
            Instance.DiscordService = new DiscordService();
            Instance.SLSAuthService = new SLSAuthService();
            Instance.ModalService = new ModalService();
            Instance.ProfileTriggerManager = new ProfileTriggerManager();
            Instance.StoreService = new StoreService();

            Instance.Branding = new RGBSyncStudioBranding();

            Core.ServiceManager.SLSManager.AppName = Instance.Branding.GetAppName();
            Core.ServiceManager.SLSManager.AppAuthor = Instance.Branding.GetAppAuthor();
        }

        public static void Shutdown()
        {
            
            Core.ServiceManager.LedService.PauseSyncing = true;
            Core.ServiceManager.Store.UnloadSLSProviders();

            Instance.DiscordService.Stop();
            Application.Current.Shutdown();
        }
    }
}
