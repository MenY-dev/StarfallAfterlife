using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class BGShopGenerator : GenerationTask
    {
        public int Seed { get; }

        public SfaRealm Realm { get; }

        public BGShopGenerator()
        {

        }

        public BGShopGenerator(SfaRealm realm, int seed = 0)
        {
            Realm = realm;
            Seed = seed;
        }

        protected override bool Generate()
        {
            Realm.BGShop = Build() ?? new();
            return true;
        }

        public List<BGShopItem> Build()
        {
            var items = new List<BGShopItem>();

            foreach (var blueprint in SfaDatabase.Instance.Blueprints.Values)
            {
                if (blueprint.Faction.IsMainFaction() == false ||
                    blueprint.BGC < 1)
                    continue;

                items.Add(new BGShopItem()
                {
                    ItemId = blueprint.Id,
                    AccesLevel = blueprint.MinLvl,
                    BGC = blueprint.BGC,
                    Faction = blueprint.Faction,
                });
            }

            items.Sort((a, b) => a.AccesLevel.CompareTo(b.AccesLevel));

            return items;
        }
    }
}
