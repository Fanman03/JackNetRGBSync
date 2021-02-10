using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RGBSyncStudio.Model;
using SimpleLed;

namespace RGBSyncStudio.Services
{
    public class ProfileService
    {
        public event EventHandler OnProfilesChanged;
        public void OnProfilesChangedInvoke(object sender, EventArgs e)
        {
            OnProfilesChanged?.Invoke(sender,e);
        }
        public readonly Dictionary<string, string> profilePathMapping = new Dictionary<string, string>();
        SimpleLogger Logger = ServiceManager.Instance.Logger;
        private DeviceMappingModels.NGSettings NGSettings = ServiceManager.Instance.ConfigService.NGSettings;
        public string NGPROFILES_DIRECTORY;
        public ProfileService(string profilesDir)
        {
            NGPROFILES_DIRECTORY = profilesDir;
        }

        public bool ProfilesRequiresSave()
        {
            if (CurrentProfile == null) return false;
            return (CurrentProfile.IsProfileStale);
        }

        public void SaveCurrentNGProfile()
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
                path = NGPROFILES_DIRECTORY + "\\" + id + ".rsprofile";
                profilePathMapping.Add(CurrentProfile.Name, path);
            }

            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

        public void GenerateNewProfile(string name, bool hotLoad = true)
        {
            if (NGSettings?.ProfileNames != null && NGSettings.ProfileNames.Any(x => x.ToLower() == name.ToLower()))
            {
                throw new ArgumentException("Profile name '" + name + "' already exists");
            }

            if (!Directory.Exists(NGPROFILES_DIRECTORY))
            {
                Directory.CreateDirectory(NGPROFILES_DIRECTORY);
            }

            DeviceMappingModels.NGProfile newProfile = new DeviceMappingModels.NGProfile();
            newProfile.Name = name;

            Guid idGuid = Guid.NewGuid();
            newProfile.Id = idGuid;
            string filename = idGuid.ToString() + ".rsprofile";
            string fullPath = NGPROFILES_DIRECTORY + "\\" + filename;
            string json = JsonConvert.SerializeObject(newProfile);
            File.WriteAllText(fullPath, json);

            //profilePathMapping.Add(name,fullPath);
            NGSettings.CurrentProfile = name;
            CurrentProfile = newProfile;
            profilePathMapping.Clear();
            if (hotLoad)
            {
                ServiceManager.Instance.ConfigService.HotLoadNGSettings();
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

                NGSettings.CurrentProfile = profileName;
            }
        }

        public DeviceMappingModels.NGProfile GetProfileFromName(string profileName)
        {
            if (profilePathMapping.ContainsKey(profileName))
            {
                string map = profilePathMapping[profileName];
                DeviceMappingModels.NGProfile result = GetProfileFromPath(map);

                if (result.Id == Guid.Empty)
                {
                    string gid = map.Split('\\').Last().Split('.').First();
                    result.Id = Guid.Parse(gid);
                }

                return result;
            }

            return null;
        }

        public DeviceMappingModels.NGProfile GetProfileFromPath(string path)
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<DeviceMappingModels.NGProfile>(json);
        }


        public DeviceMappingModels.NGProfile CurrentProfile;

        public void DeleteProfile(string dcName)
        {
            string path = profilePathMapping[dcName];
          ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.Delete(path);
            ServiceManager.Instance.ConfigService.HotLoadNGSettings();
        }

        public void RenameProfile(string currentProfileOriginalName, string currentProfileName)
        {
            if (profilePathMapping.ContainsKey(currentProfileOriginalName))
            {
                string map = profilePathMapping[currentProfileOriginalName];
                DeviceMappingModels.NGProfile profile = GetProfileFromPath(map);

                profile.Name = currentProfileName;

                SaveNGProfile(profile, map);
                ServiceManager.Instance.ConfigService.HotLoadNGSettings();
            }
        }


        public void SaveNGProfile(DeviceMappingModels.NGProfile profile)
        {
            string json = JsonConvert.SerializeObject(profile);
            string path = profilePathMapping[profile.Name];
            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

        public void SaveNGProfile(DeviceMappingModels.NGProfile profile, string path)
        {
            string json = JsonConvert.SerializeObject(profile);
            ServiceManager.Instance.ConfigService.TimeSettingsLastSave = DateTime.Now;
            File.WriteAllText(path, json);
            CurrentProfile.IsProfileStale = false;
        }

    }
}
