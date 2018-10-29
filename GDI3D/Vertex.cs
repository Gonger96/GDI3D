using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDI3D
{
    public interface IVertexAttribute
    {
        void Multiply2(float value);
        IVertexAttribute Multiply(float value);
        IVertexAttribute Multiply(IVertexAttribute value);
        IVertexAttribute Add(IVertexAttribute value);
        IVertexAttribute Subtract(IVertexAttribute value);
        IVertexAttribute Divide(float value);
        IVertexAttribute Divide(IVertexAttribute value);
        IVertexAttribute Clone();
    }

    public class AttributeList : IEnumerable<string>, IEnumerable
    {
        Dictionary<string, IVertexAttribute> attribute = new Dictionary<string, IVertexAttribute>();

        public AttributeList() { attribute = new Dictionary<string, IVertexAttribute>(); }
        public AttributeList(int capacity) { attribute = new Dictionary<string, IVertexAttribute>(capacity); }

        public int Count => attribute.Count;

        public T Get<T>(string identifier) where T : IVertexAttribute
        {
            return (T)attribute[identifier];
        }
        public void Set(string identifier, IVertexAttribute value)
        {
            attribute[identifier] = value;
        }
        public void Add(string id, IVertexAttribute a)
        {
            attribute.Add(id, a);
        }
        public void Remove(string id) => attribute.Remove(id);
        public IVertexAttribute this[string id]
        {
            get => attribute[id];
            set => attribute[id] = value;
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var a in attribute)
                yield return a.Key;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IVertexAttribute Lerp(IVertexAttribute start, IVertexAttribute end, float alpha)
        {
            return start.Subtract(end).Multiply(alpha).Add(start);
        }
    }

    public class Vertex
    {
        public Vector4 Position { get; set; }
        public AttributeList Attribute { get; }

        public Vertex()
        {
            Attribute = new AttributeList();
        }
        public Vertex(Vertex old)
        {
            Position = old.Position;
            Attribute = new AttributeList(old.Attribute.Count);
            foreach (string key in old.Attribute)
                Attribute.Add(key, old.Attribute[key].Clone());
        }
        public Vertex(Vector4 position)
        {
            Position = position;
            Attribute = new AttributeList();
        }
    }
}
