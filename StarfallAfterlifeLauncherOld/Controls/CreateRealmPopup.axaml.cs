using Avalonia.Controls;
using Avalonia.Styling;
using System;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class CreateRealmPopup : EditNamePopup
    {
        protected override Type StyleKeyOverride => typeof(EditNamePopup);

        public CreateRealmPopup()
        {
            InitializeComponent();
        }
    }
}
