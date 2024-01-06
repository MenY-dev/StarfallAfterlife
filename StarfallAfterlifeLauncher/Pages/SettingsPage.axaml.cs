using Avalonia;
using Avalonia.Controls;
using StarfallAfterlife.Launcher.Controls;
using StarfallAfterlife.Launcher.MapEditor;
using StarfallAfterlife.Launcher.MobsEditor;
using StarfallAfterlife.Launcher.ViewModels;
using System;

namespace StarfallAfterlife.Launcher.Pages
{
    public partial class SettingsPage : SidebarPage
    {
        protected override Type StyleKeyOverride => typeof(SidebarPage);

        public SettingsPage()
        {
            InitializeComponent();

#if DEBUG
            DebugUtilsView?.Children.Add(new SfaButton()
            {
                Content = "OPEN MAP EDITOR",
                Command = new Command(() => new MapEditorWindow().Show()),
                Margin = new Thickness(3),
            });

            DebugUtilsView?.Children.Add(new SfaButton()
            {
                Content = "OPEN MOBS EDITOR",
                Command = new Command(() => new MobsEditorWindow().Show()),
                Margin = new Thickness(3),
            });
#endif
        }
    }
}
