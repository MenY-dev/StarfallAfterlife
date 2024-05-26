using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public struct FleetIdInfo
    {
        public int LocalId;
        public FleetType Type;

        public const int UserStartIndex          = 0;
        public const int MobsStartIndex          = 1000000;
        public const int DynamicMobsStartIndex   = 1500000;
        public const int ServiceFleetStartIndex  = 2000000;
        public const int MaxIndex                = 2147482;

        public bool IsValid => Type switch
        {
            FleetType.User => LocalId >= UserStartIndex && LocalId < MobsStartIndex,
            FleetType.Mob => LocalId >= MobsStartIndex && LocalId < DynamicMobsStartIndex,
            FleetType.DynamicMob => LocalId >= DynamicMobsStartIndex && LocalId < ServiceFleetStartIndex,
            FleetType.Service => LocalId >= ServiceFleetStartIndex && LocalId < MaxIndex,
            _ => true,
        };
        
        public int ToId()
        {
            return Type switch
            {
                FleetType.User => UserStartIndex + LocalId,
                FleetType.Mob => MobsStartIndex + LocalId,
                FleetType.DynamicMob => DynamicMobsStartIndex + LocalId,
                FleetType.Service => ServiceFleetStartIndex + LocalId,
                _ => LocalId < 0 ? LocalId : MaxIndex + LocalId,
            };
        }

        public static FleetIdInfo Create(int id)
        {
            return id switch
            {
                < 0 => new() { Type = FleetType.None, LocalId = id },
                < MobsStartIndex => new() { Type = FleetType.User, LocalId = id - UserStartIndex },
                < DynamicMobsStartIndex => new() { Type = FleetType.Mob, LocalId = id - MobsStartIndex },
                < ServiceFleetStartIndex => new() { Type = FleetType.DynamicMob, LocalId = id - DynamicMobsStartIndex },
                < MaxIndex => new() { Type = FleetType.Service, LocalId = id - ServiceFleetStartIndex },
                _ => new() { Type = FleetType.None, LocalId = id - MaxIndex },
            };
        }

        public static FleetType GetType(int id) => Create(id).Type;

        public static bool IsUser(int id) => Create(id).Type == FleetType.User;

        public static bool IsMob(int id) => Create(id).Type == FleetType.Mob;

        public static bool IsDynamicMob(int id) => Create(id).Type == FleetType.DynamicMob;

        public static bool IsService(int id) => Create(id).Type == FleetType.Service;
    }
}
