using SimpleLed;
using System;
using System.Diagnostics;
using SyncStudio.ClientService;

namespace SyncStudio.WPF.Services
{
    public class SLSAuthService
    {
        private ClientService.Settings settings = new Settings();
        public SimpleLed.LoginSystem SimpleLedAuth = new LoginSystem();
        public bool SimpleLedAuthenticated = false;
        public void SimpleLedLogIn(Action onLoginAction)
        {
            Process.Start(SimpleLedAuth.Login(() =>
            {
                settings.SimpleLedUserId = SimpleLedAuth.UserId.Value;
                settings.SimpleLedUserName = SimpleLedAuth.UserName;
                settings.SimpleLedAuthToken = SimpleLedAuth.AccessToken;
                SimpleLedAuthenticated = true;
                onLoginAction?.Invoke();
            }));
        }
    }
}
