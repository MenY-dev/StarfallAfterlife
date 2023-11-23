using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MothershipAssaultBattle : MatchmakerBattle
    {
        public List<ServerCharacter> CommandA { get; } = new();

        public List<ServerCharacter> CommandB { get; } = new();

        public List<MothershipAssaultCharInfo> Chars { get; } = new();

        private readonly object _locker = new();

        public override void Init()
        {
            base.Init();
            
            lock (_locker)
                InstanceInfo.Type = InstanceType.DiscoveryBattle;
        }

        public override void Start()
        {
            base.Start();

            lock (_locker)
            {
                if (State is MatchmakerBattleState.Finished)
                    return;

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
        }

        public override void Stop()
        {
            base.Stop();
            lock (_locker)
            {
                if (State is MatchmakerBattleState.PendingMatch or MatchmakerBattleState.Started)
                    GameMode?.InstanceManager?.StopInstance(InstanceInfo);

                State = MatchmakerBattleState.Finished;
            }
        }

        public virtual void CancelMatch()
        {
            Stop();

            lock (_locker)
            {
                if (GameMode is MothershipAssaultGameMode gameMode)
                {
                    foreach (var item in Chars)
                    {
                        if (item?.IsReady != true)
                            continue;

                        gameMode.AddToQueue(item.Char);

                        item.Char?.DiscoveryClient
                            .SendBattleGroundState(resetReason: MatchMakingResetReason.LostMember);
                    }

                    Chars.Clear();
                }
            }
        }

        public void AddCharacter(ServerCharacter character, int team)
        {
            lock (_locker)
            {
                Chars.Add(new(character, team));
            }
        }

        public void RemoveCharacter(ServerCharacter character)
        {
            lock (_locker)
            {
                CommandA.Remove(character);
                CommandB.Remove(character);
            }
        }

        public MothershipAssaultCharInfo GetCharacterInfo(ServerCharacter character)
        {
            lock (_locker)
            {
                return character is null ? null : Chars?.FirstOrDefault(c => c?.Char == character);
            }
        }

        public void NotifyTheStart()
        {
            lock (this)
            {
                Task.Delay(TimeSpan.FromSeconds(16)).ContinueWith(t =>
                {
                    lock (_locker)
                        if (State == MatchmakerBattleState.Created)
                            CancelMatch();
                });

                foreach (var item in Chars)
                {
                    item?.Char?.DiscoveryClient?.SendBattleGroundMatchFinded();
                }
            }
        }

        public void AcceptMatchResult(ServerCharacter character, bool isReady)
        {
            lock (_locker)
            {
                if (GetCharacterInfo(character) is MothershipAssaultCharInfo info)
                {
                    if (isReady == false)
                    {
                        Chars.Remove(info);
                        CancelMatch();
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
        }

        public override void InstanceStateChanged(InstanceState state)
        {
            base.InstanceStateChanged(state);

            lock (_locker)
            {
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
        }

        public override bool ContainsChar(ServerCharacter character)
        {
            lock (_locker)
            {
                if (character is null)
                    return false;

                return Chars.ToArray()?.Any(c => c?.Char == character) == true;
            }
        }

        public override void CharStatusChanged(ServerCharacter character, UserInGameStatus status)
        {
            lock (_locker)
            {
                if (State == MatchmakerBattleState.Started &&
                    status != UserInGameStatus.CharInBattle)
                {
                    Chars.RemoveAll(c => c.Char == character);

                    if (Chars.Count < 1)
                        Stop();
                }
            }
        }

        public override void HandleBattleResults(JsonNode doc)
        {
            base.HandleBattleResults(doc);

            lock (_locker)
            {
                if (doc is JsonObject &&
                    doc["players"]?.AsArraySelf() is JsonArray players)
                {
                    foreach (var p in players)
                    {
                        if ((int?)p["character_id"] is int charId &&
                            CommandA.Concat(CommandB).FirstOrDefault(c => c.UniqueId == charId) is ServerCharacter character)
                            character.HandleBattleResults(p);
                    }
                }
            }
        }

    }
}
