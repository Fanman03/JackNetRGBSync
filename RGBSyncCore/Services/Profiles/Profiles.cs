using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleLed;
using SyncStudio.Domain;

namespace SyncStudio.Core.Services.Profiles
{
    public class Profiles : IProfiles
    {
        public readonly Dictionary<string, string> profilePathMapping = new Dictionary<string, string>();
        private Profile CurrentProfile;
        public Profile GetCurrentProfile()
        {
            if (CurrentProfile == null)
            {
                var profiles = GetAvailableProfiles();
                if (!profiles.Any())
                {
                    var temp = GenerateNewProfile("Default");
                    CurrentProfile = temp;
                }
            }

            return CurrentProfile;
        }

        public void SetCurrentProfile(Profile profile)
        {
            CurrentProfile = profile;
        }

        public Profile GetProfile(string profileName)
        {
            var profiles = Directory.GetFiles(ServiceManager.PROFILES_DIRECTORY, "*.rsprofile").ToList();

            foreach (string profile in profiles)
            {
                string json = File.ReadAllText(profile);
                Profile tempProfile = JsonConvert.DeserializeObject<Profile>(json);
                if (tempProfile != null)
                {
                    if (tempProfile.Name == profileName)
                    {
                        return tempProfile;
                    }
                }
            }

            return null;
        }

        public List<string> GetAvailableProfiles()
        {
            return Directory.GetFiles(ServiceManager.PROFILES_DIRECTORY, "*.rsprofile").ToList();
        }

        public void SaveProfile(Profile profile)
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
                    path = ServiceManager.PROFILES_DIRECTORY + "\\" + id + ".rsprofile";
                    profilePathMapping.Add(CurrentProfile.Name, path);
                }


                //todo update config

                File.WriteAllText(path, json);
                CurrentProfile.IsProfileStale = false;
            }
            catch { }
        }

        public Profile GenerateNewProfile(string name)
        {
            Profile newProfile = new Profile();
            newProfile.Name = name;

            Guid idGuid = Guid.NewGuid();
            newProfile.Id = idGuid;

            return newProfile;
        }

        public void RemoveProfile(string profileName)
        {
            var profiles = Directory.GetFiles(ServiceManager.PROFILES_DIRECTORY, "*.rsprofile").ToList();
            string fileToDelete = "";
            foreach (string profile in profiles)
            {
                string json = File.ReadAllText(profile);
                Profile tempProfile = JsonConvert.DeserializeObject<Profile>(json);
                if (tempProfile != null)
                {
                    if (tempProfile.Name == profileName)
                    {
                        fileToDelete = profile;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(fileToDelete))
            {
                File.Delete(fileToDelete);
            }
        }

        public Profile GetProfileFromName(string profileName)
        {

            if (profilePathMapping.ContainsKey(profileName))
            {
                string map = profilePathMapping[profileName];
                Profile result = GetProfileFromPath(map);

                if (result.Id == Guid.Empty)
                {
                    string gid = map.Split('\\').Last().Split('.').First();
                    result.Id = Guid.Parse(gid);
                }

                return result;
            }

            return null;

        }

        private Profile GetProfileFromPath(string path)
        {

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Profile>(json);

        }
        private Profile LoadProfileFromName(string profileName)
        {
            Profile result = null;

            if (profilePathMapping.ContainsKey(profileName))
            {
                string map = profilePathMapping[profileName];
                result = GetProfileFromPath(map);

                if (result.Id == Guid.Empty)
                {
                    string gid = map.Split('\\').Last().Split('.').First();
                    result.Id = Guid.Parse(gid);
                }

                if (result.ColorProfileId != null)
                {
                    try
                    {
                        CurrentProfile.LoadedColorProfile =
                            JsonConvert.DeserializeObject<ColorProfile>(
                                File.ReadAllText("ColorProfiles\\" + result.ColorProfileId + ".json"));

                        ServiceManager.ColorPallets.SetActiveColorPallet(result.LoadedColorProfile);
                    }
                    catch (Exception ee)
                    {
                        //todo put logger back
                        //Logger.Info("Color Profile not loaded, " + ee.Message);
                    }
                }

                if (result.LoadedColorProfile == null)
                {
                    result.LoadedColorProfile = new ColorProfile
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
                            },
                            new ColorBank
                            {
                                BankName = "Tertiary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#0000ff" } , new ColorObject { ColorString = "#000000" } }
                            },
                            new ColorBank
                            {
                                BankName = "Auxilary",
                                Colors = new ObservableCollection<ColorObject>{ new ColorObject { ColorString = "#ff00ff" } , new ColorObject { ColorString = "#000000" } }
                            }
                        }
                    };

                    CurrentProfile.ColorProfileId = Guid.Empty;
                    CurrentProfile.IsProfileStale = true;
                }

                //todo update config

            }

            return result;
        }
    }
}
