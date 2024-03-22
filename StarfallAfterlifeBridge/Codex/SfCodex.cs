using StarfallAfterlife.Bridge.SfPackageLoader;
using StarfallAfterlife.Bridge.SfPackageLoader.FileSysten;
using StarfallAfterlife.Bridge.SfPackageLoader.SfTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Codex
{
    public partial class SfCodex
    {
        public Dictionary<int, SfCodexItem> Ships { get; set; }

        public Dictionary<int, SfCodexItem> Equipment { get; set; }

        public Dictionary<int, SfCodexItem> DiscoveryItems { get; set; }

        public Dictionary<string, int> ClassToIdMap { get; set; }

        public List<SfLocalization> Localizations {  get; set; }

        public static SfCodex Load(string path)
        {

            return default;
        }

        public string ToJsonString()
        {
            var doc = new JsonObject();
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true,
            };

            doc["ships"] = JsonSerializer.SerializeToNode(Ships.Values);
            doc["equipment"] = JsonSerializer.SerializeToNode(Equipment.Values);
            doc["discovery_items"] = JsonSerializer.SerializeToNode(DiscoveryItems.Values);
            doc["localizations"] = JsonSerializer.SerializeToNode(Localizations);

            return JsonSerializer.Serialize(doc, options);
        }

        public int? ClassToId(string key)
        {
            if (ClassToIdMap?.TryGetValue(key, out var id) == true)
                return id;

            return null;
        }
    }
}
