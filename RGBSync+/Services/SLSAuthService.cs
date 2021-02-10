using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLed;

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
