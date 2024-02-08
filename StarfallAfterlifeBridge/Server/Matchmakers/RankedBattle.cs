using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class RankedBattle : MatchmakerBattle
    {
        public string Map { get; set; }

        public List<RankedPlayerInfo> Players { get; } = new();

        protected Dictionary<int, JsonNode> Fleets { get; } = new();

        private readonly object _locker = new();

        public virtual void AddPlayer(RankedPlayerInfo player)
        {
            lock (_locker)
            {
                Players.Add(player);
            }
        }

        public override void Start()
        {
            base.Start();

            lock (_locker)
            {
                if (State is MatchmakerBattleState.Finished)
                    return;

                foreach (var info in Players)
                {
                    var player = info?.Player;

                    if (player is null ||
                        info.Status != RankedLobbyUserStatus.Ready)
                        continue;

                    player.SendRankedMatchMakingStage(MatchMakingStage.StartingInstance);
                    
                    InstanceInfo.Players.Add(new()
                    {
                        Id = player.PlayerId,
                        FleetId = info.FleetId,
                        Name = player.UniqueName,
                        Auth = player.Auth,
                        Team = info.Team,
                        IsSpectator = info.Team == -1 ? 1 : 0,
                    });
                }

                CreateFleets();

                State = MatchmakerBattleState.PendingMatch;
                InstanceInfo.Type = InstanceType.RankedMode;
                InstanceInfo.Map = CreateMap();
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
                Matchmaker?.RemoveBattle(this);
            }
        }

        protected virtual string CreateMap()
        {
            Map ??= new Random128().Next(0, 8) switch
            {
                0 => "r1_confrontation",
                1 => "r1_dom_corridor",
                2 => "r1_dom_frontline",
                3 => "r1_dom_pass",
                4 => "r1_split",
                5 => "r1_split_dom",
                6 => "r2_cross",
                7 => "r2_nebula",
                _ => Map,
            };

            return Map;
        }

        protected virtual void CreateFleets()
        {
            lock (_locker)
            {
                foreach (var info in Players.ToArray())
                {
                    if (info is null)
                        continue;

                    var selectedFleetId = info.FleetId;
                    var fleet = info.Player?.RankedFleets?.ToArray().FirstOrDefault(f => f?.Id == selectedFleetId);

                    if (fleet?.Ships?.ToArray() is ShipConstructionInfo[] ships)
                    {
                        Fleets[selectedFleetId] = ships.Select(s => new JsonObject
                        {
                            ["id"] = SValue.Create(s.Id),
                            ["data"] = SValue.Create(JsonHelpers.SerializeUnbuffered(s)),
                        }).ToJsonArray();
                    }
                }
            }
        }

        public virtual JsonNode GetRankedFleet(int fleetId)
        {
            lock (_locker)
                return Fleets.GetValueOrDefault(fleetId)?.Clone();
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

                    foreach (var item in Players)
                    {
                        item.Player?.SendStartBattle(
                            "ranked",
                            Matchmaker?.CreateBattleIpAddress(),
                            InstanceInfo?.Port ?? -1,
                            item.Player?.Auth);
                    }
                }
                else if (state == InstanceState.Finished)
                {
                    Stop();
                }
            }
        }

        public override bool ContainsUser(SfaServerClient user)
        {
            if (user is null)
                return false;

            lock (_locker)
                return Players.FirstOrDefault(p => p.Player?.PlayerId == user.PlayerId) is not null;
        }

        public override void UserStatusChanged(SfaServerClient user, UserInGameStatus status)
        {
            base.UserStatusChanged(user, status);

            lock (_locker)
            {
                if (user is not null &&
                    State == MatchmakerBattleState.Started &&
                    status != UserInGameStatus.RankedInBattle)
                {
                    Players.RemoveAll(p => p.Player?.PlayerId == user.PlayerId);

                    if (Players.Count < 1)
                        Stop();
                }
            }
        }
    }
}
