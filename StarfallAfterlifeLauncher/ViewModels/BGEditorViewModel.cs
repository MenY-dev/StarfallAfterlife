using Avalonia.Threading;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Matchmakers;
using StarfallAfterlife.Launcher.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class BGEditorViewModel : ViewModelBase
    {
        public ObservableCollection<BGRoomViewModel> Rooms { get; } = new();

        public ObservableCollection<PlayerStatusInfoViewModel> Characters { get; } = new();

        public BGRoomViewModel SelectedRoom { get => _selectedRoom; set => SetAndRaise(ref _selectedRoom, value); }

        public bool PlayerSelecterVisible { get => _playerSelecterVisible; set => SetAndRaise(ref _playerSelecterVisible, value); }

        public CreateServerPageViewModel ServerPage
        {
            get => _serverPage;
            set
            {
                SetAndRaise(ref _serverPage, value);
            }
        }

        protected SfaServer Server => ServerPage?.Server;

        protected MothershipAssaultGameMode GameMode => Server?.Matchmaker?.MothershipAssaultGameMode;

        private CreateServerPageViewModel _serverPage;
        private BGRoomViewModel _selectedRoom;
        private bool _playerSelecterVisible = false;

        public BGEditorViewModel() { }

        public BGEditorViewModel(CreateServerPageViewModel serverPage)
        {
            ServerPage = serverPage;
            Update();
        }

        public void Update()
        {
            if (GameMode?.GetRooms() is MothershipAssaultRoom[] rawRooms)
            {
                var toRemove = Rooms.Where(r => rawRooms.Contains(r.Data) == false).ToArray();
                var toAdd = rawRooms.Where(r => Rooms.Any(i => i.Data == r) == false).ToArray();

                foreach (var room in toRemove)
                    Rooms.Remove(room);

                foreach (var room in toAdd)
                    Rooms.Add(new(this, room));
            }
        }

        public void AddRoom()
        {
            new EditNamePopup()
            {
                Title = "Enter Room Name",
                TextFilter = @"([\s]*[\S]+[\s]*)+$",
            }.ShowDialog("BG Room")
            .ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
            {
                if (t.Result.IsDone == true &&
                    t.Result.Text is string roomName &&
                    GameMode is MothershipAssaultGameMode gameMode)
                {
                    gameMode.CreateNewRoom(roomName);
                    Update();
                }
            }));
        }

        public void DeleteRoom(object room)
        {
            if (room is BGRoomViewModel vm &&
                GameMode is MothershipAssaultGameMode gameMode)
            {
                var dialog = new SfaMessageBox()
                {
                    Text = $"Delete Room {vm.Name}?",
                    Title = "Delete Room...",
                    Buttons = MessageBoxButton.Delete |
                              MessageBoxButton.Cancell
                };

                dialog.ShowDialog().ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    if (dialog.PressedButton == MessageBoxButton.Delete &&
                        GameMode is MothershipAssaultGameMode gameMode)
                    {
                        gameMode.RemoveRoom(vm.Data);
                        Update();
                    }
                }));
            }
        }

        public void EditRoom(object room)
        {
            if (room is BGRoomViewModel vm)
            {
                SelectedRoom = vm;
            }
        }

        public void CloseEditor()
        {
            SelectedRoom = null;
        }

        public void ShowPlayerSelector()
        {
            var addedChars = SelectedRoom?.Characters?.ToArray().Select(c => c.Info).ToList() ?? new();
            var chars = (ServerPage?.Players ?? new())
                .ToArray()
                .Where(i => i.CharacterId > -1 && i.CharacterName is not null && addedChars.Contains(i) == false)
                .ToArray()
                .OrderBy(i => i.CharacterName);

            Characters.Clear();

            foreach (var item in chars)
                Characters.Add(item);

            PlayerSelecterVisible = true;
        }

        public void ClosePlayerSelector()
        {
            PlayerSelecterVisible = false;
        }

        public void AddPlayer(object player)
        {
            if (player is PlayerStatusInfoViewModel vm)
            {
                SelectedRoom?.AddCharacter(vm);
            }

            PlayerSelecterVisible = false;
        }

        public void RemovePlayer(object player)
        {
            if (player is PlayerStatusInfoViewModel vm)
            {
                SelectedRoom?.RemoveCharacter(vm);
            }
        }
    }
}
