using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Inventory
{
    public class CargoTransaction
    {
        public static int RemoveItemFromFleet(ServerCharacter character, int itemId, int count, string uniqueData = null)
        {
            var removedItems = 0;

            if (character.Ships is List<ShipConstructionInfo> ships &&
                count > 0)
            {
                foreach (var cargo in
                    from ship in ships
                    where ship != null && ship.Cargo != null
                    select ship.Cargo)
                {
                    if (cargo[itemId, uniqueData]?.Count is int availableItems &&
                        availableItems > 0)
                    {
                        removedItems += cargo.Remove(itemId, count - removedItems, uniqueData);

                        if (removedItems >= count)
                            return removedItems;
                    }
                }
            }

            return removedItems;
        }
    }
}
