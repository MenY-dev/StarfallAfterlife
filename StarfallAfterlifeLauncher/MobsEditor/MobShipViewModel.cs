using Avalonia.Data.Converters;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.MobsEditor
{
    public class MobShipViewModel : ViewModelBase
    {
        public string Name
        {
            get => SfaDatabase.Instance.GetShip(Ship.Data.Hull)?.HullName;
        }

        public ShipRole Role
        {
            get => (ShipRole)ServiceData.Role;
            set
            {
                var oldValue = (ShipRole)ServiceData.Role;
                ServiceData.Role = (int)value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public int MinLvl
        {
            get => ServiceData.FleetMin;
            set
            {
                var oldValue = ServiceData.FleetMin;
                ServiceData.FleetMin = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public int MaxLvl
        {
            get => ServiceData.FleetMax;
            set
            {
                var oldValue = ServiceData.FleetMax;
                ServiceData.FleetMax = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public int Xp
        {
            get => ServiceData.Xp;
            set
            {
                var oldValue = ServiceData.Xp;
                ServiceData.Xp = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public bool IsBoss
        {
            get => ServiceData?.Tags?.Contains("Mob.SpecialShip.Boss", StringComparer.InvariantCultureIgnoreCase) ?? false;
            set
            {
                var oldvalue = IsBoss;
                SetTag("Mob.SpecialShip.Boss", value);
                RaisePropertyChanged(oldvalue, value);
            }
        }

        public bool IsElite
        {
            get => ServiceData?.Tags?.Contains("Mob.SpecialShip.Elite", StringComparer.InvariantCultureIgnoreCase) ?? false;
            set
            {
                var oldvalue = IsElite;
                SetTag("Mob.SpecialShip.Elite", value);
                RaisePropertyChanged(oldvalue, value);
            }
        }

        public ShipClass Class
        {
            get => SfaDatabase.Instance.GetShip(Ship.Data.Hull)?.HullClass ?? ShipClass.Unknown;
        }

        public IList<HardpointInfo> Hardpoints
        {
            get => SfaDatabase.Instance.GetShip(Ship.Data.Hull)?.Hardpoints;
        }

        public IEnumerable<string> Tags
        {
            get => ServiceData?.Tags;
        }

        public static IValueConverter TagNameConverter = new FuncValueConverter<string, string>(
            t => t?.Replace("Mob.SpecialShip.", "", StringComparison.InvariantCultureIgnoreCase));

        public string BT
        {
            get => ServiceData.BT;
            set
            {
                var oldValue = ServiceData.BT;
                ServiceData.BT = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public static IEnumerable<string> BTValues => BTS.Keys;

        public static readonly IValueConverter BTNames =
            new FuncValueConverter<string, string>(
                val => val is null ? null : BTS.GetValueOrDefault(val) ?? val);

        protected static Dictionary<string, string> BTS { get; } = new()
        {
            { "/Game/gameplay/ai/boss/BtBoardingBoss.BtBoardingBoss", "[boss] Boarding" },
            { "/Game/gameplay/ai/boss/BtFreezerBoss.BtFreezerBoss", "[boss] Freezer" },
            { "/Game/gameplay/ai/boss/BtJokerBoss.BtJokerBoss", "[boss] Joker" },
            { "/Game/gameplay/ai/boss/BtMommyBoss.BtMommyBoss", "[boss] Mommy" },
            { "/Game/gameplay/ai/boss/BtPenetratorBoss.BtPenetratorBoss", "[boss] Penetrator" },
            { "/Game/gameplay/ai/boss/BtRamBoss.BtRamBoss", "[boss] Ram" },
            { "/Game/gameplay/ai/boss/BtRedShieldBoss.BtRedShieldBoss", "[boss] Red Shield" },
            { "/Game/gameplay/ai/boss/BtSniperBoss.BtSniperBoss", "[boss] Sniper" },
            { "/Game/gameplay/ai/boss/BtTurtleBoss.BtTurtleBoss", "[boss] Turtle" },
            { "/Game/gameplay/ai/boss/BtWrathBoss.BtWrathBoss", "[boss] Wrath" },

            { "/Game/gameplay/ai/boss/BtDiscoveryFreezerBoss.BtDiscoveryFreezerBoss", "[boss] Discovery Freezer" },
            { "/Game/gameplay/ai/boss/BtDiscoverySniperBoss.BtDiscoverySniperBoss", "[boss] Discovery Sniper" },
            { "/Game/gameplay/ai/boss/BtDiscoveryTurtleBoss.BtDiscoveryTurtleBoss", "[boss] Discovery Turtle" },

            { "/Game/gameplay/ai/ships/BT_Freighter.BT_Freighter", "[ships] Freighter" },
            { "/Game/gameplay/ai/ships/BT_MinerFreighter.BT_MinerFreighter", "[ships] Miner Freighter" },
            { "/Game/gameplay/ai/ships/BT_Scout.BT_Scout", "[ships] Scout" },
            { "/Game/gameplay/ai/ships/BT_ScreechersRammingShip.BT_ScreechersRammingShip", "[ships] Screechers Ramming Ship" },
            { "/Game/gameplay/ai/ships/BT_Ship_AttackVisibleEnemy.BT_Ship_AttackVisibleEnemy", "[ships] Attack Visible Enemy" },
            { "/Game/gameplay/ai/ships/BT_Ship_Default.BT_Ship_Default", "[ships] Default" },
            { "/Game/gameplay/ai/ships/BT_ShipForceFindEnemy.BT_ShipForceFindEnemy", "[ships] Force Find Enemy" },
            { "/Game/gameplay/ai/ships/BT_Sniper.BT_Sniper", "[ships] Sniper" },
            { "/Game/gameplay/ai/ships/BT_Support.BT_Support", "[ships] Support" },

            { "/Game/gameplay/ai/pirates/BtAtlantesDisaster.BtAtlantesDisaster", "[pirates] Atlantes Disaster" },
            { "/Game/gameplay/ai/pirates/BtLoganPulsar.BtLoganPulsar", "[pirates] Logan Pulsar" },
            { "/Game/gameplay/ai/pirates/BtManyshooter.BtManyshooter", "[pirates] Manyshooter" },
            { "/Game/gameplay/ai/pirates/BtMistery.BtMistery", "[pirates] Mistery" },

            { "/Game/gameplay/ai/raid/BT_Stealth_StarHammerMark.BT_Stealth_StarHammerMark", "[raid] Stealth Star Hammer Mark" },
            { "/Game/gameplay/ai/raid/BT_SupportRepairShip.BT_SupportRepairShip", "[raid] Support Repair Ship" },

            { "/Game/gameplay/ai/BT_AssaultTaran.BT_AssaultTaran", "ssault Taran" },
            { "/Game/gameplay/ai/BT_CaptureMob.BT_CaptureMob", "Capture Mob" },
            { "/Game/gameplay/ai/BT_CommonShip.BT_CommonShip", "Common Ship" },
            { "/Game/gameplay/ai/BT_DungeonOutpostShip.BT_DungeonOutpostShip", "Dungeon Outpost Ship" },
            { "/Game/gameplay/ai/BT_FleetControlledShip.BT_FleetControlledShip", "Fleet Controlled Ship" },
            { "/Game/gameplay/ai/BT_NebulordBoarding.BT_NebulordBoarding", "Nebulord Boarding" },
            { "/Game/gameplay/ai/BT_Screecher.BT_Screecher", "Screecher" },
            { "/Game/gameplay/ai/BT_Sentry.BT_Sentry", "Sentry" },
            { "/Game/gameplay/ai/BTAlienScout.BTAlienScout", "Alien Scout" },
            { "/Game/gameplay/ai/BtAssault.BtAssault", "Assault" },
            { "/Game/gameplay/ai/Btbattleship.Btbattleship", "Battleship" },
            { "/Game/gameplay/ai/BtBoxPatrol.BtBoxPatrol", "Box Patrol" },
            { "/Game/gameplay/ai/BTCargoBoxStealer.BTCargoBoxStealer", "Cargo Box Stealer" },
            { "/Game/gameplay/ai/BTDefendCaravan.BTDefendCaravan", "Defend Caravan" },
            { "/Game/gameplay/ai/BTDefender.BTDefender", "Defender" },
            { "/Game/gameplay/ai/BTDefendPirateBoss.BTDefendPirateBoss", "Defend Pirate Boss" },
            { "/Game/gameplay/ai/BtFighter.BtFighter", "Fighter" },
            { "/Game/gameplay/ai/BTMothershipDestroyer.BTMothershipDestroyer", "Mothership Destroyer" },
            { "/Game/gameplay/ai/BTPatrol.BTPatrol", "Patrol" },
            { "/Game/gameplay/ai/BtPirateMotherhip.BtPirateMotherhip", "Pirate Motherhip" },
            { "/Game/gameplay/ai/BtScout.BtScout", "Scout" },
            { "/Game/gameplay/ai/BtSniper.BtSniper", "Sniper" },
            { "/Game/gameplay/ai/BtSpecOps.BtSpecOps", "Spec Ops" },
            { "/Game/gameplay/ai/BtSupport.BtSupport", "Support" },
        };

        public ShipRole[] ShipRoleItems { get; } = Enum.GetValues<ShipRole>();

        public ShipConstructionInfo Data => Ship.Data ??= new();

        public ShipServiceInfo ServiceData => Ship.ServiceData ??= new();

        public DiscoveryMobShipData Ship { get; set; }

        public string GetShipName(int id)
        {
            string name = SfaDatabase.Instance.GetShipName(Ship.Data.Hull);

            if (name is null)
                return null;

            if (name.StartsWith("BP_") && name.Length > 3)
                name = name[3..];

            if (name.EndsWith("_C") && name.Length > 2)
                name = name[..^2];

            return Regex.Replace(name, @"(\B[A-Z])", " $1");
        }

        protected void SetTag(string tag, bool value)
        {
            if (ServiceData is ShipServiceInfo data)
            {
                var oldTags = ServiceData?.Tags;
                var newTags = new HashSet<string>(oldTags ?? new(), StringComparer.InvariantCultureIgnoreCase);

                if (value == true)
                    newTags.Add(tag);
                else
                    newTags.Remove(tag);

                data.Tags = newTags.ToList();
                RaisePropertyChanged(oldTags, data.Tags, nameof(Tags));
            }
        }
    }
}
