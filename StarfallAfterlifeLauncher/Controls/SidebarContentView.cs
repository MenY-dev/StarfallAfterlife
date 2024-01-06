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

        private double _expandedWidth = 0;

        protected override Size ArrangeOverride(Size finalSize)
        {
            var t = double.Clamp(ExpandProgress, 0, 1);
            return base.ArrangeOverride(new(finalSize.Width * (1 - t) + _expandedWidth * t, finalSize.Height));
        }

        protected void UpdateExpandedWidth()
        {
            _expandedWidth = MeasureOverride(new(double.MaxValue, Bounds.Height)).Width;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ExpandProgressProperty)
            {
                UpdateExpandedWidth();
                UpdateLayout();
            }
        }
    }
}
