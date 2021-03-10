using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncStudio.ClientService
{
    public class Settings
    {
        private readonly EasyRest easyRest = new EasyRest("http://localhost:59023/api/Config/");

        public string SimpleLedUserName { get=>easyRest.GetSync<string>("get/SimpleLedUserName"); set=>easyRest.PostSync<string>("set/SimpleLedUserName",value); }
        public string SimpleLedAuthToken { get => easyRest.GetSync<string>("get/SimpleLedAuthToken"); set => easyRest.PostSync<string>("set/SimpleLedAuthToken", value); }
        public Guid SimpleLedUserId { get => easyRest.GetSync<Guid>("get/SimpleLedUserId"); set => easyRest.PostSync<Guid>("set/SimpleLedUserId", value); }
        public string Background { get => easyRest.GetSync<string>("get/Background"); set => easyRest.PostSync<string>("set/Background", value); }
        public float BackgroundOpacity { get => easyRest.GetSync<float>("get/BackgroundOpacity"); set => easyRest.PostSync<float>("set/BackgroundOpacity", value); }
        public float DimBackgroundOpacity { get => easyRest.GetSync<float>("get/DimBackgroundOpacity"); set => easyRest.PostSync<float>("set/DimBackgroundOpacity", value); }
        public float BackgroundBlur { get => easyRest.GetSync<float>("get/BackgroundBlur"); set => easyRest.PostSync<float>("set/BackgroundBlur", value); }
        public int UpdateRate { get => easyRest.GetSync<int>("get/UpdateRate"); set => easyRest.PostSync<int>("set/UpdateRate", value); }
        public Guid CurrentProfile { get => easyRest.GetSync<Guid>("get/CurrentProfile"); set => easyRest.PostSync<Guid>("set/CurrentProfile", value); }
        public bool ControllableBG { get => easyRest.GetSync<bool>("get/ControllableBG"); set => easyRest.PostSync<bool>("set/ControllableBG", value); }
        public bool Experimental { get => easyRest.GetSync<bool>("get/Experimental"); set => easyRest.PostSync<bool>("set/Experimental", value); }
        public string Lang { get => easyRest.GetSync<string>("get/Lang"); set => easyRest.PostSync<string>("set/Lang", value); }
        public bool EnableDiscordRPC { get => easyRest.GetSync<bool>("get/EnableDiscordRPC"); set => easyRest.PostSync<bool>("set/EnableDiscordRPC", value); }
        public bool MinimizeToTray { get => easyRest.GetSync<bool>("get/MinimizeToTray"); set => easyRest.PostSync<bool>("set/MinimizeToTray", value); }
        public bool RainbowTabBars { get => easyRest.GetSync<bool>("get/RainbowTabBars"); set => easyRest.PostSync<bool>("set/RainbowTabBars", value); }

    }
}
