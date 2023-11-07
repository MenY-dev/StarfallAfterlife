using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class SurvivalModeBattle : MatchmakerBattle
    {
        public List<QuickMatchCharInfo> Characters { get; } = new();

        public byte Difficulty { get; internal set; }

        private readonly object _lockher = new();

        public override void Start()
        {
            base.Start();

            lock (_lockher)
            {
                foreach (var item in Characters)
                {
                    item?.Char?.DiscoveryClient.SendQuickMatchState(MatchMakingStage.StartingInstance);
                }

                State = MatchmakerBattleState.PendingMatch;

                InstanceInfo.Type = InstanceType.SurvivalMode;
                InstanceInfo.Characters.AddRange(Characters.Select(c => c.InstanceCharacter));
                InstanceInfo.ExtraData = CreateExtraData().ToJson().ToJsonString();
                GameMode.InstanceManager.StartInstance(InstanceInfo);
            }
        }

        protected virtual InstanceExtraData CreateExtraData()
        {
            return new InstanceExtraData
            {
                NoPlayersLifetime = 10,
                SpecOpsDifficulty = Difficulty,
                ParentObjId = 1000000,
                ParentObjType = (int)DiscoveryObjectType.Mothership,
                ParentObjGroup = 0,
                ParentObjLvl = 7,
            };
        }

        public override void Stop()
        {
            base.Stop();

            lock (_lockher)
            {
                State = MatchmakerBattleState.Finished;
                Matchmaker?.InstanceManager?.StopInstance(InstanceInfo);
                Matchmaker.RemoveBattle(this);
            }
        }

        public override void InstanceStateChanged(InstanceState state)
        {
            base.InstanceStateChanged(state);

            lock (_lockher)
            {
                if (state == InstanceState.Started)
                {
                    State = MatchmakerBattleState.Started;

                    foreach (var character in Characters)
                        JoinToInstance(character);
                }
                else if (state == InstanceState.Finished)
                {
                    Stop();
                }
            }
        }

        public void JoinToInstance(QuickMatchCharInfo character)
        {
            lock (_lockher)
            {
                if (character is null)
                    return;

                character.Char.DiscoveryClient?.Invoke(c => c.SendStartBattle(
                    "quick_match",
                    Matchmaker?.CreateBattleIpAddress(),
                    InstanceInfo?.Port ?? -1,
                    character.InstanceCharacter?.Auth,
                    -1,
                    -1));
            }
        }

        public void OnFleetLeavesFromInstance(DiscoveryObjectType fleetType, int fleetId, SystemHex hex)
        {
            if (fleetType is DiscoveryObjectType.UserFleet)
            {
                Characters.RemoveAll(c => c.InstanceCharacter.Id == fleetId);

                if (Characters.Count < 1)
                    Stop();
            };
        }

        public override void CharStatusChanged(ServerCharacter character, UserInGameStatus status)
        {
            if (status != UserInGameStatus.CharInBattle)
            {
                Characters.RemoveAll(c => c.Char == character);

                if (Characters.Count < 1)
                    Stop();
            }
        }
    }
}
