using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class Settings
    {
        public string SimpleLedUserName { get; set; }
        public string SimpleLedAuthToken { get; set; }
        public Guid SimpleLedUserId { get; set; }
        public string Background { get; set; }
        public Color AccentColor { get; set; }
        public float BackgroundOpacity { get; set; }
        public float DimBackgroundOpacity { get; set; }
        public float BackgroundBlur { get; set; }
        public int UpdateRate { get; set; }
        public Guid CurrentProfile { get; set; }
        public bool ControllableBG { get; set; }
        public bool Experimental { get; set; }
        public string Lang { get; set; }
        public bool EnableDiscordRPC { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool RainbowTabBars { get; set; }

        //public ObservableCollection<DeviceSettings> DeviceSettings { get; set; }
    }
}