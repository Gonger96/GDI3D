using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDI3D
{
    public struct Vector4 : IVertexAttribute
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public float R { get => X; set => X = value; }
        public float G { get => Y; set => Y = value; }
        public float B { get => Z; set => Z = value; }
        public float A { get => W; set => W = value; }

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public Vector4(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 1;
        }
        public Vector4(float v) => X = Y = Z = W = v;
        public Vector4(Vector3 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
            W = 1;
        }
        public Vector4(Vector2 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = 1;
            W = 1;
        }
        public static Vector4 One => new Vector4(1, 1, 1, 1);
        public static Vector4 Zero => new Vector4();
        public Vector2 XY => new Vector2(X, Y);
        public Vector3 XYZ => new Vector3(X, Y, Z);
        public Vector3 RGB => new Vector3(X, Y, Z);
        public Vector3 ToVector3() => new Vector3(this);
        public Vector2 ToVector2() => new Vector2(new Vector3(this));

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        public float MagnitudeSquared => X * X + Y * Y + Z * Z + W * W;

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
            W = W * mInv;
        }

        public Vector4 Normalised
        {
            get
            {
                Vector4 v = this;
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
            W *= mInv;
        }

        public Vector4 NormalisedF
        {
            get
            {
                Vector4 v = this;
                v.NormaliseF();
                return v;
            }
        }

        public float Dot(Vector4 other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;

        public static Vector4 operator +(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        }

        public static Vector4 operator -(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        }

        public static Vector4 operator *(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        }

        public static Vector4 operator /(Vector4 left, Vector4 right)
        {
            return new Vector4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
        }

        public static Vector4 operator *(Vector4 left, float right)
        {
            return new Vector4(left.X * right, left.Y * right, left.Z * right, left.W * right);
        }

        public static Vector4 operator *(float left, Vector4 right)
        {
            return right * left;
        }

        public static Vector4 operator /(Vector4 left, float right)
        {
            return new Vector4(left.X / right, left.Y / right, left.Z / right, left.W / right);
        }

        public static Vector4 operator -(Vector4 v)
        {
            return new Vector4(-v.X, -v.Y, -v.Z, -v.W);
        }

        public static bool operator ==(Vector4 left, Vector4 right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W;
        }

        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return left.X != right.X || left.Y != right.Y || left.Z != right.Z || left.W != right.W;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector4))
                return false;
            return ((Vector4)obj) == this;
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(HashCodeHelper.CombineHashCodes(HashCodeHelper.CombineHashCodes(X.GetHashCode(), Y.GetHashCode()), Z.GetHashCode()), W.GetHashCode());
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
                    case 3:
                        return W;
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
                    case 3:
                        W = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public void Homogenise()
        {
            if (W == 0 || W == 1)
                return;
            W = 1 / W;
            X *= W;
            Y *= W;
            Z *= W;
        }

        public Vector4 Homogenised
        {
            get
            {
                Vector4 copy = this;
                copy.Homogenise();
                return copy;
            }
        }

        public static Vector4 Lerp(Vector4 start, Vector4 end, float alpha)
        {
            return (start - end) * alpha + start;
        }

        public void Multiply2(float value) { this *= value; }
        public IVertexAttribute Multiply(float value) => this * value;
        public IVertexAttribute Multiply(IVertexAttribute value) => this * (Vector4)value;

        public IVertexAttribute Add(IVertexAttribute value) => this + (Vector4)value;
        public IVertexAttribute Subtract(IVertexAttribute value) => this - (Vector4)value;

        public IVertexAttribute Divide(float value) => this / value;
        public IVertexAttribute Divide(IVertexAttribute value) => this / (Vector4)value;
        public IVertexAttribute Clone() => new Vector4(X, Y, Z, W);
    }
}