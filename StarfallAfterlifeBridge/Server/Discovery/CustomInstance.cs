using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class CustomInstance
    {
        public int System { get; set; } = -1;
        public int Id { get; set; } = -1;
        public SystemHex Hex { get; set; }
        public byte Type { get; set; } = 0;

        public CustomInstance(int system, int id, byte type, SystemHex hex)
        {
            System = system;
            Id = id;
            Type = type;
            Hex = hex;
        }
    }
}
