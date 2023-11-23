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

        private readonly object _locker = new();

        public void AddToQueue(ServerCharacter character)
        {
            lock (_locker)
            {
                if (character is null)
                    return;

                if (Queue.Contains(character))
                    return;

                Queue.AddLast(character);
                UpdateQueue();
            }
        }

        public void RemoveFromQueue(ServerCharacter character)
        {
            lock (_locker)
            {
                Queue.Remove(character);


                foreach (var item in Matchmaker.GetBattles(character)
                    .Select(b => b as MothershipAssaultBattle)
                    .Where(b => b is not null))
                {
                    item.AcceptMatchResult(character, false);
                }
            }
        }

        public void AcceptMatch(ServerCharacter character)
        {
            lock (_locker)
            {
                Matchmaker?
                   .GetBattles(character)
                   .Select(b => b as MothershipAssaultBattle)
                   .Where(b => b is not null and { State: MatchmakerBattleState.Created })
                   .FirstOrDefault()?
                   .AcceptMatchResult(character, true);
            }
        }

        public override void Init(SfaMatchmaker matchmaker)
        {
            base.Init(matchmaker);

            lock (_locker)
            {
                Queue.Clear();
            }
        }

        protected void UpdateQueue()
        {
            var server = Server;

            if (server is null)
                return;

            lock (_locker)
            {
                var playersCount = server.GetClientsInDiscovery().Count;
                var roomSize = Math.Max(Math.Min(6, playersCount - (playersCount % 2)), 1);
                var teamSize = Math.Max(1, roomSize / 2);

                if (Queue.Count < roomSize)
                    return;

                var players = new HashSet<(ServerCharacter Char, CharacterParty Party)>();

                foreach (var entry in Queue)
                {
                    if (entry.Party is CharacterParty party &&
                        party.Members.Count > 1)
                    {
                        foreach (var member in party.CreateMembersSnapshot())
                        {
                            if (server.GetCharacter(member.Id) is ServerCharacter character &&
                                character.DiscoveryClient?.State is SfaCharacterState.InShipyard or SfaCharacterState.InGalaxy)
                                players.Add((character, party));
                        }
                    }
                    else if (entry.DiscoveryClient?.State is SfaCharacterState.InShipyard or SfaCharacterState.InGalaxy)
                    {
                        players.Add((entry, null));
                    }
                }

                if (players.Count < roomSize)
                    return;

                var groups = players
                    .Where(p => p.Party is not null)
                    .GroupBy(p => p.Party)
                    .Select(p => (Party: p.Key, Chars: p.Select(i => i.Char).ToArray()))
                    .Where(p => p.Chars.Length > 1)
                    .OrderBy(p => -p.Chars.Length)
                    .ToList();

                var queue = new Queue<ServerCharacter>(players.Select(i => i.Char));
                var team1 = new List<ServerCharacter>();
                var team2 = new List<ServerCharacter>();

                if (groups.Count > 0)
                    team1.AddRange(groups[0].Chars.Take(teamSize));

                if (groups.Count > 1)
                    team2.AddRange(groups[1].Chars.Take(teamSize));

                queue = new(queue.Where(c => (team1.Contains(c) && team2.Contains(c)) == false));

                static void FillTeam(List<ServerCharacter> team, Queue<ServerCharacter> queue, int teamSize)
                {
                    for (int i = 0; i < queue.Count; i++)
                    {
                        if (team.Count >= teamSize)
                            return;

                        if (queue.TryDequeue(out var player) == false)
                            return;

                        team.Add(player);
                    }
                }

                FillTeam(team1, queue, teamSize);
                FillTeam(team2, queue, teamSize);

                if (roomSize > team1.Count + team2.Count)
                    return;

                var battle = new MothershipAssaultBattle()
                {
                    GameMode = this
                };

                foreach (var item in team1)
                {
                    battle.AddCharacter(item, 0);
                    Queue.Remove(item);
                }

                foreach (var item in team2)
                {
                    battle.AddCharacter(item, 1);
                    Queue.Remove(item);
                }

                Matchmaker?.AddBattle(battle);
                battle.NotifyTheStart();
            }
        }
    }
}
