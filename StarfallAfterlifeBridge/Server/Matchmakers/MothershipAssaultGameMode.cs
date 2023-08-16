using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MothershipAssaultGameMode : MatchmakerGameMode
    {
        private LinkedList<ServerCharacter> Queue { get; } = new();

        private readonly object _lockher = new();

        public void AddToQueue(ServerCharacter character)
        {
            lock (_lockher)
            {

                if (Queue.Contains(character))
                    return;

                Queue.AddLast(character);
                UpdateQueue();
            }
        }

        public void RemoveFromQueue(ServerCharacter character)
        {
            lock (_lockher)
            {
                Queue.Remove(character);


                foreach (var item in Matchmaker.Battles
                    .Select(b => b as MothershipAssaultBattle)
                    .Where(b => 
                        b is not null &&
                        b.Chars?.FirstOrDefault(c => c.Char == character) is not null))
                {
                    item.AcceptMatchResult(character, false);
                }
            }
        }

        public void AcceptMatch(ServerCharacter character)
        {
            lock (_lockher)
            {
                Matchmaker.Battles
                    .Select(b => b as MothershipAssaultBattle)
                    .Where(b =>
                        b is not null &&
                        b.Chars?.FirstOrDefault(c => c.Char == character) is not null)
                    .FirstOrDefault()?
                    .AcceptMatchResult(character, true);
            }
        }

        public override void Init(SfaMatchmaker matchmaker)
        {
            base.Init(matchmaker);

            lock (_lockher)
            {
                Queue.Clear();
            }
        }

        protected void UpdateQueue()
        {
            var server = Server;

            if (server is null)
                return;

            lock (_lockher)
            {
                var playersCount = server.GetClientsInDiscovery().Count;
                var roomSize = Math.Max(Math.Min(6, playersCount - (playersCount % 2)), 1);
                var teamSize = Math.Max(1, roomSize / 2);

                if (Queue.Count < roomSize)
                    return;

                var players = new List<ServerCharacter>();

                for ( var i = 0; i < roomSize; i++)
                {
                    players.Add(Queue.First.Value);
                    Queue.RemoveFirst();
                }

                players.Sort((x, y) => x.Faction.CompareTo(y.Faction));

                var match = new MothershipAssaultBattle()
                {
                    GameMode = this
                };

                for (int i = 0; i < players.Count; i++)
                {
                    match.AddCharacter(players[i], i < teamSize ? 0 : 1);
                }

                Matchmaker?.AddBattle(match);
                match.NotifyTheStart();
            }
        }
    }
}
