using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.SfPackageLoader;
using StarfallAfterlife.Bridge.SfPackageLoader.FileSysten;
using StarfallAfterlife.Bridge.SfPackageLoader.SfTypes;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static readonly int Version = 1;

        public Dictionary<int, SfCodexItem> Ships { get; set; }

        public Dictionary<int, SfCodexItem> Equipment { get; set; }

        public Dictionary<int, SfCodexItem> DiscoveryItems { get; set; }

        public Dictionary<string, SfCodexTypes.DamageType> DamageTypes { get; set; }

        public Dictionary<string, int> ClassToIdMap { get; set; }
        
        public List<SfLocalization> Localizations {  get; set; }

        public static SfCodex Load(string path)
        {
            try
            {
                return Load(JsonHelpers.ParseNodeFromFileUnbuffered(path));
            }
            catch { }

            return null;
        }


        public static SfCodex Load(JsonNode doc)
        {
            void AddItem(JsonNode node, Dictionary<int, SfCodexItem> collection, SfCodex codex)
            {
                try
                {
                    var itemFields = new Dictionary<string, object>();
                    var codexItem = new SfCodexItem()
                    {
                        Id = (int?)node["id"] ?? 0,
                        Class = (string)node["class"],
                        BaseClass = (string)node["base_class"],
                        Name = (string)node["name"],
                        NameKey = (string)node["name_key"],
                        DescriptionKey = (string)node["description_key"],
                        Fields = itemFields,
                    };

                    if (node["fields"]?.AsObjectSelf() is JsonObject nodeFields)
                    {
                        foreach (var field in nodeFields)
                        {
                            if (field.Key is string key &&
                                GetPropertyInfo(key) is SfCodexPropertyInfo info)
                            {
                                itemFields[key] = info.GetValue(field.Value);
                            }
                        }
                    }

                    collection[codexItem.Id] = codexItem;

                    if (codexItem.Class is not null)
                        codex.ClassToIdMap[codexItem.Class] = codexItem.Id;
                }
                catch { }
            }

            try
            {
                if (doc is not JsonObject ||
                    (int?)doc["version"] != SfCodex.Version)
                    return null;

                var codex = new SfCodex()
                {
                    Ships = new(),
                    Equipment = new(),
                    DiscoveryItems = new(),
                    DamageTypes = new(),
                    ClassToIdMap = new(),
                    Localizations = new(),
                };

                if (doc["ships"]?.AsArraySelf() is JsonArray ships)
                    foreach (var node in ships)
                        AddItem(node, codex.Ships, codex);

                if (doc["equipment"]?.AsArraySelf() is JsonArray equipment)
                    foreach (var node in equipment)
                        AddItem(node, codex.Equipment, codex);

                if (doc["discovery_items"]?.AsArraySelf() is JsonArray discoveryItems)
                    foreach (var node in discoveryItems)
                        AddItem(node, codex.DiscoveryItems, codex);

                if (doc["damage_types"]?.AsArraySelf() is JsonArray damageTypes)
                {
                    foreach (var node in damageTypes)
                    {
                        try
                        {
                            var item = JsonSerializer.Deserialize<SfCodexTypes.DamageType>(node);

                            if (item.Class is not null)
                                codex.DamageTypes[item.Class] = item;
                        }
                        catch { }
                    }
                }

                if (doc["localizations"]?.AsArraySelf() is JsonArray localizations)
                {
                    foreach (var node in localizations)
                    {
                        try
                        {
                            var item = JsonSerializer.Deserialize<SfLocalization>(node);

                            if (item is not null)
                                codex.Localizations.Add(item);
                        }
                        catch { }
                    }
                }

                return codex;
            }
            catch { }

            return null;
        }

        public string ToJsonString()
        {
            var doc = new JsonObject();
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true,
                TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver,
            };

            doc["version"] = Version;
            doc["ships"] = JsonSerializer.SerializeToNode(Ships?.Values);
            doc["equipment"] = JsonSerializer.SerializeToNode(Equipment?.Values);
            doc["discovery_items"] = JsonSerializer.SerializeToNode(DiscoveryItems?.Values);
            doc["damage_types"] = JsonSerializer.SerializeToNode(DamageTypes?.Values);
            doc["localizations"] = JsonSerializer.SerializeToNode(Localizations);

            return JsonSerializer.Serialize(doc, options);
        }

        public int? ClassToId(string key)
        {
            if (ClassToIdMap?.TryGetValue(key, out var id) == true)
                return id;

            return null;
        }

        public SfCodexItem GetItem(int id) =>
            Ships?.GetValueOrDefault(id) ??
            Equipment?.GetValueOrDefault(id) ??
            DiscoveryItems?.GetValueOrDefault(id);

        public string GetText(string key, string localization = null, string tag = null)
        {
            var loc = GetLocalization(localization) ?? Localizations?.FirstOrDefault();
            return loc?.GetText(key, tag);
        }

        public SfLocalization GetLocalization(string key)
        {
            return Localizations?.FirstOrDefault(l =>
                l.Code == key || l.Code?.Equals(key, StringComparison.OrdinalIgnoreCase) == true);
        }

        public SfCodexTypes.DamageType GetDamageType(string className) =>
            className is null ? null : DamageTypes?.GetValueOrDefault(className);
    }
}
