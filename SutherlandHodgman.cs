using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDI3D
{
    internal struct Triangle
    {
        public Triangle(Vertex v1, Vertex v2, Vertex v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
        public Vertex V1 { get; set; }
        public Vertex V2 { get; set; }
        public Vertex V3 { get; set; }
    }

    static class SutherlandHodgman
    {
        private static float GetInterpolator(float w1, float w2, float x1, float x2)
        {
            return (w1 - x1) / (x2 - (w2-w1) - x1);
        }

        private static void ClipFar(Vertex prev, Vertex cur, List<Vertex> clipped)
        {
            bool prevVisible = prev.Position.Z <= prev.Position.W;
            bool curVisible = cur.Position.Z <= cur.Position.W;
            if (prevVisible ^ curVisible)
            {
                Vertex v = new Vertex();
                float alpha = GetInterpolator(prev.Position.W, cur.Position.W, prev.Position.Z, cur.Position.Z);
                v.Position = MathHelper.Lerp(prev.Position, cur.Position, alpha);
                foreach (var key in prev.Attribute)
                {
                    IVertexAttribute data = MathHelper.Lerp(prev.Attribute[key], cur.Attribute[key], alpha);
                    v.Attribute.Add(key, data);
                }
                clipped.Add(v);
            }
            if (curVisible)
                clipped.Add(cur);
        }

        private static void ClipNear(Vertex prev, Vertex cur, List<Vertex> clipped)
        {
            bool prevVisible = prev.Position.Z >= -prev.Position.W;
            bool curVisible = cur.Position.Z >= -cur.Position.W;
            if (prevVisible ^ curVisible)
            {
                Vertex v = new Vertex();
                float alpha = GetInterpolator(-prev.Position.W, -cur.Position.W, prev.Position.Z, cur.Position.Z);
                v.Position = MathHelper.Lerp(prev.Position, cur.Position, alpha);
                foreach (var key in prev.Attribute)
                {
                    IVertexAttribute data = MathHelper.Lerp(prev.Attribute[key], cur.Attribute[key], alpha);
                    v.Attribute.Add(key, data);
                }
                ClipFar(prev, v, clipped);
            }
            if (curVisible)
                ClipFar(prev, cur, clipped);
        }

        public static List<Triangle> ClipTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            List<Vertex> clipped = new List<Vertex>();
            ClipNear(v2, v0, clipped);
            ClipNear(v0, v1, clipped);
            ClipNear(v1, v2, clipped);
            if (clipped.Count == 3)
                return new List<Triangle>() { new Triangle(clipped[0], clipped[1], clipped[2]) };
            else if (clipped.Count > 3)
                return Triangulate(clipped);
            else
                return new List<Triangle>() { new Triangle(v0, v1, v2) };
        }

        private static List<Triangle> Triangulate(List<Vertex> polygon)
        {
            List<Triangle> triangles = new List<Triangle>();
            List<int> indices = new List<int>() { 0, 1, 2 };
            Func<int, Vertex> Vert = (int index) =>
            {
                if (indices.Contains(index))
                    return new Vertex(polygon[index]);
                indices.Add(index);
                return polygon[index];
            };
            int i2 = 2;
            int i3 = polygon.Count - 1;
            triangles.Add(new Triangle(polygon[0], polygon[1], polygon[2]));
            while (true)
            {
                if (i3 == i2)
                    break;
                triangles.Add(new Triangle(Vert(i2), Vert(i3), Vert((i3 + 1 >= polygon.Count()) ? 0 : i3 + 1)));
                i2++;
                if (i3 == i2)
                    break;
                triangles.Add(new Triangle(Vert(i2), Vert(i2 - 1), Vert(i3)));
                i3--;
            }
            return triangles;
        }
    }
}
