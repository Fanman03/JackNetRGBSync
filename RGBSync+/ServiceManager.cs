using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.Services;
using SimpleLed;

namespace RGBSyncPlus
{
    public class ServiceManager
    {
        public static ServiceManager Instance;

        public ApplicationManager ApplicationManager;
        public SLSManager SLSManager;
        public LedService LedService;
        public ApiServerService ApiServerService;
        public SimpleLogger Logger;
        public ConfigService ConfigService;
        public ProfileService ProfileService;
        public DiscordService DiscordService;
        public SLSAuthService SLSAuthService;
        public ColorProfileService ColorProfileService;
        public static void Initialize(string slsConfigsDirectory, string ngProfileDir)
        {
            Instance = new ServiceManager();
            Instance.ApplicationManager = new ApplicationManager();
            Instance.LedService = new LedService();
            Instance.SLSManager = new SLSManager(slsConfigsDirectory);
            Instance.ApiServerService = new ApiServerService();
            Instance.Logger = new SimpleLogger();
            Instance.ConfigService = new ConfigService(ngProfileDir, slsConfigsDirectory);
            Instance.ProfileService = new ProfileService(ngProfileDir);
            Instance.DiscordService = new DiscordService();
            Instance.SLSAuthService = new SLSAuthService();
            Instance.ColorProfileService = new ColorProfileService();

        }
    }
}
