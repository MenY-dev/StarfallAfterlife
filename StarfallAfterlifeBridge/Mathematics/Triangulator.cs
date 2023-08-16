using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Mathematics
{
    public class Triangulator
    {
        public List<Vector2> Points { get; }

        public List<Edge> Edges { get; protected set; }

        public List<Node> Nodes { get; protected set; }

        public Triangulator(IEnumerable<Vector2> points)
        {
            Points = new(points);
            Edges = new();
        }

        public void Build()
        {
            int pointsCount = Points.Count;
            DateTime startTime = DateTime.Now;
            BuildingProcess builder = new BuildingProcess(Points);

            builder.Process();

            foreach (var tris in builder.Triangles)
            {
                foreach (var edge in tris.GetEdges())
                {
                    if (edge.A < pointsCount && edge.B < pointsCount)
                        Edges.Add(new(edge.A, edge.B));
                }
            }

            Nodes = new List<Node>(Points.Count);

            for (int i = 0; i < Points.Count; i++)
                Nodes.Add(new Node(i));

            foreach (var edge in Edges)
            {
                if (Nodes[edge.B].Children.Contains(edge.A) == false &&
                    Nodes[edge.A].Children.Contains(edge.B) == false)
                    Nodes[edge.A].Children.Add(edge.B);
            }
        }

        private class BuildingProcess
        {
            public List<Vector2> Points = new();
            public List<Tris> Triangles = new();
            private readonly object locker = new();

            public BuildingProcess(List<Vector2> points)
            {
                int pointsCount = points.Count;
                Points = new(points.Concat(GetSuperTriangle(points)));
                Triangles.Add(new(Points, pointsCount, pointsCount + 1, pointsCount + 2));
            }

            public void Process()
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    List<int> badTriangles = new();
                    List<Edge> poligon = new();

                    Parallel.For(0, Triangles.Count, (n) =>
                    {
                        if (IsInsideСircle(n, i))
                        {
                            lock (locker)
                                badTriangles.Add(n);
                        }
                    });

                    badTriangles.Sort();

                    foreach (var tris in badTriangles)
                    {
                        poligon.AddRange(GetUnusedEdges(badTriangles, tris));
                    }

                    foreach (var tris in badTriangles.Reverse<int>())
                    {
                        Triangles.RemoveAt(tris);
                    }

                    foreach (var edge in poligon)
                    {
                        Triangles.Add(new(Points, edge.A, i, edge.B));
                    }

                }
            }

            private IEnumerable<Edge> GetUnusedEdges(IEnumerable<int> triangles, int targetTris)
            {
                foreach (var targetEdge in Triangles[targetTris].GetEdges())
                {
                    bool isUsed = false;

                    foreach (var tris in triangles)
                    {
                        if (tris == targetTris)
                            continue;

                        foreach (var edge in Triangles[tris].GetEdges())
                        {
                            if ((edge.A == targetEdge.A && edge.B == targetEdge.B) ||
                                (edge.B == targetEdge.A && edge.A == targetEdge.B))
                            {
                                isUsed = true;
                                break;
                            }
                        }
                    }

                    if (isUsed == false)
                        yield return targetEdge;
                }
            }

            private bool IsInsideСircle(int tris, int point)
            {
                Tris triangle = Triangles[tris];
                return (triangle.Center - Points[point]).GetSize() < triangle.Radius;
            }

            private List<Vector2> GetSuperTriangle(List<Vector2> points, float safeArea = 20)
            {
                float minX = points[0].X,
                      minY = points[0].Y,
                      maxX = points[0].X,
                      maxY = points[0].Y;

                for (int i = 0; i < points.Count; i++)
                {
                    minX = MathF.Min(points[i].X, minX);
                    minY = MathF.Min(points[i].Y, minY);
                    maxX = MathF.Max(points[i].X, maxX);
                    maxY = MathF.Max(points[i].Y, maxY);
                }

                minX -= safeArea;
                minY -= safeArea;
                maxX += safeArea;
                maxY += safeArea;

                float dx = (maxX - minX),
                      dy = (maxY - minY);

                return new()
                {
                    new(minX - dx, minY - dy * 3),
                    new(minX - dx, maxY + dy),
                    new(maxX + dx * 3, maxY + dy)
                };
            }
        }

        public class Node
        {
            public int Point;
            public List<int> Children = new();

            public Node(int pointIndex)
            {
                Point = pointIndex;
            }
        }

        public struct Edge
        {
            public int A, B;

            public Edge(int a, int b)
            {
                A = a;
                B = b;
            }

            public override string ToString()
            {
                return $"Edge({A}, {B})";
            }
        }

        public class Tris
        {
            public int A, B, C;
            public float Radius;
            public Vector2 Center;

            public static bool operator ==(Tris a, Tris b) =>
                a.A == b.A &&
                a.B == b.B &&
                a.C == b.C;

            public static bool operator !=(Tris a, Tris b) => !(a == b);

            public Tris(List<Vector2> mesh, int a, int b, int c)
            {
                A = a;
                B = b;
                C = c;

                Center = GetCircleCenter(mesh);
                Radius = (Center - mesh[a]).GetSize();
            }

            public Vector2 GetCircleCenter(List<Vector2> mesh)
            {
                Vector2 a = mesh[A], b = mesh[B], c = mesh[C];
                double aax = a.X * a.X;
                double aay = a.Y * a.Y;
                double bbx = b.X * b.X;
                double bby = b.Y * b.Y;
                double ccx = c.X * c.X;
                double ccy = c.Y * c.Y;
                double bc = bbx + bby - ccx - ccy;
                double ca = ccx + ccy - aax - aay;
                double ab = aax + aay - bbx - bby;
                double d = a.Y * bc + b.Y * ca + c.Y * ab;
                double e = a.X * bc + b.X * ca + c.X * ab;
                double f = a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);

                if (f == 0)
                    return new Vector2(float.PositiveInfinity, float.PositiveInfinity);

                return new Vector2(-(float)(d / f * 0.5), (float)(e / f * 0.5));
            }

            public IEnumerable<Edge> GetEdges()
            {
                yield return new Edge(A, B);
                yield return new Edge(B, C);
                yield return new Edge(C, A);
            }

            public override string ToString()
            {
                return $"Tris({A}, {B}, {C})";
            }

            public override bool Equals(object obj) => base.Equals(obj);

            public override int GetHashCode()
            {
                return HashCode.Combine(A, B, C);
            }
        }
    }
}
