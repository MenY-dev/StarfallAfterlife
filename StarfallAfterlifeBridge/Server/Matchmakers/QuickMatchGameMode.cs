using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class QuickMatchGameMode : MatchmakerGameMode
    {
        public StationAttackBattle CreateStationAttackMatch(byte difficulty, ServerCharacter character)
        {
            if (character is null ||
                Matchmaker.GetBattle(character) is MatchmakerBattle currentBattle &&
                currentBattle.State is not (MatchmakerBattleState.Created or MatchmakerBattleState.Finished))
                return null;

            var battle = new StationAttackBattle() { GameMode = this, Difficulty = difficulty };
            battle.Characters.Add(new(character));
            Matchmaker.AddBattle(battle);
            return battle;
        }

        public SurvivalModeBattle CreateSurvivalMatch(byte difficulty, ServerCharacter character)
        {
            if (character is null ||
                Matchmaker.GetBattle(character) is MatchmakerBattle currentBattle &&
                currentBattle.State is not (MatchmakerBattleState.Created or MatchmakerBattleState.Finished))
                return null;

            var battle = new SurvivalModeBattle() { GameMode = this, Difficulty = difficulty };
            battle.Characters.Add(new(character));
            Matchmaker.AddBattle(battle);
            return battle;
        }
    }
}
