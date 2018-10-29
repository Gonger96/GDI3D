using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDI3D.Shader
{
    public interface IPixelShader
    {
        bool Shader(Vertex input, out Vector4 colour);
    }
    public static class BasicPixelShader
    {
        /// <summary>
        /// Vertices need "COLOUR" attribute
        /// </summary>
        /// <param name="input"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static bool VertexColour(Vertex input, out Vector4 colour)
        {
            colour = new Vector4(input.Attribute.Get<Vector3>("COLOUR"));
            return true;
        }
        /// <summary>
        /// Always returns white
        /// </summary>
        /// <param name="input"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static bool White(Vertex input, out Vector4 colour)
        {
            colour = Vector4.One;
            return true;
        }
    }

    /// <summary>
    /// Vertices need "TEXCOORD" attribute
    /// </summary>
    public class TexturePixelShader : IPixelShader
    {
        public Texture2 Texture { get; set; }
        public bool Shader(Vertex input, out Vector4 colour)
        {
            colour = Texture.Sample(input.Attribute.Get<Vector2>("TEXCOORD"));
            return true;
        }
    }

    /// <summary>
    /// Shades with given normals.
    /// Vertices need "NORMAL" attribute.
    /// </summary>
    public class NormalPixelShader : IPixelShader
    {
        public bool Shader(Vertex input, out Vector4 colour)
        {
            Vector3 viewDirection = -input.Attribute.Get<Vector3>("NORMAL0");
            viewDirection.NormaliseF();
            float view = MathHelper.Clamp(input.Attribute.Get<Vector3>("NORMAL").NormalisedF.Dot(viewDirection), 0, 1);
            colour = new Vector4(view);
            return true;
        }
    }

    public interface IVertexShader
    {
        Vertex Shader(Vertex input);
    }
    public static class BasicVertexShader
    {
        /// <summary>
        /// Does not transform the vertices at all.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Vertex None(Vertex input) => input;
    }

    public class ProjectiveVertexShader : IVertexShader
    {
        public Viewport Viewport { get; set; }
        public Vertex Shader(Vertex input)
        {
            Vertex output = input;
            output.Position = Viewport.WorldViewProjection * input.Position;
            return output;
        }
    }

    /// <summary>
    /// Adds a "NORMAL0" attribute to each vertex
    /// </summary>
    public class NormalVertexShader : IVertexShader
    {
        public Viewport Viewport { get; set; }
        public Vertex Shader(Vertex input)
        {
            Vertex output = input;
            // Add vector to the current vertex
            output.Attribute.Add("NORMAL0", (Viewport.WorldView * input.Position).ToVector3());
            // Transform normal
            output.Attribute["NORMAL"] = Viewport.WorldViewInverseTransposed.MultiplyNormal(output.Attribute.Get<Vector3>("NORMAL"));
            output.Position = Viewport.WorldViewProjection * input.Position;
            return output;
        }
    }
}
