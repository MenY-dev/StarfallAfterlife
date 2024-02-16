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
                    CurrentSystemId = 1235,
                    CurrentSystemName = "System 1235",
                });
            }
        }


        public static readonly IValueConverter InGameStatusConverter =
            new FuncValueConverter<UserInGameStatus, string>(v => v switch
            {
                UserInGameStatus.CharMainMenu => App.GetString("s_type_user_game_status_char_main_menu"),
                UserInGameStatus.CharSearchingForGame => App.GetString("s_type_user_game_status_char_searching_game"),
                UserInGameStatus.CharInBattle => App.GetString("s_type_user_game_status_char_in_battle"),
                UserInGameStatus.CharInDiscovery => App.GetString("s_type_user_game_status_char_in_discovery"),
                UserInGameStatus.RankedMainMenu => App.GetString("s_type_user_game_status_ranked_main_menu"),
                UserInGameStatus.RankedSearchingForGame => App.GetString("s_type_user_game_status_ranked_searching_game"),
                UserInGameStatus.RankedInBattle => App.GetString("s_type_user_game_status_ranked_in_battle"),
                UserInGameStatus.None => App.GetString("s_type_user_game_status_ranked_none"),
                _ => v.ToString()
            } ?? v.ToString());
    }
}
