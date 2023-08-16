using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class DiscoveryMobShipData : ICloneable
    {
        [JsonPropertyName("Data")]
        public ShipConstructionInfo Data { get; set; } = new();

        [JsonPropertyName("ServiceData")]
        public ShipServiceInfo ServiceData { get; set; } = new();

        object ICloneable.Clone() => Clone();

        public DiscoveryMobShipData Clone()
        {
            var clone = MemberwiseClone() as DiscoveryMobShipData;
            clone.Data = Data?.Clone();
            clone.ServiceData = ServiceData?.Clone();
            return clone;
        }

        public bool IsBoss() => ServiceData?.Tags?.Contains("Mob.specialship.Boss", StringComparer.InvariantCultureIgnoreCase) == true;

        public bool IsElite() => ServiceData?.Tags?.Contains("Mob.specialship.Elite", StringComparer.InvariantCultureIgnoreCase) == true;

        public IReadOnlyCollection<int> GetDropItems()
        {
            var items = new HashSet<int>();

            if (ServiceData is ShipServiceInfo mobInfo)
            {
                if (Data is ShipConstructionInfo shipInfo &&
                    mobInfo.DropState == true &&
                    IsBoss() == false &&
                    IsElite() == false)
                {
                    foreach (var item in shipInfo.HardpointList?
                        .SelectMany(h => h?.EquipmentList ?? new())
                        .Where(e => e is not null)
                        .Select(e => e.Equipment) ?? Enumerable.Empty<int>())
                        items.Add(item);
                }

                foreach (var item in mobInfo.DropTree?
                    .GetAllItems() ?? Enumerable.Empty<int>())
                    items.Add(item);
            }

            return items;
        }
    }
}
