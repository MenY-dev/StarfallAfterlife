using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class SfaProfile
    {
        public SfaGameProfile GameProfile { get; set; }

        public List<SfaRealmInfo> Realms { get; } = new();

        public List<DiscoverySession> Sessions{ get; } = new();

        public SfaRealmInfo CurrentRealm { get; set; }

        public CharacterProgress CurrentProgress { get; set; }

        public DiscoverySession CurrentSession { get; set; }

        public SfaDatabase Database { get; set; }

        public GalaxyMapCache MapsCache
        {
            get
            {
                lock (Locker)
                {
                    if (_mapsCache is null)
                    {
                        _mapsCache = new GalaxyMapCache();
                        _mapsCache.Location = Path.Combine(ProfileDirectory, "Maps");
                        _mapsCache.Init();
                    }

                    return _mapsCache;
                }
            }
            set
            {
                lock (Locker)
                    _mapsCache = value;
            }
        }

        public string ProfileDirectory { get; set; }

        public string GameProfileLocation => Path.Combine(ProfileDirectory, "Profile.json");

        public string RealmsDirectory => Path.Combine(ProfileDirectory, "Realms");

        public string SessionsDirectory => Path.Combine(ProfileDirectory, "Sessions");

        protected object Locker { get; } = new object();

        private GalaxyMapCache _mapsCache;

        public bool Load()
        {
            if (Directory.Exists(ProfileDirectory) == false)
                return false;

            try
            {
                Realms.Clear();
                Sessions.Clear();

                string text = File.ReadAllText(GameProfileLocation);
                GameProfile = JsonSerializer.Deserialize<SfaGameProfile>(text);

                if (Directory.Exists(RealmsDirectory))
                {
                    var realmsPaths = Directory.GetDirectories(RealmsDirectory);

                    foreach (var path in realmsPaths)
                    {
                        var realm = new SfaRealm();
                        realm.LoadInfo(path);
                        Realms.Add(new SfaRealmInfo() { Realm = realm, RealmDirectory = path });
                    }
                }

                if (Directory.Exists(SessionsDirectory))
                {
                    var sessionsPaths = Directory.GetFiles(SessionsDirectory);

                    foreach (var path in sessionsPaths)
                    {
                        var session = new DiscoverySession() { Path = path };

                        if (session.Load() == true)
                            Sessions.Add(session);
                    }
                }

            }
            catch {}

            return true;
        }

        public SfaRealmInfo AddNewRealm(SfaRealm realm)
        {
            string directory = CreateNewRealmDirectory();

            if (directory is null)
                return null;

            var newRealm = new SfaRealmInfo()
            {
                Realm = realm,
                RealmDirectory = directory,
            };

            Realms.Add(newRealm);
            return newRealm;
        }

        public SfaRealmInfo GetProfileRealm(string realmId) =>
            Realms.FirstOrDefault(r => r?.Realm is not null && r.Realm.Id == realmId);


        public SfaRealmInfo GetProfileRealm(SfaRealm realm) =>
            Realms.FirstOrDefault(r =>
                r?.Realm is not null &&
                (r.Realm == realm || r.Realm.Id == realm?.Id));

        public void SelectRealm(SfaRealmInfo realm)
        {
            CurrentRealm = realm;
        }


        public void SaveGameProfile()
        {
            try
            {
                if (Directory.Exists(ProfileDirectory) == false)
                    Directory.CreateDirectory(ProfileDirectory);

                if (GameProfile is not null)
                {
                    var gameProfileText = JsonSerializer.Serialize(GameProfile);
                    File.WriteAllText(GameProfileLocation, gameProfileText);
                }
            }
            catch {}
        }

        public void SaveCharacterProgress()
        {
            CurrentProgress?.Save();
        }

        public void Save()
        {
            try
            {
                SaveGameProfile();
                SaveCharacterProgress();

                foreach (var item in Realms)
                    item?.Save();

                foreach (var item in Sessions)
                    item?.Save();
            }
            catch { }
        }

        public void Use(Action<SfaProfile> action)
        {
            lock (Locker)
            {
                action.Invoke(this);
            }
        }

        public void Use<TState>(Action<SfaProfile, TState> action, TState state)
        {
            lock (Locker)
            {
                action.Invoke(this, state);
            }
        }

        protected string CreateNewRealmDirectory()
        {
            string root = RealmsDirectory;

            if (Directory.Exists(root) == false)
                Directory.CreateDirectory(root);

            for (int i = 0; i < short.MaxValue; i++)
            {
                string newDir = Path.Combine(root, $"realm{i}");

                if (Directory.Exists(newDir) == false)
                    return newDir;
            }

            return null;
        }

        public Character CreateNewCharacter(string name, Faction faction)
        {
            GameProfile ??= new();

            var profile = GameProfile.DiscoveryModeProfile ??= new();
            profile.Chars ??= new();

            if (profile.Chars.FirstOrDefault(c => c.Name == name) is not null ||
                faction is not (Faction.Deprived or Faction.Eclipse or Faction.Vanguard))
                return null;

            var character = new NewCharacterGenerator().CreateCharacter(name, faction);

            for (int i = 0; i < profile.Chars.Count + 1; i++)
            {
                if (profile.Chars.Any(c => c.Id == i) == false)
                {
                    character.Id = i;
                    break;
                }
            }

            profile.Chars.Add(character);
            return character;
        }

        public void RemoveCharacter(Character character)
        {
            if (GameProfile?.DiscoveryModeProfile?.Chars is List<Character> chars &&
                chars.Contains(character) == true)
            {
                int charId = character.Id;

                chars.Remove(character);
                CurrentRealm?.RemoveProgress(charId);

                foreach (var realm in Realms)
                    realm?.RemoveProgress(charId);
            }
        }

        public void RemoveProfileFiles()
        {
            try
            {
                if (Directory.Exists(ProfileDirectory) == true)
                    Directory.Delete(ProfileDirectory, true);
            }
            catch { }
        }

        public void SelectCharacter(Character character)
        {
            if (GameProfile is SfaGameProfile profile &&
                profile.DiscoveryModeProfile?.Chars?.Contains(character) == true)
            {
                profile.CurrentCharacter = character;

                if (CurrentRealm?.Progress is List<CharacterProgress> characters)
                {
                    var charProgress = characters.FirstOrDefault(p => p.CharacterId == character.Id);

                    if (charProgress is null)
                    {
                        charProgress = CurrentRealm.CreateProgress(character.Id);
                        characters.Add(charProgress);
                        charProgress.Save();
                    }

                    CurrentProgress = charProgress;

                    if (CurrentRealm?.Realm?.Id is string realmId &&
                        Sessions.FirstOrDefault(s => s?.CharacterId == character.Id && s.RealmId == realmId) is DiscoverySession session &&
                        session.Ships is not null and { Count: > 0 })
                        CurrentSession = session;
                    else
                        CurrentSession = null;
                }
            }
        }

        public DiscoverySession StartNewSessionForCurrentChar()
        {
            if (GameProfile?.CurrentCharacter is Character character &&
                CurrentRealm?.Realm is SfaRealm realm)
            {
                try
                {
                    foreach (var item in Sessions)
                    {
                        if (item.CharacterId == character.Id)
                            item.RemoveSessionFile();

                    }

                    Sessions.RemoveAll(s => s.CharacterId == character.Id);

                    var session = new DiscoverySession
                    {
                        Path = Path.Combine(SessionsDirectory, $"char_{character.Id}_session.json"),
                        RealmId = realm.Id,
                        CharacterId = character.Id,
                        SessionStartIGC = character.IGC,
                        SessionStartBGC = character.BGC,
                        SessionStartXp = character.Xp,
                        SessionStartAccessLevel = character.AccessLevel,
                        SessionStartPP = character.ProductionPoints,
                        SessionStartTime = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        SessionStartInventory = character.Inventory?
                            .Where(i => i.IsEmpty == false)
                            .Select(i => i.Clone())
                            .ToList() ?? new(),
                        StartShipsXps = new(character.Ships?
                            .DistinctBy(s => s.Id)
                            .Select(s => KeyValuePair.Create(s.Id, s.Xp))
                            .ToList() ?? new()),
                        StartHullXps = new(character.Ships?
                            .DistinctBy(s => s.Id)
                            .Select(s => KeyValuePair.Create(s.Id, s.Level))
                            .ToList() ?? new()),
                        StartSeasonsProgress = new(CurrentProgress?.SeasonsProgress ?? new()),
                        StartSeasonsRewards = new(CurrentProgress?.SeasonsRewards ?? new()),
                    };

                    if (character.Detachments is CharacterDetachments allDetachments &&
                        allDetachments.FirstOrDefault(d => d.Key == character.CurrentDetachment) is var detachment &&
                        detachment.Value?.Slots is DetachmentSlots slots)
                    {
                        (session.Ships ??= new()).AddRange(slots.Select(slot =>
                        {
                            var ship = character.GetShip(slot.Value)?.Data?.Clone();

                            if (ship is not null)
                            {
                                ship.Detachment = detachment.Key;
                                ship.Slot = slot.Key;
                            }

                            return ship;
                        }).Where(s => s is not null));
                    }

                    session.Save();
                    Sessions.Add(session);
                    CurrentSession = session;
                    return session;
                }
                catch { }
            }

            return null;
        }
    }
}
