using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class DetachmentSlots : ProfileDictionary<int, int>
    {
        public override int this[int key] { get => GetValue(key); set => SetValue(key, value, true); }

        public DetachmentSlots() { }

        public DetachmentSlots(params int[] slots)
        {
            if (slots is null)
                return;

            foreach (var item in slots)
                SetValue(item, -1, true);
        }

        public override void Add(int key, int value) => SetValue(key, value, false);

        public override bool Remove(int key)
        {
            SetValue(key, -1, false);
            return true;
        }
    }
}
