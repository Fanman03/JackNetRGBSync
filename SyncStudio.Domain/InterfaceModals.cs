using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class InterfaceEvents
    {
        public class InterfaceDeviceChangeEventArgs
        {
            public InterfaceControlDevice ControlDevice { get; private set; }

            public InterfaceDeviceChangeEventArgs(InterfaceControlDevice controlDevice)
            {
                ControlDevice = controlDevice;
            }
        }

        public delegate void InterfaceDeviceChangeEventHandler(object sender, InterfaceDeviceChangeEventArgs e);
    }

    public class InterfaceControlDevice
    {
        public bool HasUI { get; set; }
        public ControlChannel ControlChannel { get; set; }
        public string Name { get; set; }
        public CustomDeviceSpecification CustomDeviceSpecification { get; set; }
        public OverrideSupport OverrideSupport { get; set; } = OverrideSupport.None;
        public string TitleOverride { get; set; }
        public string DeviceType { get; set; }
        public InterfaceDriverProperties InterfaceDriverProperties { get; set; }

        public int LedShift { get; set; } = 0;
        public bool Reverse { get; set; } = false;

        public string ChannelUniqueId { get; set; }
        public string UniqueIdentifier { get; set; }



        private byte[] pngData;

        public byte[] PngData
        {
            get => pngData;
            set
            {
                pngData= value;
                if (value != null)
                {
                    try
                    {
                        Bitmap = new Bitmap(new MemoryStream(value));
                    }
                    catch
                    {
                    }
                }
            }
        }

        private Bitmap bitmap;

        [JsonIgnore]
        public Bitmap Bitmap
        {
            get => bitmap;
            set => bitmap = value;
        }
    }

    public class InterfaceDriverProperties
        {
            public string Name { get; set; }

            /// <summary>
            /// Can this device Pull LEDs from its Device/SDK?
            /// </summary>
            public bool SupportsPull { get; set; }
            /// <summary>
            /// Does this device support pushing to the Device/SDK
            /// </summary>
            public bool SupportsPush { get; set; }
            /// <summary>
            /// Is this device a "source" - does it generate its own LEDs?
            /// </summary>
            public bool IsSource { get; set; }
            /// <summary>
            /// Does this device support custom configs
            /// </summary>
            public bool SupportsCustomConfig { get; set; }
            /// <summary>
            /// Driver specific UUID
            /// </summary>
            public Guid InstanceId { get; set; }

            /// <summary>
            /// Name of the creator of this driver.
            /// </summary>
            public string Author { get; set; }

            /// <summary>
            /// Current Version Number
            /// </summary>
            public ReleaseNumber CurrentVersion { get; set; }
            /// <summary>
            /// Text about this driver
            /// </summary>
            public string Blurb { get; set; }
            /// <summary>
            /// Link to public GitHub (or alternative) project page.
            /// </summary>
            public string GitHubLink { get; set; }
            /// <summary>
            /// Link to driver's homepage
            /// </summary>
            public string HomePage { get; set; }
            /// <summary>
            /// Is this a publicly released driver ( considered beta otherwise )
            /// </summary>
            public bool IsPublicRelease { get; set; }

            public List<USBDevice> SupportedDevices { get; set; }

            public ProductCategory ProductCategory { get; set; }
            public Guid ProductId { get; set; }
            public Guid AuthorId { get; set; }
            public Decimal Price { get; set; }

        }
    }
