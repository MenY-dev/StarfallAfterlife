using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Launcher.MapEditor
{
    public class MapView : UserControl
    {
        public GalaxyMap Map
        {
            get => _map;
            set
            {
                _map = value;
                Offset = Vector.Zero;
                Scale = 0.001d;
                SelectedSystem = -1;
                SelectedHex = -1;
                Dispatcher.UIThread.Invoke(InvalidateVisual);
            }
        }

        public double Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                Dispatcher.UIThread.Invoke(InvalidateVisual);
            }
        }

        public Vector Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                Dispatcher.UIThread.Invoke(InvalidateVisual);
            }
        }

        public int SelectedSystem { get => _selectedSystem; protected set => _selectedSystem = value; }
        public int SelectedHex { get => _selectedHex; protected set => _selectedHex = value; }

        public event EventHandler<EventArgs> SystemChanged;
        public event EventHandler<EventArgs> HexChanged;

        private GalaxyMap _map;
        private double _scale = 0.001d;
        private Vector _offset;
        private bool _pointerPressed;
        private Point _pointerDownPos;
        private Point _lastPointerPos;
        private Typeface _typeFace = new Typeface("Arial");
        private int _selectedSystem = -1;
        private int _selectedHex = -1;
        private float _systemRadius = 600;

        private Brush bgBrush = new SolidColorBrush(Colors.Transparent);
        private Brush textBrush = new SolidColorBrush(Colors.White);
        private Brush dBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        private Brush eBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
        private Brush vBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        private Brush neutralBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
        private Brush screechersBrush = new SolidColorBrush(Color.FromArgb(255, 255, 50, 0));
        private Brush nebulordsBrush = new SolidColorBrush(Color.FromArgb(255, 20, 60, 255)); 
        private Brush pyramidBrush = new SolidColorBrush(Color.FromArgb(255, 120, 0, 200));
        private Brush freeTradersBrush = new SolidColorBrush(Color.FromArgb(200, 30, 255, 0));
        private Brush scientistsBrush = new SolidColorBrush(Color.FromArgb(255, 190, 210, 210));
        private Brush mineworkerUnionBrush = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        private Brush asteroidsBrush = new SolidColorBrush(Colors.White);
        private Brush nebulaBrush = new LinearGradientBrush()
        {
            StartPoint = new(0, 0, RelativeUnit.Absolute),
            EndPoint = new(8.6, 5, RelativeUnit.Absolute),
            SpreadMethod = GradientSpreadMethod.Repeat,
            GradientStops = new GradientStops
            {
                new GradientStop(Colors.White, 0),
                new GradientStop(Colors.White, 0.24999),
                new GradientStop(Colors.Transparent, 0.25),
                new GradientStop(Colors.Transparent, 1),
            },
        };

        public MapView()
        {
            IsHitTestVisible = true;

        }

        private Brush GetBrush(Faction faction) => faction switch
        {
            Faction.Deprived => dBrush,
            Faction.Eclipse => eBrush,
            Faction.Vanguard => vBrush,
            Faction.Screechers => screechersBrush,
            Faction.Nebulords => nebulordsBrush,
            Faction.Pyramid => pyramidBrush,
            Faction.FreeTraders => freeTradersBrush,
            Faction.Scientists => scientistsBrush,
            Faction.MineworkerUnion => mineworkerUnionBrush,
            _ => neutralBrush
        };


        public override void Render(DrawingContext context)
        {
            var scale = Scale;
            var map = Map;
            var systems = Map?.Systems;

            if (map is null || systems is null || scale <= 0)
            {
                base.Render(context);
                return;
            }

            var textPen = new Pen(textBrush, 1 / Scale);
            var transform = CreateTransformMatrix();

            context.FillRectangle(bgBrush, Bounds, 0);
            var push = context.PushTransform(transform);

            using (context.PushOpacity(0.5d))
            {
                for (int i = 0; i < systems.Count; i++)
                {
                    var system = systems[i];
                    var edges = system.TerritoryEdges;

                    if (system is null ||
                        system.Faction == Faction.None ||
                        edges is null)
                        continue;

                    var points = new List<Point>();

                    for (int n = 0; n < edges.Count; n++)
                    {
                        var edge = edges[n];

                        if (edge is null)
                            continue;

                        var neighbor = map.GetSystem(edge.Neighbor);

                        if (neighbor is null)
                            continue;

                        points.Add(new Point(edge.Y, -(edge.X)));
                    }

                    context.DrawGeometry(GetBrush((Faction)system.Faction), null, new PolylineGeometry(points, true));
                }
            }

            using (context.PushOpacity(0.25d))
            {
                if (scale > 0.01 && scale < 0.1)
                {
                    for (int i = 0; i < systems.Count; i++)
                    {
                        var system = systems[i];

                        if (system is null)
                            continue;

                        var connections = system.Portals;

                        if (connections is null)
                            continue;

                        var pos = new Point(system.Y, -system.X);

                        for (int n = 0; n < connections.Count; n++)
                        {
                            var con = map.GetSystem(connections[n]?.Destination ?? -1);

                            if (con is null)
                                continue;

                            var target = new Point(con.Y, -con.X);

                            if (Bounds.Contains(transform.Transform(pos)) == false &&
                                Bounds.Contains(transform.Transform(target)) == false)
                                continue;

                            context.DrawLine(textPen, pos, target);
                        }
                    }
                }
            }


            if (scale > 0.1)
            {
                RenderSystems(context, transform);
            }

            for (int i = 0; i < systems.Count; i++)
            {
                var system = systems[i];

                if (system is null)
                    continue;

                var pos = new Point(system.Y, -system.X);

                if (Bounds.Contains(transform.Transform(pos)) == false)
                    continue;

                context.DrawEllipse(textBrush, null, pos, 2 / scale, 2 / scale);

                if (scale < 0.1 && system.Id == SelectedSystem)
                {
                    context.DrawEllipse(null, textPen, pos, 20 / scale, 20 / scale);
                }
            }

            if (scale > 0.01)
            {
                for (int i = 0; i < systems.Count; i++)
                {
                    var system = systems[i];

                    if (system is null)
                        continue;

                    var pos = new Point(system.Y, -system.X);
                    var name = system.Name ?? "";
                    var absolutePos = transform.Transform(pos);

                    if (Bounds.Contains(absolutePos) == false)
                        continue;

                    var text = new FormattedText(
                        name,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeFace,
                        12 / scale,
                        textBrush);

                    pos = new Point(pos.X - text.Width / 2, pos.Y - text.Height / 2 - 10 / scale);

                    context.DrawText(text, pos);
                }
            }

            push.Dispose();
        }

        protected void RenderSystems(DrawingContext context, Matrix transform)
        {
            var scale = Scale;
            var map = Map;
            var systems = Map?.Systems;

            if (map is null || systems is null || scale <= 0)
                return;

            var textPen = new Pen(textBrush, 1 / Scale);

            for (int i = 0; i < systems.Count; i++)
            {
                var system = systems[i];

                if (system is null)
                    continue;

                var pos = new Point(system.Y, -system.X);
                var absolutePos = transform.Transform(pos);

                if (Bounds.Inflate(_systemRadius * 2 * scale).Contains(absolutePos) == false)
                    continue;

                var points = SystemHex.Sides
                    .Select(h => SystemHexMap.HexToSystemPoint(h * (int)_systemRadius))
                    .Select(p => new Point(pos.X + p.Y, pos.Y - p.X)).ToList();

                context.DrawGeometry(null, textPen, new PolylineGeometry(points, true));
                RenderSystemContent(system, context, transform);
            }
        }

        protected void RenderSystemContent(GalaxyMapStarSystem system, DrawingContext context, Matrix transform)
        {
            var scale = Scale;
            var map = Map;

            if (map is null || system is null || scale <= 0)
                return;

            var textPen = new Pen(textBrush, 1 / Scale);

            if (system.Id == SelectedSystem)
            {
                using (context.PushOpacity(0.25))
                {
                    var hexes = Enumerable
                    .Range(0, SystemHexMap.HexesCount);

                    foreach (var item in hexes)
                    {
                        var localPoint = SystemHexMap.ArrayIndexToSystemPoint(item);
                        var point = new Point(
                            system.Y + localPoint.Y * (_systemRadius * 0.06),
                            -system.X - localPoint.X * (_systemRadius * 0.06));

                        var corners = SystemHex.Sides
                            .Select(h => SystemHexMap.HexToSystemPoint(h))
                            .Select(p => new Point(
                                point.X + p.X * SystemHexMap.SystemHexSizeY * (_systemRadius * 0.02),
                                point.Y - p.Y * SystemHexMap.SystemHexSizeY * (_systemRadius * 0.02)))
                            .ToList();

                        context.DrawGeometry(
                            item == SelectedHex ? textBrush : null,
                            textPen,
                            new PolylineGeometry(corners, true));
                    }
                }
            }

            if (SelectedHex > -1 && system.Id == SelectedSystem)
            {
                var localPoint = SystemHexMap.ArrayIndexToSystemPoint(SelectedHex);
                var point = new Point(
                    system.Y + localPoint.Y * (_systemRadius * 0.06),
                    -system.X - localPoint.X * (_systemRadius * 0.06));

                var corners = SystemHex.Sides
                    .Select(h => SystemHexMap.HexToSystemPoint(h))
                    .Select(p => new Point(
                        point.X + p.X * SystemHexMap.SystemHexSizeY * (_systemRadius * 0.02),
                        point.Y - p.Y * SystemHexMap.SystemHexSizeY * (_systemRadius * 0.02)))
                    .ToList();

                context.DrawGeometry(null, textPen, new PolylineGeometry(corners, true));
            }

            var asteroidsMap = new SystemHexMap(system.AsteroidsMask);
            var nebulaMap = new SystemHexMap(system.NebulaMask);

            using (context.PushOpacity(0.25))
            {
                for (int i = 0; i < SystemHexMap.HexesCount; i++)
                {
                    if (asteroidsMap[i] == false)
                        continue;

                    var localPoint = SystemHexMap.ArrayIndexToSystemPoint(i);
                    var point = new Point(
                        system.Y + localPoint.Y * (_systemRadius * 0.06),
                        -system.X - localPoint.X * (_systemRadius * 0.06));

                    var corners = SystemHex.Sides
                        .Select(h => SystemHexMap.HexToSystemPoint(h))
                        .Select(p => new Point(
                            point.X + p.X * SystemHexMap.SystemHexSizeY * (_systemRadius * 0.02),
                            point.Y - p.Y * SystemHexMap.SystemHexSizeY * (_systemRadius * 0.02)))
                        .ToList();

                    context.DrawGeometry(asteroidsBrush, null, new PolylineGeometry(corners, true));
                }
            }

            using (context.PushOpacity(0.25))
            {
                for (int i = 0; i < SystemHexMap.HexesCount; i++)
                {
                    if (nebulaMap[i] == false)
                        continue;

                    var localPoint = SystemHexMap.ArrayIndexToSystemPoint(i);
                    var point = new Point(
                        system.Y + localPoint.Y * (_systemRadius * 0.06),
                        -system.X - localPoint.X * (_systemRadius * 0.06));

                    var corners = SystemHex.Sides
                        .Select(h => SystemHexMap.HexToSystemPoint(h))
                        .Select(p => new Point(
                            point.X + p.X * SystemHexMap.SystemHexSizeY * (_systemRadius * 0.02),
                            point.Y - p.Y * SystemHexMap.SystemHexSizeY * (_systemRadius * 0.02)))
                        .ToList();

                    var geometry = new PolylineGeometry(corners, true);
                    context.DrawGeometry(nebulaBrush, null, geometry);
                }
            }

            var pos = new Point(system.Y, -system.X);
            var radius = system.Size / 9;
            context.DrawEllipse(null, textPen, pos, radius, radius);

            RenderObjects(system.Planets, system, context, transform);
            RenderObjects(system.PiratesOutposts, system, context, transform);
            RenderObjects(system.PiratesStations, system, context, transform);
            RenderObjects(system.TradeStations, system, context, transform);
            RenderObjects(system.RepairStations, system, context, transform);
            RenderObjects(system.FuelStations, system, context, transform);
            RenderObjects(system.ScienceStations, system, context, transform);
            RenderObjects(system.MinerMotherships, system, context, transform);
            RenderObjects(system.Motherships, system, context, transform);
            RenderObjects(system.QuickTravalGates, system, context, transform);
            RenderObjects(system.RichAsteroids, system, context, transform);
            RenderObjects(system.Portals, system, context, transform);
        }

        protected void RenderObjects<T>(List<T> objs, GalaxyMapStarSystem system, DrawingContext context, Matrix transform) where T : IGalaxyMapObject
        {
            if (objs is null)
                return;

            for (int i = 0; i < objs.Count; i++)
            {
                RenderSystemObject(objs[i], system, context, transform);
            }
        }

        protected void RenderSystemObject(IGalaxyMapObject obj, GalaxyMapStarSystem system, DrawingContext context, Matrix transform)
        {
            var scale = Scale;
            var map = Map;

            if (obj is null || map is null || system is null || scale <= 0)
                return;

            var textPen = new Pen(textBrush, 1 / Scale);
            var localPos = SystemHexMap.HexToSystemPoint(obj.X, obj.Y) * 36;
            var pos = new Point(system.Y + localPos.Y, -(system.X + localPos.X));
            var systemUnit = 1 / (_systemRadius * 0.03);
            if (Bounds.Contains(transform.Transform(pos)) == false)
                return;

            if (obj is GalaxyMapPlanet planet)
            {
                var radius = planet.Size * systemUnit;
                context.DrawEllipse(textBrush, null, pos, 2 / scale, 2 / scale);
                context.DrawEllipse(null, textPen, pos, radius, radius);

                var text = new FormattedText(
                        planet.Name ?? string.Empty,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeFace,
                        12 / scale,
                        textBrush);

                var textPos = new Point(pos.X - text.Width / 2, pos.Y - text.Height / 2 - 20 / scale);

                context.DrawText(text, textPos);
            }
            else if (obj is GalaxyMapPiratesStation station)
            {
                context.DrawEllipse(textBrush, null, pos, 2 / scale, 2 / scale);

                var points = new Point[]
                {
                    new(pos.X, pos.Y - systemUnit * 700),
                    new(pos.X - systemUnit * 600, pos.Y + systemUnit * 400),
                    new(pos.X + systemUnit * 600, pos.Y + systemUnit * 400),
                };

                context.DrawGeometry(
                    null,
                    textPen,
                    new PolylineGeometry(points, true));

                var text = new FormattedText(
                        $"{station.ObjectType}({station.Level})",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeFace,
                        12 / scale,
                        textBrush);

                var textPos = new Point(pos.X - text.Width / 2, pos.Y - text.Height / 2 - 20 / scale);
                context.DrawText(text, textPos);
            }
            else if (obj is GalaxyMapRichAsteroid richAsteroid)
            {
                var radius = systemUnit * 100;
                context.DrawEllipse(null, textPen, pos + new Point(-300 * systemUnit, 150 * systemUnit), radius, radius);
                context.DrawEllipse(null, textPen, pos + new Point(400 * systemUnit, -100 * systemUnit), radius, radius);
                context.DrawEllipse(null, textPen, pos + new Point(200 * systemUnit, 200 * systemUnit), radius, radius);
                context.DrawEllipse(null, textPen, pos + new Point(-100 * systemUnit, 0 * systemUnit), radius, radius);
                context.DrawEllipse(null, textPen, pos + new Point(-200 * systemUnit, -300 * systemUnit), radius, radius);
                context.DrawEllipse(null, textPen, pos + new Point(-100 * systemUnit, 400 * systemUnit), radius, radius);
                context.DrawEllipse(null, textPen, pos + new Point(250 * systemUnit, -300 * systemUnit), radius, radius);
            }
            else if (obj is GalaxyMapPortal portal)
            {
                var radius = systemUnit * 500;
                var radius2 = systemUnit * 300;
                var offset = 200 * systemUnit;
                context.DrawEllipse(null, textPen, pos, radius, radius);
                context.DrawEllipse(null, textPen, pos + new Point(-offset, 0), radius2, radius2);
                context.DrawEllipse(null, textPen, pos + new Point(0, -offset), radius2, radius2);
                context.DrawEllipse(null, textPen, pos + new Point(offset, 0), radius2, radius2);
                context.DrawEllipse(null, textPen, pos + new Point(0, offset), radius2, radius2);
                context.DrawEllipse(textBrush, null, pos, 40 * systemUnit, 40 * systemUnit);

                var text = new FormattedText(
                        $"To {map.GetSystem(portal.Destination)?.Name}",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeFace,
                        12 / scale,
                        textBrush);

                var textPos = new Point(pos.X - text.Width / 2, pos.Y - text.Height / 2 - 20 / scale);
                context.DrawText(text, textPos);
            }
            else
            {
                context.DrawEllipse(textBrush, null, pos, 2 / scale, 2 / scale);

                var offset = 1000 * systemUnit;
                var points = new Point[]
                {
                    new(pos.X, pos.Y - offset),
                    new(pos.X - offset, pos.Y),
                    new(pos.X, pos.Y + offset),
                    new(pos.X + offset, pos.Y),
                };

                context.DrawGeometry(
                    null,
                    textPen,
                    new PolylineGeometry(points, true));

                var text = new FormattedText(
                        $"{obj.ObjectType}",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeFace,
                        12 / scale,
                        textBrush);

                var textPos = new Point(pos.X - text.Width / 2, pos.Y - text.Height / 2 - 20 / scale);
                context.DrawText(text, textPos);
            }
        }

        public Matrix CreateTransformMatrix()
        {
            var transform = Matrix.Identity;
            transform = transform.Append(Matrix.CreateScale(Scale, Scale));
            transform = transform.Append(Matrix.CreateTranslation(Offset));
            transform = transform.Append(Matrix.CreateTranslation(Bounds.Width / 2, Bounds.Height / 2));
            return transform;
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);

            var newScale = e.Delta.Y < 0 ? Scale / 1.25 : Scale * 1.25;
            newScale = Math.Min(1, Math.Max(0.0005d, newScale));
            var totalDelta = newScale / Scale - 1;
            var origin = (_lastPointerPos - Offset - new Vector(Bounds.Width / 2, Bounds.Height / 2)) * totalDelta;
            Offset -= origin;
            Scale = newScale;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            var pointer = e.GetCurrentPoint(this);

            if (pointer.Properties.IsLeftButtonPressed == true)
            {
                _lastPointerPos = pointer.Position;
                _pointerDownPos = pointer.Position;
                _pointerPressed = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            var pointer = e.GetCurrentPoint(this);

            if (_pointerPressed == true &&
                pointer.Properties.IsLeftButtonPressed == false)
            {
                _pointerPressed = false;

                if (Scale > 0 &&
                    ((Vector)(_pointerDownPos - pointer.Position)).Length < 5 &&
                    Map?.Systems is List<GalaxyMapStarSystem> systems &&
                    CreateTransformMatrix().TryInvert(out var transform) == true)
                {
                    GalaxyMapStarSystem neerestSystem = null;
                    var smallestDistance = double.MaxValue;
                    var pointerPos = transform.Transform(pointer.Position);

                    for (int i = 0; i < systems.Count; i++)
                    {
                        var system = systems[i];

                        if (system is null)
                            continue;

                        var pos = new Vector(system.Y, -system.X);
                        var distanceToPointer = (pos - pointerPos).Length;

                        if (distanceToPointer < smallestDistance)
                        {
                            smallestDistance = distanceToPointer;
                            neerestSystem = system;
                        }
                    }

                    SelectSystem(neerestSystem?.Id ?? -1);

                    if (Scale > 0.1 &&
                        smallestDistance < 3000 &&
                        neerestSystem is not null)
                    {
                        var localPointer = new Vector2(
                            -((float)pointerPos.Y + neerestSystem.X) / (_systemRadius * 0.06f),
                            ((float)pointerPos.X - neerestSystem.Y) / (_systemRadius * 0.06f));

                        var hex = SystemHexMap.SystemPointToHex(localPointer);

                        SelectHex(hex.GetSize() > 16 ? -1 : SystemHexMap.HexToArrayIndex(hex));
                    }
                    else
                    {
                        SelectHex(-1);
                    }
                }
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            var pointer = e.GetCurrentPoint(this);
            var delta = _lastPointerPos - pointer.Position;
            _lastPointerPos = pointer.Position;

            if (_pointerPressed == true)
            {
                Offset -= delta;
            }
        }

        void SelectSystem(int systemId)
        {
            SelectedSystem = systemId;
            Trace.WriteLine("system:" + systemId);
            Dispatcher.UIThread.Invoke(() =>
            {
                InvalidateVisual();
                SystemChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        void SelectHex(int hexId)
        {
            SelectedHex = hexId;
            Trace.WriteLine($"hex:{SystemHexMap.ArrayIndexToHex(hexId)}({hexId})");
            Dispatcher.UIThread.Invoke(() =>
            {
                InvalidateVisual();
                HexChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
