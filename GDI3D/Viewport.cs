using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GDI3D
{
    public enum ResolutionGate { Fill, Overscan };
    public enum CullMode { AntiClockwise, Clockwise, None }
    public enum DepthTestState { Enable, Disable, ReadOnly }
    public enum BlendFunction { Zero, One, SrcColour, OneMinusSrcColour, DstColour, OneMinusDstColour, SrcAlpha, OneMinusSrcAlpha, DstAlpha, OneMinusDstAlpha, ConstantColour, OneMinusConstantColour, ConstantAlpha, OneMinusConstAlpha }
    public enum BlendEquation { Add, Subtract, ReverseSubtract }
    public enum Comparison { Never, Less, LessEqual, Greater, GreaterEqual, Equal, NotEqual, Always }
    public enum StencilOperation { Keep, Zero, Replace, Increment, IncWrap, Decrement, DecWrap, Invert }
    
    public class Viewport : IDisposable
    {
        private object syncLock = new object();
        private Bitmap bitmap = null;
        private const int BytesPerPixel = 4;
        private byte[] buffer = null;
        private float[] zBuffer = null;
        private byte[] stencilBuffer = null;
        public float FocalLength { get; private set; }
        public float Ratio { get; private set; }
        public float FieldOfView { get; private set; }
        public float NearPlane { get; private set; }
        public float FarPlane { get; private set; }
        public ResolutionGate Mode { get; private set; } = ResolutionGate.Fill;
        const float inchTomm = 25.4f;
        public RectangleF Arpeture { get; private set; }
        public Size DeviceSize { get; private set; }
        public Matrix4 Transform { get; set; }
        public Matrix4 Projection { get; private set; }
        public Matrix4 WorldViewProjection { get; private set; }
        public Matrix4 WorldView { get; private set; }
        public Matrix4 WorldViewInverseTransposed { get; private set; }

        public CullMode CullMode { get; set; }
        // Depth Test
        public float DepthBias { get; set; } = 0;
        public DepthTestState DepthTest { get; set; }
        public Comparison DepthFunction { get; set; } = Comparison.Less;
        // Alpha blending
        public BlendFunction SrcBlendFunction { get; set; }
        public BlendFunction DestBlendFunction { get; set; }
        public BlendEquation BlendEquation { get; set; } = BlendEquation.Add;
        public Vector4 BlendColour { get; set; }
        public float BlendAlpha { get; set; }
        public bool BlendEnable { get; set; } = false;
        // Stencil Test
        public bool StencilEnable { get; set; } = false;
        public byte StencilFunctionMask { get; set; } = 0xff;
        public byte StencilMask { get; set; } = 0xFF;
        public byte StencilReference { get; set; }
        public Comparison StencilFunction { get; set; }
        public StencilOperation StencilFail { get; set; } = StencilOperation.Keep;
        public StencilOperation DepthFail { get; set; } = StencilOperation.Keep;
        public StencilOperation StencilDepthPass { get; set; } = StencilOperation.Keep;


        public Func<Vertex, Vertex> VertexShader { get; set; }
        public delegate bool PixelShaderDelegate(Vertex vIn, out Vector4 colour);
        public PixelShaderDelegate PixelShader { get; set; }

        public IEnumerable<Vertex> VertexBuffer { get; set; }
        public IEnumerable<UInt32> IndexBuffer { get; set; }

        public Viewport(SizeF arpetureSize, Size deviceSize, float focalLen, float nearPlane, float farPlane, ResolutionGate mode = ResolutionGate.Fill)
        {
            Transform = Matrix4.Identity;
            Mode = mode;
            FocalLength = focalLen;
            NearPlane = nearPlane;
            FarPlane = farPlane;
            DeviceSize = deviceSize;
            float top = ((arpetureSize.Height * inchTomm / 2) / focalLen) * nearPlane;
            float right = ((arpetureSize.Width * inchTomm / 2) / focalLen) * nearPlane;
            Arpeture = RectangleF.FromLTRB(-right, top, right, -top);

            FieldOfView = (float)(2 * 180 / Math.PI * Math.Atan((arpetureSize.Width * inchTomm / 2) / focalLen));
            Ratio = arpetureSize.Width / arpetureSize.Height;
            BuildProjection();
            SetupFramebuffer();
        }
        void SetupFramebuffer()
        {
            buffer = new byte[DeviceSize.Width * DeviceSize.Height * BytesPerPixel];
            zBuffer = new float[DeviceSize.Width * DeviceSize.Height];
            stencilBuffer = new byte[DeviceSize.Width * DeviceSize.Height];
            bitmap = new Bitmap(DeviceSize.Width, DeviceSize.Height, DeviceSize.Width * BytesPerPixel, System.Drawing.Imaging.PixelFormat.Format32bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0));
        }
        public void LookAt(Vector3 from, Vector3 to) => LookAt(from, to, new Vector3(0, 1, 0));
        public void LookAt(Vector3 from, Vector3 to, Vector3 upDirection)
        {
            Vector3 forward = (from - to).Normalised;
            var tmpUp = upDirection.Normalised;
            Vector3 right = tmpUp.Cross(forward);
            Vector3 up = forward.Cross(right);

            Matrix4 transform = Matrix4.Identity;

            transform.M00 = right.X;
            transform.M01 = right.Y;
            transform.M02 = right.Z;
            transform.M10 = up.X;
            transform.M11 = up.Y;
            transform.M12 = up.Z;
            transform.M20 = forward.X;
            transform.M21 = forward.Y;
            transform.M22 = forward.Z;

            transform.M30 = from.X;
            transform.M31 = from.Y;
            transform.M32 = from.Z;
            Transform = transform;
        }
        private void BuildProjection()
        {
            float deviceAspectRatio = DeviceSize.Width / (float)DeviceSize.Height;
            float xscale = 1;
            float yscale = 1;
            if (Mode == ResolutionGate.Fill)
            {
                if (Ratio > deviceAspectRatio)
                    xscale = deviceAspectRatio / Ratio;
                else
                    yscale = Ratio / deviceAspectRatio;
            }
            else
            {
                if (Ratio > deviceAspectRatio)
                    yscale = Ratio / deviceAspectRatio;
                else
                    xscale = deviceAspectRatio / Ratio;
            }
            float top = Arpeture.Top * yscale;
            float right = Arpeture.Right * xscale;
            Arpeture = RectangleF.FromLTRB(-right, top, right, -top);
            Projection = new Matrix4(Arpeture.Left, Arpeture.Right, Arpeture.Top, Arpeture.Bottom, NearPlane, FarPlane);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    bitmap.Dispose();
                }
                buffer = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        private Vector4 Rasterize(Vector4 v)
        {
            return new Vector4((v.X + 1) * 0.5f * DeviceSize.Width, (1 - (v.Y + 1) * 0.5f) * DeviceSize.Height, v.Z, v.W);
        }

        public void ClearFrameBuffer() => ClearFrameBuffer(Vector3.Zero);
        public void ClearFrameBuffer(Vector3 colour)
        {
            ClearColourBuffer(colour);
            ClearZBuffer();
            ClearStencilBuffer();
        }

        public void ClearColourBuffer(Vector3 colour)
        {
            for(int i = 0; i < buffer.Length; i+=BytesPerPixel)
            {
                buffer[i] = (byte)(colour.Z * 255);
                buffer[i + 1] = (byte)(colour.Y * 255);
                buffer[i + 2] = (byte)(colour.X * 255);
            }
        }

        public void ClearZBuffer()
        {
            for (int i = 0; i < zBuffer.Length; ++i)
                zBuffer[i] = FarPlane;
        }

        public void ClearStencilBuffer()
        {
            for (int i = 0; i < stencilBuffer.Length; ++i)
                stencilBuffer[i] = 0;
        }

        public void DrawBufferToDC(Graphics g)
        {
            lock(syncLock)
                g.DrawImageUnscaled(bitmap, 0, 0);
        }

        public void DrawBufferToDC(Graphics g, Point p)
        {
            lock (syncLock)
                g.DrawImageUnscaled(bitmap, p);
        }

        public void DrawIndexedTriangles(Matrix4 world)
        {
            lock (syncLock)
                DrawIndexedTrianglesInternal(world);
        }

        private void DrawIndexedTrianglesInternal(Matrix4 worldToPrimitive)
        {
            //// Converts primitives from 3D world space to 2D raster space, if needed each primitive gets clipped between farplane and nearplane
            //if(DepthTest == DepthTestState.Enable)
            //    ResetZ();
            WorldView = worldToPrimitive * Transform;
            WorldViewInverseTransposed = WorldView.Inverse.Transposed;
            WorldViewProjection = WorldView * Projection;
            
            for (int i = 0; i < IndexBuffer.Count(); i += 3)
            {
                Vertex v0 = null;
                Vertex v1 = null; 
                Vertex v2 = null;
                v0 = VertexShader(new Vertex(VertexBuffer.ElementAt((int)IndexBuffer.ElementAt(i))));
                v1 = VertexShader(new Vertex(VertexBuffer.ElementAt((int)IndexBuffer.ElementAt(i + 1))));
                v2 = VertexShader(new Vertex(VertexBuffer.ElementAt((int)IndexBuffer.ElementAt(i + 2))));
                // Now in clip space

                // Out of bounds -> throw away
                if (v0.Position.W <= 0 && v1.Position.W <= 0 && v2.Position.W <= 0)
                    continue;

                if (BoundaryHelper.IsXOutside(v0.Position, v1.Position, v2.Position) ||
                    BoundaryHelper.IsYOutside(v0.Position, v1.Position, v2.Position) ||
                    BoundaryHelper.IsZOutside(v0.Position, v1.Position, v2.Position, out bool clipTriangle)) // Whole triangle outside the frustum -> throw away
                    continue;
                List<Triangle> triangles = new List<Triangle>() { new Triangle(v0, v1, v2) };
                if (clipTriangle)
                {
                    // Clip the triangle and loop through each
                    triangles = SutherlandHodgman.ClipTriangle(v0, v1, v2);
                }

                foreach (var triangle in triangles)
                {
                    Vertex v0C = triangle.V1;
                    Vertex v1C = triangle.V2;
                    Vertex v2C = triangle.V3;

                    v0C.Position = v0C.Position.Homogenised;
                    v1C.Position = v1C.Position.Homogenised;
                    v2C.Position = v2C.Position.Homogenised;
                    // Divide all vertex attribute (v0, v1, v2 must have equal attributes)
                    foreach(var key in v0C.Attribute)
                    {
                        v1C.Attribute[key].Multiply2(v1C.Position.W);
                        v2C.Attribute[key].Multiply2(v2C.Position.W);
                        v0C.Attribute[key].Multiply2(v0C.Position.W);
                    }
                    // Now in NDC space

                    v0C.Position = Rasterize(v0C.Position);
                    v1C.Position = Rasterize(v1C.Position);
                    v2C.Position = Rasterize(v2C.Position);
                    // Now in raster space
                    // Rasterize current triangle
                    RasterizeTriangle(v0C, v1C, v2C);
                }
            }
        }
        private void RasterizeTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            // Rasterizes triangle, barycentric version

            // Bounding box
            float xMin = MathHelper.Min3(v0.Position.X, v1.Position.X, v2.Position.X);
            float xMax = MathHelper.Max3(v0.Position.X, v1.Position.X, v2.Position.X);
            float yMin = MathHelper.Min3(v0.Position.Y, v1.Position.Y, v2.Position.Y);
            float yMax = MathHelper.Max3(v0.Position.Y, v1.Position.Y, v2.Position.Y);
            // Out of screen, throw away
            if (xMin > DeviceSize.Width - 1 || xMax < 0 || yMin > DeviceSize.Height - 1 || yMax < 0)
                return;
            // Limit to boundaries
            int x0 = (int)Math.Max(0, xMin);
            int x1 = (int)Math.Min(DeviceSize.Width - 1, Math.Floor(xMax));
            int y0 = (int)Math.Max(0, yMin);
            int y1 = (int)Math.Min(DeviceSize.Height - 1, Math.Floor(yMax));

            float area = 1 / MathHelper.Edge(v0.Position, v1.Position, v2.Position);
            Vector4 firstPixel = new Vector4(x0 + 0.5f, y0 + 0.5f, 0, 0); // Centre of the first pixel

            // Precompute edge function
            float w0c = MathHelper.Edge(v1.Position, v2.Position, firstPixel);
            float w0StepX = (v2.Position.Y - v1.Position.Y);
            float w0StepY = -(v2.Position.X - v1.Position.X);
            w0c -= w0StepX;
            float w0Start = w0c;

            float w1c = MathHelper.Edge(v2.Position, v0.Position, firstPixel);
            float w1StepX = (v0.Position.Y - v2.Position.Y);
            float w1StepY = -(v0.Position.X - v2.Position.X);
            w1c -= w1StepX;
            float w1Start = w1c;

            float w2c = MathHelper.Edge(v0.Position, v1.Position, firstPixel);
            float w2StepX = (v1.Position.Y - v0.Position.Y);
            float w2StepY = -(v1.Position.X - v0.Position.X);
            w2c -= w2StepX;
            float w2Start = w2c;

            for(int y = y0; y <= y1; ++y)
            {
                for(int x = x0; x <= x1; ++x)
                {
                    w0c += w0StepX; // Increment X
                    w1c += w1StepX;
                    w2c += w2StepX;

                    float w0 = w0c; // Barycentric coordinates
                    float w1 = w1c;
                    float w2 = w2c;

                    bool cullACW = !(w0 >= 0 && w1 >= 0 && w2 >= 0);
                    bool cullCW = !(w0 <= 0 && w1 <= 0 && w2 <= 0);

                    if (CullMode == CullMode.AntiClockwise && cullACW)
                        continue; // Pixel isn't within the triangle
                    else if (CullMode == CullMode.Clockwise && cullCW)
                        continue;
                    else if (CullMode == CullMode.None && cullACW && cullCW)
                        continue;

                    w0 *= area;
                    w1 *= area;
                    w2 *= area;

                    Vertex result = new Vertex();
                    // Interpolate final vertex
                    result.Position = v0.Position * w0 + v1.Position * w1 + v2.Position * w2;
                    // Divide by W
                    result.Position = result.Position.Homogenised;

                    int sequentialPosition = y * DeviceSize.Width + x;
                    
                    // Depth Test
                    bool depthTest = true;
                    if (DepthTest != DepthTestState.Disable)
                        depthTest = TestFunction(DepthFunction, result.Position.Z + DepthBias, zBuffer[sequentialPosition]);
                    // Stencil Test
                    if(StencilEnable)
                    {
                        byte destStencil = (byte)(stencilBuffer[sequentialPosition] & StencilFunctionMask);
                        byte srcStencil = (byte)(StencilReference & StencilFunctionMask);
                        bool stencilTest = TestFunction(StencilFunction, srcStencil, destStencil);
                        if (!stencilTest)
                            stencilBuffer[sequentialPosition] = GetStencilValue(StencilFail, srcStencil, destStencil); // Only Depth passes
                        else if(stencilTest && !depthTest)
                            stencilBuffer[sequentialPosition] = GetStencilValue(DepthFail, srcStencil, destStencil); // Only stencil passes
                        else if(stencilTest && depthTest)
                            stencilBuffer[sequentialPosition] = GetStencilValue(StencilDepthPass, srcStencil, destStencil); // Both pass
                        if (!stencilTest) // Skip if failed
                            continue;
                    }
                    if (!depthTest) // Skip if depthtest failed
                        continue;
                    
                    // Interpolate vertex attribute
                    foreach (var key in v0.Attribute)
                    {
                        IVertexAttribute data = v0.Attribute[key].Multiply(w0).Add(v1.Attribute[key].Multiply(w1).Add(v2.Attribute[key].Multiply(w2))).Multiply(result.Position.W);
                        result.Attribute.Add(key, data);
                    }

                    if (PixelShader(result, out Vector4 finalColour))
                    {
                        if (DepthTest == DepthTestState.Enable)
                        {
                            // Write Z Buffer if not discarded
                            zBuffer[sequentialPosition] = result.Position.Z;
                        }
                        int sequentialPos2 = (BytesPerPixel * y * DeviceSize.Width) + x * BytesPerPixel;
                        if (BlendEnable)
                        {
                            Vector4 destColour = new Vector4();
                            destColour.Z = buffer[sequentialPos2] / 255.0f;
                            destColour.Y = buffer[sequentialPos2 + 1] / 255.0f;
                            destColour.X = buffer[sequentialPos2 + 2] / 255.0f;
                            destColour.W = buffer[sequentialPos2 + 3] / 255.0f;
                            finalColour = Blend(finalColour, destColour);
                        }
                        buffer[sequentialPos2] = (byte)(finalColour.Z * 255);
                        buffer[sequentialPos2 + 1] = (byte)(finalColour.Y * 255);
                        buffer[sequentialPos2 + 2] = (byte)(finalColour.X * 255);
                        buffer[sequentialPos2 + 3] = (byte)(finalColour.W * 255);
                    }
                }
                w0Start += w0StepY; // Increment Y
                w0c = w0Start;
                w1Start += w1StepY;
                w1c = w1Start;
                w2Start += w2StepY;
                w2c = w2Start;
            }
        }

        private byte GetStencilValue(StencilOperation op, byte src, byte dest)
        {
            switch(op)
            {
                case StencilOperation.Decrement:
                    return Math.Max((byte)0, dest--);
                case StencilOperation.DecWrap:
                    {
                        if (dest == 0)
                            return 0xFF;
                        return dest--;
                    }
                case StencilOperation.Increment:
                    return Math.Min((byte)0xFF, dest);
                case StencilOperation.IncWrap:
                    {
                        if (dest == 0xFF)
                            return 0;
                        return dest++;
                    }
                case StencilOperation.Invert:
                    return (byte)~dest;
                case StencilOperation.Keep:
                    return dest;
                case StencilOperation.Replace:
                    return (byte)(src & StencilMask);
                case StencilOperation.Zero:
                    return 0;
                default:
                    throw new ArgumentException("Invalid StencilOperation");
            }
        }

        private bool TestFunction<T>(Comparison func, T src, T dest) where T : IComparable<T>
        {
            dynamic zSrc = src; // Stupid Generics
            dynamic zDest = dest;
            switch(func)
            {
                case Comparison.Always:
                    return true;
                case Comparison.Equal:
                    return zSrc == zDest;
                case Comparison.Greater:
                    return zSrc > zDest;
                case Comparison.GreaterEqual:
                    return zSrc >= zDest;
                case Comparison.Less:
                    return zSrc < zDest;
                case Comparison.LessEqual:
                    return zSrc <= zDest;
                case Comparison.Never:
                    return false;
                case Comparison.NotEqual:
                    return zSrc != zDest;
                default:
                    throw new ArgumentException("Invalid comparison");
            }
        }

        private Vector4 GetBlendFactor(BlendFunction function, Vector4 colourSrc, Vector4 colourDest)
        {
            switch(function)
            {
                case BlendFunction.Zero:
                    return Vector4.Zero;
                case BlendFunction.One:
                    return Vector4.One;
                case BlendFunction.SrcColour:
                    return colourSrc;
                case BlendFunction.OneMinusSrcColour:
                    return Vector4.One - colourSrc;
                case BlendFunction.DstColour:
                    return colourDest;
                case BlendFunction.OneMinusDstColour:
                    return Vector4.One - colourDest;
                case BlendFunction.SrcAlpha:
                    return new Vector4(colourSrc.W);
                case BlendFunction.OneMinusSrcAlpha:
                    return new Vector4(1 - colourSrc.W);
                case BlendFunction.DstAlpha:
                    return new Vector4(colourDest.W);
                case BlendFunction.OneMinusDstAlpha:
                    return new Vector4(1 - colourDest.W);
                case BlendFunction.ConstantColour:
                    return BlendColour;
                case BlendFunction.OneMinusConstantColour:
                    return Vector4.One - BlendColour;
                case BlendFunction.ConstantAlpha:
                    return new Vector4(BlendAlpha);
                case BlendFunction.OneMinusConstAlpha:
                    return new Vector4(1 - BlendAlpha);
                default:
                    throw new ArgumentException("Undefined Blendfunction");
            }
        }

        private Vector4 Blend(Vector4 colourSrc, Vector4 colourDest)
        {
            var factorSrc = GetBlendFactor(SrcBlendFunction, colourSrc, colourDest);
            var factorDest = GetBlendFactor(DestBlendFunction, colourSrc, colourDest);
            switch(BlendEquation)
            {
                case BlendEquation.Add:
                    return colourSrc * factorSrc + colourDest * factorDest;
                case BlendEquation.Subtract:
                    return colourSrc * factorSrc - colourDest * factorDest;
                case BlendEquation.ReverseSubtract:
                    return colourDest * factorDest + colourSrc * factorSrc;
                default:
                    throw new ArgumentException("Undefined Blendequation");
            }
        }
    }
}