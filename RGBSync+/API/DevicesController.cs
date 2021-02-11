using System;
using SimpleLed;
using System.Collections.Generic;
using System.Drawing;
using System.Web.Http;

namespace RGBSyncStudio.API
{
    public class DevicesController : ApiController
    {
        public class ApiDeviceModel
        {
            public Bitmap ProductImage { get; set; }
            public bool Reverse { get; set; }
            public int LedShift { get; set; }
            public ControlDevice.LedUnit[] LEDs { get; set; }
            public ISimpleLed Driver { get; set; }
            public string DeviceType { get; set; }
            public string ConnectedTo { get; set; }
            public string TitleOverride { get; set; }
            public OverrideSupport OverrideSupport { get; set; }
            public string Name { get; set; }
            public int GridWidth { get; set; }
            public bool Has2DSupport { get; set; }
            public int GridHeight { get; set; }
            public CustomDeviceSpecification CustomDeviceSpecification { get; set; }
            public bool In2DMode { get; set; }
            public int LedCount { get; set; }
        }

        public List<ApiDeviceModel> GetDevices()
        {
            List<ApiDeviceModel> result = new List<ApiDeviceModel>();
            foreach (ControlDevice instanceSlsDevice in ServiceManager.Instance.LedService.SLSDevices)
            {
                result.Add(new ApiDeviceModel
                {
                    DeviceType = instanceSlsDevice.DeviceType,
                    Name = instanceSlsDevice.Name,
                    ConnectedTo = instanceSlsDevice.ConnectedTo,
                    Driver = instanceSlsDevice.Driver,
                    LedCount = instanceSlsDevice.LEDs.Length,
                    //ProductImage = instanceSlsDevice.ProductImage,
                    GridWidth = instanceSlsDevice.GridWidth,
                    GridHeight = instanceSlsDevice.GridHeight,
                    Has2DSupport = instanceSlsDevice.Has2DSupport,
                    OverrideSupport = instanceSlsDevice.OverrideSupport,
                    Reverse = instanceSlsDevice.Reverse,
                    LedShift = instanceSlsDevice.LedShift,
                    TitleOverride = instanceSlsDevice.TitleOverride,
                    CustomDeviceSpecification = instanceSlsDevice.CustomDeviceSpecification,
                    In2DMode = instanceSlsDevice.In2DMode,
                    LEDs = instanceSlsDevice.LEDs
                });
            }

            return result;
        }
    }
}
