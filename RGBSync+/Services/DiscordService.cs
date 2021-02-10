using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;

namespace RGBSyncStudio.Services
{
    public class DiscordService
    {
        public DiscordRpcClient client;
        public void ConnectDiscord()
        {
            client = new DiscordRpcClient("581567509959016456");
            client.Initialize();

            ServiceManager.Instance.Logger.Info("Discord RPC enabled.");
            if (client.IsDisposed == true)
            {
                ServiceManager.Instance.Logger.Info("Discord RPC client disposed, initializing new one.");
                client = new DiscordRpcClient("581567509959016456");
                client.Initialize();
            }

            ServiceManager.Instance.Logger.Info("Setting Discord presensce.");
            client.SetPresence(new RichPresence()
            {
                State = "Profile: " + ServiceManager.Instance.ConfigService.Settings.Name,
                Details = "Syncing lighting effects",
                Assets = new Assets()
                {
                    LargeImageKey = "large_image",
                    LargeImageText = "RGB Sync",
                    SmallImageKey = "small_image",
                    SmallImageText = "by Fanman03"
                }
            });
        }

        public void Stop()
        {
            if (ServiceManager.Instance.ConfigService.NGSettings.EnableDiscordRPC == true)
            {
                try
                {
                    client.Dispose();

                }
                catch
                {
                }
            }

            client = null;
        }
    }
}
