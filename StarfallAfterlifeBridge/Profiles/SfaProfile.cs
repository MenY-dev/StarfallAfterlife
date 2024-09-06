using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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

        public SfaProfileInfo Info { get; set; }

        public bool IsSupported { get; set; } = true;

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

        public string InfoLocation => Path.Combine(ProfileDirectory, "Info.json");

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
                Info = null;

                try
                {
                    if (File.Exists(InfoLocation) &&
                        File.ReadAllText(InfoLocation) is string infoJson &&
                        JsonSerializer.Deserialize<SfaProfileInfo>(infoJson) is SfaProfileInfo info)
                        Info = info;
                }
                catch { }

                if (Info is null)
                {
                    Info = new();
                    SaveInfo();
                }

                if (Info.Version > SfaProfileInfo.CurrentVersion)
                {
                    IsSupported = false;
                    return false;
                }

                IsSupported = true;

                try
                {
                    string text = File.ReadAllText(GameProfileLocation);
                    GameProfile = JsonSerializer.Deserialize<SfaGameProfile>(text);
                    ApplyGameProfileFixes();
                }
                catch { }

                try
                {
                    if (Directory.Exists(RealmsDirectory))
                    {
                        var realmsPaths = FileHelpers.GetDirectoriesSelf(RealmsDirectory);

                        foreach (var path in realmsPaths)
                        {
                            var realm = new SfaRealm();
                            realm.LoadInfo(path);
                            Realms.Add(new SfaRealmInfo() { Realm = realm, RealmDirectory = path });
                        }

                        if (Info?.LastRealm is string lastRealmId)
                            CurrentRealm = Realms.FirstOrDefault(r => r?.Realm?.Id == lastRealmId);
                    }
                }
                catch { }

                try
                {
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
                catch { }

                return true;
            }
            catch {}

            return false;
        }

        private void ApplyGameProfileFixes()
        {
            if (GameProfile is SfaGameProfile profile &&
                (Database ?? SfaDatabase.Instance) is SfaDatabase database)
            {
                foreach (var character in profile.DiscoveryModeProfile.Chars)
                {
                    if (character is null)
                        continue;

                    foreach (var item in character.Inventory?.ToList() ?? new())
                    {
                        if (database.GetItem(item.Id) is SfaItem blueprint &&
                            item.Type != blueprint.ItemType)
                        {
                            character.Inventory?.Remove(item, item.Count);
                            character.Inventory?.Add(new InventoryItem
                            {
                                Id = item.Id,
                                Type = blueprint.ItemType,
                                Count = item.Count,
                                UniqueData = item.UniqueData,
                                IGCPrice = item.IGCPrice,
                                BGCPrice = item.BGCPrice
                            }, item.Count);
                        }
                    }
                }
            }
        }

        public SfaRealmInfo AddNewRealm(SfaRealm realm)
        {
            string directory = CreateNewRealmDirectory(realm?.Name);

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
            (Info ??= new()).LastRealm = realm?.Realm?.Id;
            SaveInfo(true);
        }


        public void SaveGameProfile()
        {
            try
            {
                var doc = JsonHelpers.ParseNodeFromFileUnbuffered(GameProfileLocation)?
                    .AsObjectSelf() ?? new JsonObject();

                doc.Override(JsonHelpers.ParseNodeUnbuffered(GameProfile)?.AsObjectSelf());
                doc.WriteToFileUnbuffered(GameProfileLocation, new()
                {
                    WriteIndented = true,
                    TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver
                });
            }
            catch { }

            SaveInfo();
        }

        public void SaveCharacterProgress()
        {
            CurrentProgress?.Save();
            SaveInfo();
        }

        public void SaveInfo(bool writeNewVersion = true)
        {
            try
            {
                var info = Info ??= new();

                if (writeNewVersion == true)
                    info.Version = SfaProfileInfo.CurrentVersion;

                var path = InfoLocation;
                var doc = JsonHelpers.ParseNodeFromFileUnbuffered(path)?
                    .AsObjectSelf() ?? new JsonObject();

                doc.Override(JsonHelpers.ParseNodeUnbuffered(Info)?.AsObjectSelf());
                doc.WriteToFileUnbuffered(path, new()
                {
                    WriteIndented = true,
                    TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver
                });
            }
            catch { }
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

        protected string CreateNewRealmDirectory(string realmName)
        {
            realmName ??= "realm";

            var dir = FileHelpers.CreateUniqueDirectory(RealmsDirectory,
                FileHelpers.ReplaceInvalidFileNameChars(realmName, '_'));

            return dir.FullName;
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
            var rnd = new Random();
            int newId;

            do
            {
                newId = rnd.Next() + 1;
            } while (profile.Chars.Any(c => c.Id == newId));

            character.Id = newId;
            character.Guid = Guid.NewGuid();
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

                if (Sessions?.FirstOrDefault(s => s.CharacterId == charId) is DiscoverySession session)
                {
                    Sessions.Remove(session);
                    session.RemoveSessionFile();

                    if (CurrentSession ==  session)
                        CurrentSession = null;
                }
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

        public Character GetCharacter(int id)
        {
            return GameProfile?.DiscoveryModeProfile?.Chars?.FirstOrDefault(
                c => c.Id == id);
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
                    {
                        CurrentSession = session;
                        character.IsReadyToDropSession = true;
                    }
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

        public DiscoverySession[] GetSessions(string realmId)
        {
            if (realmId == null)
                return null;

            return Sessions?
                .Where(s => s.RealmId is not null && s.RealmId == realmId)
                .ToArray() ?? Array.Empty<DiscoverySession>();

        }

        public void FinishSession(DiscoverySession session, bool dropSession = false)
        {
            if (Sessions.Contains(session) == false)
                return;

            if (GetCharacter(session.CharacterId) is Character character)
            {
                if (CurrentSession == session)
                    CurrentSession = null;

                Sessions?.Remove(session);
                session.RemoveSessionFile();

                character.LastSession = session;
                character.HasSessionResults = true;
                
                if (dropSession == true)
                {
                    var database = Database ?? SfaDatabase.Instance;

                    foreach (var info in session.Ships)
                    {
                        if (character.GetShip(info?.Id ?? -1) is FleetShipInfo fleetShip &&
                            database.GetShip(fleetShip.Data?.Hull ?? -1) is ShipBlueprint blueprint)
                            fleetShip.TimeToRepair = blueprint.TimeToRepair;
                    }
                }
                else
                {
                    var newItems = session.Ships?
                        .Where(s => s?.Cargo is not null)
                        .SelectMany(s => s.Cargo)
                        .Where(i => i.IsEmpty == false)
                        .ToList() ?? new();

                    foreach (var item in newItems)
                    {
                        character.AddInventoryItem(item, item.Count);
                    }
                }

                SaveGameProfile();
            }
        }
    }
}
