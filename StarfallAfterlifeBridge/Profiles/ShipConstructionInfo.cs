using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ShipConstructionInfo : ICloneable
    {

        [JsonPropertyName("elid")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("plid")]
        public int FleetId { get; set; } = 0;

        [JsonPropertyName("hull")]
        public int Hull { get; set; } = 0;

        [JsonPropertyName("xp")]
        public int Xp { get; set; } = 0;

        [JsonPropertyName("armor")]
        public float ArmorDelta { get; set; } = 0;

        [JsonPropertyName("structure")]
        public float StructureDelta { get; set; } = 0;

        [JsonPropertyName("detachment")]
        public int Detachment { get; set; } = 0;

        [JsonPropertyName("slot")]
        public int Slot { get; set; } = 0;

        [JsonPropertyName("ship_skin")]
        public int ShipSkin { get; set; } = 0;

        [JsonPropertyName("skin_color_1")]
        public int SkinColor1 { get; set; } = 0;

        [JsonPropertyName("skin_color_2")]
        public int SkinColor2 { get; set; } = 0;

        [JsonPropertyName("skin_color_3")]
        public int SkinColor3 { get; set; } = 0;

        [JsonPropertyName("shipdecal")]
        public int ShipDecal { get; set; } = 0;

        [JsonPropertyName("hplist")]
        public List<ShipHardpoint> HardpointList { get; set; } = new();

        [JsonPropertyName("progression")]
        public List<ShipProgression> Progression { get; set; } = new();

        [JsonPropertyName("cargo_hold_size")]
        public int CargoHoldSize { get; set; } = 0;

        [JsonPropertyName("cargo_list")]
        public InventoryStorage Cargo { get; set; } = new();

        object ICloneable.Clone() => Clone();

        public ShipConstructionInfo Clone()
        {
            var clone = MemberwiseClone() as ShipConstructionInfo;
            clone.HardpointList = HardpointList?.Select(i => i?.Clone())?.ToList();
            clone.Progression = Progression?.Select(i => i?.Clone())?.ToList();
            clone.Cargo = Cargo?.Clone();
            return clone;
        }
    }
}
