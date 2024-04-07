using Avalonia.Controls;
using StarfallAfterlife.Launcher.Controls;
using System;

namespace StarfallAfterlife.Launcher.Pages
{
    public partial class CodexPage : SidebarPage
    {
        protected override Type StyleKeyOverride => typeof(SidebarPage);

        public CodexPage()
        {
            InitializeComponent();
        }
    }
}
