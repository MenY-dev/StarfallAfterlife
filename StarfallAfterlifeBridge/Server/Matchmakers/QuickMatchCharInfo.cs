using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class QuickMatchCharInfo
    {
        public ServerCharacter Char { get; set; }

        public bool IsReady { get; set; } = false;

        public InstanceCharacter InstanceCharacter { get; }

        public QuickMatchCharInfo(ServerCharacter character)
        {
            if (character is null)
                return;

            Char = character;

            InstanceCharacter = new InstanceCharacter
            {
                Id = character.UniqueId,
                Name = character.Name,
                Auth = Guid.NewGuid().ToString(),
                Faction = character.Faction,
                Team = 0,
                Role = DiscoveryPlayerInstanceStatus.Initiate,
                PartyId = character.Party?.Id ?? 0,
                Features = new()
                {
                    DropRules = new()
                    {
                        new DiscoveryDropRule { Type = DropRuleType.Default }
                    }
                },
            };
        }
    }
}
