using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Utils;
using Avalonia.Data;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace StarfallAfterlife.Launcher.Controls
{
    [PseudoClasses(":checked")]
    public class SfaMenuItem : MenuItem
    {
        public SfaMenuFlyout Flyout { get; set; }

        public static readonly StyledProperty<bool> IsCheckedProperty =
            AvaloniaProperty.Register<SfaMenuFlyout, bool>(nameof(IsChecked), false);

        public bool IsChecked
        {
            get => GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        protected override void OnClick(RoutedEventArgs e)
        {
            if (e.Handled == false && Flyout is SfaMenuFlyout flyout)
            {
                flyout.ItemSelectCommand?.Execute(DataContext);
                flyout.SelectedItem = DataContext;
            }

            base.OnClick(e);
        }
    }
}
