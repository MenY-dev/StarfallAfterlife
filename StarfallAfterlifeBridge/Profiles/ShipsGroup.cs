using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ShipsGroup
    {
        [JsonPropertyName("group_num")]
        public int Number { get; set; } = 0;

        [JsonPropertyName("lock_formation")]
        public int LockFormation { get; set; } = 0;

        [JsonPropertyName("ships")]
        public List<FormationShip> Ships { get; set; } = new();
    }
}
