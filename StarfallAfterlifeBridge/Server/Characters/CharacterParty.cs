using StarfallAfterlife.Bridge.Server.Matchmakers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public class CharacterParty
    {
        public int Id { get; set; } = 0;

        public string Auth { get; set; } = null;

        public List<CharacterPartyMember> Members { get; } = new();

        public SfaServer Server { get; set; }

        public CharacterPartyMember AddMember(int charId, PartyMemberStatus status = PartyMemberStatus.Joined)
        {
            CharacterPartyMember member = null;

            Server?.UseClients(_ =>
            {
                var characret = Server.GetCharacter(charId);

                if (characret is null)
                    return;

                member = new() { Id = charId, Name = characret.Name, Status = status };
                Members.Add(member);

                if (status is PartyMemberStatus.Joined)
                {
                    characret.Party?.RemoveMember(charId);
                    characret.Party = this;
                }

                foreach (var m in Members)
                    Server.GetCharacter(m.Id)?.DiscoveryClient?.Invoke(c => c.SendPartyMembers());
            });

            return member;
        }

        public CharacterPartyMember RemoveMember(int charId)
        {
            CharacterPartyMember member = null;

            Server?.UseClients(_ =>
            {
                member = Members.FirstOrDefault(m => m.Id == charId);
                Members.Remove(member);

                if (Server.GetCharacter(charId) is ServerCharacter character)
                {
                    character.Party = null;
                    character.DiscoveryClient?.Invoke(c => c.SendPartyMembers());
                }

                if (Members.Count == 0)
                {
                    Server.Parties.Remove(this);
                    return;
                }

                foreach (var m in Members)
                    Server.GetCharacter(m.Id)?.DiscoveryClient?.Invoke(c => c.SendPartyMembers());
            });

            return member;
        }

        public CharacterPartyMember[] CreateMembersSnapshot()
        {
            var snapshot = new List<CharacterPartyMember>();

            Server?.UseClients(_ =>
            {
                snapshot.AddRange(Members);
            });

            return snapshot.ToArray();
        }

        public void SetMemberStarSystem(int memberId, int systemId)
        {
            Server?.UseClients(_ =>
            {
                var member = Members.FirstOrDefault(m => m.Id == memberId);

                if (member is null)
                    return;

                member.CurrentStarSystem = systemId;

                foreach (var m in Members)
                    Server.GetCharacter(m.Id)?.DiscoveryClient?.Invoke(c => c.SendPartyMembers());
            });
        }

        public static CharacterParty Create(SfaServer server, int ownerId)
        {
            CharacterParty party = null;

            server?.UseClients(_ =>
            {
                var character = server?.GetCharacter(ownerId);

                if (character is null)
                    return;

                character.Party?.RemoveMember(ownerId);

                var member = new CharacterPartyMember()
                {
                    Id = ownerId,
                    Name = character.UniqueName,
                    Status = PartyMemberStatus.Joined,
                };

                party = new CharacterParty()
                {
                    Server = server,
                };

                party.Id = server.Parties.Add(party);
                party.Members.Add(member);
                character.Party = party;

                character.DiscoveryClient?.Invoke(c => c.SendPartyMembers());
            });

            return party;
        }

        public void BroadcastMembers()
        {
            Server?.UseClients(_ =>
            {
                var server = Server;

                if (server is null)
                    return;

                var battles = new HashSet<DiscoveryBattle>();

                foreach (var member in Members)
                {
                    var character = server.GetCharacter(member.Id);

                    if (character is null)
                        continue;

                    if (server.Matchmaker?.GetBattles(character) is MatchmakerBattle[] charBattles)
                        foreach (var battle in charBattles)
                            battles.Add(battle as DiscoveryBattle);

                    character.DiscoveryClient?.Invoke(c => c.SendPartyMembers());
                }

                server.Invoke(() =>
                {
                    foreach (var battle in battles)
                        battle?.UpdatePartyMembers(Id, Members.ToList());
                });
            });
        }
    }
}
