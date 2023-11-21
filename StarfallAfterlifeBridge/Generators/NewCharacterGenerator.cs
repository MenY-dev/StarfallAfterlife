using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class NewCharacterGenerator : GenerationTask
    {
        protected override bool Generate()
        {
            return true;
        }

        public Character CreateCharacter(string name, Faction faction)
        {
            var detachment = 1732028966;
            var character = new Character()
            {
                Faction = (int)faction,
                Name = name,
                IGC = 10000,
                BGC = 100,
                AccessLevel = 1,
                Level = 0,
                ProductionPoints = 3000,
                CurrentDetachment = detachment,
                Ships = CreateShips(faction)
            };

            character.Detachments[detachment].Slots[167313036] = character.Ships.ElementAtOrDefault(0)?.Id ?? 0;
            character.Detachments[detachment].Slots[167313034] = character.Ships.ElementAtOrDefault(1)?.Id ?? 0;

            ResolveExploration(character);

            return character;
        }

        protected void ResolveExploration(Character character)
        {
            character.ProjectResearch = character.Ships
                .SelectMany(s => s.Data.HardpointList)
                .SelectMany(h => h.EquipmentList)
                .GroupBy(eq => eq.Equipment)
                .Select(i => new ResearchInfo() { Entity = i.Key, IsOpened = 1, Xp = 100 })
                .ToList();
        }

        public List<FleetShipInfo> CreateShips(Faction faction)
        {
            var ships = new List<FleetShipInfo>();

            if (faction is Faction.Deprived)
            {
                ships.Add(new()
                {
                    Id = 1,
                    Data = new()
                    {
                        Id = 1,
                        Hull = 889729523,
                        HardpointList = new()
                        {
                            new() { Hardpoint = "EngineeringBay", EquipmentList = new()
                            {
                                new() { X = 0, Y = 1, Equipment = 1239432978 },
                                new() { X = 1, Y = 0, Equipment = 1239432978 },
                                new() { X = 1, Y = 1, Equipment = 1239432978 },
                                new() { X = 0, Y = 0, Equipment = 1239432978 },
                            }},
                            new() { Hardpoint = "EngineeringBay2", EquipmentList = new()
                            {
                                new() { X = 0, Y = 1, Equipment = 838934763 },
                                new() { X = 0, Y = 0, Equipment = 232644897 },
                                new() { X = 1, Y = 1, Equipment = 838934763 },
                                new() { X = 1, Y = 0, Equipment = 232644897 },
                            }},
                            new() { Hardpoint = "EngineeringBay3", EquipmentList = new()
                            {
                                new() { X = 1, Y = 0, Equipment = 1439388296 },
                            }},
                            new() { Hardpoint = "FrontBeam", EquipmentList = new()
                            {
                                new() { X = 0, Y = 1, Equipment = 251198060 },
                                new() { X = 1, Y = 0, Equipment = 251198060 },
                                new() { X = 1, Y = 1, Equipment = 251198060 },
                                new() { X = 0, Y = 0, Equipment = 251198060 },
                            }},
                            new() { Hardpoint = "LeftBeams", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 251198060 },
                                new() { X = 1, Y = 0, Equipment = 251198060 },
                                new() { X = 2, Y = 0, Equipment = 251198060 },
                            }},
                            new() { Hardpoint = "RightBeams", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 251198060 },
                                new() { X = 1, Y = 0, Equipment = 251198060 },
                                new() { X = 2, Y = 0, Equipment = 251198060 },
                            }},
                        },
                    },
                });


                ships.Add(new()
                {
                    Id = 2,
                    Data = new()
                    {
                        Id = 2,
                        Hull = 1021913978,
                        HardpointList = new()
                        {
                            new() { Hardpoint = "EngeneeringHP", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 1281308446 },
                                new() { X = 0, Y = 2, Equipment = 232644897 },
                                new() { X = 1, Y = 2, Equipment = 838934763 },
                            }},
                            new() { Hardpoint = "Right Rockets", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 308815052 },
                                new() { X = 0, Y = 2, Equipment = 1778316309 },
                                new() { X = 0, Y = 1, Equipment = 308815052 },
                            }},
                            new() { Hardpoint = "Left Rockets", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 308815052 },
                                new() { X = 0, Y = 2, Equipment = 1778316309 },
                                new() { X = 0, Y = 1, Equipment = 308815052 },
                            }},
                            new() { Hardpoint = "CenterRockets", EquipmentList = new()
                            {
                                new() { X = 0, Y = 1, Equipment = 308815052 },
                                new() { X = 0, Y = 0, Equipment = 308815052 },
                            }},
                        },
                    },
                });
            }
            else if (faction is Faction.Eclipse)
            {
                ships.Add(new()
                {
                    Id = 1,
                    Data = new()
                    {
                        Id = 1,
                        Hull = 2075552304,
                        HardpointList = new()
                        {
                            new() { Hardpoint = "Enginering", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 1778316309 },
                                new() { X = 0, Y = 1, Equipment = 1778316309 },
                                new() { X = 2, Y = 0, Equipment = 232644897 },
                                new() { X = 2, Y = 1, Equipment = 232644897 },
                            }},
                            new() { Hardpoint = "Enginering1", EquipmentList = new()
                            {
                                new() { X = 1, Y = 0, Equipment = 1439388296 },
                                new() { X = 0, Y = 0, Equipment = 232644897 },
                                new() { X = 0, Y = 1, Equipment = 232644897 },
                            }},
                            new() { Hardpoint = "LeftBeams", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 251198060 },
                                new() { X = 2, Y = 0, Equipment = 251198060 },
                                new() { X = 1, Y = 1, Equipment = 251198060 },
                                new() { X = 2, Y = 1, Equipment = 251198060 },
                                new() { X = 1, Y = 0, Equipment = 251198060 },
                                new() { X = 0, Y = 1, Equipment = 251198060 },
                            }},
                            new() { Hardpoint = "RightBeams", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 251198060 },
                                new() { X = 2, Y = 0, Equipment = 251198060 },
                                new() { X = 1, Y = 1, Equipment = 251198060 },
                                new() { X = 2, Y = 1, Equipment = 251198060 },
                                new() { X = 1, Y = 0, Equipment = 251198060 },
                                new() { X = 0, Y = 1, Equipment = 251198060 },
                            }},
                        },
                    },
                });


                ships.Add(new()
                {
                    Id = 2,
                    Data = new()
                    {
                        Id = 2,
                        Hull = 1490694361,
                        HardpointList = new()
                        {
                            new() { Hardpoint = "EngineeringBay1", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 1281308446 },
                            }},
                            new() { Hardpoint = "EngineeringBay2", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 1239432978 },
                                new() { X = 1, Y = 0, Equipment = 1778316309 },
                                new() { X = 3, Y = 0, Equipment = 1239432978 },
                            }},
                            new() { Hardpoint = "EngineeringBay3", EquipmentList = new()
                            {
                                new() { X = 1, Y = 1, Equipment = 838934763 },
                                new() { X = 0, Y = 0, Equipment = 232644897 },
                                new() { X = 0, Y = 1, Equipment = 838934763 },
                                new() { X = 1, Y = 0, Equipment = 232644897 },
                            }},
                            new() { Hardpoint = "FrontRightCannons", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 232046526 },
                                new() { X = 0, Y = 1, Equipment = 232046526 },
                                new() { X = 0, Y = 2, Equipment = 232046526 },
                                new() { X = 0, Y = 3, Equipment = 232046526 },
                            }},
                            new() { Hardpoint = "FrontLeftCannons", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 232046526 },
                                new() { X = 0, Y = 1, Equipment = 232046526 },
                                new() { X = 0, Y = 2, Equipment = 232046526 },
                                new() { X = 0, Y = 3, Equipment = 232046526 },
                            }},
                            new() { Hardpoint = "FrontCannons", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 232046526 },
                                new() { X = 0, Y = 1, Equipment = 232046526 },
                                new() { X = 0, Y = 2, Equipment = 232046526 },
                                new() { X = 1, Y = 0, Equipment = 232046526 },
                                new() { X = 1, Y = 1, Equipment = 232046526 },
                                new() { X = 1, Y = 2, Equipment = 232046526 },
                            }},
                        },
                    },
                });
            }
            else if (faction is Faction.Vanguard)
            {
                ships.Add(new()
                {
                    Id = 1,
                    Data = new()
                    {
                        Id = 1,
                        Hull = 161972268,
                        HardpointList = new()
                        {
                            new() { Hardpoint = "LeftRockets", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 308815052 },
                                new() { X = 0, Y = 1, Equipment = 308815052 },
                            }},
                            new() { Hardpoint = "RightRockets", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 308815052 },
                                new() { X = 0, Y = 1, Equipment = 308815052 },
                            }},
                            new() { Hardpoint = "CenterRockets", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 308815052 },
                                new() { X = 0, Y = 1, Equipment = 308815052 },
                            }},
                            new() { Hardpoint = "EngineeringBay1", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 1439388296 },
                            }},
                            new() { Hardpoint = "EngineeringBay2", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 1778316309 },
                                new() { X = 0, Y = 1, Equipment = 232644897 },
                                new() { X = 0, Y = 2, Equipment = 232644897 },
                                new() { X = 1, Y = 1, Equipment = 232644897 },
                                new() { X = 1, Y = 2, Equipment = 232644897 },
                            }},
                        },
                    },
                });


                ships.Add(new()
                {
                    Id = 2,
                    Data = new()
                    {
                        Id = 2,
                        Hull = 12115151,
                        HardpointList = new()
                        {
                            new() { Hardpoint = "EngineeringBay", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 838934763 },
                                new() { X = 0, Y = 1, Equipment = 838934763 },
                                new() { X = 1, Y = 0, Equipment = 232644897 },
                                new() { X = 1, Y = 1, Equipment = 232644897 },
                                new() { X = 2, Y = 0, Equipment = 1239432978 },
                                new() { X = 2, Y = 1, Equipment = 1239432978 },
                            }},
                            new() { Hardpoint = "EngineeringBay1", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 1281308446 },
                            }},
                            new() { Hardpoint = "LeftTurretHardpoint", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 232046526 },
                                new() { X = 0, Y = 1, Equipment = 232046526 },
                                new() { X = 0, Y = 2, Equipment = 232046526 },
                                new() { X = 1, Y = 0, Equipment = 232046526 },
                                new() { X = 1, Y = 1, Equipment = 232046526 },
                                new() { X = 1, Y = 2, Equipment = 232046526 },
                            }},
                            new() { Hardpoint = "RightTurretHardpoint", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 232046526 },
                                new() { X = 0, Y = 1, Equipment = 232046526 },
                                new() { X = 0, Y = 2, Equipment = 232046526 },
                                new() { X = 1, Y = 0, Equipment = 232046526 },
                                new() { X = 1, Y = 1, Equipment = 232046526 },
                                new() { X = 1, Y = 2, Equipment = 232046526 },
                            }},
                            new() { Hardpoint = "CenterTurret", EquipmentList = new()
                            {
                                new() { X = 0, Y = 0, Equipment = 232046526 },
                                new() { X = 0, Y = 1, Equipment = 232046526 },
                                new() { X = 0, Y = 2, Equipment = 232046526 },
                                new() { X = 1, Y = 0, Equipment = 232046526 },
                                new() { X = 1, Y = 1, Equipment = 232046526 },
                                new() { X = 1, Y = 2, Equipment = 232046526 },
                            }},
                        },
                    },
                });
            }

            return ships;
        }
    }
}
