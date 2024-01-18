using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Quests;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class UserFleet : DiscoveryFleet
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.UserFleet;

        private SystemHex _lastHex = SystemHex.Zero; 

        public SystemHexMap ExploreMap { get; } = new();

        protected override void UpdateLocation()
        {
            base.UpdateLocation();

            if (Hex != _lastHex)
            {
                OnHexChanged(Hex);
                _lastHex = Hex;
            }

            if (State == FleetState.InGalaxy &&
                Immortal == false &&
                Stealth == false &&
                DockObjectType == DiscoveryObjectType.None &&
                System.GetBattle(Hex) is StarSystemBattle battle &&
                battle.IsFinished == false &&
                battle.GetMember(this) is null)
            {
                battle.AddToBattle(this, BattleRole.Join, CreateHexOffset());
            }
        }

        protected virtual void OnHexChanged(SystemHex newHex)
        {
            bool isNebula = System?.NebulaMap?[newHex] ?? false;
            int vision = isNebula ? NebulaVision : Vision;
            Broadcast<IUserFleetListener>(l => l.OnSystemExplorationChanged(this, newHex, vision));
        }

        protected override void OnSystemChanged(StarSystem system)
        {
            base.OnSystemChanged(system);
            OnHexChanged(Hex);
        }

        public void LeaveFromGalaxy()
        {
            System?.RemoveFleet(this);
            Listeners?.Clear();
        }
    }
}
