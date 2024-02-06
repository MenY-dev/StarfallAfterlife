using StarfallAfterlife.Bridge.Instances;
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
        public List<SfaServerClient> Players { get; } = new();

        protected Dictionary<int, JsonNode> Fleets { get; } = new();

        private readonly object _locker = new();

        public override void Start()
        {
            base.Start();

            lock (_locker)
            {
                if (State is MatchmakerBattleState.Finished)
                    return;

                int team = 0;

                foreach (var player in Players)
                {
                    player.SendRankedMatchMakingStage(MatchMakingStage.StartingInstance);

                    InstanceInfo.Players.Add(new()
                    {
                        Id = player.PlayerId,
                        FleetId = player.SelectedRankedFleet,
                        Name = player.UniqueName,
                        Auth = player.Auth,
                        Team = team,
                        IsSpectator = player.IsSpectator ? 1 : 0,
                    });

                    team = team == 0 ? 1 : 0;
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
            return null;
        }

        protected virtual void CreateFleets()
        {
            lock (_locker)
            {
                foreach (var player in Players.ToArray())
                {
                    if (player is null)
                        continue;

                    var selectedFleetId = player.SelectedRankedFleet;
                    var fleet = player.RankedFleets?.ToArray().FirstOrDefault(f => f?.Id == selectedFleetId);

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
                        item.SendStartBattle(
                            "ranked",
                            Matchmaker?.CreateBattleIpAddress(),
                            InstanceInfo?.Port ?? -1,
                            item.Auth);
                    }
                }
                else if (state == InstanceState.Finished)
                {
                    State = MatchmakerBattleState.Finished;
                    Players.Clear();
                }
            }
        }

        public override bool ContainsUser(SfaServerClient user)
        {
            lock (_locker)
                return Players.Contains(user);
        }

        public override void UserStatusChanged(SfaServerClient user, UserInGameStatus status)
        {
            base.UserStatusChanged(user, status);

            lock (_locker)
            {
                if (State == MatchmakerBattleState.Started &&
                    status != UserInGameStatus.RankedInBattle)
                {
                    Players.RemoveAll(p => p == user);

                    if (Players.Count < 1)
                        Stop();
                }
            }
        }
    }
}
