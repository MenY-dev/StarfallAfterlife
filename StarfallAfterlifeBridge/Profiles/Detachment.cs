using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class Detachment
    {
        [JsonPropertyName("xp")]
        public virtual int Xp { get; set; } = 0;

        [JsonPropertyName("slots")]
        public virtual DetachmentSlots Slots { get; protected set; } = new();

        [JsonPropertyName("abilities")]
        public virtual DetachmentAbilities Abilities { get; set; } = new();

        public Detachment(params int[] slots)
        {
            Slots = new(slots);
        }

        [JsonConstructor()]
        public Detachment(int xp, DetachmentSlots slots, DetachmentAbilities abilities)
        {
            Xp = xp;
            Slots = slots;
            Abilities = abilities;
        }

        public void Update(Detachment newData)
        {
            if (newData is null || newData.Slots is null || newData.Abilities is null)
                return;

            Xp = newData.Xp;
            Abilities = newData.Abilities;

            foreach (var item in newData.Slots)
            {
                Slots[item.Key] = item.Value;
            }
        }
    }
}
