using SimpleLed;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web.Http;
using MarkdownUI.WPF;
using SyncStudio.Core.Models;
using SyncStudio.Domain;

namespace SyncStudio.Service.API
{
    [RoutePrefix("api/Devices")]
    public class DevicesController : ApiController
    {
        [HttpGet]
        [Route("GetDevices")]
        public IEnumerable<InterfaceControlDevice> GetDevices()
        {
            return SyncStudio.Core.ServiceManager.Devices.GetDevices().Select(ToICD);
        }

        [HttpPost]
        [Route("AddDevice")]
        public void AddDevice(InterfaceControlDevice device)
        {
            //Core.ServiceManager.Devices.AddDevice(ToICD(device));
        }

        [HttpPost]
        [Route("RemoveDevice")]
        public void RemoveDevice(InterfaceControlDevice device)
        {
            //Core.ServiceManager.Devices.RemoveDevice(device);
        }



        [HttpGet]
        [Route("GetUIBundle/{driverName}")]
        public MarkdownUIBundle GetUI(string driverName)
        {
            return Core.ServiceManager.Store.GetUIBundle(driverName);
        }


        [HttpGet]
        [Route("GetOverrideTemplates/{uniqueIdentifier}")]
        public List<SLSManager.CDSBundle> GetOverrideTemplates(string uniqueIdentifier)
        {
            var devices = Core.ServiceManager.Devices.GetDevices();
            var match = devices.FirstOrDefault(x => x.UniqueIdentifier == uniqueIdentifier);
            return Core.ServiceManager.SLSManager.GetCDSBundles(match);
        }

        [HttpGet]
        [Route("GetOverride/{uniqueIdentifier}")]
        public DeviceOverrides GetOverride(string uniqueIdentifier)
        {
            var devices = Core.ServiceManager.Devices.GetDevices();
            var match = devices.FirstOrDefault(x => x.UniqueIdentifier == uniqueIdentifier);
            if (match == null)
            {
                return null;
            }
            return Core.ServiceManager.Devices.GetOverride(match);
            //return null;
        }

        [HttpPost]
        [Route("SetOverride")]
        public void SetOverride(InterfaceControlDevice device, DeviceOverrides overRide)
        {
            //  Core.ServiceManager.Devices.SetOverride(device, overRide);
        }

        [HttpGet]
        [Route("GetControlDeviceFromName/{providerName}/{name}")]
        public InterfaceControlDevice GetControlDeviceFromName(string providerName, string name)
        {
            return ToICD(Core.ServiceManager.Devices.GetControlDeviceFromName(providerName, name));
        }

        [HttpGet]
        [Route("SyncDevice/{fromUID}/{toUID}")]
        public void SyncDevice(string fromUID, string toUID)
        {
            Core.ServiceManager.Devices.SyncDevice(fromUID, toUID);
        }



        public InterfaceControlDevice ToICD(ControlDevice cd)
        {
            var thing = cd.Driver.GetProperties();
            InterfaceControlDevice result = new InterfaceControlDevice()
            {
                PngData = ImageToByte2(cd.ProductImage),
                HasUI = cd.Driver is ISimpleLedWithConfig,
                UniqueIdentifier = cd.UniqueIdentifier,
                Name = cd.Name,
                ChannelUniqueId = cd.ChannelUniqueId,
                TitleOverride = cd.TitleOverride,
                Reverse = cd.Reverse,
                OverrideSupport = cd.OverrideSupport,
                ControlChannel = new ControlChannel
                {
                    Name = cd.ControlChannel?.Name,
                    Serial = cd.ControlChannel?.Serial
                },
                DeviceType = cd.DeviceType,
                CustomDeviceSpecification = cd.CustomDeviceSpecification,
                LedShift = cd.LedShift,
                InterfaceDriverProperties = cd.DriverProperties == null ?
                null :
                new InterfaceDriverProperties
                {
                    Author = cd.DriverProperties.Author,
                    AuthorId = cd.DriverProperties.AuthorId,
                    Blurb = cd.DriverProperties.Blurb,
                    CurrentVersion = cd.DriverProperties.CurrentVersion,
                    GitHubLink = cd.DriverProperties.GitHubLink,
                    HomePage = cd.DriverProperties.HomePage,
                    InstanceId = cd.DriverProperties.InstanceId,
                    IsPublicRelease = cd.DriverProperties.IsPublicRelease,
                    IsSource = cd.DriverProperties.IsSource,
                    Name = cd.DriverName,
                    Price = cd.DriverProperties.Price,
                    ProductCategory = cd.DriverProperties.ProductCategory,
                    ProductId = cd.DriverProperties.ProductId,
                    SupportsCustomConfig = cd.DriverProperties.SupportsCustomConfig,
                    SupportsPull = cd.DriverProperties.SupportsPull,
                    SupportsPush = cd.DriverProperties.SupportsPush
                }
            };

            return result;
        }

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
