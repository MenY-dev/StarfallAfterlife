using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public class HouseEffect
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; set; }
    }
}
