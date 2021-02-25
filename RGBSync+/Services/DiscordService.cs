using DiscordRPC;

namespace SyncStudio.WPF.Services
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
                State = "Profile: " + SyncStudio.Core.ServiceManager.Profiles.GetCurrentProfile().Name,
                Details = "Syncing lighting effects",
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    LargeImageText = ServiceManager.Instance.Branding.GetAppName()
                }
            });
        }

        public void Stop()
        {
            if (ServiceManager.Instance.ConfigService.Settings.EnableDiscordRPC == true)
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
