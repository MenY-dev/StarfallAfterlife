using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Utils;
using Avalonia.Interactivity;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaMenuItem : MenuItem
    {
        public SfaMenuFlyout Flyout { get; set; }

        protected override void OnClick(RoutedEventArgs e)
        {
            if (e.Handled == false)
            {
                Flyout?.ItemSelectCommand?.Execute(DataContext);
            }

            base.OnClick(e);
        }
    }
}
