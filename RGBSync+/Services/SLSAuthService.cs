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
                ServiceManager.Instance.ConfigService.Settings.SimpleLedUserId = SimpleLedAuth.UserId.Value;
                ServiceManager.Instance.ConfigService.Settings.SimpleLedUserName = SimpleLedAuth.UserName;
                ServiceManager.Instance.ConfigService.Settings.SimpleLedAuthToken = SimpleLedAuth.AccessToken;
                SimpleLedAuthenticated = true;
                onLoginAction?.Invoke();
            }));
        }
    }
}
