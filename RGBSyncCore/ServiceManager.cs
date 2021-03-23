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

        public static SLSManager SLSManager;

        public static IStore Store;// = new Store();
        public static IDevices Devices;// = new Devices();
        public static IProfiles Profiles;// = new Profiles();
        public static IColorPallets ColorPallets;// = new ColorPallets();
        public static LedService LedService;// = new LedService();

        public static Action<string> LoadingMessage;
        public static Action<float> LoadingAmount;
        public static Action<float> LoadingMax;

        static ServiceManager()
        {
            SLSManager = new SLSManager(SLSCONFIGS_DIRECTORY);

            Store = new Store();
            Devices = new Devices();
            Profiles = new Profiles();
            ColorPallets = new ColorPallets();
            LedService = new LedService();
        }

        private static List<SerializableEvent> events = new List<SerializableEvent>();
        public static void PushEvent(SerializableEvent @event)
        {
            events.Add(@event);
        }
        public static void PushEvent(string name, object eventArgs)
        {
            events.Add(new SerializableEvent(name, eventArgs));
        }
        public static List<SerializableEvent> PullEvents()
        {
            List<SerializableEvent> result = events.ToList();
            events.Clear();
            return result;
        }
    }
}
