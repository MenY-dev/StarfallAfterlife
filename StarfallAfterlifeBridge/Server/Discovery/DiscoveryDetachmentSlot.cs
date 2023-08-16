using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryDetachmentSlot
    {
        private DiscoveryDetachment Detachment { get; }

        public int Id { get; }

        public DiscoveryShip Ship { get; protected set; }

        public DiscoveryDetachmentSlot(int id, DiscoveryDetachment detachment)
        {
            Detachment = detachment;
            Id = id;
        }

        public void SetShip(DiscoveryShip ship)
        {
            Ship = ship;
        }
    }
}
