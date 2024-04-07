using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.SfPackageLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    public static class SfCodexTypes
    {
        public struct Vector3
        {
            [JsonPropertyName("x")]
            public float X { get; set; }

            [JsonPropertyName("y")]
            public float Y { get; set; }

            [JsonPropertyName("z")]
            public float Z { get; set; }

        }

        public struct ItemDropInfo
        {
            [JsonPropertyName("item")]
            public int Item { get; set; }

            [JsonPropertyName("min")]
            public int Min { get; set; }

            [JsonPropertyName("max")]
            public int Max { get; set; }
        }

        public struct ItemCountInfo
        {
            [JsonPropertyName("item")]
            public int Item { get; set; }

            [JsonPropertyName("count")]
            public int Count { get; set; }
        }

        public struct HardpointComponent
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("type")]
            public TechType Type { get; set; }

            [JsonPropertyName("width")]
            public int Width { get; set; }

            [JsonPropertyName("height")]
            public int Height { get; set; }

            [JsonPropertyName("grid_x")]
            public int GridX { get; set; }

            [JsonPropertyName("grid_y")]
            public int GridY { get; set; }

            [JsonPropertyName("angle"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public float TurnAngle { get; set; }

            [JsonPropertyName("pos"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public Vector3 Position { get; set; }

            [JsonPropertyName("rot"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public Vector3 Rotation { get; set; }
        }

        public class DamageType
        {
            [JsonPropertyName("class")]
            public string Class { get; set; }

            [JsonPropertyName("xlsStructureDamageMultiplier")]
            public float StructureDamageMultiplier { get; set; } = 1;

            [JsonPropertyName("xlsArmorDamageMultiplier")]
            public float ArmorDamageMultiplier { get; set; } = 1;

            [JsonPropertyName("xlsShieldDamageMultiplier")]
            public float ShieldDamageMultiplier { get; set; } = 1;

            [JsonPropertyName("xlsArmorHitCrewDamageChance")]
            public float ArmorHitCrewDamageChance { get; set; } = 1;

            [JsonPropertyName("xlsArmorHitCrewDamageMultilier")]
            public float ArmorHitCrewDamageMultilier { get; set; } = 1;

            [JsonPropertyName("xlsStructureHitCrewDamageMultilier")]
            public float StructureHitCrewDamageMultilier { get; set; } = 1;

            [JsonPropertyName("xlsMaxStructureDamageMultiplier")]
            public float MaxStructureDamageMultiplier { get; set; } = 1;

            [JsonPropertyName("xlsMaxArmorDamageMultiplier")]
            public float MaxArmorDamageMultiplier { get; set; } = 1;

            [JsonPropertyName("xlsPenetrateArmor")]
            public bool PenetrateArmor { get; set; } = false;

            [JsonPropertyName("xlsIgnoreArmor")]
            public bool IgnoreArmor { get; set; } = false;

            [JsonPropertyName("xlsPenetrateShields")]
            public bool PenetrateShields { get; set; } = false;

            [JsonPropertyName("xlsIgnoreShields")]
            public bool IgnoreShields { get; set; } = false;

            [JsonPropertyName("IgnoreStasis")]
            public bool IgnoreStasis { get; set; } = false;

            public static DamageType Load(UObject obj)
            {
                var type = new DamageType();

                if (obj is null)
                    return type;

                type.StructureDamageMultiplier = obj.GetValue<float>("xlsStructureDamageMultiplier", 1);
                type.ArmorDamageMultiplier = obj.GetValue<float>("xlsArmorDamageMultiplier", 1);
                type.ShieldDamageMultiplier = obj.GetValue<float>("xlsShieldDamageMultiplier", 1);
                type.ArmorHitCrewDamageChance = obj.GetValue<float>("xlsArmorHitCrewDamageChance", 1);
                type.ArmorHitCrewDamageMultilier = obj.GetValue<float>("xlsArmorHitCrewDamageMultilier", 1);
                type.StructureHitCrewDamageMultilier = obj.GetValue<float>("xlsStructureHitCrewDamageMultilier", 1);
                type.MaxStructureDamageMultiplier = obj.GetValue<float>("xlsMaxStructureDamageMultiplier", 1);
                type.MaxArmorDamageMultiplier = obj.GetValue<float>("xlsMaxArmorDamageMultiplier", 1);
                type.PenetrateArmor = obj.GetValue<bool>("xlsPenetrateArmor", false);
                type.IgnoreArmor = obj.GetValue<bool>("xlsIgnoreArmor", false);
                type.PenetrateShields = obj.GetValue<bool>("xlsPenetrateShields", false);
                type.IgnoreShields = obj.GetValue<bool>("xlsIgnoreShields", false);
                type.IgnoreStasis = obj.GetValue<bool>("IgnoreStasis", false);

                return type;
            }

            public float ApplyToShild(float baseDamage) => baseDamage * ShieldDamageMultiplier;

            public float ApplyToArmor(float baseDamage) => baseDamage * ArmorDamageMultiplier;

            public float ApplyToStructure(float baseDamage) => baseDamage * StructureDamageMultiplier;

            public IEnumerable<string> GetDamageDetailsKeys()
            {
                if (PenetrateArmor == true)
                    yield return "PENETRATESARMOR";

                if (IgnoreArmor == true)
                    yield return "IGNORESARMOR";

                if (PenetrateShields == true)
                    yield return "PENETRATESSHIELD";

                if (IgnoreShields == true)
                    yield return "IGNORESSHIELD";
            }
        }
    }
}
