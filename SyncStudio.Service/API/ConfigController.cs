using System;
using System.IO;
using System.Web.Http;
using Newtonsoft.Json;
using SyncStudio.Domain;

namespace SyncStudio.Service.API
{
    [RoutePrefix("api/Config")]
    public class ConfigController : ApiController
    {
        private Settings settings;
        public ConfigController()
        {
            if (File.Exists("config.json"))
            {
                string json = File.ReadAllText("config.json");
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }
            else
            {
                settings = new Settings();
            }
        }

        private void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText("config.json", json);
        }
        //Gets

        [HttpGet]
        [Route("get/SimpleLedUserName")]
        public string GetSimpleLedUserName() => settings.SimpleLedUserName;
        
        [HttpGet]
        [Route("get/SimpleLedAuthToken")]
        public string GetSimpleLedAuthToken() => settings.SimpleLedAuthToken;
        [HttpGet]
        [Route("get/SimpleLedUserId")]
        public Guid GetSimpleLedUserId() => settings.SimpleLedUserId;
        [HttpGet]
        [Route("get/Background")] 
        public string GetBackground() => settings.Background;
        [HttpGet]
        [Route("get/BackgroundOpacity")]
        public float GetBackgroundOpacity() => settings.BackgroundOpacity;
        [HttpGet]
        [Route("get/DimBackgroundOpacity")]
        public float GetDimBackgroundOpacity() => settings.DimBackgroundOpacity;
        [HttpGet]
        [Route("get/BackgroundBlur")]
        public float GetBackgroundBlur() => settings.BackgroundBlur;
        [HttpGet]
        [Route("get/UpdateRate")]
        public int GetUpdateRate() => settings.UpdateRate;
        [HttpGet]
        [Route("get/CurrentProfile")]
        public Guid GetCurrentProfile() => settings.CurrentProfile;
        [HttpGet]
        [Route("get/ControllableBG")]
        public bool GetControllableBG() => settings.ControllableBG;
        [HttpGet]
        [Route("get/Experimental")]
        public bool GetExperimental() => settings.Experimental;
        [HttpGet]
        [Route("get/Lang")]
        public string GetLang() => settings.Lang;
        [HttpGet]
        [Route("get/EnableDiscordRPC")]
        public bool GetEnableDiscordRPC() => settings.EnableDiscordRPC;
        [HttpGet]
        [Route("get/MinimizeToTray")]
        public bool GetMinimizeToTray() => settings.MinimizeToTray;
        [HttpGet]
        [Route("get/RainbowTabBars")]
        public bool GetRainbowTabBars() => settings.RainbowTabBars;

        //Sets

        [HttpPost]
        [Route("set/SimpleLedUserName")]
        public void SetSimpleLedUserName([FromBody]string userName)
        {
            settings.SimpleLedUserName = userName;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/SimpleLedAuthToken")]
        public void SetSimpleLedAuthToken([FromBody] string authToken)
        {
            settings.SimpleLedAuthToken = authToken;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/SimpleLedUserId")]
        public void SetSimpleLedUserId([FromBody] Guid userId)
        {
            settings.SimpleLedUserId = userId;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/Background")]
        public void SetBackground([FromBody] string background)
        {
            settings.Background = background;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/BackgroundOpacity")]
        public void SetBackgroundOpacity([FromBody] float backgroundOpacity)
        {
            settings.BackgroundOpacity = backgroundOpacity;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/DimBackgroundOpacity")]
        public void SetDimBackgroundOpacity([FromBody] float dimBackgroundOpacity)
        {
            settings.DimBackgroundOpacity = dimBackgroundOpacity;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/BackgroundBlur")]
        public void SetBackgroundBlur([FromBody] float blur)
        {
            settings.BackgroundBlur = blur;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/UpdateRate")]
        public void SetUpdateRate([FromBody] int updateRate)
        {
            settings.UpdateRate = updateRate;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/CurrentProfile")]
        public void SetCurrentProfile([FromBody] Guid profileId)
        {
            settings.CurrentProfile = profileId;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/ControllableBG")]
        public void SetControllableBG([FromBody] bool enabled)
        {
            settings.ControllableBG = enabled;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/Experimental")]
        public void SetExperimental([FromBody] bool enabled)
        {
            settings.Experimental = enabled;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/Lang")]
        public void SetLang([FromBody] string lang)
        {
            settings.Lang = lang;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/EnableDiscordRPC")]
        public void SetEnableDiscordRPC([FromBody] bool enabled)
        {
            settings.EnableDiscordRPC = enabled;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/MinimizeToTray")]
        public void SetMinimizeToTray([FromBody] bool enabled)
        {
            settings.MinimizeToTray = enabled;
            SaveSettings();
        }

        [HttpPost]
        [Route("set/RainbowTabBars")]
        public void SetRainbowTabBars([FromBody] bool enabled)
        {
            settings.RainbowTabBars = enabled;
            SaveSettings();
        }
    }
}