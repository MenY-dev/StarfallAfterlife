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
    public class DiscoveryBattleCharacterInfo
    {
        public BattleMember Member { get; set; }

        public ServerCharacter ServerCharacter { get; }

        public DiscoveryClient Client { get; }

        public InstanceCharacter InstanceCharacter { get; }

        public bool InBattle { get; set; }

        public DiscoveryBattleCharacterInfo(BattleMember member, ServerCharacter character)
        {
            if (character is null || member is null)
                return;

            Member = member; 
            ServerCharacter = character;
            Client = character.DiscoveryClient;

            var DropRules = new List<DiscoveryDropRule>
            {
                new DiscoveryDropRule { Type = DropRuleType.Default }
            };

            DropRules.AddRange(character.CreateDropRules() ?? new());

            InstanceCharacter = new InstanceCharacter
            {
                Id = ServerCharacter.UniqueId,
                Name = ServerCharacter.Name,
                Auth = Guid.NewGuid().ToString(),
                Faction = ServerCharacter.Faction,
                Role = 1,
                HexOffsetX = member.HexOffset.X,
                HexOffsetY = member.HexOffset.Y,
                Features = new()
                {
                    DropRules = DropRules,
                },
            };
        }
    }
}
