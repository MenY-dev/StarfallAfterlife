using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class StaggeredPanel : Panel
    {
        public static readonly StyledProperty<int> MaxColumnsProperty =
            AvaloniaProperty.Register<StaggeredPanel, int>(nameof(MaxColumns));

        public static readonly StyledProperty<double> ColumnWidthProperty =
            AvaloniaProperty.Register<StaggeredPanel, double>(nameof(ColumnWidth));

        public int MaxColumns { get => GetValue(MaxColumnsProperty); set => SetValue(MaxColumnsProperty, value); }

        public double ColumnWidth { get => GetValue(ColumnWidthProperty); set => SetValue(ColumnWidthProperty, value); }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var maxWidth = 0.0;
            var maxHeight = 0.0;

            foreach (var child in CreateLayout(finalSize))
            {
                var rect = child.Value;
                child.Key.Arrange(rect);
                maxWidth = Math.Max(maxWidth, rect.Right);
                maxHeight = Math.Max(maxHeight, rect.Bottom);
            }

            return new Size(maxWidth, maxHeight);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var maxWidth = 0.0;
            var maxHeight = 0.0;

            foreach (var child in CreateLayout(availableSize))
            {
                var rect = child.Value;
                maxWidth = Math.Max(maxWidth, rect.Right);
                maxHeight = Math.Max(maxHeight, rect.Bottom);
            }

            return new Size(maxWidth, maxHeight);
        }

        protected List<KeyValuePair<Layoutable, Rect>> CreateLayout(Size finalSize)
        {
            var result = new List<KeyValuePair<Layoutable, Rect>>();
            var visualChildren = VisualChildren;
            var targetSize = finalSize;
            var maxWidth = 0.0;
            var maxColumns = MaxColumns;
            var layoutableCount = 0;

            if (IsSet(ColumnWidthProperty))
            {
                var targetColumnsCount = (int)Math.Floor(finalSize.Width / Math.Max(1, ColumnWidth));

                if (maxColumns > 0 && targetColumnsCount > maxColumns)
                    targetColumnsCount = maxColumns;

                targetSize = new Size(
                    Math.Max(0, finalSize.Width / Math.Max(1, targetColumnsCount)),
                    finalSize.Height);
            }

            for (var i = 0; i < visualChildren.Count; i++)
            {
                if (visualChildren[i] is Layoutable layoutable &&
                    layoutable.IsVisible == true)
                {
                    layoutable.Measure(targetSize);
                    maxWidth = Math.Max(maxWidth, layoutable.DesiredSize.Width);
                    layoutableCount++;
                }
            }

            if (IsSet(ColumnWidthProperty))
                maxWidth = ColumnWidth;

            var columnsCount = (int)Math.Floor(finalSize.Width / Math.Max(1, maxWidth));
            columnsCount = Math.Max(1, Math.Min(layoutableCount, columnsCount));

            if (maxColumns > 0 && columnsCount > maxColumns)
                columnsCount = maxColumns;

            var targetWidth = Math.Max(1, finalSize.Width / columnsCount);
            var columns = Enumerable.Range(0, columnsCount)
                .Select(i => KeyValuePair.Create(i, new Point(targetWidth * i, 0))).ToArray();

            for (var i = 0; i < visualChildren.Count; i++)
            {
                if (visualChildren[i] is Layoutable layoutable &&
                    layoutable.IsVisible == true)
                {
                    var targetColumn = columns.MinBy(p => p.Value.Y);
                    var start = targetColumn.Value;
                    var rect = new Rect(
                        start.X,
                        start.Y,
                        targetWidth,
                        layoutable.DesiredSize.Height);

                    result.Add(new KeyValuePair<Layoutable, Rect>(layoutable, rect));
                    columns[targetColumn.Key] = new(targetColumn.Key, rect.BottomLeft);
                }
            }

            return result;
        }
    }
}
