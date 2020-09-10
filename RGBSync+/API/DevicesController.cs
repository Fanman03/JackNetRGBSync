using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SimpleLed;

namespace RGBSyncPlus.API
{
    public class DevicesController : ApiController
    {
        public class ApiDeviceModel
        {
            public string ProviderName { get; set; }
            public string DeviceName { get; set; }
            public int LEDCount { get; set; }
        }

        public List<ApiDeviceModel> GetDevices()
        {
            List<ApiDeviceModel> result = new List<ApiDeviceModel>();
            foreach (ControlDevice instanceSlsDevice in ApplicationManager.Instance.SLSDevices)
            {
                result.Add(new ApiDeviceModel
                {
                    DeviceName = instanceSlsDevice.DeviceType,
                    ProviderName = instanceSlsDevice.Driver.Name(),
                    LEDCount = instanceSlsDevice.LEDs.Length
                });
            }

            return result;
        }
    }
}
