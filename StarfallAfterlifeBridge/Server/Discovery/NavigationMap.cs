using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class NavigationMap
    {
        public List<MapNode> Map { get; } = new();
        public Dictionary<MapNode, List<Vector2>> OverlaysMap { get; } = new();

        public List<Connection> DebugPath;

        public class Connection
        {
            public Vector2 Root;
            public Vector2 Target;
            public MapNode RootNode;
            public MapNode TargetNode;
        }

        public class MapNode
        {
            public Vector2 Location;
            public float Radius;
            public List<Connection> Connections = new();
        }

        public class GraphNode
        {
            public Connection Connection;
            public float Coast;
            public GraphNode Parent;
            public List<GraphNode> Neighbors = new();
        }

        public class Graph
        {
            public MapNode EnterNode { get; protected set; }

            public MapNode ExitNode { get; protected set; }

            public List<MapNode> PointsOverlays { get; } = new();

            public List<Connection> EnterConnections { get; } = new();

            public List<Connection> ExitConnections { get; } = new();

            public Dictionary<MapNode, List<Connection>> MapPairs { get; } = new();

            public Dictionary<Connection, GraphNode> GraphPairs { get; } = new();

            public bool DirectConnection { get; protected set; } = false;

            private Graph() { }

            public static Graph Create(Vector2 from, Vector2 to, List<MapNode> map)
            {
                Graph graph = new Graph();
                graph.Update(from, to, map);
                return graph;
            }

            protected void Update(Vector2 from, Vector2 to, List<MapNode> map)
            {
                EnterNode = new MapNode { Location = from };
                ExitNode = new MapNode { Location = to };

                // Search for circles that overlap the start and end points
                foreach (var node in map)
                {
                    if (from.GetDistanceTo(node.Location) <= node.Radius ||
                        to.GetDistanceTo(node.Location) <= node.Radius)
                        PointsOverlays.Add(node);
                }

                // Early exit if there are no intersections
                if (CanConnectionIntersectWhis(
                    new Connection { Root = from, Target = to }, map.Except(PointsOverlays)) == false)
                {
                    DirectConnection = true;
                    return;
                }

                // Create enter connections
                foreach (var node in map)
                {
                    EnterConnections.AddRange(GetNonIntersectingConnections(
                        MakeConnections(EnterNode, node),
                        map.Where(n => n != EnterNode && n != node && PointsOverlays.Contains(n) == false)));
                }

                EnterNode.Connections.AddRange(EnterConnections);
                MapPairs[EnterNode] = EnterConnections;

                // Creating a connection map
                foreach (var node in map)
                    MapPairs[node] = new List<Connection>(node.Connections);

                // Create exit connections
                foreach (var node in map)
                {
                    var connections = GetNonIntersectingConnections(
                        MakeConnections(node, ExitNode),
                        map.Where(n => n != ExitNode && n != node && PointsOverlays.Contains(n) == false));

                    MapPairs[node].AddRange(connections);
                    ExitConnections.AddRange(connections);
                }

                MapPairs[ExitNode] = new();

                // Creating a graph map
                foreach (var item in MapPairs.Values)
                {
                    if (item is null)
                        continue;

                    foreach (var connection in item)
                    {
                        GraphPairs[connection] = new GraphNode { Connection = connection };
                    }
                }

                foreach (var item in GraphPairs)
                {
                    var targetMapNode = item.Value.Connection?.TargetNode;

                    if (targetMapNode is null || PointsOverlays.Contains(targetMapNode))
                        continue;

                    var connections = MapPairs[targetMapNode];

                    if (connections is not null)
                    {
                        foreach (var connection in connections)
                        {
                            if (PointsOverlays.Contains(connection.TargetNode))
                                continue;

                            item.Value.Neighbors.Add(GraphPairs[connection]);
                        }
                    }
                }
            }

            public float GetCoast(GraphNode from, GraphNode to)
            {
                float coast = to.Connection.Root.GetDistanceTo(to.Connection.Target);

                if (from is not null &&
                    to.Connection.RootNode is not null &&
                    to.Connection.RootNode == from.Connection.TargetNode)
                {
                    var center = to.Connection.RootNode.Location;
                    var a = from.Connection.Target - center;
                    var b = to.Connection.Root - center;
                    coast += SfMath.Abs(a.GetAngleTo(b)) * to.Connection.RootNode.Radius;
                }

                return coast;
            }

            public float GetHeuristic(GraphNode node)
            {
                return node.Connection.Target.GetDistanceTo(ExitNode.Location);
            }

            public IEnumerable<Vector2> CreateArc(Vector2 startDirection, float angle, float stepSize = 0.5235f)
            {
                if (angle < 0)
                    stepSize *= -1;

                int count = (int)float.Abs(angle / stepSize);

                for (int i = 1; i < count; i++)
                    yield return startDirection.Rotate(stepSize * i);
            }

            public List<Vector2> BuildPath(GraphNode lastNode)
            {
                var path = new List<Vector2>();
                var node = lastNode;

                while (node is not null)
                {
                    path.Add(node.Connection.Target);
                    path.Add(node.Connection.Root);

                    var parentNode = node.Parent;
                    var center = node.Connection.RootNode.Location;

                    if (parentNode is not null)
                    {
                        var startDir = node.Connection.Root - center;
                        var endDir = parentNode.Connection.Target - center;

                        foreach (var item in CreateArc(startDir, startDir.GetAngleTo(endDir)))
                            path.Add(center + item);
                    }

                    node = parentNode;
                }

                path.Reverse();
                return path;
            }
        }

        public List<Vector2> CalculatePath(Vector2 from, Vector2 to)
        {
            var graph = Graph.Create(from, to, Map);

            if (graph.DirectConnection == true)
                return new() { from, to };

            var reachable = new List<GraphNode>();
            var costSoFar = new Dictionary<GraphNode, float>();
            GraphNode resultNode = null;

            foreach (var connection in graph.EnterConnections)
            {
                var node = graph.GraphPairs[connection];
                node.Coast = graph.GetCoast(null, node);
                costSoFar[node] = node.Coast;
                node.Coast += graph.GetHeuristic(node);
                reachable.Add(graph.GraphPairs[connection]);
            }

            while (reachable.Count > 0)
            {
                var node = GetNextNode(reachable);

                if (node is null)
                    break;

                if (node.Connection.TargetNode == graph.ExitNode)
                {
                    resultNode = node;
                    break;
                }

                reachable.Remove(node);

                foreach (var nextNode in node.Neighbors)
                {
                    if (IsTransitionCorrect(node, nextNode) == false)
                        continue;

                    float newCoast = costSoFar[node] + graph.GetCoast(node, nextNode);

                    if (costSoFar.ContainsKey(nextNode) == false || newCoast < costSoFar[nextNode])
                    {
                        costSoFar[nextNode] = newCoast;
                        nextNode.Coast = newCoast + graph.GetHeuristic(nextNode);
                        nextNode.Parent = node;
                        reachable.Add(nextNode);
                    }
                }
            }

            static List<Connection> BuildDebugPath(GraphNode lastNode)
            {
                var node = lastNode;
                var path = new List<Connection>();

                while (node is not null)
                {
                    path.Add(node.Connection);
                    node = node.Parent;
                }

                path.Reverse();
                return path;
            }

            DebugPath = BuildDebugPath(resultNode);
            return graph.BuildPath(resultNode);
        }

        protected GraphNode GetNextNode(IEnumerable<GraphNode> nodes)
        {
            GraphNode nextNode = null;
            float bestCost = float.MaxValue;

            foreach (var node in nodes)
            {
                if (node.Coast < bestCost)
                {
                    nextNode = node;
                    bestCost = node.Coast;
                }
            }

            return nextNode;
        }

        protected bool IsTransitionCorrect(GraphNode from, GraphNode to)
        {
            var fromConnection = from.Connection;
            var toConnection = to.Connection;

            if (from == to || fromConnection.TargetNode == toConnection.TargetNode)
                return false;

            if (fromConnection.TargetNode is not null &&
                OverlaysMap.TryGetValue(fromConnection.TargetNode, out List<Vector2> overlays) == true)
            {
                var center = fromConnection.TargetNode.Location;
                var a = SfMath.Mod2PI((fromConnection.Target - center).GetAtan2());
                var b = SfMath.Mod2PI((toConnection.Root - center).GetAtan2());

                foreach (var item in overlays)
                {
                    if (SfMath.IsAngleInRange(a, b, item.X) ||
                        SfMath.IsAngleInRange(a, b, item.Y))
                        return false;
                }
            }

            return true;
        }

        public NavigationMap Rebuild()
        {
            OverlaysMap.Clear();

            foreach (var nodeA in Map)
            {
                nodeA?.Connections?.Clear();

                foreach (var nodeB in Map)
                {
                    if (nodeB == nodeA)
                        continue;

                    if (TryGetIntersect(nodeA, nodeB, out float min, out float max) == true)
                    {
                        if (OverlaysMap.TryGetValue(nodeA, out var overlays) == false)
                            OverlaysMap[nodeA] = new List<Vector2>() { new Vector2(min, max) };
                        else
                            OverlaysMap[nodeA].Add(new Vector2(min, max));
                    }

                    var connections = MakeConnections(nodeA, nodeB);
                    var otherNodes = Map.Where(n => n != nodeA && n != nodeB);

                    foreach (var connection in connections)
                    {
                        if (CanConnectionIntersectWhis(connection, otherNodes) == false)
                            nodeA.Connections.Add(connection);
                    }
                }
            }

            return this;
        }

        public static bool TryGetIntersect(MapNode a, MapNode b, out float minAngle, out float maxAngle)
        {
            minAngle = 0f;
            maxAngle = 0f;

            if (a == b)
                return false;

            var direction = b.Location - a.Location;
            var distance = direction.GetSize();

            if (a.Radius <= 0f || b.Radius <= 0f ||
                a.Radius + b.Radius < distance ||
                a.Radius > b.Radius + distance ||
                b.Radius > a.Radius + distance)
                return false;

            var directionAngle = direction.GetAtan2();
            var rA = a.Radius;
            var rB = b.Radius;
            var overlayDistance = (SfMath.Pow(rA) - SfMath.Pow(rB) + SfMath.Pow(distance)) / (distance * 2);
            var overlayAngle = SfMath.Acos(overlayDistance / rA);

            minAngle = SfMath.Mod2PI(directionAngle + overlayAngle);
            maxAngle = SfMath.Mod2PI(directionAngle - overlayAngle);

            return true;
        }

        public static IEnumerable<Connection> MakeConnections(MapNode from, MapNode to)
        {
            var a = from.Location;
            var b = to.Location;
            var ar = from.Radius;
            var br = to.Radius;
            var ab = b - a;
            var aDir = ab.Normalize();
            var bDir = aDir.GetNegative();
            var distance = ab.GetSize();

            if (distance <= 0)
                yield break;

            var innerAngle = MathF.Acos((ar + br) / distance);

            yield return new Connection()
            {
                Root = a + aDir.Rotate(innerAngle) * ar,
                Target = b + bDir.Rotate(innerAngle) * br,
                RootNode = from,
                TargetNode = to
            };

            yield return new Connection()
            {
                Root = a + aDir.Rotate(-innerAngle) * ar,
                Target = b + bDir.Rotate(-innerAngle) * br,
                RootNode = from,
                TargetNode = to
            };

            if (ar < br)
                aDir = bDir;

            var outerAngle = MathF.Acos(MathF.Abs(ar - br) / distance);
            var upDir = aDir.Rotate(outerAngle);
            var downDir = aDir.Rotate(-outerAngle);

            yield return new Connection()
            {
                Root = a + upDir * ar,
                Target = b + upDir * br,
                RootNode = from,
                TargetNode = to
            };

            yield return new Connection()
            {
                Root = a + downDir * ar,
                Target = b + downDir * br,
                RootNode = from,
                TargetNode = to
            };
        }

        public static bool CanConnectionIntersectWhis(Connection connection, IEnumerable<MapNode> nodes)
        {
            foreach (var item in nodes)
            {
                Vector2 center = item.Location;
                Vector2 direction = connection.Target - connection.Root;

                if (direction == Vector2.Zero)
                    continue;

                float hCenter = Vector2.Dot(center - connection.Root, direction) / Vector2.Dot(direction, direction);
                float distance = (connection.Root + direction * SfMath.Clamp01(hCenter)).GetDistanceTo(center);

                if (distance <= item.Radius)
                    return true;
            }

            return false;
        }

        public static IEnumerable<Connection> GetNonIntersectingConnections(IEnumerable<Connection> connections, IEnumerable<MapNode> nodes)
        {
            foreach (var connection in connections)
            {
                if (CanConnectionIntersectWhis(connection, nodes) == false)
                    yield return connection;
            }
        }

        public static NavigationMap Create(IEnumerable<Vector2> centers, IEnumerable<float> radiuses)
        {
            var map = new NavigationMap();
            var nodes = map.Map;
            int i = 0;

            foreach (var center in centers)
                nodes.Add(new MapNode() { Location = center });

            foreach (var radius in radiuses)
            {
                if (nodes.Count <= i)
                    break;

                nodes[i].Radius = radius;

                i++;
            }

            return map.Rebuild();
        }
    }
}
