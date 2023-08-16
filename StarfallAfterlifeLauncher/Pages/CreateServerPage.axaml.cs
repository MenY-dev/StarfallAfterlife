using Avalonia.Controls;
using StarfallAfterlife.Launcher.Controls;
using System;

namespace StarfallAfterlife.Launcher.Pages
{
    public partial class CreateServerPage : SfaTabPage
    {
        protected override Type StyleKeyOverride => typeof(SfaTabPage);

        public CreateServerPage()
        {
            InitializeComponent();
        }
    }
}
