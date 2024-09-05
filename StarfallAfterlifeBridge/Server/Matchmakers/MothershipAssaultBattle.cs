using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Profiles;
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

        public MothershipAssaultRoom Room { get; set; }

        public int CancellationReasonChar { get; protected set; } = -1;

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
                InstanceInfo.MothershipIncomeOverride = Room?.MothershipIncome ?? 80;
                InstanceInfo.FreighterSpawnPeriod = Room?.FreighterSpawnPeriod ?? 90;
                InstanceInfo.ShieldNeutralizerSpawnPeriod = Room?.ShieldNeutralizerSpawnPeriod ?? 90;
                InstanceInfo.Characters.AddRange(Chars.Select(c => c.InstanceCharacter));
                InstanceInfo.Map = CreateMap();
                GameMode.InstanceManager.StartInstance(InstanceInfo);
                Room?.OnBattleStarted(this);
            }
        }

        protected string CreateMap()
        {
            if (Room is MothershipAssaultRoom room &&
                room.Map != MothershipAssaultMap.Random)
            {
                return room.Map switch
                {
                    MothershipAssaultMap.Large => "bg_MothershipAssault",
                    MothershipAssaultMap.Small => "bg_MothershipAssault_2",
                    _ => null,
                };
            }

            return new Random128().Next() % 3 < 2 ?
                "bg_MothershipAssault" :
                "bg_MothershipAssault_2";
        }

        public override void Stop()
        {
            base.Stop();

            lock (_locker)
            {
                if (State is MatchmakerBattleState.PendingMatch or MatchmakerBattleState.Started)
                    GameMode?.InstanceManager?.StopInstance(InstanceInfo);

                State = MatchmakerBattleState.Finished;
                Matchmaker?.RemoveBattle(this);
                Room?.OnBattleFinished(this);
            }
        }

        public virtual void CancelMatch()
        {
            Stop();

            lock (_locker)
            {
                if (Room is MothershipAssaultRoom room)
                {
                    foreach (var item in Chars)
                    {
                        if (item?.IsReady != true)
                            continue;

                        item.Char?.DiscoveryClient
                            .SendBattleGroundState(resetReason: MatchMakingResetReason.LostMember);
                    }
                }
                else if (GameMode is MothershipAssaultGameMode gameMode)
                {
                    foreach (var item in Chars)
                    {
                        if (item?.IsReady != true)
                            continue;

                        gameMode.AddToQueue(item.Char);

                        item.Char?.DiscoveryClient
                            .SendBattleGroundState(resetReason: MatchMakingResetReason.LostMember);
                    }
                }


                Chars.Clear();
                Matchmaker?.InstanceManager?.StopInstance(InstanceInfo);
                Room?.OnBattleCancelled(this);
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
                        CancellationReasonChar = info.Char?.UniqueId ?? -1;
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
                            item.Char,
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
