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
        public DiscoveryMobInfo this[int id] { get => Get(id); }

        public SortedList<int, DiscoveryMobInfo> Mobs { get; } = new();

        private int _maxId = FleetIdInfo.DynamicMobsStartIndex;
        private Queue<int> _freeIds = new();

        public DiscoveryMobInfo Get(int id)
        {
            return Mobs.GetValueOrDefault(id);
        }

        public void Add(DiscoveryMobInfo mob)
        {
            if (mob is null)
                return;

            Add(CreateNewId(), mob);
        }

        protected void Add(int id, DiscoveryMobInfo mob)
        {
            if (mob is null)
                return;

            mob.Id = id;
            _maxId = Math.Max(id, _maxId);
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
            if (Mobs.Remove(id) == true)
            {
                _freeIds.Enqueue(id);
                return true;
            }

            return false;
        }
    }
}
