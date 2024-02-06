using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class RankedGameMode : MatchmakerGameMode
    {
        private HashSet<SfaServerClient> Queue { get; } = new();

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

                foreach (var p in players)
                {
                    battle.Players.Add(p);
                    p.SendRankedMatchMakingStage(MatchMakingStage.StartingInstance);
                }

                Matchmaker.AddBattle(battle);
                battle.Start();
            }
        }
    }
}
