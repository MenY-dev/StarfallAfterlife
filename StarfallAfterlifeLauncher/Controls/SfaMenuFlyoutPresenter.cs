using Avalonia.Controls;
using Avalonia.Controls.Platform;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaMenuFlyoutPresenter : MenuFlyoutPresenter
    {
        public SfaMenuFlyout Flyout { get; set; }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new SfaMenuItem() { Flyout = Flyout };
        }
    }
}
