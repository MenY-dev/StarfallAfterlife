using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class MobDataRequestEventArgs : EventArgs
    {
        public string InstanceAuth { get; }
        public int MobId { get; }
        public bool IsCustom { get; }
        public Faction Faction { get; }
        public string[] Tags { get; }

        public MobDataRequestEventArgs(string instanceAuth, int mobId)
        {
            InstanceAuth = instanceAuth;
            MobId = mobId;
        }

        public MobDataRequestEventArgs(string instanceAuth, int mobId, Faction faction, string[] tags)
        {
            IsCustom = true;
            InstanceAuth = instanceAuth;
            MobId = mobId;
            Faction = faction;
            Tags = tags;
        }
    }
}
