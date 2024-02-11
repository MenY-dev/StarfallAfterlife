using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class StarSystemObject : DiscoveryObject
    {
        public virtual StarSystem System { get; protected set; }

        public Faction Faction { get; set; } = Faction.None;

        public int FactionGroup { get; set; } = -1;

        public List<int> AvailableQuests { get; } = new();

        public SfaDatabase Database { get; set; }

        public override Vector2 Location
        {
            get => base.Location;
            set
            {
                if (base.Location != value)
                    hex = SystemHexMap.SystemPointToHex(value);

                base.Location = value;
            }
        }

        public virtual SystemHex Hex
        {
            get => hex;
            set
            {
                if (hex != value)
                {
                    hex = value;
                    base.Location = SystemHexMap.HexToSystemPoint(hex);
                }
            }
        }

        private SystemHex hex;

        protected override void OnParentObjectChanged(DiscoveryObject newParent, DiscoveryObject oldParent)
        {
            if (newParent is StarSystem system)
            {
                System = system;
                OnSystemChanged(system);
            }
            else
            {
                System = null;
            }
        }

        protected virtual void OnSystemChanged(StarSystem system)
        {

        }

        public override void Init()
        {
            base.Init();
            Database = System?.Galaxy.Database;
        }

        public override void Broadcast<TListener>(Action<TListener> action)
        {
            if (action is null)
                return;

            base.Broadcast(action, l => l is not IStarSystemListener);
            System?.Broadcast(action, l => l is IStarSystemListener);
        }

        public override void Broadcast<TListener>(Action<TListener> action, Func<TListener, bool> predicate)
        {
            if (action is null || predicate is null)
                return;

            base.Broadcast(action, l => l is not IStarSystemListener && predicate.Invoke(l));
            System?.Broadcast(action, l => l is IStarSystemListener && predicate.Invoke(l));
        }

        public bool IsSystemObjectEquals(StarSystemObject other)
        {
            if (other is null ||
                other.Type != Type ||
                other.Id != Id)
                return false;

            return true;
        }
    }
}
