using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RGBSyncPlus.Model;

namespace RGBSyncPlus.UI
{
    public class StoreHandler
    {
        public List<PositionalAssignment.PluginDetails> Plugins = new List<PositionalAssignment.PluginDetails>();

        public List<PositionalAssignment.PluginDetails> DownloadStoreManifest()
        {
            string contents;
            using (var wc = new System.Net.WebClient())
            {
                contents = wc.DownloadString("https://raw.githubusercontent.com/SimpleLed/Store/master/manifest.json");
                return (JsonConvert.DeserializeObject<List<PositionalAssignment.PluginDetails>>(contents));
            }
        }
    }
}
