using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class ProfileInfoViewModel : ViewModelBase
    {
        public string Name => Profile.GameProfile.Nickname;

        public SfaProfile Profile { get; set; }

        public ProfileInfoViewModel(SfaProfile profile)
        {
            Profile = profile;
        }
    }
}
