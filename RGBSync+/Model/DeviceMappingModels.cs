using Newtonsoft.Json;

using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RGBSyncStudio.Model
{
    public class DeviceMappingModels
    {
        public class DeviceMap
        {
            public ControlDevice Source { get; set; }
            public List<ControlDevice> Dest { get; set; }
        }
        public class DeviceMapping
        {
            public DeviceProxy SourceDevice { get; set; }
            public List<DeviceProxy> DestinationDevices { get; set; }
        }

        public class DeviceProxy
        {
            public string DriverName { get; set; }
            public string DeviceName { get; set; }
            public DeviceProxy() { }

            public DeviceProxy(ControlDevice device)
            {
                DriverName = device.Driver.Name();
                DeviceName = device.Name;
            }
        }



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
                        if (ServiceManager.Instance?.ConfigService != null) {
                            ServiceManager.Instance.ConfigService.SaveSettings();
                            areSettingStale = false;
                        }
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
                    if (value)
                    {
                        ServiceManager.Instance.LedService.RssBackgroundDevice.Enable();
                    }
                    else
                    {
                        ServiceManager.Instance.LedService.RssBackgroundDevice.Disable();
                    }
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

            private ObservableCollection<NGDeviceSettings> deviceSettings;

            public ObservableCollection<NGDeviceSettings> DeviceSettings
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
                    ServiceManager.Instance.ApplicationManager.FireLanguageChangedEvent();
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

        public class Profile : BaseViewModel
        {

            private ColorProfile colorProfile;

            [JsonIgnore]
            public ColorProfile LoadedColorProfile
            {
                get => colorProfile;
                set { SetProperty(ref colorProfile, value); }
            }
            private Guid? colorProfileId;

            public Guid? ColorProfileId
            {
                get => colorProfileId;
                set
                {
                    SetProperty(ref colorProfileId, value);
                    IsProfileStale = true;
                }
            }

            [JsonIgnore]
            public bool IsProfileStale { get; set; }

            private string name;
            public string Name
            {
                get => name;
                set
                {
                    SetProperty(ref name, value);
                    IsProfileStale = true;
                }
            }

            private ObservableCollection<NGDeviceProfileSettings> deviceProfileSettings = new ObservableCollection<NGDeviceProfileSettings>();

            public ObservableCollection<NGDeviceProfileSettings> DeviceProfileSettings
            {
                get => deviceProfileSettings;
                set
                {
                    SetProperty(ref deviceProfileSettings, value);
                    IsProfileStale = true;
                }
            }

            public Guid Id { get; set; }
        }

        public class NGDeviceSettings : BaseViewModel
        {
            [JsonIgnore] public bool AreDeviceSettingsStale;

            private string name;
            public string Name
            {
                get => name;
                set
                {
                    SetProperty(ref name, value);
                    AreDeviceSettingsStale = true;
                }
            }


            private string providerName;
            public string ProviderName
            {
                get => providerName;
                set
                {
                    SetProperty(ref providerName, value);
                    AreDeviceSettingsStale = true;
                }
            }

            private int ledShift = 0;

            public int LEDShift
            {
                get => ledShift;
                set
                {
                    SetProperty(ref ledShift, value);
                    AreDeviceSettingsStale = true;
                }
            }

            private bool reverse = false;

            public bool Reverse
            {
                get => reverse;
                set
                {
                    SetProperty(ref reverse, value);
                    AreDeviceSettingsStale = true;
                }
            }


            private bool ledCountOverride = false;

            public bool LEDCountOverride
            {
                get => ledCountOverride;
                set
                {
                    SetProperty(ref ledCountOverride, value);
                    AreDeviceSettingsStale = true;
                }
            }

            private int ledCountOverrideValue = 0;

            public int LEDCountOverrideValue
            {
                get => ledCountOverrideValue;
                set
                {
                    SetProperty(ref ledCountOverrideValue, value);
                    AreDeviceSettingsStale = true;
                }
            }
        }

        public class SourceControllingModel : BaseViewModel
        {
            private string name;
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }


            private string providerName;
            public string ProviderName
            {
                get => providerName;
                set => SetProperty(ref providerName, value);
            }

            private string connectedTo;

            public string ConnectedTo
            {
                get => connectedTo;
                set => SetProperty(ref connectedTo, value);
            }

            private bool isCurrent;

            public bool IsCurrent
            {
                get => isCurrent;
                set => SetProperty(ref isCurrent, value);
            }
        }

        public class SourceModel : BaseViewModel
        {
            public override string ToString()
            {
                return Device?.DeviceType + " - " + Device?.Name;
            }

            private string titleOverride;
            private string subTitleOverride;
            private string channelOverride;
            private string deviceType;
            [JsonIgnore]
            public string TitleOverride
            {
                get => titleOverride;
                set => SetProperty(ref titleOverride, value);
            }

            [JsonIgnore]
            public string DeviceType
            {
                get => deviceType;
                set => SetProperty(ref deviceType, value);
            }

            [JsonIgnore]
            public string SubTitleOverride
            {
                get => subTitleOverride;
                set => SetProperty(ref subTitleOverride, value);
            }

            [JsonIgnore]
            public string ChannelOverride
            {
                get => channelOverride;
                set => SetProperty(ref channelOverride, value);
            }

            private bool isControllingSomething;

            public bool IsControllingSomething
            {
                get => isControllingSomething;
                set => SetProperty(ref isControllingSomething, value);
            }


            private bool isHidden;

            public bool IsHidden
            {
                get => isHidden;
                set => SetProperty(ref isHidden, value);
            }


            private bool hasConfig;

            public bool HasConfig
            {
                get => hasConfig;
                set => SetProperty(ref hasConfig, value);
            }


            private string name;
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }


            private string providerName;
            public string ProviderName
            {
                get => providerName;
                set => SetProperty(ref providerName, value);
            }

            private string connectedTo;

            public string ConnectedTo
            {
                get => connectedTo;
                set => SetProperty(ref connectedTo, value);
            }

            private string controlling;

            [JsonIgnore]
            public string Controlling
            {
                get => controlling;
                set => SetProperty(ref controlling, value);
            }

            private int controllingModelsCount;

            [JsonIgnore]
            public int ControllingModelsCount
            {
                get => controllingModelsCount;
                set => SetProperty(ref controllingModelsCount, value);
            }

            private ObservableCollection<SourceControllingModel> controllingModels;

            [JsonIgnore]
            public ObservableCollection<SourceControllingModel> ControllingModels
            {
                get => controllingModels;
                set
                {
                    SetProperty(ref controllingModels, value);
                    //     ControllingModelsCount = controllingModels.Count;
                }
            }

            private bool enabled;

            public bool Enabled
            {
                get => enabled;
                set
                {
                    SetProperty(ref enabled, value);
                }
            }

            private BitmapImage image;

            public BitmapImage Image
            {
                get => image;
                set => SetProperty(ref image, value);
            }

            public ControlDevice Device { get; set; }

            private bool hovered;
            [JsonIgnore]
            public bool Hovered { get => hovered; set => SetProperty(ref hovered, value); }
        }

        public class NGDeviceProfileSettings : BaseViewModel
        {
            [JsonIgnore]
            public ControlDevice Device { get; set; }

            [JsonIgnore]
            public ControlDevice SourceDevice { get; set; }

            private string name;
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }


            private string providerName;
            public string ProviderName
            {
                get => providerName;
                set => SetProperty(ref providerName, value);
            }

            private string connectedTo;

            public string ConnectedTo
            {
                get => connectedTo;
                set => SetProperty(ref connectedTo, value);
            }




            private string sourceName;
            public string SourceName
            {
                get => sourceName;
                set => SetProperty(ref sourceName, value);
            }


            private string sourceProviderName;
            public string SourceProviderName
            {
                get => sourceProviderName;
                set => SetProperty(ref sourceProviderName, value);
            }

            private string sourceConnectedTo;

            public string SourceConnectedTo
            {
                get => sourceConnectedTo;
                set => SetProperty(ref sourceConnectedTo, value);
            }



        }

        public class DeviceOverrides : BaseViewModel
        {
            private string titleOverride;
            private string subTitleOverride;
            private string channelOverride;
            private CustomDeviceSpecification customDeviceSpecification;

            public CustomDeviceSpecification CustomDeviceSpecification
            {
                get => customDeviceSpecification;
                set
                {
                    SetProperty(ref customDeviceSpecification, value);
                    PushUpdate();
                }
            }
            public string TitleOverride
            {
                get => titleOverride;
                set
                {
                    SetProperty(ref titleOverride, value);
                    PushUpdate();
                }
            }

            public string SubTitleOverride
            {
                get => subTitleOverride;
                set
                {
                    SetProperty(ref subTitleOverride, value);
                    PushUpdate();
                }
            }

            public string ChannelOverride
            {
                get => channelOverride;
                set
                {
                    SetProperty(ref channelOverride, value);
                    PushUpdate();
                }
            }

            private string name;
            public string Name
            {
                get => name;
                set
                {
                    SetProperty(ref name, value);
                    // PushUpdate();
                }
            }

            private string connectedTo;

            public string ConnectedTo
            {
                get => connectedTo;
                set
                {
                    SetProperty(ref connectedTo, value);
                    // PushUpdate();
                }
            }

            private string providerName;
            public string ProviderName
            {
                get => providerName;
                set
                {
                    SetProperty(ref providerName, value);
                    // PushUpdate();
                }
            }

            private void PushUpdate()
            {
                ServiceManager.Instance.LedService.SetOverride(this);
            }
        }

        public class Device : BaseViewModel
        {
            private DeviceOverrides overrides;

            [JsonIgnore]
            public DeviceOverrides Overrides
            {
                get => overrides;
                set => SetProperty(ref overrides, value);
            }

            private bool selected;

            public bool Selected
            {
                get => selected;
                set => SetProperty(ref selected, value);
            }

            private string name;
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }

            private string connectedTo;

            public string ConnectedTo
            {
                get => connectedTo;
                set => SetProperty(ref connectedTo, value);
            }

            private string title;

            public string Title
            {
                get => title;
                set => SetProperty(ref title, value);
            }

            private string providerName;
            public string ProviderName
            {
                get => providerName;
                set => SetProperty(ref providerName, value);
            }

            private BitmapImage image;

            public BitmapImage Image
            {
                get => image;
                set => SetProperty(ref image, value);
            }

            private ControlDevice controlDevice;

            public ControlDevice ControlDevice
            {
                get => controlDevice;
                set => SetProperty(ref controlDevice, value);
            }

            private bool supportsPush;

            public bool SupportsPush
            {
                get => supportsPush;
                set => SetProperty(ref supportsPush, value);
            }


            private bool supportsPull;

            public bool SupportsPull
            {
                get => supportsPull;
                set => SetProperty(ref supportsPull, value);
            }

            private DriverProperties driverProps;

            public DriverProperties DriverProps
            {
                get => driverProps;
                set => SetProperty(ref driverProps, value);
            }

            private string sunkTo;

            public string SunkTo
            {
                get => sunkTo;
                set => SetProperty(ref sunkTo, value);
            }
        }

        public class DeviceMappingViewModel : BaseViewModel
        {
            public DeviceMappingViewModel()
            {
                DestinationDevices.CollectionChanged += DestinationDevices_CollectionChanged;
            }

            private void DestinationDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {

                SyncBack?.Invoke(this);
            }

            public Action<object> SyncBack;

            public string ProviderName { get; set; }
            private bool enabled;

            public bool Enabled
            {
                get => enabled;
                set
                {
                    SetProperty(ref enabled, value);
                    SyncBack?.Invoke(this);
                }
            }
            public ObservableCollection<DeviceMappingItemViewModel> DestinationDevices { get; set; } = new ObservableCollection<DeviceMappingItemViewModel>();
            public bool expanded;
            public bool Expanded
            {
                get => expanded;
                set
                {
                    SetProperty(ref expanded, value);
                    SyncBack?.Invoke(this);
                }
            }

            public Guid Id { get; set; }
        }

        public class DeviceMappingItemViewModel : BaseViewModel
        {
            public Action<object> SyncBack;
            public ControlDevice DestinationDevice { get; set; }
            private bool enabled;

            public bool Enabled
            {
                get => enabled;
                set
                {
                    SetProperty(ref enabled, value);
                    SyncBack?.Invoke(this);
                }
            }

            public Guid ParentId { get; set; }
        }


    }
}
