using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GDI3D
{
    public static class MathHelper
    {
        public const float RadToDeg = (float)(180.0 / Math.PI);
        public const float DegToRad = (float)(Math.PI / 180.0);
        public static float Clamp(float value, float min, float max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
        {
            return new Vector2(Clamp(value.X, min.X, max.X), Clamp(value.Y, min.Y, max.Y));
        }

        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(Clamp(value.X, min.X, max.X), Clamp(value.Y, min.Y, max.Y), Clamp(value.Z, min.Z, max.Z));
        }

        public static Vector4 Clamp(Vector4 value, Vector4 min, Vector4 max)
        {
            return new Vector4(Clamp(value.X, min.X, max.X), Clamp(value.Y, min.Y, max.Y), Clamp(value.Z, min.Z, max.Z), Clamp(value.W, min.W, max.W));
        }

        public static float Lerp(float start, float end, float alpha)
        {
            return (end - start) * alpha + start;
        }

        public static float Edge(Vector4 a, Vector4 b, Vector4 c)
        {
            return (c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X);
        }

        public static Vector2 Lerp(Vector2 start, Vector2 end, float alpha)
        {
            return (end - start) * alpha + start;
        }

        public static Vector3 Lerp(Vector3 start, Vector3 end, float alpha)
        {
            return (end - start) * alpha + start;
        }

        public static Vector4 Lerp(Vector4 start, Vector4 end, float alpha)
        {
            return (end - start) * alpha + start;
        }

        public static IVertexAttribute Lerp(IVertexAttribute start, IVertexAttribute end, float alpha)
        {
            return end.Subtract(start).Multiply(alpha).Add(start);
        }

        public static float Min3(float f1, float f2, float f3)
        {
            float min1 = f1 < f2 ? f1 : f2;
            return min1 < f3 ? min1 : f3;
        }
        public static float Max3(float f1, float f2, float f3)
        {
            float max1 = f1 > f2 ? f1 : f2;
            return max1 > f3 ? max1 : f3;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct FloatUint32
        {
            [FieldOffset(0)]
            public float f;
            [FieldOffset(0)]
            public UInt32 i;
            public FloatUint32(float fl)
            {
                i = 0; // Stupid compiler doesn't know that it's unioned
                f = fl;
            }
        }
        public static float FastInvSqrt(float val)
        {
            FloatUint32 fi = new FloatUint32(val);
            const float threeHalfs = 1.5f;
            float inputHalf = val * 0.5f;
            fi.i = 0x5f3759df - (fi.i >> 1);
            fi.f = fi.f * (threeHalfs - (inputHalf * fi.f * fi.f)); // First iteration
            //fi.f = fi.f * (threeHalfs - (inputHalf * fi.f * fi.f)); // Second iteration uncomment for higher precision
            //fi.f = fi.f * (threeHalfs - (inputHalf * fi.f * fi.f)); // Third iteration
            return fi.f;
        }
    }
}
