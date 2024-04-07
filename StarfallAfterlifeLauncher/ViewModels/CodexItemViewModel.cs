using StarfallAfterlife.Bridge.Codex;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.SfPackageLoader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CodexItemViewModel : ViewModelBase
    {
        public int Id { get => _id; set => SetAndRaise(ref _id, value); }

        public string Name { get => _name; set => SetAndRaise(ref _name, value); }

        public string Description { get => _description; set => SetAndRaise(ref _description, value); }

        public int TechLevel { get => _techLevel; set => SetAndRaise(ref _techLevel, value); }

        public ObservableCollection<CodexItemPropertyViewModel> Properties { get; } = new();

        public ObservableCollection<CodexItemPropertyViewModel> TradeProperties { get; } = new();

        public ObservableCollection<CodexItemPropertyViewModel> ProductionProperties { get; } = new();

        public ObservableCollection<CodexItemPropertyViewModel> DisassemblyProperties { get; } = new();

        public ObservableCollection<string> DamageTypeDetails { get; } = new();

        public ObservableCollection<string> Tags { get; } = new();

        public ObservableCollection<SfCodexTypes.HardpointComponent> Slots { get; } = new();

        public byte[][] ItemSlots { get => _itemSlots; set => SetAndRaise(ref _itemSlots, value); }

        public int ItemWidth { get => _itemWidth; set => SetAndRaise(ref _itemWidth, value); }

        public int ItemHeight { get => _itemHeight; set => SetAndRaise(ref _itemHeight, value); }

        public SfCodexItem Item { get; set; }

        public CodexViewModel Codex { get; set; }

        private int _id;
        private string _name;
        private string _description;
        private int _techLevel;
        private byte[][] _itemSlots;
        private int _itemWidth;
        private int _itemHeight;

        public CodexItemViewModel() { }

        public CodexItemViewModel(CodexViewModel codex, SfCodexItem item)
        {
            Item = item;
            Codex = codex;

            if (Item is null || Codex is null)
                return;

            Id = item.Id;
            Name = codex.GetName(item);
            Description = codex.GetDescription(item);

            var fields = item?.Fields?
                .Select(i => (Info: SfCodex.GetPropertyInfo(i.Key), Value: i.Value))
                .Where(i => i.Info is not null)
                .OrderBy(i => i.Info.Flags.HasFlag(SfCodexPropertyFlags.MainInfo) ? -2 :
                              i.Info.Flags.HasFlag(SfCodexPropertyFlags.SecondaryInfo) ? -1 :
                              i.Info.Flags.HasFlag(SfCodexPropertyFlags.AdditionalInfo) ? 1 : 0);

            if (fields is null)
                return;

            foreach (var field in fields)
            {
                var info = field.Info;
                var flags = info.Flags;

                if (flags.HasFlag(SfCodexPropertyFlags.Internal) == true)
                    continue;

                if (flags.HasFlag(SfCodexPropertyFlags.Trade) == true)
                    AddProperty(codex, item, info, field.Value, TradeProperties);
                else if (flags.HasFlag(SfCodexPropertyFlags.Production) == true)
                    AddProperty(codex, item, info, field.Value, ProductionProperties);
                else if (flags.HasFlag(SfCodexPropertyFlags.Disassembly) == true)
                    AddProperty(codex, item, info, field.Value, DisassemblyProperties);
                else
                    AddProperty(codex, item, info, field.Value, Properties);
            }

            if (SfCodex.GetPropertyInfo("EquipmentTags") is SfCodexPropertyInfo equipmentTagsInfo &&
                equipmentTagsInfo.GetValue(item) is string[] equipmentTags)
            {
                var sections = new string[] { "DamageType", "Equipment", "ModuleType" };
                var separators = new char[] { '.', ',' };

                foreach (var key in equipmentTags
                    .Where(t => sections.Any(s => t is not null && t.StartsWith(s, StringComparison.OrdinalIgnoreCase) == true))
                    .Select(t => t.Split(separators).LastOrDefault())
                    .Where(t => string.IsNullOrWhiteSpace(t) == false))
                {
                    var tag = codex.GetName(key, "GameplayTags") ?? key;

                    if (Tags.Contains(tag) == false)
                        Tags.Add(tag);
                }
            }

            if (item?.Fields is not null)
            {
                if ((item.Fields.GetValueOrDefault("ShipFaction") ??
                     item.Fields.GetValueOrDefault("Race")) is Faction faction)
                {
                    if (faction is not Faction.None &&
                        codex?.GetFactionName(faction) is string factionName)
                        Tags.Add(factionName);

                }

                if (item.Fields.GetValueOrDefault("Width") is int itemWidth &&
                    item.Fields.GetValueOrDefault("Height") is int itemHeight)
                {
                    ItemSlots = Enumerable.Repeat(Enumerable.Repeat((byte)0, itemHeight).ToArray(), itemWidth).ToArray();
                    ItemWidth = itemWidth;
                    ItemHeight = itemHeight;
                }

                var techLevel = item.Fields.GetValueOrDefault("TechLevel") ??
                                item.Fields.GetValueOrDefault("ItemTechLevel");

                TechLevel = Math.Max(techLevel is int ? (int)techLevel : 1, 1);

                if (item.Fields.GetValueOrDefault("HullClass") is string hullClass)
                {
                    var key = hullClass switch
                    {
                        "EGameplayShipClass::GSCFrigate" => "Frigate",
                        "EGameplayShipClass::GSCCruiser" => "Cruiser",
                        "EGameplayShipClass::GSCBattlecruiser" => "Battlecruiser",
                        "EGameplayShipClass::GSCBattleship" => "Battleship",
                        "EGameplayShipClass::GSCDreadnought" => "Dreadnought",
                        "EGameplayShipClass::GSCCarrier" => "Carrier",
                        _ => null,
                    };

                    if (key is not null)
                        Tags.Add(codex.GetName(key, "Common") ?? key);
                }

                if (item.Fields.GetValueOrDefault("HullHardpoints") is IEnumerable<SfCodexTypes.HardpointComponent> hps)
                {
                    var slotVariants = new HashSet<TechType>();

                    foreach (var hardpoint in hps)
                    {
                        Slots.Add(hardpoint);

                        if (hardpoint.Type is TechType.Ballistic or TechType.Beam or
                                              TechType.Missile or TechType.Carrier)
                        {
                            slotVariants.Add(hardpoint.Type);
                        }
                    }

                    foreach (var slot in slotVariants)
                    {
                        var key = slot switch
                        {
                            TechType.Ballistic => "Ballistic",
                            TechType.Beam => "Beam",
                            TechType.Missile => "Missiles",
                            TechType.Carrier => "Carrier",
                            _ => slot.ToString()
                        };

                        Tags.Add(codex.GetName(key, "GameplayTags") ?? key);
                    }
                }

                Tags.Add("ID: " + item.Id.ToString());
            }
        }

        protected void AddProperty(
            CodexViewModel codex, SfCodexItem item,
            SfCodexPropertyInfo info, object value,
            ICollection<CodexItemPropertyViewModel> collection)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;
            var prop = new CodexItemPropertyViewModel()
            {
                Value = value,
                Key = info.Name,
                Item = this,
                Name = App.GetString($"codex_name_override_{info.Name}") ??
                       codex.GetName(info.DisplayName) ??
                       info.Name
            };

            if ("RequiredItems".Equals(info.Name, comparison))
            {
                if (value is SfCodexTypes.ItemCountInfo[] items)
                {
                    prop.Value = items
                        .Select(i =>
                        {
                            var name = codex.GetName(codex.Codex.GetItem(i.Item));
                            return $"{name ?? i.Item.ToString()} {i.Count}x";
                        })
                        .ToArray();

                    collection?.Add(prop);
                }
            }
            else if ("DropItemsOnDisassemble".Equals(info.Name, comparison))
            {
                if (value is SfCodexTypes.ItemDropInfo[] items)
                {
                    prop.Value = items
                        .Select(i =>
                        {
                            var name = codex.GetName(codex.Codex.GetItem(i.Item));
                            return $"{name ?? i.Item.ToString()} ({i.Min}-{i.Max})";
                        })
                        .ToArray();

                    collection?.Add(prop);
                }
            }
            else if ("ProjectToOpen".Equals(info.Name, comparison))
            {
                if (value is int project)
                {
                    prop.Value = codex.GetName(codex.Codex.GetItem(project));
                    collection?.Add(prop);
                }
            }
            else if ("xlsDamage".Equals(info.Name, comparison))
            {
                var damage = value is float ? (float)value : 0;
                var dpb = damage * (SfCodex.GetPropertyInfo("xlsShootsPerUsage")?.GetValue<float>(item, 1) ?? 1);
                var cooldown = (SfCodex.GetPropertyInfo("xlsCooldownTime")?.GetValue<float>(item, 1) ?? 1);
                var dps = dpb / (cooldown <= 0 ? 1 : cooldown);
                var damageTypeClass = SfCodex.GetPropertyInfo("xlsDamageTypeClass")?.GetValue<string>(item);
                var damageType = codex.Codex?.GetDamageType(damageTypeClass) ?? new();

                prop.Value = dpb;
                collection?.Add(prop);

                void AddDamageProp(string name, float value)
                {
                    var propInfo = SfCodex.GetPropertyInfo(name);

                    if (propInfo is null)
                        return;

                    AddProperty(codex, item, propInfo, value, collection);
                }

                AddDamageProp("xlsDPSToStructure", damageType.ApplyToStructure(dps));
                AddDamageProp("xlsDPSToArmor", damageType.ApplyToArmor(dps));
                AddDamageProp("xlsDPSToShield", damageType.ApplyToShild(dps));
                AddDamageProp("xlsDMGToStructure", damageType.ApplyToStructure(dpb));
                AddDamageProp("xlsDMGToArmor", damageType.ApplyToArmor(dpb));
                AddDamageProp("xlsDMGToShield", damageType.ApplyToShild(dpb));

                foreach (var details in damageType
                    .GetDamageDetailsKeys()
                    .Select(k => codex.GetName(k, "Codex"))
                    .Where(i => i is not null))
                    DamageTypeDetails.Add(details);
            }
            else
            {
                collection?.Add(prop);
            }
        }
    }
}
