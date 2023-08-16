using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using static StarfallAfterlife.Bridge.Native.Windows.Win32;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class Planet : DockableObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.Planet;

        public string Name { get; set; }

        public PlanetType PlanetType { get; set; }

        public int Size { get; set; }

        public float Temperature { get; set; }

        public float Atmosphere { get; set; }

        public float Gravitation { get; set; }

        public float NoubleGases { get; set; }

        public float RadiactiveMetals { get; set; }

        public float SuperConductors { get; set; }

        public int Circle { get; set; }

        public List<int> SecretLocations { get; set; } = new List<int>() { 0 };

        public Planet() { }

        public Planet(int id)
        {
            Id = id;
        }


        public override void Init()
        {
            base.Init();

            if (System?.Info is GalaxyMapStarSystem system &&
                system.Planets.FirstOrDefault(i => i.Id == Id) is GalaxyMapPlanet info)
            {
                Name = info.Name;
                Faction = info.Faction;
                PlanetType = info.Type;
                Size = info.Size;
                Hex = info.Hex;
                Temperature = info.Temperature;
                Atmosphere = info.Atmosphere;
                Gravitation = info.Gravitation;
                NoubleGases = info.NoubleGases;
                RadiactiveMetals = info.RadiactiveMetals;
                SuperConductors = info.SuperConductors;
                SecretLocations = info.SecretLocations;
                Circle = system?.Level ?? -1;
            }

            GeneratePlanetStorage(Id);
            //GeneratePlanetTaskBoard(Id);
            LoadPlanetTaskBoard();
        }

        private void LoadPlanetTaskBoard()
        {
            var tasks = Galaxy?.Realm?.QuestsDatabase.GetTaskBoardQuests((byte)Type, Id);

            if (tasks is null)
                return;

            foreach (var item in tasks)
            {
                TaskBoard.Add(CreateTaskBoardItem(item.Id, item.LogicId, new QuestReward
                {
                    IGC = 20000,
                    Xp = 10000,
                    Items = new List<QuestItemInfo>
                    {
                        new QuestItemInfo { Id = 1550293210, Type = 1, Count = 6 },
                    },
                }));
            }
        }

        private void GeneratePlanetTaskBoard(int seed)
        {
            TaskBoard.Clear();

            if (Faction is not Faction.Vanguard or Faction.Deprived or Faction.Eclipse)
                return;

            int startId = Id * 100;

            TaskBoard.Add(CreateTaskBoardItem(startId + 0, 373947261, new QuestReward
            {
                IGC = 20000,
                Xp = 10000,
                Items = new List<QuestItemInfo>
                {
                    new QuestItemInfo { Id = 1550293210, Type = 1, Count = 2 },
                    new QuestItemInfo { Id = 1517501299, Type = 1, Count = 2 },
                },
            }));

            TaskBoard.Add(CreateTaskBoardItem(startId + 1, 1238655910, new QuestReward
            {
                IGC = 20000,
                Xp = 10000,
                Items = new List<QuestItemInfo>
                {
                    new QuestItemInfo { Id = 1550293210, Type = 1, Count = 6 },
                },
            }));

            foreach (var item in TaskBoard)
                AvailableQuests.Add(item.QuestId);
        }

        protected virtual TaskBoardEntry CreateTaskBoardItem(int itemId, int questId, QuestReward reward)
        {
            if (Database?.QuestsLogics.TryGetValue(questId, out var exploreLogic) == true)
            {
                return new TaskBoardEntry
                {
                    QuestId = itemId,
                    LogicId = exploreLogic.Id,
                    Logic = exploreLogic,
                    Owner = this,
                    Reward = reward,
                };
            }

            return null;
        }

        protected virtual void GeneratePlanetStorage(int seed)
        {
            Storages.Clear();

            if (Faction == Faction.None)
                return;

            Random rnd = new Random(seed);

            if (rnd.Next(0, 3) == 0)
                AddShopStorage(GenerateWeaponStorage(seed));

            if (rnd.Next(0, 3) == 0)
                AddShopStorage(GenerateEngineeringStorage(seed, false));

            if (rnd.Next(0, 3) == 0)
                AddShopStorage(GenerateEngineeringStorage(seed, true));

            if (rnd.Next(0, 3) == 0)
                AddShopStorage(GenerateJunkyardStorage(seed));

            if (rnd.Next(0, 3) == 0)
                AddShopStorage(GenerateResourcesStorage(seed));
        }

        protected void AddShopStorage(ObjectStorage storage)
        {
            if (storage is not null && storage.Count > 0)
                Storages.Add(storage);
        }

        protected virtual ObjectStorage GenerateWeaponStorage(int seed)
        {
            var rnd = new Random(seed);
            var storage = new ObjectStorage(this, "s_weapon", StorageType.Shop);

            if (Database.CircleDatabase.TryGetValue(Circle, out var dtb))
            {
                foreach (var item in dtb.Equipments.Values)
                {
                    if (TechType.Weapons.HasFlag(item.TechType) == false ||
                        item.IsDefective == true ||
                        item.IsImproved == true)
                        continue;

                    storage.Add(item, 999);
                }
            }

            return storage;
        }

        protected virtual ObjectStorage GenerateJunkyardStorage(int seed)
        {
            var rnd = new Random(seed);
            var storage = new ObjectStorage(this, "s_junkie", StorageType.Shop, true);

            if (Database.CircleDatabase.TryGetValue(Circle, out var dtb))
            {
                foreach (var item in dtb.Equipments.Values)
                {
                    if (item.IsDefective == false)
                        continue;

                    storage.Add(item, 999);
                }
            }

            return storage;
        }

        protected virtual ObjectStorage GenerateEngineeringStorage(int seed, bool hightTech = false)
        {
            var rnd = new Random(seed);
            var storage = new ObjectStorage(
                this, hightTech == true ? "s_engineering_lab" : "s_engineering_department", StorageType.Shop, true);

            if (Database.CircleDatabase.TryGetValue(Circle, out var dtb))
            {
                foreach (var item in dtb.Equipments.Values)
                {
                    if (TechType.Weapons.HasFlag(item.TechType) == true ||
                        item.TechType == TechType.Ship ||
                        item.IsDefective == true ||
                        item.IsImproved == true ||
                        item.GalaxyValue < 1)
                        continue;

                    if (hightTech == false && item.TechLvl > 2 ||
                        hightTech == true && item.TechLvl < 3)
                        continue;

                    storage.Add(item, 999);
                }
            }

            return storage;
        }

        private ObjectStorage GenerateResourcesStorage(int seed)
        {
            var rnd = new Random(seed);
            var storage = new ObjectStorage(this, "s_resources", StorageType.Shop, true);

            if (Database.CircleDatabase.TryGetValue(Circle, out var dtb))
            {
                foreach (var item in dtb.DiscoveryItems.Values)
                {
                    if (item.ItemType != InventoryItemType.DiscoveryItem ||
                        item.Tags.Contains("Item.Role.Ore") == false)
                        continue;

                    storage.Add(item, 999);
                }
            }

            return storage;
        }
    }
}
