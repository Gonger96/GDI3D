using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GDI3D
{
    public struct Vector2 : IVertexAttribute
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
        public Vector2(float v)
        {
            X = Y = v;
        }
        public Vector2(Vector3 vec)
        {
            X = vec.X;
            Y = vec.Y;
            if(vec.Z != 1 && vec.Z != 0)
            {
                X /= vec.Z;
                Y /= vec.Z;
            }
        }

        public static Vector2 One => new Vector2(1, 1);
        public static Vector2 Zero => new Vector2();

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);

        public float MagnitudeSquared => X * X + Y * Y;

        public bool IsNormalised => Magnitude == 1.0f;

        public void Normalise()
        {
            float mag = Magnitude;
            if (mag == 0)
                return;
            float mInv = 1.0f / mag;
            X = X * mInv;
            Y = Y * mInv;
        }

        public void NormaliseF()
        {
            float magSquared = MagnitudeSquared;
            if (magSquared == 0)
                return;
            float mInv = MathHelper.FastInvSqrt(magSquared);
            X *= mInv;
            Y *= mInv;
        }

        public Vector2 NormalisedF
        {
            get
            {
                Vector2 v = this;
                v.NormaliseF();
                return v;
            }
        }

        public Vector2 Normalised
        {
            get
            {
                Vector2 v = this;
                v.Normalise();
                return v;
            }
        }

        public float Dot(Vector2 other)
        {
            return X * other.X + Y * other.Y;
        }

        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2 operator *(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X * right.X, left.Y * right.Y);
        }

        public static Vector2 operator /(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X / right.X, left.Y / right.Y);
        }

        public static Vector2 operator *(Vector2 left, float right)
        {
            return new Vector2(left.X * right, left.Y * right);
        }

        public static Vector2 operator *(float left, Vector2 right)
        {
            return right * left;
        }

        public static Vector2 operator /(Vector2 left, float right)
        {
            return new Vector2(left.X / right, left.Y / right);
        }

        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.X, -v.Y);
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return left.X != right.X || left.Y != right.Y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2))
                return false;
            return ((Vector2)obj) == this;
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(X.GetHashCode(), Y.GetHashCode());
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
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch(index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public float Slope(Vector2 src)
        {
            return (Y - src.Y) / (X - src.X);
        }

        public static Vector2 Lerp(Vector2 start, Vector2 end, float alpha)
        {
            return (start - end) * alpha + start;
        }
        public void Multiply2(float value) { this *= value; }
        public IVertexAttribute Multiply(float value) => this * value;
        public IVertexAttribute Multiply(IVertexAttribute value) => this * (Vector2)value;

        public IVertexAttribute Add(IVertexAttribute value) => this + (Vector2)value;
        public IVertexAttribute Subtract(IVertexAttribute value) => this - (Vector2)value;

        public IVertexAttribute Divide(float value) => this / value;
        public IVertexAttribute Divide(IVertexAttribute value) => this / (Vector2)value;
        public IVertexAttribute Clone() => new Vector2(X, Y);
    }
}
