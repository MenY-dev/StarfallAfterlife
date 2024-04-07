using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using StarfallAfterlife.Bridge.Codex;
using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class ShipSlotsView : UserControl
    {
        protected record struct SlotInfo(string Name, TechType Type, int X, int Y, int Width, int Height);

        public static readonly StyledProperty<object> SlotsProperty =
            AvaloniaProperty.Register<ShipSlotsView, object>(nameof(Slots));

        public static readonly StyledProperty<double> CellSizeProperty =
            AvaloniaProperty.Register<ShipSlotsView, double>(nameof(CellSize), 26);

        public static readonly StyledProperty<bool> ShowTextProperty =
            AvaloniaProperty.Register<ShipSlotsView, bool>(nameof(ShowText), true);

        public static readonly StyledProperty<bool> FitSlotsProperty =
            AvaloniaProperty.Register<ShipSlotsView, bool>(nameof(FitSlots), false);

        public object Slots { get => GetValue(SlotsProperty); set => SetValue(SlotsProperty, value); }

        public double CellSize { get => GetValue(CellSizeProperty); set => SetValue(CellSizeProperty, value); }

        public bool ShowText { get => GetValue(ShowTextProperty); set => SetValue(ShowTextProperty, value); }

        public bool FitSlots { get => GetValue(FitSlotsProperty); set => SetValue(FitSlotsProperty, value); }

        protected int _hardpointsMinX = 0;
        protected int _hardpointsMaxX = 0;
        protected int _hardpointsMinY = 0;
        protected int _hardpointsMaxY = 0;
        protected List<SlotInfo> _slotsData;


        public override void Render(DrawingContext context)
        {
            var hardpoints = _slotsData ?? new();
            var cellSize = CalculateCellSize(Bounds.Size);
            var minX = _hardpointsMinX;
            var maxX = _hardpointsMaxX;
            var minY = _hardpointsMinY;
            var maxY = _hardpointsMaxY;

            if (cellSize <= 0)
                return;

            var cellPen = new Pen(Foreground, 1);
            var hpPen = new Pen(Foreground, 2);
            var origin = new Point(
                (Bounds.Width - Math.Abs(maxX - minX) * cellSize) / 2 - minX * cellSize,
                (Bounds.Height - Math.Abs(maxY - minY) * cellSize + FontSize) / 2 - minY * cellSize);

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
                        var text = new FormattedText(
                            hp.Name,
                            CultureInfo.CurrentCulture,
                            FlowDirection,
                            typeface,
                            FontSize,
                            Foreground);

                        var cellLoc = new Point(
                            origin.X + hp.X * cellSize,
                            origin.Y + hp.Y * cellSize);

                        var xPos = cellLoc.X + ((hp.Width * cellSize) - text.Width) / 2;
                        var yPos = cellLoc.Y - text.Height - 2;

                        if (xPos < 0)
                            xPos = 0;

                        if (Bounds.Width < xPos + text.Width)
                            xPos = Bounds.Width - text.Width;

                        context.DrawText(text, new Point(xPos, yPos));
                    }
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var slotsSize = CalculateSlotsSize(availableSize).Inflate(Padding);
            var contentSize = base.MeasureOverride(availableSize);

            if (FitSlots == false)
                slotsSize.Inflate(Padding);

            return new Size(
                Math.Max(slotsSize.Width, contentSize.Width),
                Math.Max(slotsSize.Height, contentSize.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var slotsSize = CalculateSlotsSize(finalSize);
            var contentSize = base.ArrangeOverride(finalSize);
            var size = new Size(
                Math.Max(slotsSize.Width, contentSize.Width),
                Math.Max(slotsSize.Height, contentSize.Height));

            return size;
        }

        protected Size CalculateSlotsSize(Size availableSize)
        {
            var cellSize = CalculateCellSize(availableSize);
            var topLabelSize = FontSize;

            return new Size(
                (_hardpointsMaxX - _hardpointsMinX) * cellSize,
                (_hardpointsMaxY - _hardpointsMinY) * cellSize + topLabelSize);
        }

        protected double CalculateCellSize(Size availableSize)
        {
            if (FitSlots == true)
            {
                int xCount = Math.Max(_hardpointsMaxX - _hardpointsMinX, 1);
                int yCount = Math.Max(_hardpointsMaxY - _hardpointsMinY, 1);
                var bounds = availableSize.Deflate(Padding);
                double xSize = bounds.Width / xCount;
                double ySize = bounds.Height / yCount;
                return Math.Min(CellSize, Math.Min(xSize, ySize));
            }
            else
            {
                return CellSize;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SlotsProperty)
            {
                if (change.OldValue is INotifyCollectionChanged oldNotifyCollection)
                    oldNotifyCollection.CollectionChanged -= SlotsCollectionChanged;

                if (change.NewValue is INotifyCollectionChanged newNotifyCollection)
                    newNotifyCollection.CollectionChanged += SlotsCollectionChanged;

                HandleNewSlots(change.NewValue);
            }
            else if (change.Property == CellSizeProperty ||
                     change.Property == ShowTextProperty)
            {
                InvalidateVisual();
            }
        }

        private void SlotsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HandleNewSlots(sender);
        }

        protected void HandleNewSlots(object slots)
        {
            if (slots is IEnumerable<SfCodexTypes.HardpointComponent> codexHardpoints)
                _slotsData = codexHardpoints.Select(h => new SlotInfo(
                    (App.GetString("s_type_tech_type_" + h.Type) ?? h.Type.ToString()).ToUpper(), 
                    h.Type, h.GridX, h.GridY, h.Width, h.Height)).ToList();
            else
                _slotsData = null;

            if (_slotsData is not null &&
                _slotsData.Count > 0)
            {
                _hardpointsMinX = _slotsData.Min(h => h.X);
                _hardpointsMaxX = _slotsData.Max(h => h.X + h.Width);
                _hardpointsMinY = _slotsData.Min(h => h.Y);
                _hardpointsMaxY = _slotsData.Max(h => h.Y + h.Height);
            }
            else
            {
                _hardpointsMinX = _hardpointsMaxX = 0;
                _hardpointsMinY = _hardpointsMaxY = 0;
            }

            InvalidateVisual();
        }
    }
}
