using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;
using SyncStudio.Domain;

namespace SyncStudio.ClientService
{
    public class Events
    {

        private readonly EasyRest easyRest = new EasyRest("http://localhost:59023/api/Events/");

        public event InterfaceEvents.InterfaceDeviceChangeEventHandler DeviceAdded;
        public event InterfaceEvents.InterfaceDeviceChangeEventHandler DeviceRemoved;

        public Events()
        {
            DispatcherTimer eventTimer = new DispatcherTimer();
            eventTimer.Interval = TimeSpan.FromSeconds(5);
            eventTimer.Tick += EventTimerOnTick;
            eventTimer.Start();
        }
        private bool okayToPollEvents = true;
        private async void EventTimerOnTick(object sender, EventArgs e)
        {
            //add all events here - if events aren't subbed, no point polling.
            if (DeviceAdded == null && DeviceRemoved == null)
            {
                return;
            }
            if (!okayToPollEvents) return;
            okayToPollEvents = false;

            var events = await easyRest.Get<List<SerializableEvent>>("/GetEvents");

            foreach (var @event in events)
            {
                switch (@event.Name)
                {
                    case "DeviceAdded":
                    {
                        DeviceAdded?.Invoke(this, new InterfaceEvents.InterfaceDeviceChangeEventArgs(JsonConvert.DeserializeObject<InterfaceControlDevice>(@event.EventArgs)));
                        break;
                    }

                    case "DeviceRemoved":
                    {
                        DeviceRemoved?.Invoke(this, new InterfaceEvents.InterfaceDeviceChangeEventArgs(JsonConvert.DeserializeObject<InterfaceControlDevice>(@event.EventArgs)));
                        break;
                    }
                }
            }

            okayToPollEvents = true;
        }
    }
}
