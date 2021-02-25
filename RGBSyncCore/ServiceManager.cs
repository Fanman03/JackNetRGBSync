using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;
using SyncStudio.Core.Services.ColorPallets;
using SyncStudio.Core.Services.Device;
using SyncStudio.Core.Services.Led;
using SyncStudio.Core.Services.Profiles;
using SyncStudio.Core.Services.Store;
using SyncStudio.Domain;

namespace SyncStudio.Core
{
    public static class ServiceManager
    {
        public const string SLSPROVIDER_DIRECTORY = "Providers";
        public const string PROFILES_DIRECTORY = "Profiles";
        public const string SLSCONFIGS_DIRECTORY = "Configs";

        public static SLSManager SLSManager = new SLSManager(SLSCONFIGS_DIRECTORY);

        public static IStore Store = new Store();
        public static IDevices Devices = new Devices();
        public static IProfiles Profiles = new Profiles();
        public static IColorPallets ColorPallets = new ColorPallets();
        public static LedService LedService = new LedService();

        public static Action<string> LoadingMessage;
        public static Action<float> LoadingAmount;
        public static Action<float> LoadingMax;

    }
}
