using StarfallAfterlife.Bridge.SfPackageLoader.FileSysten;
using StarfallAfterlife.Bridge.SfPackageLoader.SfTypes;
using StarfallAfterlife.Bridge.SfPackageLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using StarfallAfterlife.Bridge.Database;

namespace StarfallAfterlife.Bridge.Codex
{
    public partial class SfCodex
    {
        public static SfCodex LoadFromGame(string path)
        {
            try
            {
                var packs = Directory.GetFiles(path, "*-WindowsNoEditor.pak");
                var uefs = new UEFileSystem();

                foreach (var pack in packs)
                    uefs.LoadPack(pack);

                return LoadFromGame(uefs);
            }
            catch { }

            return null;
        }

        public static SfCodex LoadFromGame(UEFileSystem uefs)
        {
            var codex = new SfCodex();

            try
            {
                codex.LoadLocalizationFromGame(uefs);
                codex.LoadItemsFromGame(uefs);
                codex.LoadDamageTypesFromGame(uefs);
            }
            catch
            {
                return null;
            }

            return codex;
        }

        private void LoadItemsFromGame(UEFileSystem uefs)
        {
            var sfConverters = SfObjectPropertyConverters.Converters.ToList();

            var shipsFiles = Enumerable.Empty<UEFSDirectory>()
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/deprived").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/eclipse").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/vanguard").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/yoba").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/npc_criterion").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/npc_free_traders").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/npc_mineworkers_union").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/npc_nebulords").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/npc_pyramid").GetDirectories())
                .Concat(uefs.GetDirectory("/Starfall/Content/gameplay/ships/npc_screechers").GetDirectories())
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/ships/npc_raid"))
                .SelectMany(d => d.GetFiles());

            var equipmentFiles = Enumerable.Empty<UEFSDirectory>()
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/armor"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/carrier"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/engineering"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/engines"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/miner"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/misc"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/shields"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/weapon/beams"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/weapon/cannons"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/weapon/missiles"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/spec_ops/normal"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/spec_ops/hard"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/equipment/spec_ops/expert"))
                .SelectMany(d => d.GetFiles());

            var discoItemsFiles = Enumerable.Empty<UEFSDirectory>()
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/items"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/items/consumable"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/items/consumable/spec_ops/expert"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/items/consumable/spec_ops/hard"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/items/consumable/spec_ops/normal"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/items/quest_items"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/projects"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/projects/sale_project"))
                .Append(uefs.GetDirectory("/Starfall/Content/gameplay/discovery/projects/unique_project"))
                .SelectMany(d => d.GetFiles());

            UAsset LoadAsset(UEFSFileInfo fileInfo)
            {
                try
                {
                    if (fileInfo.Path?.EndsWith(".uasset") != true)
                        return null;

                    using var assetReader = fileInfo.Open();
                    using var uexpReader = uefs.GetFile(fileInfo.Path[..(fileInfo.Path.Length - 7)] + ".uexp")?.Open();
                    var asset = new UAsset();

                    if (uexpReader is null)
                        return null;

                    assetReader.Read(asset);
                    asset.LoadObjectsData(uexpReader, new() { Converters = sfConverters });

                    return asset;
                }
                catch { return null; }
            }

            UObject GetItemObject(UAsset asset)
            {
                try
                {
                    foreach (var obj in asset.Objects ?? new())
                    {
                        if (obj is not null &&
                            ((int?)obj["StarfallItemId"] ?? (int?)obj["Internal_ID"]) is int itemId &&
                            itemId != 0)
                        {
                            return obj;
                        }
                    }

                    return null;
                }
                catch { return null; }
            }

            SfCodexItem AddItem(UAsset asset, UObject obj, Dictionary<int, SfCodexItem> dtb)
            {
                var itenId = (int?)obj["StarfallItemId"] ?? (int?)obj["Internal_ID"] ?? 0;
                var export = asset.GetExport(obj);
                var itemClass = asset.GetClassName(export?.ObjectIndex ?? 0);
                var itemBaseClass = asset.GetClassName(export?.TemplateIndex ?? 0);

                if (itenId == 0 || itemClass is null)
                    return null;

                var fields = new Dictionary<string, object>();
                var context = new UPropertyConverterContext(this, obj, asset, uefs);
                var nameText = ((FText?)obj["ItemFullName"]) ?? ((FText?)obj["m_ShipName"]) ?? default;
                var item = new SfCodexItem()
                {
                    Id = itenId,
                    Class = itemClass,
                    BaseClass = itemBaseClass,
                    Name = nameText.Text,
                    NameKey = nameText.Key,
                    DescriptionKey = ((FText?)obj["ItemFullDescription"])?.Key ?? ((FText?)obj["HullFullDiscription"])?.Key,
                    Fields = fields,
                };

                foreach (var objField in obj)
                {
                    var info = GetPropertyInfo(objField.Name);

                    if (info is null)
                        continue;

                    var value = info.Converter is null ? objField.Value : info.Converter.Invoke(objField, context);

                    if (value is null || info.Type.IsAssignableFrom(value.GetType()))
                        fields[info.Name] = value;
                }

                dtb[itenId] = item;
                return item;
            }

            Equipment = new();
            Ships = new();
            DiscoveryItems = new();
            ClassToIdMap = new();

            foreach (var item in Enumerable.Empty<UEFSFileInfo>()
                .Concat(equipmentFiles)
                .Concat(shipsFiles)
                .Concat(discoItemsFiles))
            {
                UAsset asset;

                if ((asset = LoadAsset(item)) is null)
                    continue;

                foreach (var obj in asset.Objects)
                {
                    if (((int?)obj["StarfallItemId"] ?? (int?)obj["Internal_ID"]) is int id)
                    {
                        if (asset.GetClassName(asset.GetExport(obj)?.ObjectIndex ?? 0) is string name)
                            ClassToIdMap[name] = id;

                        break;
                    }
                }
            }

            foreach (var item in equipmentFiles)
            {
                UAsset asset;
                UObject obj;

                if ((asset = LoadAsset(item)) is null ||
                    (obj = GetItemObject(asset)) is null)
                    continue;

                if (AddItem(asset, obj, Equipment) is SfCodexItem newItem)
                {
                    // Patch for empty cooling time
                    if (newItem.Fields is not null &&
                        newItem.Fields.ContainsKey("xlsCooldownTime") == false)
                    {
                        var comparison = StringComparison.OrdinalIgnoreCase;
                        var fields = newItem.Fields.ToList();

                        var index = fields.FindIndex(i => i.Key.Equals("xlsOnDestructionExplodeDamage", comparison));

                        if (index < 0)
                            index = fields.FindIndex(i => i.Key.Equals("xlsEqStructurePoints", comparison));

                        if (index > -1)
                        {
                            newItem.Fields.Clear();
                            fields.Insert(index + 1, new("xlsCooldownTime", 5f));

                            foreach (var field in fields)
                                newItem.Fields[field.Key] = field.Value;
                        }
                    }
                }
            }

            foreach (var item in shipsFiles)
            {
                UAsset asset;
                UObject obj;

                if ((asset = LoadAsset(item)) is null ||
                    (obj = GetItemObject(asset)) is null)
                    continue;

                if (AddItem(asset, obj, Ships) is SfCodexItem newItem)
                {
                    if (newItem.Fields is not null &&
                        newItem.Fields.ContainsKey("ShipFaction") == false)
                        newItem.Fields["ShipFaction"] = Faction.Deprived;

                    (FObjectExport Export, UObject Object, string Type) GetHardpointInfo(UAsset asset, FObjectExport export)
                    {
                        var obj = asset.GetObject(export);
                        string className = asset.GetClassName(export.TemplateIndex);
                        string type = null;

                        if (obj is null)
                            return (export, obj, type);

                        try
                        {
                            const string typePropName = "HardpointType";

                            if ("HardpointComponent".Equals(className, StringComparison.OrdinalIgnoreCase))
                                type = (string)obj[typePropName] ?? "ETechType::TTUnknown";
                            else if ("EngineHardpointComponent".Equals(className, StringComparison.OrdinalIgnoreCase))
                                type = (string)obj[typePropName] ?? "ETechType::TTEngine";
                            else if ("WeaponHardpointComponent".Equals(className, StringComparison.OrdinalIgnoreCase))
                                type = (string)obj[typePropName] ?? "ETechType::TTBallisticWeapon";
                            else if ("CarrierHardpointComponent".Equals(className, StringComparison.OrdinalIgnoreCase))
                                type = (string)obj[typePropName] ?? "ETechType::TTCarrier";
                            else if ("LensHardpointComponent".Equals(className, StringComparison.OrdinalIgnoreCase))
                                type = (string)obj[typePropName] ?? "ETechType::TTBeamWeapon";
                            else if ("SingleTurretHardpointComponent".Equals(className, StringComparison.OrdinalIgnoreCase))
                                type = (string)obj[typePropName] ?? "ETechType::TTBallisticWeapon";
                            else if ("MultiTurretHardpointComponent".Equals(className, StringComparison.OrdinalIgnoreCase))
                                type = (string)obj[typePropName] ?? "ETechType::TTBallisticWeapon";
                            else if ("RocketsHardpointComponent".Equals(className, StringComparison.OrdinalIgnoreCase))
                                type = (string)obj[typePropName] ?? "ETechType::TTMissileWeapon";
                        }
                        catch { }

                        return (export, obj, type);
                    }

                    var hardpoints = asset.Exports?
                        .Select(i => GetHardpointInfo(asset, i))
                        .Where(i => i.Object is not null && i.Type is not null)
                        .ToList() ?? new();

                    var codexHardpoints = new List<SfCodexTypes.HardpointComponent>();

                    foreach (var hp in hardpoints)
                    {
                        codexHardpoints.Add(new()
                        {
                            Name = hp.Object.Name?.Split("_GEN_VARIABLE").FirstOrDefault(),
                            Type = hp.Type switch
                            {
                                "ETechType::TTBallisticWeapon" => TechType.Ballistic,
                                "ETechType::TTBeamWeapon" => TechType.Beam,
                                "ETechType::TTMissileWeapon" => TechType.Missile,
                                "ETechType::TTCarrier" => TechType.Carrier,
                                "ETechType::TTEngineering" => TechType.Engineering,
                                "ETechType::TTEngine" => TechType.Engine,
                                _ => TechType.Unknown,
                            },
                            Width = (int?)hp.Object["Width"] ?? 0,
                            Height = (int?)hp.Object["Height"] ?? 0,
                            GridX = (int?)hp.Object["GridPositionX"] ?? 0,
                            GridY = (int?)hp.Object["GridPositionY"] ?? 0,
                            TurnAngle = (float?)hp.Object["TurnAngle"] ?? 0,
                            Position = hp.Object["RelativeLocation"].Value is FVector pos ? new() { X = pos.X, Y = pos.Y, Z = pos.Z} : default,
                            Rotation = hp.Object["RelativeRotation"].Value is FRotator rot ? new() { X = rot.Pitch, Y = rot.Yaw, Z = rot.Roll } : default,
                        });
                    }

                    newItem.Fields["HullHardpoints"] = codexHardpoints.ToArray();
                }
            }

            foreach (var item in discoItemsFiles)
            {
                UAsset asset;
                UObject obj;

                if ((asset = LoadAsset(item)) is null ||
                    (obj = GetItemObject(asset)) is null)
                    continue;

                AddItem(asset, obj, DiscoveryItems);
            }
        }
        private void LoadDamageTypesFromGame(UEFileSystem uefs)
        {
            var sfConverters = SfObjectPropertyConverters.Converters.ToList();

            DamageTypes ??= new();

            foreach (var fileInfo in uefs
                .GetDirectory("/Starfall/Content/gameplay/equipment/weapon").GetFiles()
                .Where(f => f.Path.EndsWith(".uasset")))
            {
                if (fileInfo.Path?.EndsWith(".uasset") != true)
                    continue;

                using var assetReader = fileInfo.Open();
                using var uexpReader = uefs.GetFile(fileInfo.Path[..(fileInfo.Path.Length - 7)] + ".uexp")?.Open();
                var asset = new UAsset();

                if (uexpReader is null)
                    continue;

                assetReader.Read(asset);
                asset.LoadObjectsData(uexpReader, new() { Converters = sfConverters });

                var dataExport = asset.Exports?.FirstOrDefault(o => o.ObjectFlags.HasFlag(
                    EObjectFlags.ClassDefaultObject | EObjectFlags.ArchetypeObject)) ?? default;

                if ("StarfallDamageType".Equals(asset.GetClassName(dataExport.TemplateIndex)) == false)
                    continue;

                if (asset.GetClassName(dataExport.ObjectIndex) is string name &&
                    asset.GetObject(dataExport) is UObject obj &&
                    SfCodexTypes.DamageType.Load(obj) is SfCodexTypes.DamageType damageType)
                {
                    damageType.Class = name;
                    DamageTypes[name] = damageType;
                }
            }
        }

        private void LoadLocalizationFromGame(UEFileSystem uefs)
        {
            foreach (var item in uefs
                .GetDirectory("/Starfall")
                .GetFilesRecursively()
                .Where(f => f.Path.EndsWith(".locres")))
            {
                if (string.IsNullOrWhiteSpace(item.Path) == true)
                    continue;

                var loc = item.Path?
                    .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .ElementAtOrDefault(Index.FromEnd(2));

                if (loc is null)
                    continue;

                if (loc.StartsWith("en") == true) loc = "en";
                else if (loc.StartsWith("ru") == true) loc = "ru";
                else continue;

                using var reader = item.Open();
                var locres = new FTextLocalizationResource();
                reader.Read(locres);

                foreach (var group in locres.Namespaces ?? new())
                {
                    foreach (var text in group.Value ?? new())
                    {
                        AddTextLocalization(loc, group.Key, text.Key, text.Value);
                    }
                }
            }

            foreach (var loc in Localizations ??= new())
            {
                loc.Namespaces = loc.Namespaces?
                    .OrderBy(n => !string.IsNullOrWhiteSpace(n.Name))
                    .ThenBy(n => !StringComparer.OrdinalIgnoreCase.Equals(n.Name, "Equipment"))
                    .ThenBy(n => !StringComparer.OrdinalIgnoreCase.Equals(n.Name, "Shipyard"))
                    .ThenBy(n => !StringComparer.OrdinalIgnoreCase.Equals(n.Name, "Common"))
                    .ThenBy(n => !StringComparer.OrdinalIgnoreCase.Equals(n.Name, "Codex"))
                    .ThenBy(n => !StringComparer.OrdinalIgnoreCase.Equals(n.Name, "GameplayTags"))
                    .ToList();
            }
        }

        private void AddTextLocalization(string loc, string group, string key, string text)
        {
            if (loc is null || group is null || key is null || text is null)
                return;

            var localization = (Localizations ??= new())
                .FirstOrDefault(l => StringComparer.OrdinalIgnoreCase.Equals(l?.Code, loc));

            if (localization is null)
            {
                localization = new() { Code = loc };
                Localizations.Add(localization);
            }

            var locGroup = (localization.Namespaces ??= new())
                .FirstOrDefault(n => StringComparer.OrdinalIgnoreCase.Equals(n?.Name, group));

            if (locGroup is null)
            {
                locGroup = new() { Name = group };
                localization.Namespaces.Add(locGroup);
            }

            (locGroup.Strings ??= new())[key] = text;
        }

        protected class UPropertyFilter
        {
            public string Name { get; }

            public string Type { get; }

            public Func<UProperty, object> Converter { get; }

            protected Func<string, bool> NameComparer;

            public UPropertyFilter(string name, string type = null, Func<UProperty, object> converter = null)
            {
                Name = name;
                Type = type;
                Converter = converter;

                if (name is not null)
                {
                    if (name.StartsWith('*'))
                    {
                        name = name[1..];
                        NameComparer = s => s?.EndsWith(name, StringComparison.OrdinalIgnoreCase) == true;
                    }
                    else if (name.EndsWith('*'))
                    {
                        name = name[..(name.Length - 1)];
                        NameComparer = s => s?.StartsWith(name, StringComparison.OrdinalIgnoreCase) == true;
                    }
                    else
                    {
                        NameComparer = s => s?.Equals(name, StringComparison.OrdinalIgnoreCase) == true;
                    }
                }
            }

            public bool Check(UProperty property)
            {
                if (NameComparer is not null && (
                    property.Name is null ||
                    NameComparer(property.Name) == false))
                    return false;

                if (Type is not null && (
                    property.Type is null ||
                    Type.Equals(property.Type, StringComparison.OrdinalIgnoreCase) == false))
                    return false;

                return true;
            }

            public object GetValue(UProperty property)
            {
                if (Converter is null)
                    return property.Value;

                return Converter(property);
            }
        }
    }
}
