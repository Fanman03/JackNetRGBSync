using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncStudio.Core.Models;

namespace SyncStudio.Core.Services.Store
{
    public interface IStore
    {
        Task<IEnumerable<ProviderInfo>> GetStoreProviders();
        IEnumerable<ProviderInfo> GetInstalledProviders();
        Task<bool> InstallProvider(Guid providerId, ReleaseNumber version, Action<string> setInstallingMessage, Action<int> setInstallingPercentage);
        bool RemoveProvider(Guid providerId, Action<string> setInstallingMessage, Action<int> setInstallingPercentage);
        void LoadPluginFolder(string folder);
        void UnloadSLSProviders();
    }
}
