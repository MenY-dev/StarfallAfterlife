using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System.Linq;
using static StarfallAfterlife.Bridge.Networking.SFCP;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class AppHeader : UserControl
    {
        private const double _barWidth = 340d;
        private const double _barHeight = 10d;

        public static readonly StyledProperty<PathGeometry> BackgroundGeometryProperty =
            AvaloniaProperty.Register<AppHeader, PathGeometry>(
                name: nameof(BackgroundGeometry),
                defaultValue: null,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<PathGeometry> UnderlineGeometryProperty =
            AvaloniaProperty.Register<AppHeader, PathGeometry>(
                name: nameof(UnderlineGeometry),
                defaultValue: null,
                defaultBindingMode: BindingMode.TwoWay);

        public PathGeometry BackgroundGeometry
        {
            get => GetValue(BackgroundGeometryProperty);
            set => SetValue(BackgroundGeometryProperty, value);
        }

        public PathGeometry UnderlineGeometry
        {
            get => GetValue(UnderlineGeometryProperty);
            set => SetValue(UnderlineGeometryProperty, value);
        }

        public AppHeader()
        {
            InitializeComponent();
            UpdateGeometry();
        }

        public virtual void UpdateGeometry()
        {
            var rect = new Rect(Bounds.Size);
            var barWidth = _barWidth;
            var barHeight = _barHeight;

            BackgroundGeometry = new PathGeometry()
            {
                Figures = new()
                {
                     CreateBackgroundFigure(rect, barWidth, barHeight)
                }
            };

            UnderlineGeometry = new PathGeometry()
            {
                Figures = new()
                {
                     new PathFigure()
                     {
                         StartPoint = new Point(rect.Left, rect.Bottom - barHeight),
                         IsClosed = false,
                         Segments = new()
                         {
                             new LineSegment(){ Point = new Point(rect.Center.X - barWidth * 0.5d, rect.Bottom - barHeight) },
                             new LineSegment(){ Point = new Point(rect.Center.X - barWidth * 0.5d + barHeight, rect.Bottom) },
                             new LineSegment(){ Point = new Point(rect.Center.X + barWidth * 0.5d - barHeight, rect.Bottom) },
                             new LineSegment(){ Point = new Point(rect.Center.X + barWidth * 0.5d, rect.Bottom - barHeight) },
                             new LineSegment(){ Point = new Point(rect.Right, rect.Bottom - barHeight) },
                         }
                     }
                }
            };
        }

        public PathFigure CreateBackgroundFigure(Rect bounds, double barWidth, double barHeight, Matrix matrix = default)
        {
            if (matrix == default)
                matrix = Matrix.Identity;

            return new PathFigure()
            {
                StartPoint = matrix.Transform(bounds.TopLeft),
                IsClosed = true,
                Segments = new()
                {
                    new LineSegment(){ Point = matrix.Transform(new Point(bounds.Right, bounds.Top)) },
                    new LineSegment(){ Point = matrix.Transform(new Point(bounds.Right, bounds.Bottom - barHeight)) },
                    new LineSegment(){ Point = matrix.Transform(new Point(bounds.Center.X + barWidth * 0.5d, bounds.Bottom - barHeight)) },
                    new LineSegment(){ Point = matrix.Transform(new Point(bounds.Center.X + barWidth * 0.5d - barHeight, bounds.Bottom)) },
                    new LineSegment(){ Point = matrix.Transform(new Point(bounds.Center.X - barWidth * 0.5d + barHeight, bounds.Bottom)) },
                    new LineSegment(){ Point = matrix.Transform(new Point(bounds.Center.X - barWidth * 0.5d, bounds.Bottom - barHeight)) },
                    new LineSegment(){ Point = matrix.Transform(new Point(bounds.Left, bounds.Bottom - barHeight)) },
                }
            };
        }

        public Geometry CreateMaskGeometry(Visual target)
        {
            if (target is null)
                return null;

            var matrix = this.TransformToVisual(target) ?? Matrix.Identity;
            var size = target.Bounds.Size;

            return new PathGeometry()
            {
                Figures = new()
                {
                    new PathFigure()
                    {
                        IsClosed = true,
                        Segments = new()
                        {
                            new LineSegment(){ Point = new Point(size.Width, 0) },
                            new LineSegment(){ Point = new Point(size.Width, size.Height) },
                            new LineSegment(){ Point = new Point(0, size.Height) },
                        }
                    },
                    CreateBackgroundFigure(new Rect(Bounds.Size), _barWidth, _barHeight, matrix)
                }
            };
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateGeometry();
        }
    }
}
