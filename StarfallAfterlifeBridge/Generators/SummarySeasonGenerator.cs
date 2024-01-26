using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class SummarySeasonGenerator : GenerationTask
    {
        public int Seed { get; }

        public SfaRealm Realm { get; }

        public SummarySeasonGenerator()
        {

        }

        public SummarySeasonGenerator(SfaRealm realm, int seed = 0)
        {
            Realm = realm;
            Seed = seed;
        }

        protected override bool Generate()
        {
            if (Realm is SfaRealm realm)
                realm.Seasons = Build();

            return true;
        }

        public WeeklyQuestsInfo Build()
        {
            var info = new WeeklyQuestsInfo();
            const int questId = 4;
            const int levelXp = 750000;

            info.Rewards = CreateRewards(questId);
            info.Stages = CreateAndApplyStages(questId, levelXp, info.Rewards);
            info.Seasons.Add(new()
            {
                Id = questId,
                IsActive = 1,
            });

            return info;
        }

        protected List<WeeklyQuestStage> CreateAndApplyStages(int questId, int levelXp, List<WeeklyReward> rewards)
        {
            var stages = new List<WeeklyQuestStage>();
            var id = questId * 1000000;
            var xp = 0;

            foreach (var reward in rewards)
            {
                id++;
                xp += levelXp;

                stages.Add(new WeeklyQuestStage()
                {
                    Id = id,
                    QuestId = questId,
                    XpToOpen = xp,
                    SkipSfcPrice = 0,
                });

                reward.Stage = id;
            }

            return stages;
        }

        protected List<WeeklyReward> CreateRewards(int questId)
        {
            var id = questId * 1000000;
            var rewards = new List<WeeklyReward>
            {
                new(){ Data = new(){ EquipmentId = 911617938 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // Asimov2350J
                new(){ Data = new(){ ShipProjectId = 1383252631 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Anubis
                new(){ Data = new(){ EquipmentId = 2049612645 }, Count = 8, Type = WeeklyRewardType.UniqueEquipment }, // Goliath618S
                new(){ Data = new(){ ShipProjectId = 2083892232 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Ankou
                new(){ Data = new(){ EquipmentId = 571425764 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // ClarityLRM2
                new(){ Data = new(){ ShipProjectId = 924727673 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Orcus
                
                new(){ Data = new(){ EquipmentId = 447019111 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // DefensiveCannonSentinel
                new(){ Data = new(){ ShipProjectId = 439449052 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Chimera
                new(){ Data = new(){ EquipmentId = 1946236408 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // MeltingRocketDoom
                new(){ Data = new(){ ShipProjectId = 1688575110 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Oria
                
                new(){ Data = new(){ EquipmentId = 1022134915 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // DistorterRL36T
                new(){ Data = new(){ ShipProjectId = 107205271 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Shammuramat
                new(){ Data = new(){ EquipmentId = 643108958 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // SpectralCannonEcho
                new(){ Data = new(){ ShipProjectId = 713931320 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Djehuty
                
                new(){ Data = new(){ EquipmentId = 412781013 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // AntimatterTorpedoJuggernaut
                new(){ Data = new(){ ShipProjectId = 1265695556 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Maltet
                new(){ Data = new(){ EquipmentId = 32941856 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // CascadeMissileLauncher
                new(){ Data = new(){ ShipProjectId = 583232661 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Nirari
                
                new(){ Data = new(){ EquipmentId = 1263554434 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // Project01XAscension
                new(){ Data = new(){ ShipProjectId = 8166718 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Moloch
                new(){ Data = new(){ EquipmentId = 1912871818 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // Fireblast3527
                new(){ Data = new(){ ShipProjectId = 587404833 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Aurora-512
                
                new(){ Data = new(){ EquipmentId = 682531569 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // ProtonLanceColibri
                new(){ Data = new(){ ShipProjectId = 1573617605 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Amber
                new(){ Data = new(){ EquipmentId = 481388254 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // RocketLauncherRetribution
                new(){ Data = new(){ ShipProjectId = 250699329 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Endeavour
                
                new(){ Data = new(){ EquipmentId = 911617938 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // Asimov2350J
                new(){ Data = new(){ ShipProjectId = 1300966717 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Hermes
                new(){ Data = new(){ EquipmentId = 2049612645 }, Count = 8, Type = WeeklyRewardType.UniqueEquipment }, // Goliath618S
                new(){ Data = new(){ ShipProjectId = 1926251238 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Traveler-3
                
                new(){ Data = new(){ EquipmentId = 571425764 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // ClarityLRM2
                new(){ Data = new(){ ShipProjectId = 836319570 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Manticora
                new(){ Data = new(){ EquipmentId = 447019111 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // DefensiveCannonSentinel
                new(){ Data = new(){ ShipProjectId = 1259732331 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // ITO
                
                new(){ Data = new(){ EquipmentId = 1946236408 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // MeltingRocketDoom
                new(){ Data = new(){ ShipProjectId = 1452827324 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Gammarus
                new(){ Data = new(){ EquipmentId = 1022134915 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // DistorterRL36T
                new(){ Data = new(){ ShipProjectId = 621386391 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Crusta
                
                new(){ Data = new(){ EquipmentId = 643108958 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // SpectralCannonEcho
                new(){ Data = new(){ ShipProjectId = 112480543 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Legacy
                new(){ Data = new(){ EquipmentId = 412781013 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // AntimatterTorpedoJuggernaut
                new(){ Data = new(){ ShipProjectId = 152348735 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Cenotaph
                
                new(){ Data = new(){ EquipmentId = 32941856 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // CascadeMissileLauncher
                new(){ Data = new(){ ShipProjectId = 2003616236 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Rebirth
                new(){ Data = new(){ EquipmentId = 1263554434 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // Project01XAscension
                new(){ Data = new(){ ShipProjectId = 2064339753 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Rudren
                
                new(){ Data = new(){ EquipmentId = 1912871818 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // Fireblast3527
                new(){ Data = new(){ ShipProjectId = 2041672194 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Slash
                new(){ Data = new(){ EquipmentId = 682531569 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // ProtonLanceColibri
                new(){ Data = new(){ ShipProjectId = 1472154812 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Heresy
                new(){ Data = new(){ EquipmentId = 481388254 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // RocketLauncherRetribution
                new(){ Data = new(){ ShipProjectId = 11556023 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Nero
                new(){ Data = new(){ EquipmentId = 911617938 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // Asimov2350J
                new(){ Data = new(){ ShipProjectId = 1195151783 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Eco
                new(){ Data = new(){ EquipmentId = 2049612645 }, Count = 8, Type = WeeklyRewardType.UniqueEquipment }, // Goliath618S
                new(){ Data = new(){ ShipProjectId = 1162415908 }, Count = 3, Type = WeeklyRewardType.ShipProject }, // Argus
                new(){ Data = new(){ EquipmentId = 571425764 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // ClarityLRM2
                new(){ Data = new(){ EquipmentId = 447019111 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // DefensiveCannonSentinel
                new(){ Data = new(){ EquipmentId = 1946236408 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // MeltingRocketDoom
                new(){ Data = new(){ EquipmentId = 1022134915 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // DistorterRL36T
                new(){ Data = new(){ EquipmentId = 643108958 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // SpectralCannonEcho
                new(){ Data = new(){ EquipmentId = 412781013 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // AntimatterTorpedoJuggernaut
                new(){ Data = new(){ EquipmentId = 32941856 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // CascadeMissileLauncher
                new(){ Data = new(){ EquipmentId = 1263554434 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // Project01XAscension
                new(){ Data = new(){ EquipmentId = 1912871818 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // Fireblast3527
                new(){ Data = new(){ EquipmentId = 682531569 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // ProtonLanceColibri
                new(){ Data = new(){ EquipmentId = 481388254 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // RocketLauncherRetribution

                new(){ Data = new(){ EquipmentId = 911617938 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // Asimov2350J
                new(){ Data = new(){ EquipmentId = 2049612645 }, Count = 8, Type = WeeklyRewardType.UniqueEquipment }, // Goliath618S
                new(){ Data = new(){ EquipmentId = 571425764 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // ClarityLRM2
                new(){ Data = new(){ EquipmentId = 447019111 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // DefensiveCannonSentinel
                new(){ Data = new(){ EquipmentId = 1946236408 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // MeltingRocketDoom
                new(){ Data = new(){ EquipmentId = 1022134915 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // DistorterRL36T
                new(){ Data = new(){ EquipmentId = 643108958 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // SpectralCannonEcho
                new(){ Data = new(){ EquipmentId = 412781013 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // AntimatterTorpedoJuggernaut
                new(){ Data = new(){ EquipmentId = 32941856 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // CascadeMissileLauncher
                new(){ Data = new(){ EquipmentId = 1263554434 }, Count = 6, Type = WeeklyRewardType.UniqueEquipment }, // Project01XAscension
                new(){ Data = new(){ EquipmentId = 1912871818 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // Fireblast3527
                new(){ Data = new(){ EquipmentId = 682531569 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // ProtonLanceColibri
                new(){ Data = new(){ EquipmentId = 481388254 }, Count = 4, Type = WeeklyRewardType.UniqueEquipment }, // RocketLauncherRetribution

                //new(){ Data = new(){ ShipProjectId = 1042845690 }, Count = 2, Type = WeeklyRewardType.ShipProject }, // Merit
                //new(){ Data = new(){ ShipProjectId = 3944632 }, Count = 2, Type = WeeklyRewardType.ShipProject }, // Creed
                //new(){ Data = new(){ ShipProjectId = 1343212635 }, Count = 2, Type = WeeklyRewardType.ShipProject }, // Astu
                //new(){ Data = new(){ ShipProjectId = 1871570463 }, Count = 2, Type = WeeklyRewardType.ShipProject }, // Acumen
                //new(){ Data = new(){ ShipProjectId = 789746730 }, Count = 2, Type = WeeklyRewardType.ShipProject }, // Valor
                //new(){ Data = new(){ ShipProjectId = 1639619449 }, Count = 2, Type = WeeklyRewardType.ShipProject }, // Duty
            };

            foreach (var reward in rewards)
                reward.Id = id++;

            return rewards;
        }
    }
}
