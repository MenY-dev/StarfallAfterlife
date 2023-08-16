using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class DropTreeNode : ICloneable
    {
        [JsonPropertyName("type")]
        public int Type { get; set; } = -1;

        [JsonPropertyName("chance")]
        public float Chance { get; set; } = 0;

        [JsonPropertyName("weight")]
        public float Weight { get; set; } = 0;

        [JsonPropertyName("item_id")]
        public int ItemId { get; set; } = 0;

        [JsonPropertyName("item_type")]
        public int ItemType { get; set; } = 0;

        [JsonPropertyName("item_min")]
        public int ItemMin { get; set; } = 0;

        [JsonPropertyName("item_max")]
        public int ItemMax { get; set; } = 0;

        [JsonPropertyName("childs")]
        public List<DropTreeNode> Childs { get; set; }

        object ICloneable.Clone() => Clone();

        public DropTreeNode Clone()
        {
            var clone = MemberwiseClone() as DropTreeNode;
            clone.Childs = Childs?.Select(i => i?.Clone())?.ToList();
            return clone;
        }

        public IReadOnlyCollection<int> GetAllItems()
        {
            var items = new HashSet<int>();

            if (Type == 2)
            {
                items.Add(ItemId);
            }
            else
            {
                foreach (var item in Childs?
                    .Select(c => c.GetAllItems())
                    .Where(c => c is not null)
                    .SelectMany(c => c) ?? Enumerable.Empty<int>())
                    items.Add(item);
            }

            return items;
        }
    }
}
