using Avalonia.Controls.Shapes;
using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class ProfileInfoViewModel : ViewModelBase
    {
        public string Name => Profile?.GameProfile?.Nickname;

        public DateTime LastPlay => Profile?.Info?.LastPlay ?? DateTime.MinValue;

        public bool LastPlayAvailable => LastPlay != DateTime.MinValue;

        public ObservableCollection<CharacterInfoViewModel> Chars{ get; } = new();

        public SfaProfile Profile
        {
            get => _profile;
            set
            {
                _profile = value;
                Update();
            }
        }

        private SfaProfile _profile;

        public ProfileInfoViewModel()
        {
        }

        public ProfileInfoViewModel(SfaProfile profile)
        {
            Profile = profile;
        }

        public void Update()
        {
            UpdateCharacters();

            RaisePropertyUpdate(Name, nameof(Name));
            RaisePropertyUpdate(LastPlay, nameof(LastPlay));
            RaisePropertyUpdate(LastPlayAvailable, nameof(LastPlayAvailable));

            foreach (var item in Chars.ToArray())
                item?.Update();
        }

        public void UpdateCharacters()
        {
            if (Profile?.GameProfile.DiscoveryModeProfile.Chars?.ToArray() is Character[] profileChars)
            {
                var charsVM = Chars.ToArray();

                foreach (var item in charsVM)
                {
                    if (profileChars.Contains(item?.Info) == false)
                        Chars.Remove(item);
                }

                charsVM = Chars.ToArray();

                for (int i = 0; i < profileChars.Length; i++)
                {
                    var item = profileChars[i];

                    if (charsVM.Any(s => s?.Info == item) == false)
                        Chars.Insert(i, new(item));
                }
            }
        }
    }
}
