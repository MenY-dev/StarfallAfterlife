using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class RankedGameMode : MatchmakerGameMode
    {
        private HashSet<SfaServerClient> Queue { get; } = new();

        private Dictionary<SfaServerClient, RankedLobby> Lobbies { get; } = new();

        private readonly object _locker = new();

        public void AddToQueue(SfaServerClient client)
        {
            lock (_locker)
            {
                Queue.Add(client);
                UpdateQueue();
            }
        }

        public bool RemoveFromQueue(SfaServerClient client)
        {
            lock (_locker)
            {
                var result = Queue.Remove(client);
                UpdateQueue();
                return result;
            }
        }

        public bool Contains(SfaServerClient client)
        {
            lock (_locker)
                return Queue.Contains(client);
        }

        private void UpdateQueue()
        {
            var roomSize = Server?.GetClientsInRankedMode().Count == 1 ? 1 : 2;
            
            while (Queue.Count >= roomSize)
            {
                var players = Queue.Take(roomSize).ToArray();
                Queue.RemoveWhere(p => players.Contains(p));

                var battle = new RankedBattle
                {
                    GameMode = this,
                };

                var team = new Random128().Next(0, 2);

                foreach (var p in players)
                {
                    battle.AddPlayer(new()
                    {
                        Player = p,
                        Team = team,
                        FleetId = p.SelectedRankedFleet,
                        Status = RankedLobbyUserStatus.Ready,
                    });

                    team = team == 0 ? 1 : 0;
                }

                Matchmaker.AddBattle(battle);
                battle.Start();
            }
        }


        public RankedLobby CreateLobby(SfaServerClient owner)
        {
            if (owner is null)
                return null;

            lock (_locker)
            {
                foreach (var item in Lobbies.ToArray())
                    LeaveTheLobby(item.Key?.PlayerId ?? -1, owner.PlayerId);

                var lobby = new RankedLobby() { GameMode = this };
                Lobbies.Add(owner, lobby);
                lobby.AddPlayer(owner, RankedLobbyUserStatus.Ready, 0, true);
                return lobby;
            }
        }

        public void LeaveTheLobby(int ownerId, int playerId)
        {
            if (ownerId < 1)
                return;

            lock (_locker)
            {
                if (GetLobbyByOwner(ownerId) is RankedLobby lobby)
                {
                    if (lobby.OwnerId == playerId)
                        CloseLobby(ownerId);
                    else
                        lobby.RemovePlayer(playerId);

                }
            }
        }

        public RankedLobby GetLobby(SfaServerClient player) =>
            GetLobby(player?.PlayerId ?? -1);

        public RankedLobby GetLobby(int playerId)
        {
            lock (_locker)
                return Lobbies.Values.FirstOrDefault(
                    l => l.Players.Any(i => i.Player?.PlayerId == playerId));
        }

        public RankedLobby GetLobbyByOwner(SfaServerClient player)
        {
            lock (_locker)
                return Lobbies.GetValueOrDefault(player);
        }

        public RankedLobby GetLobbyByOwner(int playerId)
        {
            lock (_locker)
                return Lobbies.Values.FirstOrDefault(
                    l => l.OwnerId == playerId);
        }

        public void CloseLobby(int ownerId)
        {
            lock (_locker)
            {
                var toRemove = Lobbies.Where(l => l.Key?.PlayerId == ownerId).ToArray();

                foreach (var item in toRemove)
                {
                    item.Value?.Close();
                    Lobbies.Remove(item.Key);
                }
            }
        }
    }
}
