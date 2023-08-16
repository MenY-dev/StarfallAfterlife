using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static StarfallAfterlife.PathFindingTestForm;

namespace StarfallAfterlife
{
    public class PathFindingTestForm : Form
    {
        public struct Circle
        {
            public Vector2 Center;
            public float Radius;
        }

        public Vector2 PointA = new Vector2(50, 50), PointB = new Vector2(700, 700);

        public List<Circle> Circles = new()
        {
            new Circle {Center = new Vector2(100, 100), Radius = 25 },
            new Circle {Center = new Vector2(500, 200), Radius = 50 },
            new Circle {Center = new Vector2(150, 300), Radius = 100 },
            new Circle {Center = new Vector2(400, 400), Radius = 100 },
            new Circle {Center = new Vector2(320, 160), Radius = 100 },
            new Circle {Center = new Vector2(530, 630), Radius = 150 },
            new Circle {Center = new Vector2(220, 550), Radius = 100 },
            new Circle {Center = new Vector2(630, 350), Radius = 100 },
            new Circle {Center = new Vector2(660, 120), Radius = 100 },
            new Circle {Center = new Vector2(310, 710), Radius = 50 },
            new Circle {Center = new Vector2(80, 700), Radius = 75 },
            new Circle {Center = new Vector2(70, 460), Radius = 50 },
            new Circle {Center = new Vector2(730, 500), Radius = 50 },
        };

        public NavigationMap NavigationMap { get; protected set; } = new NavigationMap();

        public List<Vector2> Path { get; protected set; }
        public List<NavigationMap.Connection> DebugPath { get; protected set; }

        public void RebuildMap()
        {
            NavigationMap = NavigationMap.Create(
                Circles.Select(c => c.Center),
                Circles.Select(c => c.Radius));

            Path = NavigationMap.CalculatePath(PointA, PointB);
            DebugPath = NavigationMap.DebugPath;

            BeginInvoke(new Action(Refresh));
        }



        #region GUI

        public int EditablePoint = -1, EditableCircle = -1;
        float pointEditRadius = 10;
        Vector2 PointerOffset = new Vector2();

        public PathFindingTestForm()
        {
            InitializeComponent();
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint,
                true);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PathFindingTestForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 800);
            this.Name = "PathFindingTestForm";
            this.ResumeLayout(false);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RebuildMap();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var nodes = NavigationMap.Map;
            var path = Path;
            using var textBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
            using var bgBrush = new SolidBrush(Color.FromArgb(20, 35, 40));
            using var linePen = new Pen(Color.FromArgb(20, 80, 100), 1);
            using var circlePen = new Pen(Color.FromArgb(200, 35, 40), 2);
            using var circleEditPen = new Pen(Color.FromArgb(100, 200, 200), 4);
            using var pointAPen = new Pen(Color.FromArgb(20, 35, 200), 4);
            using var pointBPen = new Pen(Color.FromArgb(20, 200, 40), 4);
            using var pointEditPen = new Pen(Color.FromArgb(100, 200, 200), 6);

            e.Graphics.FillRectangle(bgBrush, e.ClipRectangle);

            foreach (var node in nodes)
            {
                foreach (var connection in node.Connections)
                {
                    e.Graphics.DrawLine(
                        linePen,
                        connection.Root.X, connection.Root.Y,
                        connection.Target.X, connection.Target.Y);
                }
            }

            for (int i = 0; i < Circles.Count; i++)
            {
                var circle = Circles[i];
                DrawCircle(e.Graphics, i == EditableCircle ? circleEditPen : circlePen, circle.Center, circle.Radius);

                e.Graphics.DrawString(
                    $"{i}:({circle.Center.X}, {circle.Center.Y})",
                    Font, textBrush, circle.Center.X, circle.Center.Y);
            }

            DrawCircle(
                e.Graphics,
                EditablePoint == 0 ? pointEditPen : pointAPen,
                PointA, 10);

            DrawCircle(
                e.Graphics,
                EditablePoint == 1 ? pointEditPen : pointBPen,
                PointB, 10);

            if (path is not null)
            {
                var points = path.Select(p => new PointF(p.X, p.Y)).ToArray();
                using var pathPen = new Pen(Color.FromArgb(20, 255, 100), 2);

                if (points.Length > 1)
                    e.Graphics.DrawLines(pathPen, points);
            }

            if (DebugPath is not null)
            {
                using var debugBrush = new SolidBrush(Color.FromArgb(255, 0, 255));
                using var debugNormalBrush = new SolidBrush(Color.FromArgb(255, 255, 255));

                foreach (var point in DebugPath.Select(c => c.Root))
                    e.Graphics.FillEllipse(debugBrush, point.X - 5, point.Y - 5, 10, 10);

                foreach (var point in DebugPath.Select(c => c.Target))
                    e.Graphics.FillEllipse(debugNormalBrush, point.X - 5, point.Y - 5, 10, 10);
            }
        }

        protected void DrawCircle(Graphics graphics, Pen pen, Vector2 location, float radius)
        {
            var center = location - radius;
            graphics.DrawEllipse(pen, center.X, center.Y, radius * 2, radius * 2);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var location = e.Location.ToSfVector2();

            if (location.GetDistanceTo(PointA) <= pointEditRadius)
            {
                EditablePoint = 0;
                PointerOffset = location - PointA;
            }
            else if (location.GetDistanceTo(PointB) <= pointEditRadius)
            {
                EditablePoint = 1;
                PointerOffset = location - PointB;
            }
            else
            {
                for (int i = 0; i < Circles.Count; i++)
                {
                    var circle = Circles[i];

                    if (location.GetDistanceTo(circle.Center) <= circle.Radius)
                    {
                        EditableCircle = i;
                        PointerOffset = location - circle.Center;
                        break;
                    }
                }
            }

            RebuildMap();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var location = e.Location.ToSfVector2();

            if (EditablePoint == 0)
            {
                PointA = location - PointerOffset;
                RebuildMap();
            }
            else if (EditablePoint == 1)
            {
                PointB = location - PointerOffset;
                RebuildMap();
            }
            else if (EditableCircle > -1 && EditableCircle < Circles.Count)
            {
                var circle = Circles[EditableCircle];
                circle.Center = location - PointerOffset;
                Circles[EditableCircle] = circle;
                RebuildMap();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            EditablePoint = -1;
            EditableCircle = -1;

            RebuildMap();
        }

        #endregion // GUI
    }

    public static class PointExtention
    {
        public static Vector2 ToSfVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }
    }
}
