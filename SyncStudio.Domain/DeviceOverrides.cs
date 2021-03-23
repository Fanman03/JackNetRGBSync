using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceOverrides
    {
        private string titleOverride;
        private string subTitleOverride;
        private string channelOverride;
        private CustomDeviceSpecification customDeviceSpecification;

        public CustomDeviceSpecification CustomDeviceSpecification
        {
            get => customDeviceSpecification;
            set
            {
                customDeviceSpecification = value;
                PushUpdate();
            }
        }
        public string TitleOverride
        {
            get => titleOverride;
            set
            {
                titleOverride = value;
                PushUpdate();
            }
        }
        
        public string UID { get; set; }

        public string SubTitleOverride
        {
            get => subTitleOverride;
            set
            {
                subTitleOverride = value;
                PushUpdate();
            }
        }

        //public string ChannelOverride
        //{
        //    get => channelOverride;
        //    set
        //    {
        //        SetProperty(ref channelOverride, value);
        //        PushUpdate();
        //    }
        //}

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                // PushUpdate();
            }
        }

        private string connectedTo;

        public string ConnectedTo
        {
            get => connectedTo;
            set
            {
                connectedTo = value;
                // PushUpdate();
            }
        }

        private string providerName;
        public string ProviderName
        {
            get => providerName;
            set
            {
                providerName = value;
                // PushUpdate();
            }
        }

        private void PushUpdate()
        {
            //todo
            //SyncStudio.Core.ServiceManager.LedService.SetOverride(this);
        }
    }
}
