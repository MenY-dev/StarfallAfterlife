using StarfallAfterlife.Bridge.Server.Matchmakers;
using StarfallAfterlife.Launcher.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class BGRoomViewModel : ViewModelBase
    {
        public string Name
        {
            get => Data?.Name;
            set
            {
                if (Data is MothershipAssaultRoom data)
                    SetAndRaise(data.Name, value, v => data.Name = v);
            }
        }

        public MothershipAssaultMap Map
        {
            get => Data?.Map ?? MothershipAssaultMap.Random;
            set
            {
                if (Data is MothershipAssaultRoom data)
                    SetAndRaise(data.Map, value, v => data.Map = v);
            }
        }

        public int MothershipIncome
        {
            get => Data?.MothershipIncome ?? 0;
            set
            {
                if (Data is MothershipAssaultRoom data)
                    SetAndRaise(data.MothershipIncome, value, v => data.MothershipIncome = v);
            }
        }

        public float FreighterSpawnPeriod
        {
            get => Data?.FreighterSpawnPeriod ?? 0;
            set
            {
                if (Data is MothershipAssaultRoom data)
                    SetAndRaise(data.FreighterSpawnPeriod, value, v => data.FreighterSpawnPeriod = v);
            }
        }

        public float ShieldNeutralizerSpawnPeriod
        {
            get => Data?.ShieldNeutralizerSpawnPeriod ?? 0;
            set
            {
                if (Data is MothershipAssaultRoom data)
                    SetAndRaise(data.ShieldNeutralizerSpawnPeriod, value, v => data.ShieldNeutralizerSpawnPeriod = v);
            }
        }

        public ObservableCollection<BGPlayerViewModel> Characters { get; } = new();

        public MothershipAssaultMap[] MapVariants => Enum.GetValues<MothershipAssaultMap>();

        public MothershipAssaultRoom Data { get; set; }

        public BGEditorViewModel Editor { get; set; }

        public BGRoomViewModel() { }

        public BGRoomViewModel(BGEditorViewModel editor, MothershipAssaultRoom data)
        {
            Data = data;
            Editor = editor;
            UpdateCharacters();
        }

        public void UpdateCharacters()
        {
            Characters.Clear();

            if (Data is MothershipAssaultRoom data)
                foreach (var item in data.Characters.ToArray())
                    if (Editor.ServerPage?.GetCharacterVM(item.Key) is PlayerStatusInfoViewModel character)
                        Characters.Add(new(this, character, item.Value));
        }

        public void AddCharacter(PlayerStatusInfoViewModel player)
        {
            if (player is null || player.CharacterId < 0)
                return;

            Data.AddCharacter(player.CharacterId);
            UpdateCharacters();
        }

        public void RemoveCharacter(PlayerStatusInfoViewModel player)
        {
            if (player is null || player.CharacterId < 0)
                return;

            Data.RemoveCharacter(player.CharacterId);
            UpdateCharacters();
        }

        public void Start()
        {
            Data?.Start();
        }
    }
}
