using Avalonia.Controls;
using Avalonia.Styling;
using StarfallAfterlife.Launcher.Controls;
using System;

namespace StarfallAfterlife.Launcher.Pages
{
    public partial class SinglePlayerModePage : SidebarPage
    {
        protected override Type StyleKeyOverride => typeof(SidebarPage);

        public SinglePlayerModePage()
        {
            InitializeComponent();
        }
    }
}
