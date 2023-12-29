using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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

        public static readonly StyledProperty<object> SelectedItemProperty =
            AvaloniaProperty.Register<SfaMenuFlyout, object>(nameof(SelectedItem), defaultBindingMode: BindingMode.TwoWay);

        public ICommand ItemSelectCommand
        {
            get => GetValue(ItemSelectCommandProperty);
            set => SetValue(ItemSelectCommandProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private SfaMenuFlyoutPresenter _presenter;

        protected override Control CreatePresenter()
        {
            return _presenter = new SfaMenuFlyoutPresenter
            {
                Flyout = this,
                ItemsSource = Items,
                [!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty],
                [!ItemsControl.ItemContainerThemeProperty] = this[!ItemContainerThemeProperty],
            };
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SelectedItemProperty)
            {
                _presenter?.UpdateSelection(SelectedItem);
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            _presenter?.UpdateSelection(SelectedItem);
        }
    }
}
