using DiscordRPC;

namespace RGBSyncStudio.Services
{
    public class DiscordService
    {
        public DiscordRpcClient client;
        public void ConnectDiscord()
        {
            client = new DiscordRpcClient("768250991149580310");
            client.Initialize();

            ServiceManager.Instance.Logger.Info("Discord RPC enabled.");
            if (client.IsDisposed == true)
            {
                ServiceManager.Instance.Logger.Info("Discord RPC client disposed, initializing new one.");
                client = new DiscordRpcClient("768250991149580310");
                client.Initialize();
            }

            ServiceManager.Instance.Logger.Info("Setting Discord presence.");
            client.SetPresence(new RichPresence()
            {
                State = "Profile: " + ServiceManager.Instance.ProfileService.CurrentProfile.Name,
                Details = "Syncing lighting effects",
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    LargeImageText = "RGB Sync Studio"
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
