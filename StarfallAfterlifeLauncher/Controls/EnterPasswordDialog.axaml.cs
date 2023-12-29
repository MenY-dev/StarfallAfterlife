using Avalonia.Controls;
using System;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class EnterPasswordDialog : EditNamePopup
    {
        protected override Type StyleKeyOverride => typeof(EditNamePopup);

        public EnterPasswordDialog()
        {
            TextFilter = @"([\s]*[\S]+[\s]*)+$";
            InitializeComponent();
        }
    }
}
