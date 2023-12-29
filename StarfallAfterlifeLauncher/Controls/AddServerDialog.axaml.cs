using Avalonia.Controls;
using System;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class AddServerDialog : EditNamePopup
    {
        protected override Type StyleKeyOverride => typeof(EditNamePopup);

        public AddServerDialog()
        {
            TextFilter = @"([\s]*[\S]+[\s]*)+$";
            InitializeComponent();
        }
    }
}
