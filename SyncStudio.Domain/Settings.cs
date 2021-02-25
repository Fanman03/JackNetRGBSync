using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Newtonsoft.Json;
using SimpleLed;

namespace SyncStudio.Domain
{
    public class Settings : BaseViewModel
    {
        private string simpleLedUserName;

        public string SimpleLedUserName
        {
            get => simpleLedUserName;
            set
            {
                SetProperty(ref simpleLedUserName, value);
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }


        private string simpleLedAuthToken;

        public string SimpleLedAuthToken
        {
            get => simpleLedAuthToken;
            set
            {
                SetProperty(ref simpleLedAuthToken, value);
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }

        private Guid simpleLedUserId;

        public Guid SimpleLedUserId
        {
            get => simpleLedUserId;
            set
            {
                SetProperty(ref simpleLedUserId, value);
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }


        private string background;

        public string Background
        {
            get => background;
            set
            {
                SetProperty(ref background, value);
                BGChangedEvent?.Invoke(this, new EventArgs());
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }

        private Color accentColor;

        public Color AccentColor
        {
            get => accentColor;
            set
            {
                SetProperty(ref accentColor, value);
                BGChangedEvent?.Invoke(this, new EventArgs());
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }


        private float bgopacity;

        public float BackgroundOpacity
        {
            get => bgopacity;
            set
            {
                SetProperty(ref bgopacity, value);
                BGChangedEvent?.Invoke(this, new EventArgs());
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }

        private float dimopacity;

        public float DimBackgroundOpacity
        {
            get => dimopacity;
            set
            {
                SetProperty(ref dimopacity, value);
                BGChangedEvent?.Invoke(this, new EventArgs());
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }

        private float backgroundBlur;

        public float BackgroundBlur
        {
            get => backgroundBlur;
            set
            {
                SetProperty(ref backgroundBlur, value);
                BGChangedEvent?.Invoke(this, new EventArgs());
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }


        public event EventHandler BGChangedEvent;

        private int updateRate = 30;

        public int UpdateRate
        {
            get => updateRate;
            set
            {
                SetProperty(ref updateRate, value);
                AreSettingsStale = true;
                //ProfileChange?.Invoke(this, new EventArgs());
            }
        }

        public delegate void ProfileChangeEventHandler(object sender, EventArgs e);
        public event ProfileChangeEventHandler ProfileChange;

        public void TriggerProfileChange()
        {
            ProfileChange?.Invoke(this, new EventArgs());
        }


        private bool areSettingStale = false;
        [JsonIgnore]
        public bool AreSettingsStale
        {
            get => areSettingStale;
            set
            {
                areSettingStale = value;
                if (value)
                {
                    //todo
                    //if (ServiceManager.Instance?.ConfigService != null) {
                    //    ServiceManager.Instance.ConfigService.SaveSettings();
                    //    areSettingStale = false;
                    //}
                }
            }
        }
        private ObservableCollection<string> profileNames;
        [JsonIgnore]
        public ObservableCollection<string> ProfileNames
        {
            get => profileNames;
            set
            {
                SetProperty(ref profileNames, value);
                AreSettingsStale = true;
                ProfileChange?.Invoke(this, new EventArgs());
            }
        }

        private string currentProfile;

        public string CurrentProfile
        {
            get => currentProfile;
            set
            {
                if (currentProfile == null)
                {
                    AreSettingsStale = true;
                    ProfileChange?.Invoke(this, new EventArgs());
                    SetProperty(ref currentProfile, value);
                }

                if (SetProperty(ref currentProfile, value))
                {
                    AreSettingsStale = true;
                    ProfileChange?.Invoke(this, new EventArgs());
                }
            }
        }

        private bool apiEnabled;

        public bool ApiEnabled
        {
            get => apiEnabled;
            set
            {
                SetProperty(ref apiEnabled, value);
                AreSettingsStale = true;
            }
        }

        private bool controllableBG;

        public bool ControllableBG
        {
            get => controllableBG;
            set
            {
                SetProperty(ref controllableBG, value);
                AreSettingsStale = true;

                //todo
                //if (value)
                //{
                //    SyncStudio.Core.ServiceManager.LedService.RssBackgroundDevice.Enable();
                //}
                //else
                //{
                //    SyncStudio.Core.ServiceManager.LedService.RssBackgroundDevice.Disable();
                //}
                BGChangedEvent?.Invoke(this, new EventArgs());
            }
        }

        private bool experimental;

        public bool Experimental
        {
            get => experimental;
            set
            {
                SetProperty(ref experimental, value);
                AreSettingsStale = true;
            }
        }

        private ObservableCollection<DeviceSettings> deviceSettings;

        public ObservableCollection<DeviceSettings> DeviceSettings
        {
            get => deviceSettings;
            set

            {
                SetProperty(ref deviceSettings, value);
                AreSettingsStale = true;
            }
        }

        private string lang;

        public string Lang
        {
            get => lang;
            set
            {
                SetProperty(ref lang, value);
                AreSettingsStale = true;
                //todo
                //   ServiceManager.Instance.ApplicationManager.FireLanguageChangedEvent();
            }
        }

        private bool minimizeToTray;
        private bool enableDiscordRPC;

        public bool EnableDiscordRPC
        {
            get => enableDiscordRPC;
            set
            {
                SetProperty(ref enableDiscordRPC, value);
                AreSettingsStale = true;
            }
        }

        public bool MinimizeToTray
        {
            get => minimizeToTray;
            set
            {
                SetProperty(ref minimizeToTray, value);
                AreSettingsStale = true;
            }
        }

        private bool rainbowTabBars;

        public bool RainbowTabBars
        {
            get => rainbowTabBars;
            set
            {
                SetProperty(ref rainbowTabBars, value);
                AreSettingsStale = true;
                BGChangedEvent?.Invoke(this, new EventArgs());
            }
        }
    }
}