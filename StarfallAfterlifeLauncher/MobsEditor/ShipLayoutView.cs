using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.MobsEditor
{
    public class ShipLayoutView : UserControl
    {
        public static readonly StyledProperty<IList<HardpointInfo>> HardpointsProperty =
            AvaloniaProperty.Register<ShipLayoutView, IList<HardpointInfo>>(nameof(Hardpoints));

        public static readonly StyledProperty<double> CellSizeProperty =
            AvaloniaProperty.Register<ShipLayoutView, double>(nameof(CellSize), 32);

        public static readonly StyledProperty<bool> ShowTextProperty =
            AvaloniaProperty.Register<ShipLayoutView, bool>(nameof(CellSize), true);

        public IList<HardpointInfo> Hardpoints { get => GetValue(HardpointsProperty); set => SetValue(HardpointsProperty, value); }

        public double CellSize { get => GetValue(CellSizeProperty); set => SetValue(CellSizeProperty, value); }

        public bool ShowText { get => GetValue(ShowTextProperty); set => SetValue(ShowTextProperty, value); }

        protected List<HardpointInfo> HardpointsCache { get; set; }

        protected int HardpointsMinX { get; set; } = 0;

        protected int HardpointsMaxX { get; set; } = 0;

        protected int HardpointsMinY { get; set; } = 0;

        protected int HardpointsMaxY { get; set; } = 0;

        public override void Render(DrawingContext context)
        {
            var hardpoints = HardpointsCache ?? new();
            var cellSize = CellSize;

            if (cellSize <= 0)
                return;

            var minX = HardpointsMinX;
            var maxX = HardpointsMaxX;
            var minY = HardpointsMinY;
            var maxY = HardpointsMaxY;
            var cellPen = new Pen(Foreground, 1);
            var hpPen = new Pen(Foreground, 2);
            var origin = new Point(
                (Bounds.Width - Math.Abs(maxX - minX) * cellSize) / 2 - minX * cellSize,
                (Bounds.Height - Math.Abs(maxY - minY) * cellSize) / 2 - minY * cellSize);

            using (context.PushOpacity(0.1))
            {
                foreach (var hp in hardpoints)
                {
                    var cellRect = new Rect(
                                origin.X + hp.X * cellSize,
                                origin.Y + hp.Y * cellSize,
                                hp.Width * cellSize,
                                hp.Height * cellSize);

                    context.DrawRectangle(Foreground, null, cellRect);
                }
            }

            using (context.PushOpacity(0.1))
            {
                foreach (var hp in hardpoints)
                {
                    for (int y = 0; y < hp.Height; y++)
                    {
                        for (int x = 0; x < hp.Width; x++)
                        {
                            var cellRect = new Rect(
                                origin.X + (hp.X + x) * cellSize,
                                origin.Y + (hp.Y + y) * cellSize,
                                cellSize,
                                cellSize);

                            context.DrawRectangle(cellPen, cellRect);
                        }
                    }
                }
            }

            using (context.PushOpacity(0.5))
            {
                foreach (var hp in hardpoints)
                {
                    var cellRect = new Rect(
                                origin.X + hp.X * cellSize,
                                origin.Y + hp.Y * cellSize,
                                hp.Width * cellSize,
                                hp.Height * cellSize);

                    context.DrawRectangle(hpPen, cellRect);
                }
            }

            if (ShowText == true)
            {
                using (context.PushOpacity(0.5))
                {
                    var typeface = new Typeface(FontFamily, FontStyle, FontWeight);

                    foreach (var hp in hardpoints)
                    {
                        var name = hp.Type.ToString();

                        var text = new FormattedText(
                            name,
                            CultureInfo.CurrentCulture,
                            FlowDirection,
                            typeface,
                            FontSize,
                            Foreground);

                        var cellLoc = new Point(
                            origin.X + hp.X * cellSize,
                            origin.Y + hp.Y * cellSize);

                        context.DrawText(text, new Point(
                            cellLoc.X + ((hp.Width * cellSize) - text.Width) / 2,
                            cellLoc.Y - text.Height - 2));
                    }
                }
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == HardpointsProperty)
            {
                HardpointsCache = (change.NewValue as IList<HardpointInfo>)?.ToList();
                HardpointsMinX = HardpointsCache?.Min(h => h.X) ?? 0;
                HardpointsMaxX = HardpointsCache?.Max(h => h.X + h.Width) ?? 0;
                HardpointsMinY = HardpointsCache?.Min(h => h.Y) ?? 0;
                HardpointsMaxY = HardpointsCache?.Max(h => h.Y + h.Height) ?? 0;

                InvalidateVisual();
            }
            else if (change.Property == CellSizeProperty)
            {
                InvalidateVisual();
            }
        }
    }
}
