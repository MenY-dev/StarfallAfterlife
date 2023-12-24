using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaMenuFlyout : MenuFlyout
    {
        public static readonly StyledProperty<ICommand> ItemSelectCommandProperty =
            AvaloniaProperty.Register<SfaMenuFlyout, ICommand>(nameof(ItemSelectCommand));

        public ICommand ItemSelectCommand
        {
            get => GetValue(ItemSelectCommandProperty);
            set => SetValue(ItemSelectCommandProperty, value);
        }

        protected override Control CreatePresenter()
        {
            return new SfaMenuFlyoutPresenter
            {
                Flyout = this,
                ItemsSource = Items,
                [!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty],
                [!ItemsControl.ItemContainerThemeProperty] = this[!ItemContainerThemeProperty],
            };
        }
    }
}
