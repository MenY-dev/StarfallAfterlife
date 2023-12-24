using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SidebarContentView : Panel
    {
        public static readonly StyledProperty<double> ExpandProgressProperty =
            AvaloniaProperty.Register<SidebarContentView, double>(
                name: nameof(ExpandProgress),
                defaultValue: 0,
                defaultBindingMode: BindingMode.TwoWay);

        public double ExpandProgress
        {
            get => GetValue(ExpandProgressProperty);
            set => SetValue(ExpandProgressProperty, value);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var t = double.Clamp(ExpandProgress, 0, 1);
            var maxSize = MeasureOverride(new(double.MaxValue, finalSize.Height));
            return base.ArrangeOverride(new(finalSize.Width * (1 - t) + maxSize.Width * t, finalSize.Height));
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ExpandProgressProperty)
                UpdateLayout();
        }
    }
}
