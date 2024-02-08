using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Server.Discovery.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class RankedLobby
    {
        public List<RankedPlayerInfo> Players { get; } = new();

        private readonly object _locker = new();

        public int OwnerId { get; set; }

        public string OwnerName { get; set; }

        public string Map { get; set; }

        public RankedGameMode GameMode { get; set; }

        protected SfaMatchmaker Matchmaker => GameMode?.Matchmaker;

        public RankedPlayerInfo AddPlayer(
            SfaServerClient player,
            RankedLobbyUserStatus status = RankedLobbyUserStatus.Invited,
            int team = -1, bool isOwner = false)
        {
            if (player is null)
                return null;

            lock (_locker)
            {
                if (Players.FirstOrDefault(p => p.Player == player) is RankedPlayerInfo awailablePlayer)
                {
                    if (isOwner == true)
                    {
                        if (OwnerId != awailablePlayer.Player.PlayerId)
                            OwnerId = awailablePlayer.Player.PlayerId;

                        if (OwnerName != awailablePlayer.Player.UniqueName)
                            OwnerName = awailablePlayer.Player.UniqueName;
                    }

                    return awailablePlayer;
                }

                if (isOwner == true)
                {
                    OwnerId = player.PlayerId;
                    OwnerName = player.UniqueName;
                }

                var info = new RankedPlayerInfo
                {
                    Player = player,
                    FleetId = player.SelectedRankedFleet,
                    Status = status,
                    Team = team,
                };

                Players.Add(info);
                RaiseLobbyUpdated();

                if (status == RankedLobbyUserStatus.Invited)
                {
                    player.SendRankedLobbyInvite(this);
                }

                return info;
            }
        }

        public void AcceptInvite(int playerId)
        {
            lock (_locker)
            {
                if (GetPlayer(playerId) is RankedPlayerInfo info)
                {
                    info.Status = RankedLobbyUserStatus.Joined;
                    RaiseLobbyUpdated();

                    GetPlayer(OwnerId)?.Player?.SendRankedLobbyInviteResponse(
                        info.Player.UniqueName, RankedLobbyInviteResponse.Accepted);
                }
            }
        }

        public void DeclineInvite(int playerId)
        {
            if (GetPlayer(playerId) is RankedPlayerInfo info)
            {
                GetPlayer(OwnerId)?.Player?.SendRankedLobbyInviteResponse(
                    info.Player.UniqueName,
                    RankedLobbyInviteResponse.Declined);
            }

            RemovePlayer(playerId);
        }

        public void RemovePlayer(int playerId)
        {
            lock (_locker)
            {
                if (playerId == OwnerId)
                {
                    GameMode?.CloseLobby(OwnerId);
                    Close();
                }
                else
                {
                    var toRemove = Players.Where(p => p?.Player?.PlayerId == playerId).ToArray();
                    Players.RemoveAll(p => toRemove.Contains(p));

                    foreach (var info in toRemove)
                        info.Player?.SendRankedLobbyUpdated(new());

                    if (Players.Count < 0)
                    {
                        GameMode?.CloseLobby(OwnerId);
                        Close();
                    }
                }

                RaiseLobbyUpdated();
            }
        }

        public RankedPlayerInfo[] GetTeam(int team)
        {
            lock (_locker)
                return Players.Where(p => p?.Team == team).ToArray();
        }

        public void SetPlayerTeam(int playerId, int team)
        {
            lock (_locker)
            {
                var player = GetPlayer(playerId);

                if (player is not null)
                    player.Team = team;

                RaiseLobbyUpdated();
            }
        }

        public void SetPlayerFleet(int playerId, int fleetId)
        {
            lock (_locker)
            {
                var player = GetPlayer(playerId);

                if (player is not null)
                    player.FleetId = fleetId;

                RaiseLobbyUpdated();
            }
        }

        public void SetMap(string map)
        {
            lock (_locker)
            {
                Map = map;
                RaiseLobbyUpdated();
            }
        }

        public void SetReady(int playerId, bool isReady)
        {
            lock (_locker)
            {
                if (GetPlayer(playerId) is RankedPlayerInfo player)
                {
                    if (isReady)
                    {
                        if (player.Status == RankedLobbyUserStatus.Joined)
                            player.Status = RankedLobbyUserStatus.Ready;
                    }
                    else
                    {
                        if (player.Status == RankedLobbyUserStatus.Ready)
                            player.Status = RankedLobbyUserStatus.Joined;
                    }

                    RaiseLobbyUpdated();
                }
            }
        }

        public void StartMatch()
        {
            lock (_locker)
            {
                if (GameMode is null)
                    return;

                var battle = new RankedBattle
                {
                    GameMode = GameMode,
                    Map = Map,
                };

                foreach (var item in Players.ToArray())
                {
                    battle.AddPlayer(item);
                }

                Matchmaker.AddBattle(battle);
                battle.Start();
                GameMode?.CloseLobby(OwnerId);
                Close();
            }
        }

        public void Close()
        {
            OwnerId = 0;
            OwnerName = null;

            foreach (var info in Players.ToArray())
                RemovePlayer(info.Player?.PlayerId ?? -1);
        }

        public RankedPlayerInfo GetPlayer(int playerId)
        {
            lock (_locker)
                return Players.FirstOrDefault(p => p?.Player?.PlayerId == playerId);
        }

        public RankedPlayerInfo GetPlayer(string playerName)
        {
            lock (_locker)
                return Players.FirstOrDefault(p => p?.Player?.UniqueName == playerName);
        }

        private void RaiseLobbyUpdated(IEnumerable<RankedPlayerInfo> players = null)
        {
            players ??= Players;

            foreach (var info in players)
            {
                if (info.Status is not (RankedLobbyUserStatus.Joined or RankedLobbyUserStatus.Ready))
                    continue;

                info.Player.SendRankedLobbyUpdated(this);
            }
        }
    }
}
