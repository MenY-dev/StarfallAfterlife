using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MothershipAssaultCharInfo
    {
        public ServerCharacter Char { get; set; }

        public int Team { get; set; } = 0;

        public bool IsReady { get; set; } = false;

        public InstanceCharacter InstanceCharacter { get; }

        public MothershipAssaultCharInfo(ServerCharacter character, int team)
        {
            if (character is null)
                return;

            Char = character;
            Team = team;

            InstanceCharacter = new InstanceCharacter
            {
                Id = character.UniqueId,
                Name = character.Name,
                Auth = Guid.NewGuid().ToString(),
                Faction = character.Faction,
                Team = Team,
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
