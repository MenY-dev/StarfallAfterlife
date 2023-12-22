using Avalonia.Controls;
using Avalonia.Styling;
using StarfallAfterlife.Launcher.Controls;
using System;

namespace StarfallAfterlife.Launcher.Pages
{
    public partial class SinglePlayerModePage : SfaTabPage
    {
        protected override Type StyleKeyOverride => typeof(SfaTabPage);

        public SinglePlayerModePage()
        {
            InitializeComponent();
        }
    }
}
