using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Launcher.ViewModels;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using static StarfallAfterlife.Bridge.Mathematics.Triangulator;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class PlayerStatusInfoEntry : UserControl
    {
        public PlayerStatusInfoEntry()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
            {
                DataContext = new PlayerStatusInfoViewModel(new()
                {
                    Name = "TestPlayer",
                    CharacterName = "TestCharacter",
                    CharacterFaction = Faction.Eclipse,
                    Status = UserInGameStatus.CharInDiscovery,
                });
            }
        }


        public static readonly IValueConverter InGameStatusConverter =
            new FuncValueConverter<UserInGameStatus, string>(v => v switch
            {
                UserInGameStatus.CharMainMenu => "In Spaceport",
                UserInGameStatus.CharSearchingForGame => "Search BG",
                UserInGameStatus.CharInBattle => "In Battle",
                UserInGameStatus.CharInDiscovery => "In Discovery",
                UserInGameStatus.RankedMainMenu => "In Ranked",
                UserInGameStatus.RankedSearchingForGame => "Search Ranked",
                UserInGameStatus.RankedInBattle => "In Ranked Battle",
                UserInGameStatus.None => "Offline",
                _ => v.ToString()
            });
    }
}
