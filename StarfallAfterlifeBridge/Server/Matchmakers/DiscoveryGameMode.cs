using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class DiscoveryGameMode : MatchmakerGameMode
    {
        public override void Init(SfaMatchmaker matchmaker)
        {
            base.Init(matchmaker);
        }

        public DiscoveryBattle CreateNewBattle(StarSystemBattle systemBattle)
        {
            if (systemBattle is not null &&
                systemBattle.IsDungeon == true &&
                systemBattle.DungeonInfo is not null)
                return CreateNewDungeon(systemBattle);

            var match = new DiscoveryBattle()
            {
                GameMode = this,
                SystemBattle = systemBattle,
            };

            match.Init();
            Matchmaker.AddBattle(match);

            return match;
        }


        public DiscoveryDungeon CreateNewDungeon(StarSystemBattle systemBattle)
        {
            var match = new DiscoveryDungeon()
            {
                GameMode = this,
                SystemBattle = systemBattle,
            };

            match.Init();
            Matchmaker.AddBattle(match);

            return match;
        }

        public DiscoveryBattle GetBattle(StarSystemBattle systemBattle) => 
            Matchmaker?.Battles.FirstOrDefault(b =>
                b is DiscoveryBattle battle &&
                battle.SystemBattle == systemBattle) as DiscoveryBattle;
    }
}
