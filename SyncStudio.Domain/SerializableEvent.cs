using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SyncStudio.Domain
{
    public class SerializableEvent
    {
        public string Name { get; set; }
        public string EventArgs { get; set; }

        public SerializableEvent(){}

        public SerializableEvent(string name, object eventArgs)
        {
            Name = name;
            EventArgs = JsonConvert.SerializeObject(eventArgs);
        }
    }
}
