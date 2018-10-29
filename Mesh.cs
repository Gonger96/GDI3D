using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDI3D.Formats
{
    public class Mesh
    {
        // Transform
        public List<uint> Indices { get; set; }
        public List<Vertex> Vertices { get; set; }
        public bool HasNormals { get; private set; }
        public bool HasColours { get; private set; }
        public bool HasUvs { get; private set; }
        public Mesh()
        {

        }

        // ASCII only
        public static Mesh FromPly(string filename)
        {
            Mesh m = new Mesh();
            List<KeyValuePair<string, string>> dataTypes = new List<KeyValuePair<string, string>>();

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    // Header
                    if (reader.ReadLine().ToLower() != "ply")
                        throw new InvalidDataException();
                    if (reader.ReadLine().ToLower() != "format ascii 1.0")
                        throw new InvalidDataException();

                    int vertexCount = 0;
                    string line = reader.ReadLine().ToLower();
                    while (!line.StartsWith("element vertex"))
                        line = reader.ReadLine().ToLower();
                    vertexCount = int.Parse(line.Substring("element vertex ".Length));

                    line = reader.ReadLine().ToLower();
                    while(!line.StartsWith("element face"))
                    {
                        if (!line.StartsWith("property"))
                            continue;
                        string type = line.Substring("property ".Length);
                        type = type.Remove(type.IndexOf(' '));
                        string data = line.TrimEnd(' ').Substring(line.LastIndexOf(' '));
                        dataTypes.Add(new KeyValuePair<string, string>(data.Trim(' '), type.Trim(' ')));
                        line = reader.ReadLine().ToLower();
                        if(data.Trim(' ') == "red")
                            m.HasColours = true;
                        else if (data.Trim(' ') == "nx")
                            m.HasNormals = true;
                        else if (data.Trim(' ') == "s")
                            m.HasUvs = true;
                    }
                    int faceCount = int.Parse(line.Substring("element face ".Length));
                    while (reader.ReadLine().ToLower().Trim(' ') != "end_header")
                        ;
                    // Data
                    m.Vertices = new List<Vertex>(vertexCount);
                    for(int i = 0; i < vertexCount; ++i)
                    {
                        // Vertex v;
                        Vertex vertex = new Vertex();
                        if (m.HasColours)
                            vertex.Attribute.Add("COLOUR", new Vector3());
                        if (m.HasNormals)
                            vertex.Attribute.Add("NORMAL", new Vector3());
                        if (m.HasUvs)
                            vertex.Attribute.Add("TEXCOORD", new Vector2());
                        line = reader.ReadLine();
                        List<string> values = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                        for(int j = values.Count-1; j >= 0; --j)
                            if (string.IsNullOrWhiteSpace(values[j]))
                                values.RemoveAt(j);
                        if (values.Count != dataTypes.Count)
                            throw new InvalidDataException();
                        for (int j = 0; j < values.Count; ++j)
                            AddData(ref vertex, values[j], dataTypes[j]);
                        if (m.HasNormals)
                            vertex.Attribute["NORMAL"] = vertex.Attribute.Get<Vector3>("NORMAL").Normalised;
                        m.Vertices.Add(vertex);
                    }
                    m.Indices = new List<uint>(faceCount * 3);
                    for(int i = 0; i < faceCount; ++i)
                    {
                        line = reader.ReadLine();
                        List<string> values = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                        for (int j = values.Count - 1; j >= 0; --j)
                            if (string.IsNullOrWhiteSpace(values[j]))
                                values.RemoveAt(j);
                        if (int.Parse(values[0]) != 3)
                            throw new InvalidDataException("Only triangulated meshes supported");
                        if (values.Count != 4)
                            throw new InvalidDataException();
                        m.Indices.Add(uint.Parse(values[1]));
                        m.Indices.Add(uint.Parse(values[2]));
                        m.Indices.Add(uint.Parse(values[3]));
                    }
                }
            }
            return m;
        }

        private static void AddData(ref Vertex vertex, string data, KeyValuePair<string, string> type)
        {
            float f = float.Parse(data, CultureInfo.InvariantCulture);
            if (type.Value == "float")
            {
                // No conversion
            }
            else if (type.Value == "uchar")
            {
                f = f / 255.0f;
            }
            else
                throw new InvalidDataException("Invalid data type " + type.Value);
            switch(type.Key)
            {
                case "x":
                    vertex.Position = new Vector4(f, vertex.Position.Y, vertex.Position.Z);
                    break;
                case "y":
                    vertex.Position = new Vector4(vertex.Position.X, f, vertex.Position.Z);
                    break;
                case "z":
                    vertex.Position = new Vector4(vertex.Position.X, vertex.Position.Y, f);
                    break;
                case "red":
                    {
                        var c = vertex.Attribute.Get<Vector3>("COLOUR");
                        vertex.Attribute["COLOUR"] = new Vector3(f, c.Y, c.Z);
                        break;
                    }
                case "green":
                    {
                        var c = vertex.Attribute.Get<Vector3>("COLOUR");
                        vertex.Attribute["COLOUR"] = new Vector3(c.X, f, c.Z);
                        break;
                    }
                case "blue":
                    {
                        var c = vertex.Attribute.Get<Vector3>("COLOUR");
                        vertex.Attribute["COLOUR"] = new Vector3(c.X, c.Y, f);
                        break;
                    }
                case "nx":
                    {
                        var c = vertex.Attribute.Get<Vector3>("NORMAL");
                        vertex.Attribute["NORMAL"] = new Vector3(f, c.Y, c.Z);
                        break;
                    }
                case "ny":
                    {
                        var c = vertex.Attribute.Get<Vector3>("NORMAL");
                        vertex.Attribute["NORMAL"] = new Vector3(c.X, f, c.Z);
                        break;
                    }
                case "nz":
                    {
                        var c = vertex.Attribute.Get<Vector3>("NORMAL");
                        vertex.Attribute["NORMAL"] = new Vector3(c.X, c.Y, f);
                        break;
                    }
                case "s":
                    {
                        var c = vertex.Attribute.Get<Vector2>("TEXCOORD");
                        vertex.Attribute["TEXCOORD"] = new Vector2(f, c.Y);
                        break;
                    }
                case "t":
                    {
                        var c = vertex.Attribute.Get<Vector2>("TEXCOORD");
                        vertex.Attribute["TEXCOORD"] = new Vector2(c.X, f);
                        break;
                    }
            }
        }
    }
}
