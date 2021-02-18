using Newtonsoft.Json;
using RGBSyncStudio.Model;
using SimpleLed;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using RGBSyncStudio.Helper;

namespace RGBSyncStudio.Services
{
    public class ProfileService
    {
        public event EventHandler OnProfilesChanged;
        public void OnProfilesChangedInvoke(object sender, EventArgs e)
        {
            OnProfilesChanged?.Invoke(sender, e);
        }
        public readonly Dictionary<string, string> profilePathMapping = new Dictionary<string, string>();
        private readonly SimpleLogger Logger = ServiceManager.Instance.Logger;
        private readonly DeviceMappingModels.Settings Settings = ServiceManager.Instance.ConfigService.Settings;
        public string PROFILES_DIRECTORY;
        public ProfileService(string profilesDir)
        {
            PROFILES_DIRECTORY = profilesDir;
        }

        public bool ProfilesRequiresSave()
        {
            if (CurrentProfile == null) return false;
            return (CurrentProfile.IsProfileStale);
        }

        public void SaveCurrentProfile()
        {
            bool success = false;
            int attempts = 0;
            while (attempts < 10 && !success)
            {
                try
                {
                    if (CurrentProfile.Id == Guid.Empty)
                    {
                        CurrentProfile.Id = Guid.NewGuid();
                    }

                    Guid id = CurrentProfile.Id;

                    string json = JsonConvert.SerializeObject(CurrentProfile);
                    string path;
                    if (profilePathMapping.ContainsKey(CurrentProfile.Name))
                    {
                        path = profilePathMapping[CurrentProfile.Name];
                    }
                    else
                    {
                        path = PROFILES_DIRECTORY + "\\" + id + ".rsprofile";
                        profilePathMapping.Add(CurrentProfile.Name, path);
                    }

                    ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
                    File.WriteAllText(path, json);
                    CurrentProfile.IsProfileStale = false;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public void GenerateNewProfile(string name, bool hotLoad = true)
        {
            if (Settings?.ProfileNames != null && Settings.ProfileNames.Any(x => x.ToLower() == name.ToLower()))
            {
                throw new ArgumentException("Profile name '" + name + "' already exists");
            }

            if (!Directory.Exists(PROFILES_DIRECTORY))
            {
                Directory.CreateDirectory(PROFILES_DIRECTORY);
            }

            DeviceMappingModels.Profile newProfile = new DeviceMappingModels.Profile();
            newProfile.Name = name;

            Guid idGuid = Guid.NewGuid();
            newProfile.Id = idGuid;
            string filename = idGuid.ToString() + ".rsprofile";
            string fullPath = PROFILES_DIRECTORY + "\\" + filename;
            string json = JsonConvert.SerializeObject(newProfile);
            File.WriteAllText(fullPath, json);

            //profilePathMapping.Add(name,fullPath);
            Settings.CurrentProfile = name;
            CurrentProfile = newProfile;
            profilePathMapping.Clear();
            if (hotLoad)
            {
                ServiceManager.Instance.ConfigService.HotLoadSettings();
            }

        }

        public void LoadProfileFromName(string profileName)
        {
            if (profilePathMapping.ContainsKey(profileName))
            {
                string map = profilePathMapping[profileName];
                CurrentProfile = GetProfileFromPath(map);

                if (CurrentProfile.Id == Guid.Empty)
                {
                    string gid = map.Split('\\').Last().Split('.').First();
                    CurrentProfile.Id = Guid.Parse(gid);
                }

                if (CurrentProfile.ColorProfileId != null)
                {
                    try
                    {
                        CurrentProfile.LoadedColorProfile =
                            JsonConvert.DeserializeObject<ColorProfile>(
                                File.ReadAllText("ColorProfiles\\" + CurrentProfile.ColorProfileId + ".json"));

                        ServiceManager.Instance.SLSManager.ColorProfile = CurrentProfile.LoadedColorProfile;
                    }
                    catch (Exception ee)
                    {
                        Logger.Info("Color Profile not loaded, " + ee.Message);
                    }
                }

                if (CurrentProfile.LoadedColorProfile == null)
                {
                    CurrentProfile.LoadedColorProfile = new ColorProfile
                    {
                        Id = Guid.Empty,
                        ProfileName = profileName,
                        ColorBanks = new ObservableCollection<ColorBank>
                        {
                            new ColorBank
                            {
                                BankName = "Primary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#ff0000" } , new ColorObject { ColorString = "#000000" } }
                            },
                            new ColorBank
                            {
                                BankName = "Secondary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#00ff00" } , new ColorObject { ColorString = "#000000" } }
                            },new ColorBank
                            {
                                BankName = "Tertiary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#0000ff" } , new ColorObject { ColorString = "#000000" } }
                            },new ColorBank
                            {
                                BankName = "Auxilary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#ff00ff" } , new ColorObject { ColorString = "#000000" } }
                            }
                        }
                    };

                    CurrentProfile.ColorProfileId = Guid.Empty;
                    CurrentProfile.IsProfileStale = true;
                }

                Settings.CurrentProfile = profileName;
            }
        }

        public DeviceMappingModels.Profile GetProfileFromName(string profileName)
        {
            return TryReallyHard.ToRun(thisCode: () =>
            {
                if (profilePathMapping.ContainsKey(profileName))
                {
                    string map = profilePathMapping[profileName];
                    DeviceMappingModels.Profile result = GetProfileFromPath(map);

                    if (result.Id == Guid.Empty)
                    {
                        string gid = map.Split('\\').Last().Split('.').First();
                        result.Id = Guid.Parse(gid);
                    }

                    return result;
                }

                return null;
            });
        }

        public DeviceMappingModels.Profile GetProfileFromPath(string path)
        {
            return TryReallyHard.ToRun(thisCode: () =>
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<DeviceMappingModels.Profile>(json);
            });
        }


        public DeviceMappingModels.Profile CurrentProfile;

        public void DeleteProfile(string dcName)
        {
            string path = profilePathMapping[dcName];
            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.Delete(path);
            ServiceManager.Instance.ConfigService.HotLoadSettings();
        }

        public void RenameProfile(string currentProfileOriginalName, string currentProfileName)
        {
            if (profilePathMapping.ContainsKey(currentProfileOriginalName))
            {
                string map = profilePathMapping[currentProfileOriginalName];
                DeviceMappingModels.Profile profile = GetProfileFromPath(map);

                profile.Name = currentProfileName;

                SaveProfile(profile, map);
                ServiceManager.Instance.ConfigService.HotLoadSettings();
            }
        }


        public void SaveProfile(DeviceMappingModels.Profile profile)
        {
            string json = JsonConvert.SerializeObject(profile);
            string path = profilePathMapping[profile.Name];
            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

        public void SaveProfile(DeviceMappingModels.Profile profile, string path)
        {
            string json = JsonConvert.SerializeObject(profile);
            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

    }
}
