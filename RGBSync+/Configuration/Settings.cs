using RGBSyncPlus.Model;
using System.Collections.Generic;

namespace RGBSyncPlus.Configuration
{
    public class Settings
    {
        #region Constants

        public const int CURRENT_VERSION = 1;

        #endregion

        #region Properties & Fields

        public int Version { get; set; } = 1;

        public string Name { get; set; } = "Default";

        
        private List<DeviceMappingModels.DeviceMapping> deviceMappingProxy;

        public List<DeviceMappingModels.DeviceMapping> DeviceMappingProxy
        {
            get => deviceMappingProxy;
            set { deviceMappingProxy = value; }
        }

        #endregion
    }
}
