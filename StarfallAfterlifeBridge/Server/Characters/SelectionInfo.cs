using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public class SelectionInfo
    {
        public SystemHex Hex { get; set; }

        public int SystemId { get; set; }

        public List<ObjectSelectionInfo> Objects { get; } = new();

        public ObjectSelectionInfo GetObjectInfo(int id, DiscoveryObjectType type) =>
            Objects.FirstOrDefault(o =>
                o.Target is not null &&
                o.Target.Id == id&&
                o.Target.Type == type);

        public ObjectSelectionInfo GetObjectInfo(StarSystemObject obj) =>
            obj is null ? null : Objects.FirstOrDefault(o => o?.Target == obj);
    }
}
