using SimpleLed;
using System;
using System.Diagnostics;

namespace RGBSyncStudio.Services
{
    public class SLSAuthService
    {
        public SimpleLed.LoginSystem SimpleLedAuth = new LoginSystem();
        public bool SimpleLedAuthenticated = false;
        public void SimpleLedLogIn(Action onLoginAction)
        {
            Process.Start(SimpleLedAuth.Login(() =>
            {
                ServiceManager.Instance.ConfigService.NGSettings.SimpleLedUserId = SimpleLedAuth.UserId.Value;
                ServiceManager.Instance.ConfigService.NGSettings.SimpleLedUserName = SimpleLedAuth.UserName;
                ServiceManager.Instance.ConfigService.NGSettings.SimpleLedAuthToken = SimpleLedAuth.AccessToken;
                SimpleLedAuthenticated = true;
                onLoginAction?.Invoke();
            }));
        }
    }
}
