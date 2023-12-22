using Avalonia.Controls;
using StarfallAfterlife.Launcher.Controls;
using System;

namespace StarfallAfterlife.Launcher.Pages
{
    public partial class FindServerPage : SfaTabPage
    {
        protected override Type StyleKeyOverride => typeof(SfaTabPage);

        public FindServerPage()
        {
            InitializeComponent();
        }
    }
}
