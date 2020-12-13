using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RGBSyncPlus.Helper;
using SimpleLed;
using Color = System.Drawing.Color;
using LinearGradientBrush = System.Windows.Media.LinearGradientBrush;

namespace RGBSyncPlus
{
    public class GradientDriver : ISimpleLed
    {
        List<ControlDevice> devices = new List<ControlDevice>();
        public void Dispose()
        {

        }

        public void Configure(DriverDetails driverDetails)
        {
            for (int i = 0; i < 4; i++)
            {
                var dd = (new ControlDevice
                {
                    Driver = this,
                    Name = "Gradient",
                    ConnectedTo = "Bank " + (i + 1),
                    DeviceType = DeviceTypes.Effect,
                    LEDs = new ControlDevice.LedUnit[16]
                });

                for (int p = 0; p < 16; p++)
                {
                    dd.LEDs[p] = new ControlDevice.LedUnit
                    {
                        Color = new LEDColor(0, 0, 0),
                        Data = new ControlDevice.LEDData { LEDNumber = p },
                        LEDName = "LED " + (p + 1)
                    };
                }

                Bitmap Bmp = new Bitmap(192, 128);
                using (Graphics gfx = Graphics.FromImage(Bmp))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, 0)))
                {
                    gfx.FillRectangle(brush, 0, 0, 192, 128);
                }
                dd.Name = "Solid Color";
                devices.Add(dd);

                DeviceAdded?.Invoke(this, new Events.DeviceChangeEventArgs(dd));
            }
        }

        private ColorProfile colorProfile;

        public void Push(ControlDevice controlDevice)
        {

        }

        public void Pull(ControlDevice controlDevice)
        {

        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPush = false,
                IsSource = true,
                SupportsPull = true,
                Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                Author = "RGB Sync Studio Team",
                Blurb = "Solid Colors provided by Color Profile",
                CurrentVersion = new ReleaseNumber(1, 0, 0, 0),
                IsPublicRelease = true,
                SetColorProfileAction = SetColorProfile
            };
        }

        public T GetConfig<T>() where T : SLSConfigData
        {
            return null;
        }

        public void PutConfig<T>(T config) where T : SLSConfigData
        {

        }

        public string Name()
        {
            return "Gradient";
        }

        public void InterestedUSBChange(int VID, int PID, bool connected)
        {

        }

        public void SetColorProfile(ColorProfile value)
        {
            colorProfile = value;
            for (int i = 0; i < 4; i++)
            {
                ControlDevice dd = devices[i];
                var cc = colorProfile.ColorBanks[i];
                var pc = cc.Colors[0];
                var dc = cc.Colors[1];
                Bitmap Bmp = new Bitmap(192, 128);

                var cpc = System.Drawing.Color.FromArgb(255, (byte)pc.Color.Red, (byte)pc.Color.Green, (byte)pc.Color.Blue);
                var cdc = System.Drawing.Color.FromArgb(255, (byte)dc.Color.Red, (byte)dc.Color.Green, (byte)dc.Color.Blue);

                using (Graphics graphics = Graphics.FromImage(Bmp))
                {
                    using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(192, 128), cpc, cdc))
                    {
                        graphics.FillRectangle(brush, 0f, 0f, 192f, 128f);
                    }
                }


                dd.LEDs = cc.Colors.Select(x => new ControlDevice.LedUnit
                {
                    Color = new LEDColor(x.Color.Red, x.Color.Green, x.Color.Blue)
                }).ToArray();

                float steps = 1 / 16f;
                
                dd.Name = value.ColorBanks[i].BankName;
                dd.ProductImage = Bmp;
            }
        }

        public event Events.DeviceChangeEventHandler DeviceAdded;
        public event Events.DeviceChangeEventHandler DeviceRemoved;
    }

    public class SolidColorDriver : ISimpleLed
    {
        List<ControlDevice> devices = new List<ControlDevice>();
        public void Dispose()
        {

        }

        public void Configure(DriverDetails driverDetails)
        {
            for (int i = 0; i < 4; i++)
            {
                var dd = (new ControlDevice
                {
                    Driver = this,
                    Name = "Solid Color",
                    ConnectedTo = "Bank " + (i + 1),
                    DeviceType = DeviceTypes.Effect,
                    LEDs = new ControlDevice.LedUnit[1]
                    {
                        new ControlDevice.LedUnit
                        {
                            Color = new LEDColor(0, 0, 0),
                            Data = new ControlDevice.LEDData {LEDNumber = 0},
                            LEDName = "LED 1"
                        }
                    },

                });

                Bitmap Bmp = new Bitmap(192, 128);
                using (Graphics gfx = Graphics.FromImage(Bmp))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, 0)))
                {
                    gfx.FillRectangle(brush, 0, 0, 192, 128);
                }
                dd.Name = "Solid Color";
                devices.Add(dd);

                DeviceAdded?.Invoke(this, new Events.DeviceChangeEventArgs(dd));
            }
        }

        private ColorProfile colorProfile;

        public void Push(ControlDevice controlDevice)
        {

        }

        public void Pull(ControlDevice controlDevice)
        {

        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPush = false,
                IsSource = true,
                SupportsPull = true,
                Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                Author = "RGB Sync Studio Team",
                Blurb = "Solid Colors provided by Color Profile",
                CurrentVersion = new ReleaseNumber(1, 0, 0, 0),
                IsPublicRelease = true,
                SetColorProfileAction = SetColorProfile
            };
        }

        public T GetConfig<T>() where T : SLSConfigData
        {
            return null;
        }

        public void PutConfig<T>(T config) where T : SLSConfigData
        {

        }

        public string Name()
        {
            return "Solid Color";
        }

        public void InterestedUSBChange(int VID, int PID, bool connected)
        {

        }

        public void SetColorProfile(ColorProfile value)
        {
            colorProfile = value;
            for (int i = 0; i < 4; i++)
            {
                ControlDevice dd = devices[i];
                var cc = colorProfile.ColorBanks[i];
                var pc = cc.Colors[0];
                Bitmap Bmp = new Bitmap(192, 128);
                using (Graphics gfx = Graphics.FromImage(Bmp))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(pc.Color.Red, pc.Color.Green, pc.Color.Blue)))
                {
                    gfx.FillRectangle(brush, 0, 0, 192, 128);
                }

                dd.LEDs[0].Color.Red = pc.Color.Red;
                dd.LEDs[0].Color.Green = pc.Color.Green;
                dd.LEDs[0].Color.Blue = pc.Color.Blue;
                dd.Name = value.ColorBanks[i].BankName;
                dd.ProductImage = Bmp;
            }
        }

        public event Events.DeviceChangeEventHandler DeviceAdded;
        public event Events.DeviceChangeEventHandler DeviceRemoved;
    }
    public class RSSBackgroundDevice : ISimpleLed
    {
        public static Assembly assembly = Assembly.GetExecutingAssembly();
        public static Stream imageStream = assembly.GetManifestResourceStream("RGBSyncPlus.UI.hires.png");
        public RSSBackgroundDevice()
        {
            dvs = new ControlDevice
            {
                Name = "Background",
                Driver = this,
                ConnectedTo = "RGB Sync Studio",
                DeviceType = DeviceTypes.Other,
                ProductImage = new Bitmap(imageStream),
                LEDs = new ControlDevice.LedUnit[8]
                {
                    new ControlDevice.LedUnit {LEDName = "Top", Data = new ControlDevice.LEDData {LEDNumber = 0}, Color = new LEDColor(0, 0, 0)},
                    new ControlDevice.LedUnit {LEDName = "TopRight", Data = new ControlDevice.LEDData {LEDNumber = 1}, Color = new LEDColor(0, 0, 0)},
                    new ControlDevice.LedUnit {LEDName = "Right", Data = new ControlDevice.LEDData {LEDNumber = 2}, Color = new LEDColor(0, 0, 0)},
                    new ControlDevice.LedUnit {LEDName = "BottomRight", Data = new ControlDevice.LEDData {LEDNumber = 3}, Color = new LEDColor(0, 0, 0)},
                    new ControlDevice.LedUnit {LEDName = "Bottom", Data = new ControlDevice.LEDData {LEDNumber = 4}, Color = new LEDColor(0, 0, 0)},
                    new ControlDevice.LedUnit {LEDName = "BottomLeft", Data = new ControlDevice.LEDData {LEDNumber = 5}, Color = new LEDColor(0, 0, 0)},
                    new ControlDevice.LedUnit {LEDName = "Left", Data = new ControlDevice.LEDData {LEDNumber = 6}, Color = new LEDColor(0, 0, 0)},
                    new ControlDevice.LedUnit {LEDName = "TopLeft", Data = new ControlDevice.LEDData {LEDNumber = 7}, Color = new LEDColor(0, 0, 0)}
                }
            };
        }

        public event EventHandler ColourChange;
        public LEDColor[] Leds = new LEDColor[8]
        {
            new LEDColor(0,0,0),
            new LEDColor(0,0,0),
            new LEDColor(0,0,0),
            new LEDColor(0,0,0),
            new LEDColor(0,0,0),
            new LEDColor(0,0,0),
            new LEDColor(0,0,0),
            new LEDColor(0,0,0)
        };

        private ControlDevice dvs;
        private bool enabled;
        public void Enable()
        {
            if (!enabled)
            {
                enabled = true;
                DeviceAdded?.Invoke(this, new Events.DeviceChangeEventArgs(dvs));
            }
        }

        public void Disable()
        {
            if (enabled)
            {
                enabled = false;
                Leds[0] = new LEDColor(0, 0, 0);
                Leds[1] = new LEDColor(0, 0, 0);
                Leds[2] = new LEDColor(0, 0, 0);
                Leds[3] = new LEDColor(0, 0, 0);
                Leds[4] = new LEDColor(0, 0, 0);
                Leds[5] = new LEDColor(0, 0, 0);
                Leds[6] = new LEDColor(0, 0, 0);
                Leds[7] = new LEDColor(0, 0, 0);

                DeviceRemoved?.Invoke(this, new Events.DeviceChangeEventArgs(dvs));
            }
        }
        public void Dispose()
        {

        }

        public void Configure(DriverDetails driverDetails)
        {

        }

        public void Push(ControlDevice controlDevice)
        {
            bool anyChanges = false;
            for (int i = 0; i < 8; i++)
            {
                if (Leds[i].Red != controlDevice.LEDs[i].Color.Red ||
                    Leds[i].Green != controlDevice.LEDs[i].Color.Green ||
                    Leds[i].Blue != controlDevice.LEDs[i].Color.Blue)
                {
                    anyChanges = true;
                }
            }

            if (anyChanges)
            {
                for (int i = 0; i < 8; i++)
                {
                    Leds[i].Red = controlDevice.LEDs[i].Color.Red;
                    Leds[i].Green = controlDevice.LEDs[i].Color.Green;
                    Leds[i].Blue = controlDevice.LEDs[i].Color.Blue;
                }

                ColourChange?.Invoke(this, new EventArgs());
            }
        }

        public void Pull(ControlDevice controlDevice)
        {

        }

        public DriverProperties GetProperties()
        {
            return new DriverProperties
            {
                SupportsPush = true,
                IsSource = false,
                SupportsPull = false,
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Author = "RGB Sync Studio Team",
                Blurb = "Control the background of your RSS window.",
                CurrentVersion = new ReleaseNumber(1, 0, 0, 0),
                IsPublicRelease = true
            };
        }

        public T GetConfig<T>() where T : SLSConfigData
        {
            return null;
        }

        public void PutConfig<T>(T config) where T : SLSConfigData
        {

        }

        public string Name()
        {
            return "App Background";
        }

        public void InterestedUSBChange(int VID, int PID, bool connected)
        {

        }


        public void SetColorProfile(ColorProfile value)
        {

        }

        public event Events.DeviceChangeEventHandler DeviceAdded;
        public event Events.DeviceChangeEventHandler DeviceRemoved;
    }
}
