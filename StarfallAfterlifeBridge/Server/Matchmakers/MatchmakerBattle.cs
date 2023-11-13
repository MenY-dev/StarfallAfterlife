using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MatchmakerBattle
    {
        public MatchmakerGameMode GameMode { get; set; }

        public SfaMatchmaker Matchmaker => GameMode?.Matchmaker;

        public SfaServer Server => GameMode?.Server;

        public MatchmakerBattleState State { get; set; } = MatchmakerBattleState.Created;

        public InstanceInfo InstanceInfo { get; } = new();

        public virtual void Init()
        {

        }

        public virtual void Start()
        {

        }

        public virtual void Stop()
        {

        }

        public virtual void InstanceStateChanged(InstanceState state)
        {

        }

        public virtual void UserStatusChanged(SfaServerClient user, UserInGameStatus status)
        {

        }

        public virtual void CharStatusChanged(ServerCharacter character, UserInGameStatus status)
        {

        }

        public virtual bool ContainsChar(ServerCharacter character)
        {
            return false;
        }

        public virtual bool ContainsUser(SfaServerClient user)
        {
            return false;
        }

        public virtual CharacterReward[] GetRewards()
        {
            return Array.Empty<CharacterReward>();
        }

        public virtual JsonArray GetDropList(string dropName)
        {
            return null;
        }

        public virtual JsonNode GetMobData(MobDataRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
