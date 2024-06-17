using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public class DynamicMobDatabase
    {
        public DynamicMob this[int id] { get => Get(id); }

        public SortedList<int, DynamicMob> Mobs { get; } = new();

        private int _maxId = FleetIdInfo.DynamicMobsStartIndex;
        private Queue<int> _freeIds = new();
        private Dictionary<DynamicMobType, int> _mobStats = new(); 

        public DynamicMob Get(int id)
        {
            return Mobs.GetValueOrDefault(id);
        }

        public int GetStats(DynamicMobType type) => _mobStats.GetValueOrDefault(type);

        public bool Add(DynamicMob mob)
        {
            if (mob is null ||
                mob.Info is null)
                return false;

            if (Mobs?.ContainsKey(mob.Info.Id) == true)
                return true;

            Add(CreateNewId(), mob);
            return true;
        }

        protected void Add(int id, DynamicMob mob)
        {
            if (mob is null)
                return;

            (mob.Info ??= new()).Id = id;
            _maxId = Math.Max(id, _maxId);
            _mobStats[mob.Type] = _mobStats.GetValueOrDefault(mob.Type) + 1;
            Mobs[id] = mob;
        }

        protected int CreateNewId()
        {
            if (_freeIds.Count > 1000)
                return _freeIds.Dequeue();

            return _maxId + 1;
        }

        public bool Remove(int id)
        {
            if (Mobs.Remove(id, out var mob) == true)
            {
                _freeIds.Enqueue(id);

                if (mob is not null)
                    _mobStats[mob.Type] = Math.Max(0, _mobStats.GetValueOrDefault(mob.Type) - 1);

                return true;
            }

            return false;
        }
    }
}
