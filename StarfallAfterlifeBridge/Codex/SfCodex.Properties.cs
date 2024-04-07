﻿using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.SfPackageLoader;
using StarfallAfterlife.Bridge.SfPackageLoader.SfTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property = StarfallAfterlife.Bridge.Codex.SfCodexPropertyInfo;
using Flags = StarfallAfterlife.Bridge.Codex.SfCodexPropertyFlags;

namespace StarfallAfterlife.Bridge.Codex
{
    public partial class SfCodex
    {
        private static readonly IReadOnlyCollection<Property> _properties = new Property[]
        {
            Property.Create<float>("BaseArmorPoints", new("Armor", "Stats"), flags: Flags.MainInfo),
            Property.Create<int>("Internal_ID", flags: Flags.Internal),
            Property.Create<string>("EquipmentLineupIndentity", flags: Flags.Internal),
            Property.Create<string>("xlsEquipmentQuality", flags: Flags.Internal),
            Property.Create<Dictionary<string, int>>("QualityData", uPropConverter: ConvertQualityData, flags: Flags.Internal),
            Property.Create<float>("xlsEqStructurePoints", new("Structure", "Stats"), flags: Flags.AdditionalInfo),
            Property.Create<float>("xlsCooldownTime", new("Cooldown", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("xlsEquipmentMass", new("Mass", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<int>("xlsIGCPrice", flags: Flags.Trade),
            Property.Create<int>("xlsTechPoints", new("CD526F1040A38294F74AC2819F70992A", "UI")),
            Property.Create<string>("BlueprintEqImage", flags: Flags.Internal),
            Property.Create<string>("BlueprintCardImage", flags: Flags.Internal),
            Property.Create<SfCodexTypes.ItemDropInfo[]>("DropItemsOnDisassemble", uPropConverter: ConvertDropItemsOnDisassemble, flags: Flags.AdditionalInfo | Flags.Disassembly),
            Property.Create<SfCodexTypes.ItemCountInfo[]>("RequiredItems", uPropConverter: ConvertRequiredItemCrafting, flags: Flags.AdditionalInfo | Flags.Production),
            Property.Create<int>("IGCToProduce", flags: Flags.Production),
            Property.Create<int>("ProductionPoints", flags: Flags.Production),
            Property.Create<int>("ProjectToOpen", uPropConverter: ConvertPathToAssetId, flags: Flags.Disassembly),
            Property.Create<int>("ProjectToOpenXp", flags: Flags.Disassembly),
            Property.Create<int>("RequiredProjectToOpenXp", flags: Flags.Internal | Flags.SecondaryInfo),
            Property.Create<int>("ProductionPointsOnDisassemble", flags : Flags.Disassembly | Flags.MainInfo),
            Property.Create<bool>("bAllowManualParamsInput", flags: Flags.Internal),
            Property.Create<int>("StarfallItemId", flags: Flags.Internal),
            Property.Create<string[]>("DiscoveryItemTags", flags: Flags.Internal),
            Property.Create<SfCodexTextKey>("ItemShortName", uPropConverter: ConvertToTextKey, flags: Flags.Internal),
            Property.Create<SfCodexTextKey>("ItemFullName", uPropConverter: ConvertToTextKey, flags: Flags.Internal),
            Property.Create<SfCodexTextKey>("ItemShortDescription", uPropConverter: ConvertToTextKey, flags: Flags.Internal),
            Property.Create<SfCodexTextKey>("ItemFullDescription", uPropConverter: ConvertToTextKey, flags: Flags.Internal),
            Property.Create<int>("ItemTechLevel", flags: Flags.Internal),
            Property.Create<int>("MinGalaxyLvl", flags: Flags.AdditionalInfo | Flags.Trade),
            Property.Create<int>("MaxGalaxyLvl", flags: Flags.AdditionalInfo | Flags.Trade),
            Property.Create<float>("GalaxyValue", flags: Flags.Internal),
            Property.Create<int>("Width", flags: Flags.Internal),
            Property.Create<int>("Height", flags: Flags.Internal),
            Property.Create<float>("Cargo", new("Cargo", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<string>("Thumbnail", flags: Flags.Internal),
            Property.Create<string>("ArmorType", flags: Flags.Internal),
            Property.Create<float>("xlsRequiredCapacity", new("Capacity", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<bool>("ShowInCodex", flags: Flags.Internal),
            Property.Create<bool>("bCharacterBound", flags: Flags.Trade | Flags.AdditionalInfo),
            Property.Create<string>("FlightUnitClass", flags: Flags.Internal),
            Property.Create<float>("xlsDamage", new("Damage", "Stats"), flags: Flags.MainInfo),
            Property.Create<string>("xlsDamageTypeClass", uPropConverter: ConvertImportIndexToObjectName, flags: Flags.Internal),
            Property.Create<float>("xlsDPSToShield", new("DPSToShield", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("xlsDPSToArmor", new("DPSToArmor", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("xlsDPSToStructure", new("DPSToStructure", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("xlsDMGToShield", new("DMGToShield", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("xlsDMGToArmor", new("DMGToArmor", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("xlsDMGToStructure", new("DMGToStructure", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("xlsMaxRange", new("Range", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("xlsProjectileSpeed", flags: Flags.AdditionalInfo),
            Property.Create<float>("xlsMaxAimingError", flags: Flags.SecondaryInfo),
            Property.Create<string[]>("EquipmentTags", flags: Flags.Internal),
            Property.Create<float>("xlsOnDestructionExplodeDamage", flags: Flags.AdditionalInfo),
            Property.Create<float>("Duration", new("Duration", "Stats"), flags: Flags.MainInfo),
            Property.Create<bool>("UseAllAtOnce", flags: Flags.Internal),
            Property.Create<string>("xlsEquipmentType", flags: Flags.Internal),
            Property.Create<bool>("xlsIsActiveEquipment", flags: Flags.AdditionalInfo),
            Property.Create<bool>("xlsIsSpecialEquipment", flags: Flags.AdditionalInfo),
            Property.Create<bool>("xlsIsEquipmentDestructable", flags: Flags.Internal),
            Property.Create<float>("xlsOnDestructionExplodeChance", flags: Flags.AdditionalInfo),
            Property.Create<float>("ShootRange", new("Range", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("ProjectileSpeed", new("PROJECTILESPEED", "Codex")),
            Property.Create<float>("Damage", new("Damage", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("Radius", new("Radius", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("InterferenceChance", flags: Flags.SecondaryInfo),
            Property.Create<string>("UsageInputType", flags: Flags.Internal),
            Property.Create<bool>("BreakShipStealth", flags: Flags.AdditionalInfo),
            Property.Create<float>("ShieldSegmentRegenValue", new("RGN", "Equipment"), flags: Flags.MainInfo),
            Property.Create<int>("DroneNumber", new("C54DDB0C4C7E63C3563014B5BA587712"), flags: Flags.MainInfo),
            Property.Create<UProperty[]>("AIActions", flags: Flags.Internal),
            Property.Create<float>("Area", new("RNG", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("Range", new("Range", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("WallMaxWidth", new("LENGTH", "Codex"), flags: Flags.SecondaryInfo),
            Property.Create<int>("BarriersNumber", flags: Flags.SecondaryInfo),
            Property.Create<float>("Capacity", new("Capacity", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<float>("RepairPoints", new("RPR", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("RepairTime", new("Duration", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("ManeuverabilityIncrease", flags: Flags.AdditionalInfo),
            Property.Create<float>("ShieldDamageMultiplier", flags: Flags.MainInfo),
            Property.Create<float>("CooldownDecreasePerTime", flags: Flags.AdditionalInfo),
            Property.Create<Faction>("Race", uPropConverter: ConvertTextToFaction, flags: Flags.Internal),
            Property.Create<int>("BGCPrice", flags : Flags.Trade),
            Property.Create<float>("ShieldPoints", new("Shield", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("Period", new("Cooldown", "Equipment"), flags: Flags.MainInfo),
            Property.Create<int>("ChargesPerRound", new("ROUNDS", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("RegenShieldPerTime", new("13115263448AF25EAB89129835CB8CCA", "Equipment"), flags: Flags.MainInfo),
            Property.Create<string>("ActivationMethod", flags: Flags.Internal),
            Property.Create<float>("ModuleDamage", new("DMG", "Equipment"), flags: Flags.MainInfo),
            Property.Create<int>("Pods", new("6774327C435CA6C4B9D1F4AAA17C59F9", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("UsageTime", new("7714F95B4664A98A362B359A2E4E11E8", "Equipment")),
            Property.Create<float>("SubMissileDamage", new("DMG", "Equipment"), flags: Flags.MainInfo),
            Property.Create<int>("DamagePerSecond", new("DamagePerSecond", "Shipyard"), flags: Flags.MainInfo),
            Property.Create<float>("DamageInterval", new("3F9E068A4299EC549E081FB95C034FC7", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("JumpRange", new("40FC81784B2DA364256AADB455416802", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("ApplyRadius", new("RNG", "Equipment"), flags : Flags.SecondaryInfo),
            Property.Create<bool>("IsPersonalWarp"),
            Property.Create<bool>("Floating"),
            Property.Create<int>("MaxCharges", new("6678094E4433FD2331E06FAFC9A7106B", "Equipment"), flags: Flags.SecondaryInfo),
            Property.Create<float>("timeIsActive", new("7714F95B4664A98A362B359A2E4E11E8", "Equipment"), flags: Flags.SecondaryInfo),
            Property.Create<int>("ModuleRange", new("RNG", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("BeamSpeed", new("PROJECTILESPEED", "Codex"), flags : Flags.SecondaryInfo),
            Property.Create<float>("damageModifierSelf", new("7E405D4E44BD74D063EBBFA50800DCE3", "Equipment")),
            Property.Create<float>("timeToCharge", new("05D05F0444418EEFC170A99F62D9E7BA", "Equipment"), flags : Flags.SecondaryInfo),
            Property.Create<float>("chargeImpulseForce", new("Acceleration", "Stats"), flags : Flags.SecondaryInfo),
            Property.Create<float>("ramDumping", new("DeccelerationSpeed", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<float>("RepairPerSecond", new("D822455444CF63E9D96613AC1D2E1033", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("SAmount", new("9154B1CB45669EDB72C990B91B854574", "Equipment"), flags: Flags.MainInfo),
            Property.Create<bool>("UseOnMyShips"),
            Property.Create<float>("ShipStructurePenaltyDamage", new("PNL", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("ShipImpulseRadius", new("F25FDE7047E1B29128A37CBDB0EF00CF", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("ImpulseStrength", new("Acceleration", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("InUseTime", new("7714F95B4664A98A362B359A2E4E11E8", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("MaxDetectionShipSpeed", new("SPD", "Equipment"), flags: Flags.SecondaryInfo),
            Property.Create<float>("DetectionRange", new("Radius", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("ActivationTimer", new("05D05F0444418EEFC170A99F62D9E7BA", "Equipment"), flags: Flags.SecondaryInfo),
            Property.Create<int>("RoundPerUsage", new("RoundsPerUsage", "Equipment"), flags: Flags.SecondaryInfo),
            Property.Create<float>("timeToDump", new("7714F95B4664A98A362B359A2E4E11E8", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("RepairPointsPerDrone", new("RPR", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("RepairTimePerDrone", new("7714F95B4664A98A362B359A2E4E11E8", "Equipment"), flags : Flags.SecondaryInfo),
            Property.Create<float>("WarpArea", new("RNG", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("AreaRange", new("AreaRange", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("PhaseRetreatImpulseStrength", new("Acceleration", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("PercentageRepairOfDamage", new("DFFF35584FC9CCF437367398861DCC89"), flags: Flags.MainInfo),
            Property.Create<float>("RepairPerTime", new("RPR", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("TimeToRepair", new("Duration", "Equipment"), flags : Flags.MainInfo),
            Property.Create<float>("Cooldown", new("Cooldown", "Equipment"), flags : Flags.MainInfo),
            Property.Create<float>("ShieldDamagePerTime", new("RGN", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("TimeToDamage", new("3F9E068A4299EC549E081FB95C034FC7", "Equipment"), flags: Flags.MainInfo),
            Property.Create<bool>("IsDefaultEquipment", flags: Flags.Internal),
            Property.Create<float>("MinerPoints", new("DMG", "Equipment"), flags: Flags.MainInfo),
            Property.Create<int>("MinerDronesCount", new("7B42ECA64F062806D6FAA1806C301A61", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("RadiusDetectResourceNodes", new("387BE4074DFB2A391F47A48B2B36AF64", "Equipment"), flags: Flags.MainInfo),
            Property.Create<int>("SalvageDronesCount", new("7B42ECA64F062806D6FAA1806C301A61", "Equipment"), flags: Flags.MainInfo),
            Property.Create<int>("ObjectFSM", flags: Flags.Internal),
            Property.Create<SfCodexTypes.HardpointComponent[]>("HullHardpoints", flags: Flags.Internal),
            Property.Create<float>("HullShieldRegen", new("ShieldRegen", "Shipyard"), flags: Flags.MainInfo),
            Property.Create<float>("HullBaseAcceleration", new("AccelerationSpeed", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("HullBaseDecceleration", new("DeccelerationSpeed", "Stats"), flags: Flags.MainInfo),
            Property.Create<int>("HullWarpPoints", new("CD526F1040A38294F74AC2819F70992A", "UI"), flags : Flags.SecondaryInfo),
            Property.Create<float>("HullMass", new("Mass", "Stats"), flags : Flags.SecondaryInfo),
            Property.Create<float>("HullMobilityFactor", flags: Flags.Internal),
            Property.Create<float>("HullSpeedValue", flags: Flags.Internal),
            Property.Create<float>("HullVitalityFactor", flags: Flags.Internal),
            Property.Create<float>("HullMobilityVitality", flags: Flags.Internal),
            Property.Create<float>("BaseShieldRegenRate", new("ShieldRegen", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("BaseShieldPoints", new("Shield", "Stats"), flags: Flags.MainInfo),
            Property.Create<string>("Segment", flags: Flags.Internal),
            Property.Create<float>("xlsDurationTime", new("Duration", "Stats"), flags: Flags.MainInfo),
            Property.Create<string>("WeaponDamageDistanceDependence"),
            Property.Create<string>("ModIcon", flags: Flags.Internal),
            Property.Create<float>("xlsWeaponChargeTime", new("05D05F0444418EEFC170A99F62D9E7BA", "Equipment"), flags: Flags.SecondaryInfo),
            Property.Create<bool>("HasChargingTime", flags: Flags.Internal),
            Property.Create<float>("HeatDamageDivider", new("NormalDamageDivider", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<float>("HeatingTime", new("HeatingTime", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<float>("CoolingTime", flags: Flags.SecondaryInfo),
            Property.Create<float>("xlsProjectileLifeTime", flags: Flags.AdditionalInfo),
            Property.Create<bool>("bCanHealAlly", flags: Flags.Internal),
            Property.Create<float>("HealFactor", flags: Flags.SecondaryInfo),
            Property.Create<int>("xlsShootsPerUsage", new("ShootsPerSalvo", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<float>("xlsShootsPeriod", flags: Flags.SecondaryInfo),
            Property.Create<float>("xlsShootsHorizontalSpread", new("HorizontalSpread", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<float>("ArmorCorrosiveDamagePercent", flags: Flags.MainInfo),
            Property.Create<float>("xlsArmorPenetrationChance", new("ArmorPenetrationChance", "Stats"), flags : Flags.MainInfo),
            Property.Create<float>("xlsShieldPenetrationChance", new("ShieldPenetrationChance", "Stats"), flags : Flags.MainInfo),
            Property.Create<float>("OverclockingTime", new("HeatingTime", "Stats"), flags: Flags.SecondaryInfo),
            Property.Create<float>("ShootingPeriodTime", new("Cooldown", "Equipment"), flags: Flags.MainInfo),
            Property.Create<float>("SectorDamage"),
            Property.Create<float>("SectorRange"),
            Property.Create<float>("SectorAngle"),
            Property.Create<float>("ShootTime", flags : Flags.SecondaryInfo),
            Property.Create<int>("SectorNum"),
            Property.Create<string>("HullClass", flags: Flags.Internal),
            Property.Create<int>("TechLevel", new("TechLevel", "Common"), flags: Flags.Internal),
            Property.Create<SfCodexTextKey>("ShipShortDescription", uPropConverter: ConvertToTextKey, flags: Flags.Internal),
            Property.Create<SfCodexTextKey>("HullFullDiscription", uPropConverter: ConvertToTextKey, flags: Flags.Internal),
            Property.Create<float>("HullStructure", new("Structure", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("HullArmor", new("Armor", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("HullShieldPoints", new("Shield", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("HullCapacity", new("Capacity", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("HullIGCPrice", new("Shipyard", "BlueprintCost"), flags: Flags.Trade),
            Property.Create<float>("HullBaseMaxSpeed", new("MaxSpeed", "Stats"), flags: Flags.MainInfo),
            Property.Create<float>("HullBaseAngularSpeed", new("AngularSpeed", "Stats"), flags: Flags.MainInfo),
            Property.Create<int>("HullFullRepairTime", flags : Flags.Production),
            Property.Create<int>("IGCForConstruction", new("ConstructionCost", "Shipyard"), flags : Flags.Trade),
            Property.Create<int>("MinutesOnConstruction", flags: Flags.Internal),
            Property.Create<SfCodexTextKey>("m_ShipName", uPropConverter: ConvertToTextKey, flags: Flags.Internal),
            Property.Create<string>("ProgressionTreeClass", uPropConverter: ConvertPathToClassName, flags: Flags.Internal),
            Property.Create<bool>("bFactionReward", flags: Flags.Internal),
            Property.Create<int>("HullCargoHoldSize", new("Cargo space", "Stats"), flags: Flags.MainInfo),
            Property.Create<bool>("ProjectIsDefaultOpen", flags: Flags.Internal),
            Property.Create<float>("MaxRoll", flags: Flags.SecondaryInfo),
            Property.Create<SfCodexTextKey>("ShipClassName", uPropConverter: ConvertToTextKey, flags: Flags.Internal),
            Property.Create<Faction>("ShipFaction", uPropConverter: ConvertTextToFaction, flags: Flags.Internal),
            Property.Create<FVector>("LiveRenderOffset", flags: Flags.Internal),
            Property.Create<int>("EntityLimit", flags: Flags.Internal),
            Property.Create<UProperty[]>("DefaultEquipment", flags: Flags.Internal),
            Property.Create<float>("XPGainMultiplier", flags: Flags.AdditionalInfo),
            Property.Create<int>("BaseIGCPrice", flags : Flags.Trade),
            Property.Create<float>("MinIGCPrice", flags: Flags.Trade),
            Property.Create<float>("MaxIGCPrice", flags: Flags.Trade),
            Property.Create<bool>("bConsumeItemOnUseInBattle", flags: Flags.Internal),
            Property.Create<int>("ProjectID", flags: Flags.Internal),
            Property.Create<int>("ProductCount", flags: Flags.Production),
            Property.Create<int>("SecondsToProduce", flags : Flags.Production),
        };

        public static IReadOnlyDictionary<string, Property> Properties => _propertiesLazy.Value;

        private static readonly Lazy<IReadOnlyDictionary<string, Property>> _propertiesLazy = new(() =>
            _properties.ToDictionary(p => p.Name));

        public static Property GetPropertyInfo(string propertyName) =>
            propertyName is null ? null : Properties.GetValueOrDefault(propertyName);
    }
}