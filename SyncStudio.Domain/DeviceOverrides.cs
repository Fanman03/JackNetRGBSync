using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class DeviceOverrides : BaseViewModel
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
                SetProperty(ref customDeviceSpecification, value);
                PushUpdate();
            }
        }
        public string TitleOverride
        {
            get => titleOverride;
            set
            {
                SetProperty(ref titleOverride, value);
                PushUpdate();
            }
        }
        
        public string UID { get; set; }

        public string SubTitleOverride
        {
            get => subTitleOverride;
            set
            {
                SetProperty(ref subTitleOverride, value);
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
                SetProperty(ref name, value);
                // PushUpdate();
            }
        }

        private string connectedTo;

        public string ConnectedTo
        {
            get => connectedTo;
            set
            {
                SetProperty(ref connectedTo, value);
                // PushUpdate();
            }
        }

        private string providerName;
        public string ProviderName
        {
            get => providerName;
            set
            {
                SetProperty(ref providerName, value);
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
