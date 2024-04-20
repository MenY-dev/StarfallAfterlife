using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Realms
{
    public class RealmObjectNameReport
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("authors")]
        public List<RealmObjectReportAuthor> Authors { get; set; }
    }
}
