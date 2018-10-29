using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GDI3D
{
    public struct Vector3 : IVertexAttribute
    {
        public float X { get; set; }
        public float R { get => X; set => X = value; }
        public float Y { get; set; }
        public float G { get => Y; set => Y = value; }
        public float Z { get; set; }
        public float B { get => Z; set => Z = value; }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(float v)
        {
            X = Y = Z = v;
        }

        public Vector3(Vector2 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = 1;
        }
        public Vector3(Vector4 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
            if (vec.W != 1 && vec.W != 0)
            {
                X /= vec.W;
                Y /= vec.W;
                Z /= vec.W;
            }
        }

        public Vector2 XY => new Vector2(X, Y);
        public Vector2 ToVector2() => new Vector2(this);
        public static Vector3 One => new Vector3(1, 1, 1);
        public static Vector3 Zero => new Vector3();

        // Spherische Koordinaten
        public Vector3(float theta, float phi)
        {
            X = (float)(Math.Cos(phi) * Math.Sin(theta));
            Y = (float)(Math.Sin(phi) * Math.Sin(theta));
            Z = (float)Math.Cos(theta);
        }

        public float Theta => (float)Math.Acos(MathHelper.Clamp(Z, -1, 1));
        public float CosTheta => Z;
        public float SinThetaSquared => Math.Max(0, 1 - CosTheta * CosTheta);
        public float SinTheta => (float)Math.Sqrt(SinThetaSquared);
        public float Phi
        {
            get
            {
                float a = (float)Math.Atan2(Y, X);
                return (a < 0) ? (float)(a + 2 * Math.PI) : a;
            }
        }
        public float CosPhi
        {
            get
            {
                float sinTheta = SinTheta;
                if (sinTheta == 0)
                    return 1;
                return MathHelper.Clamp(X / sinTheta, -1, 1);
            }
        }
        public float SinPhi
        {
            get
            {
                float sinTheta = SinTheta;
                if (sinTheta == 0)
                    return 0;
                return MathHelper.Clamp(Y / sinTheta, -1, 1);
            }
        }

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public float MagnitudeSquared => X * X + Y * Y + Z * Z;

        public bool IsNormalised => Magnitude == 1;
        public void Normalise()
        {
            float mag = Magnitude;
            if (mag == 0)
                return;
            float mInv = 1.0f / mag;
            X = X * mInv;
            Y = Y * mInv;
            Z = Z * mInv;
        }

        public Vector3 Normalised
        {
            get
            {
                Vector3 v = this;
                v.Normalise();
                return v;
            }
        }

        public void NormaliseF()
        {
            float magSquared = MagnitudeSquared;
            if (magSquared == 0)
                return;
            float mInv = MathHelper.FastInvSqrt(magSquared);
            X *= mInv;
            Y *= mInv;
            Z *= mInv;
        }

        public Vector3 NormalisedF
        {
            get
            {
                Vector3 v = this;
                v.NormaliseF();
                return v;
            }
        }

        public float Dot(Vector3 other) => X * other.X + Y * other.Y + Z * other.Z;
        public Vector3 Cross(Vector3 other) => new Vector3(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        public static Vector3 operator /(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        public static Vector3 operator *(Vector3 left, float right)
        {
            return new Vector3(left.X * right, left.Y * right, left.Z * right);
        }

        public static Vector3 operator *(float left, Vector3 right)
        {
            return right * left;
        }

        public static Vector3 operator /(Vector3 left, float right)
        {
            return new Vector3(left.X / right, left.Y / right, left.Z / right);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
                return false;
            return ((Vector3)obj) == this;
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(HashCodeHelper.CombineHashCodes(X.GetHashCode(), Y.GetHashCode()), Z.GetHashCode());
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public static Vector3 Lerp(Vector3 start, Vector3 end, float alpha)
        {
            return (start - end) * alpha + start;
        }
        public void Multiply2(float value) { this *= value; }
        public IVertexAttribute Multiply(float value) => this * value;
        public IVertexAttribute Multiply(IVertexAttribute value) => this * (Vector3)value;

        public IVertexAttribute Add(IVertexAttribute value) => this + (Vector3)value;
        public IVertexAttribute Subtract(IVertexAttribute value) => this - (Vector3)value;

        public IVertexAttribute Divide(float value) => this / value;
        public IVertexAttribute Divide(IVertexAttribute value) => this / (Vector3)value;
        public IVertexAttribute Clone() => new Vector3(X, Y, Z);
    }
}
