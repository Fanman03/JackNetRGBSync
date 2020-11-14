using SimpleLed;
using System.Collections.Generic;
using System.Web.Http;

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
