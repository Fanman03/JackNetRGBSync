using SimpleLed;
using System.Collections.Generic;
using System.Drawing;
using System.Web.Http;

namespace RGBSyncPlus.API
{
    public class DevicesController : ApiController
    {
        public class ApiDeviceModel
        {
            public string ProviderName { get; set; }
            public string DeviceType { get; set; }
            public string DeviceName { get; set; }
            public string ConnectedTo { get; set; }
            public int LEDCount { get; set; }
            public Bitmap ProductImage { get; set; }
            public int GridWidth { get; set; }
            public int GridHeight { get; set; }
            public bool Has2DSupport { get; set; } }

        public List<ApiDeviceModel> GetDevices()
        {
            List<ApiDeviceModel> result = new List<ApiDeviceModel>();
            foreach (ControlDevice instanceSlsDevice in ApplicationManager.Instance.SLSDevices)
            {
                result.Add(new ApiDeviceModel
                {
                    DeviceType = instanceSlsDevice.DeviceType,
                    DeviceName = instanceSlsDevice.Name,
                    ConnectedTo = instanceSlsDevice.ConnectedTo,
                    ProviderName = instanceSlsDevice.Driver.Name(),
                    LEDCount = instanceSlsDevice.LEDs.Length,
                    //ProductImage = instanceSlsDevice.ProductImage,
                    GridWidth = instanceSlsDevice.GridWidth,
                    GridHeight = instanceSlsDevice.GridHeight,
                    Has2DSupport = instanceSlsDevice.Has2DSupport
                });
            }

            return result;
        }
    }
}
