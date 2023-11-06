using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MothershipAssaultBattle : MatchmakerBattle
    {
        public List<ServerCharacter> CommandA { get; } = new();

        public List<ServerCharacter> CommandB { get; } = new();

        public List<MothershipAssaultCharInfo> Chars { get; } = new();

        public override void Init()
        {
            base.Init();
            InstanceInfo.Type = InstanceType.DiscoveryBattle;

        }

        public override void Start()
        {
            base.Start();

            foreach (var item in Chars)
            {
                item?.Char?.DiscoveryClient.SendBattleGroundState(MatchMakingStage.StartingInstance);
            }


            State = MatchmakerBattleState.PendingMatch;
            InstanceInfo.Type = InstanceType.MothershipAssault;
            InstanceInfo.MothershipIncomeOverride = 80;
            InstanceInfo.Characters.AddRange(Chars.Select(c => c.InstanceCharacter));
            GameMode.InstanceManager.StartInstance(InstanceInfo);
        }

        public void AddCharacter(ServerCharacter character, int team)
        {
            Chars.Add(new(character, team));
        }

        public void RemoveCharacter(ServerCharacter character)
        {
            CommandA.Remove(character);
            CommandB.Remove(character);
        }

        public MothershipAssaultCharInfo GetCharacterInfo(ServerCharacter character) =>
            character is null ? null : Chars?.FirstOrDefault(c => c?.Char == character);

        public void NotifyTheStart()
        {
            foreach (var item in Chars)
            {
                item?.Char?.DiscoveryClient?.SendBattleGroundMatchFinded();
            }
        }

        public void AcceptMatchResult(ServerCharacter character, bool isReady)
        {
            if (GetCharacterInfo(character) is MothershipAssaultCharInfo info)
            {
                if (isReady == false)
                {
                    Chars.Remove(info);

                    if (GameMode is MothershipAssaultGameMode gameMode)
                    {
                        foreach (var item in Chars)
                        {
                            gameMode.AddToQueue(item.Char);
                            item?.Char?.DiscoveryClient
                                .SendBattleGroundState(resetReason: MatchMakingResetReason.LostMember);
                        }
                    }

                    return;
                }

                info.IsReady = true;

                bool isReadyToStart = true;

                foreach (var item in Chars)
                {
                    isReadyToStart &= item.IsReady;
                }

                if (isReadyToStart == true)
                    Start();
            }
        }

        public override void InstanceStateChanged(InstanceState state)
        {
            base.InstanceStateChanged(state);

            if (state == InstanceState.Started)
            {
                var info = InstanceInfo;
                State = MatchmakerBattleState.Started;

                foreach (var item in Chars)
                    item.Char.DiscoveryClient?.Invoke(c => c.SendStartBattle(
                        "battlegrounds",
                        Matchmaker?.CreateBattleIpAddress(),
                        InstanceInfo?.Port ?? -1,
                        item.InstanceCharacter?.Auth));
            }
            else if (state == InstanceState.Finished)
            {
                State = MatchmakerBattleState.Finished;
                Chars.Clear();
            }
        }

        public override bool ContainsChar(ServerCharacter character)
        {
            if (character is null)
                return false;

            return Chars.ToArray()?.Any(c => c?.Char == character) == true;
        }
    }
}
