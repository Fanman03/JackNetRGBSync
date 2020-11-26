using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;

namespace RGBSyncPlus
{
    public class RSSBackgroundDevice : ISimpleLed
    {
        public RSSBackgroundDevice()
        {
            dvs = new ControlDevice
            {
                Name = "Background",
                Driver = this,
                ConnectedTo = "RGB Sync Studio",
                DeviceType = DeviceTypes.Other,
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
            for (int i = 0; i <8; i++)
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
                Author = "mad ninja",
                Blurb = "Control the background of your RSS window",
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
            return "RSS Background";
        }

        public void InterestedUSBChange(int VID, int PID, bool connected)
        {

        }

        public event Events.DeviceChangeEventHandler DeviceAdded;
        public event Events.DeviceChangeEventHandler DeviceRemoved;
    }
}
