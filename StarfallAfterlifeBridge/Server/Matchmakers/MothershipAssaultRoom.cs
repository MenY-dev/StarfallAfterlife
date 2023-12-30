using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MothershipAssaultRoom
    {
        public string Name { get; set; }

        public MothershipAssaultMap Map { get; set; }

        public int MothershipIncome { get; set; } = 80;

        public float FreighterSpawnPeriod { get; set; } = 90;

        public float ShieldNeutralizerSpawnPeriod { get; set; } = 90;

        public Dictionary<ServerCharacter, BGRoomTeam> Characters { get; } = new();

        public MothershipAssaultGameMode GameMode { get; protected set; }

        public SfaMatchmaker Matchmaker => GameMode?.Matchmaker;

        public SfaServer Server => GameMode?.Server;

        public MothershipAssaultBattle Battle { get; protected set; }

        protected IProgress<string> Progress { get; set; }

        public MothershipAssaultRoom(MothershipAssaultGameMode gameMode)
        {
            GameMode = gameMode;
        }

        public bool AddCharacter(int characterId, BGRoomTeam team = BGRoomTeam.Auto)
        {
            var character = Server?.GetCharacter(characterId);

            if (character is null ||
                Characters.ContainsKey(character))
                return false;

            Characters[character] = BGRoomTeam.Auto;

            return true;
        }

        public void RemoveCharacter(int characterId)
        {
            var character = Characters.Keys.ToList().FirstOrDefault(c => c.UniqueId == characterId);

            if (character is not null)
                Characters.Remove(character);
        }

        public bool SetCharacterTeam(int characterId, BGRoomTeam team)
        {
            var character = Characters.Keys.ToList().FirstOrDefault(c => c.UniqueId == characterId);

            if (character is null)
                return false;

            Characters[character] = team;
            return true;
        }

        public bool Start(IProgress<string> progress = null)
        {
            if (GameMode is null)
                return false;

            Battle?.CancelMatch();
            Battle?.Chars?.Clear();
            GameMode?.Matchmaker?.RemoveBattle(Battle);

            Progress = progress;

            var readyChars = Characters
                .ToArray()
                .Where(c => Matchmaker.GetBattle(c.Key) is null &&
                       c.Key.DiscoveryClient is DiscoveryClient discoveryClient &&
                       discoveryClient.Client?.IsConnected == true &&
                       discoveryClient.State is (SfaCharacterState.InGalaxy or SfaCharacterState.InShipyard))
                .ToList();

            var chars = readyChars.Where(c => c.Value is not BGRoomTeam.None).ToList();
            var team1 = new List<ServerCharacter>();
            var team2 = new List<ServerCharacter>();

            foreach (var character in chars)
            {
                if (character.Value == BGRoomTeam.Team1)
                    team1.Add(character.Key);
                else if (character.Value == BGRoomTeam.Team2)
                    team2.Add(character.Key);
            }

            var autoChars = chars
                .Where(c => c.Value == BGRoomTeam.Auto)
                .Select(c => c.Key)
                .ToList()
                .Randomize(Random.Shared.Next())
                .OrderBy(c => c.Faction);

            foreach (var character in autoChars)
            {
                if (team1.Count < team2.Count)
                    team1.Add(character);
                else
                    team2.Add(character);
            }

            var battle = Battle = new MothershipAssaultBattle()
            {
                GameMode = GameMode,
                Room = this,
            };

            foreach (var character in team1)
            {
                battle.AddCharacter(character, 0);
                GameMode?.RemoveFromQueue(character);
            }

            foreach (var character in team2)
            {
                battle.AddCharacter(character, 1);
                GameMode?.RemoveFromQueue(character);
            }

            Progress?.Report("notify_players");
            Matchmaker?.AddBattle(battle);
            battle.NotifyTheStart();

            return true;
        }

        public void Close()
        {
            Characters.Clear();
            Progress = null;

            if (Battle is MothershipAssaultBattle battle)
            {
                battle.Room = null;
                Battle = null;
            }
        }

        public void OnBattleCancelled(MothershipAssaultBattle battle)
        {
            Progress?.Report("cancelled");
        }

        public void OnBattleFinished(MothershipAssaultBattle battle)
        {
            Progress?.Report("finished");
        }

        public void OnBattleStarted(MothershipAssaultBattle battle)
        {
            Progress?.Report("started");
        }
    }
}
