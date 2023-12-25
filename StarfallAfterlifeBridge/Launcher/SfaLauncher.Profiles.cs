using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {
        public List<SfaProfile> Profiles { get; } = new();

        public SfaProfile CurrentProfile
        {
            get => _currentProfile;
            set
            {
                _currentProfile = value;
                LastSelectedProfileId = value?.GameProfile?.Id ?? Guid.Empty;
                SaveSettings();
            }
        }

        public string ProfilesDirectory => Path.Combine(WorkingDirectory, "Profiles");

        private SfaProfile _currentProfile;

        public SfaProfile CreateNewProfile(string profileName)
        {
            try
            {
                profileName ??= "profile";

                var dir = FileHelpers.CreateUniqueDirectory(ProfilesDirectory,
                    FileHelpers.ReplaceInvalidFileNameChars(profileName, '_'));

                if (dir is null)
                    return null;

                var profile = new SfaProfile()
                {
                    ProfileDirectory = dir.FullName,
                    Database = Database,
                    MapsCache = MapsCache,
                    GameProfile = new SfaGameProfile
                    {
                        Nickname = profileName,
                        Id = Guid.NewGuid(),
                    }
                };

                profile?.Save();
                Profiles.Add(profile);
                return profile;
            }
            catch { }
            
            return null;
        }

        protected List<SfaProfile> LoadProfiles()
        {
            List<SfaProfile> result = new();

            try
            {
                DirectoryInfo[] directories =
                    GetOrCreateDirectory(ProfilesDirectory)
                    ?.GetDirectories();

                foreach (var dir in directories)
                {
                    var profile = new SfaProfile()
                    {
                        ProfileDirectory = dir.FullName,
                        Database = Database,
                        MapsCache = MapsCache,
                    };

                    if (profile.Load() == true)
                    {
                        result.Add(profile);
                    }
                }
            }
            catch { }

            return result;
        }

        public void DeleteProfile(SfaProfile profile)
        {

            Profiles?.Remove(profile);

            if (CurrentProfile == profile)
                CurrentProfile = null;

            profile.RemoveProfileFiles();
        }
    }
}
