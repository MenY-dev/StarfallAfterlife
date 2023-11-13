using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class SecretObject : StarSystemObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.SecretObject;

        public SecretObjectType SecretType { get; set; } = SecretObjectType.None;

        public SystemHex[] PossibleLocations { get; set; } = new SystemHex[0];

        public SecretObject(SecretObjectInfo info, StarSystem system)
        {
            System = system;

            if (info is null)
                return;

            Hex = info.Hex;
            Id = info.Id;
            SecretType = info.Type;
            PossibleLocations = Hex
                .GetRing(1)
                .Where(h => h.GetSize() < 17 && System?.GetObjectAt(h, false) is null)
                .ToList()
                .Randomize(Id)
                .Take(3)
                .Concat(new[] { Hex })
                .ToArray();
        }
    }
}
