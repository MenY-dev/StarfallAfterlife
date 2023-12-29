using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaMenuFlyoutPresenter : MenuFlyoutPresenter
    {
        public SfaMenuFlyout Flyout { get; set; }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new SfaMenuItem()
            {
                Flyout = Flyout,
            };
        }

        public virtual void UpdateSelection(object selectedItem)
        {
            foreach (var item in this.GetVisualDescendants()
                                     .OfType<SfaMenuItem>())
            {
                item.IsChecked = item.DataContext == selectedItem;
            }
        }
    }
}
