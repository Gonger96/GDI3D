using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GDI3D
{
    public struct Matrix4
    {
        // War vorher float[4,4] hatte aber Probleme mit ReferenceType
        // fixed float[] geht nur für 1 dimensionale Arrays, da müsste man bei jedem Zugriff den Indexer umrechnen
        // Habe also jedes Feld einzeln deklariert und mir die Invert Methode von MS geklaut ;)
        public float M00 { get; set; }
        public float M01 { get; set; }
        public float M02 { get; set; }
        public float M03 { get; set; }

        public float M10 { get; set; }
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; }

        public float M20 { get; set; }
        public float M21 { get; set; }
        public float M22 { get; set; }
        public float M23 { get; set; }

        public float M30 { get; set; }
        public float M31 { get; set; }
        public float M32 { get; set; }
        public float M33 { get; set; }

        public static Matrix4 Identity => new Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

        public bool IsIdentity => Identity == this;

        public Matrix4(float a, float b, float c, float d, float e, float f, float g, float h, float i, float j, float k, float l, float m, float n, float o, float p)
        {
            M00 = a;
            M01 = b;
            M02 = c;
            M03 = d;
            M10 = e;
            M11 = f;
            M12 = g;
            M13 = h;
            M20 = i;
            M21 = j;
            M22 = k;
            M23 = l;
            M30 = m;
            M31 = n;
            M32 = o;
            M33 = p;
        }
        public Matrix4(float angleOfView, float nearPlane, float farPlane)
        {
            float scale = 1 / (float)Math.Tan(angleOfView * 0.5f * Math.PI / 180);
            M00 = scale; // | Scale X
            M01 = 0;
            M02 = 0;
            M03 = 0;
            M10 = 0;
            M11 = scale; // | Scale Y
            M12 = 0;
            M13 = 0;
            M20 = 0;
            M21 = 0;
            M22 = -farPlane / (farPlane - nearPlane); //             | remap Z
            M23 = -1; // W = -Z;
            M30 = 0;
            M31 = 0;
            M32 = -farPlane * nearPlane / (farPlane - nearPlane); // | 0..1
            M33 = 0;
        }
        public Matrix4(float left, float right, float top, float bottom, float nearPlane, float farPlane)
        {
            M00 = 2 * nearPlane / (right - left);
            M01 = 0;
            M02 = 0;
            M03 = 0;

            M10 = 0;
            M11 = 2 * nearPlane / (top - bottom);
            M12 = 0;
            M13 = 0;

            M20 = (right + left) / (right - left);
            M21 = (top + bottom) / (top - bottom);
            M22 = -(farPlane + nearPlane) / (farPlane - nearPlane);
            M23 = -1;

            M30 = 0;
            M31 = 0;
            M32 = -2 * farPlane * nearPlane / (farPlane - nearPlane);
            M33 = 0;
        }

        public static Matrix4 operator *(Matrix4 left, Matrix4 right)
        {
            if (left.IsIdentity)
                return right;
            if (right.IsIdentity)
                return left;
            return new Matrix4(
                left.M00 * right.M00 + left.M01 * right.M10 +
                left.M02 * right.M20 + left.M03 * right.M30,
                left.M00 * right.M01 + left.M01 * right.M11 +
                left.M02 * right.M21 + left.M03 * right.M31,
                left.M00 * right.M02 + left.M01 * right.M12 +
                left.M02 * right.M22 + left.M03 * right.M32,
                left.M00 * right.M03 + left.M01 * right.M13 +
                left.M02 * right.M23 + left.M03 * right.M33,
                left.M10 * right.M00 + left.M11 * right.M10 +
                left.M12 * right.M20 + left.M13 * right.M30,
                left.M10 * right.M01 + left.M11 * right.M11 +
                left.M12 * right.M21 + left.M13 * right.M31,
                left.M10 * right.M02 + left.M11 * right.M12 +
                left.M12 * right.M22 + left.M13 * right.M32,
                left.M10 * right.M03 + left.M11 * right.M13 +
                left.M12 * right.M23 + left.M13 * right.M33,
                left.M20 * right.M00 + left.M21 * right.M10 +
                left.M22 * right.M20 + left.M23 * right.M30,
                left.M20 * right.M01 + left.M21 * right.M11 +
                left.M22 * right.M21 + left.M23 * right.M31,
                left.M20 * right.M02 + left.M21 * right.M12 +
                left.M22 * right.M22 + left.M23 * right.M32,
                left.M20 * right.M03 + left.M21 * right.M13 +
                left.M22 * right.M23 + left.M23 * right.M33,
                left.M30 * right.M00 + left.M31 * right.M10 +
                left.M32 * right.M20 + left.M33 * right.M30,
                left.M30 * right.M01 + left.M31 * right.M11 +
                left.M32 * right.M21 + left.M33 * right.M31,
                left.M30 * right.M02 + left.M31 * right.M12 +
                left.M32 * right.M22 + left.M33 * right.M32,
                left.M30 * right.M03 + left.M31 * right.M13 +
                left.M32 * right.M23 + left.M33 * right.M33);

        }

        public static Matrix4 operator /(Matrix4 left, Matrix4 right)
        {
            return left * right.Inverse;
        }

        public void Transpose()
        {
            this = Transposed;
        }
        public Matrix4 Transposed
        {
            get
            {
                return new Matrix4(M00, M10, M20, M30, M01, M11, M21, M31, M02, M12, M22, M32, M03, M13, M23, M33);
            }
        }

        public bool IsAffine
        {
            get
            {
                return IsIdentity || (M03 == 0 && M13 == 0 && M23 == 0 && M33 == 1); // Last column = identity
            }
        }
        private float GetAffineDeterminant()
        {
            float z20 = M01 * M12 - M11 * M02;
            float z10 = M21 * M02 - M01 * M22;
            float z00 = M11 * M22 - M21 * M12;

            return M20 * z20 + M10 * z10 + M00 * z00;
        }
        public float Determinant
        {
            get
            {
                if (IsIdentity)
                    return 1;
                if (IsAffine)
                    return GetAffineDeterminant();
                // compute all six 2x2 determinants of 2nd two columns
                float y01 = M02 * M13 - M12 * M03;
                float y02 = M02 * M23 - M22 * M03;
                float y03 = M02 * M33 - M32 * M03;
                float y12 = M12 * M23 - M22 * M13;
                float y13 = M12 * M33 - M32 * M13;
                float y23 = M22 * M33 - M32 * M23;

                // Compute 3x3 cofactors for 1st the column
                float z30 = M11 * y02 - M21 * y01 - M01 * y12;
                float z20 = M01 * y13 - M11 * y03 + M31 * y01;
                float z10 = M21 * y03 - M31 * y02 - M01 * y23;
                float z00 = M11 * y23 - M21 * y13 + M31 * y12;

                return M30 * z30 + M20 * z20 + M10 * z10 + M00 * z00;
            }
        }

        // See https://referencesource.microsoft.com/#PresentationCore/Core/CSharp/System/Windows/Media3D/Matrix3D.cs
        public void Invert()
        {
            if (IsIdentity)
                return;
            if (IsAffine)
            {
                AffineInvert();
                return;
            }
            float y01 = M02 * M13 - M12 * M03;
            float y02 = M02 * M23 - M22 * M03;
            float y03 = M02 * M33 - M32 * M03;
            float y12 = M12 * M23 - M22 * M13;
            float y13 = M12 * M33 - M32 * M13;
            float y23 = M22 * M33 - M32 * M23;

            // Compute 3x3 cofactors for 1st the column
            float z30 = M11 * y02 - M21 * y01 - M01 * y12;
            float z20 = M01 * y13 - M11 * y03 + M31 * y01;
            float z10 = M21 * y03 - M31 * y02 - M01 * y23;
            float z00 = M11 * y23 - M21 * y13 + M31 * y12;

            // Compute 4x4 determinant
            float det = M30 * z30 + M20 * z20 + M10 * z10 + M00 * z00;

            if (IsZero(det))
                throw new InvalidOperationException("Matrix can't be inverted");

            // Compute 3x3 cofactors for the 2nd column
            float z31 = M00 * y12 - M10 * y02 + M20 * y01;
            float z21 = M10 * y03 - M30 * y01 - M00 * y13;
            float z11 = M00 * y23 - M20 * y03 + M30 * y02;
            float z01 = M20 * y13 - M30 * y12 - M10 * y23;

            // Compute all six 2x2 determinants of 1st two columns
            y01 = M00 * M11 - M10 * M01;
            y02 = M00 * M21 - M20 * M01;
            y03 = M00 * M31 - M30 * M01;
            y12 = M10 * M21 - M20 * M11;
            y13 = M10 * M31 - M30 * M11;
            y23 = M20 * M31 - M30 * M21;

            // Compute all 3x3 cofactors for 2nd two columns
            float z33 = M02 * y12 - M12 * y02 + M22 * y01;
            float z23 = M12 * y03 - M32 * y01 - M02 * y13;
            float z13 = M02 * y23 - M22 * y03 + M32 * y02;
            float z03 = M22 * y13 - M32 * y12 - M12 * y23;
            float z32 = M13 * y02 - M23 * y01 - M03 * y12;
            float z22 = M03 * y13 - M13 * y03 + M33 * y01;
            float z12 = M23 * y03 - M33 * y02 - M03 * y23;
            float z02 = M13 * y23 - M23 * y13 + M33 * y12;

            float rcp = 1.0f / det;

            // Multiply all 3x3 cofactors by reciprocal & transpose
            M00 = z00 * rcp;
            M01 = z10 * rcp;
            M02 = z20 * rcp;
            M03 = z30 * rcp;

            M10 = z01 * rcp;
            M11 = z11 * rcp;
            M12 = z21 * rcp;
            M13 = z31 * rcp;

            M20 = z02 * rcp;
            M21 = z12 * rcp;
            M22 = z22 * rcp;
            M23 = z32 * rcp;

            M30 = z03 * rcp;
            M31 = z13 * rcp;
            M32 = z23 * rcp;
            M33 = z33 * rcp;
        }

        private static bool IsZero(float value)
        {
            const float FLT_EPSILON = 1.192092896e-07F;
            return Math.Abs(value) < 10.0 * FLT_EPSILON;
        }

        private void AffineInvert()
        {
            float z20 = M01 * M12 - M11 * M02;
            float z10 = M21 * M02 - M01 * M22;
            float z00 = M11 * M22 - M21 * M12;
            float det = M20 * z20 + M10 * z10 + M00 * z00;

            if (IsZero(det))
                throw new InvalidOperationException("Matrix can't be inverted");

            // Compute 3x3 non-zero cofactors for the 2nd column
            float z21 = M10 * M02 - M00 * M12;
            float z11 = M00 * M22 - M20 * M02;
            float z01 = M20 * M12 - M10 * M22;

            // Compute all six 2x2 determinants of 1st two columns
            float y01 = M00 * M11 - M10 * M01;
            float y02 = M00 * M21 - M20 * M01;
            float y03 = M00 * M31 - M30 * M01;
            float y12 = M10 * M21 - M20 * M11;
            float y13 = M10 * M31 - M30 * M11;
            float y23 = M20 * M31 - M30 * M21;

            // Compute all non-zero and non-one 3x3 cofactors for 2nd
            // two columns
            float z23 = M12 * y03 - M32 * y01 - M02 * y13;
            float z13 = M02 * y23 - M22 * y03 + M32 * y02;
            float z03 = M22 * y13 - M32 * y12 - M12 * y23;
            float z22 = y01;
            float z12 = -y02;
            float z02 = y12;

            float rcp = 1.0f / det;

            // Multiply all 3x3 cofactors by reciprocal & transpose
            M00 = z00 * rcp;
            M01 = z10 * rcp;
            M02 = z20 * rcp;

            M10 = z01 * rcp;
            M11 = z11 * rcp;
            M12 = z21 * rcp;

            M20 = z02 * rcp;
            M21 = z12 * rcp;
            M22 = z22 * rcp;

            M30 = z03 * rcp;
            M31 = z13 * rcp;
            M32 = z23 * rcp;
        }

        public Matrix4 Inverse
        {
            get
            {
                Matrix4 m = this;
                m.Invert();
                return m;
            }
        }

        public static Matrix4 operator ~(Matrix4 m) => m.Inverse;

        public static Matrix4 operator *(Matrix4 m, float scalar)
        {
            return new Matrix4(m.M00 * scalar, m.M01 * scalar, m.M02 * scalar, m.M03 * scalar,
                                m.M10 * scalar, m.M11 * scalar, m.M12 * scalar, m.M13 * scalar,
                                m.M20 * scalar, m.M21 * scalar, m.M22 * scalar, m.M23 * scalar,
                                m.M30 * scalar, m.M21 * scalar, m.M32 * scalar, m.M33 * scalar);
        }
        public static Matrix4 operator *(float scalar, Matrix4 m) => m * scalar;

        public static Matrix4 operator /(Matrix4 m, float scalar)
        {
            return new Matrix4(m.M00 / scalar, m.M01 / scalar, m.M02 / scalar, m.M03 / scalar,
                    m.M10 / scalar, m.M11 / scalar, m.M12 / scalar, m.M13 / scalar,
                    m.M20 / scalar, m.M21 / scalar, m.M22 / scalar, m.M23 / scalar,
                    m.M30 / scalar, m.M21 / scalar, m.M32 / scalar, m.M33 / scalar);
        }

        //// Transform parts
        public static Vector4 operator *(Matrix4 m, Vector4 v)
        {
            Vector4 res = new Vector4();
            res.X = v.X * m.M00 + v.Y * m.M10 + v.Z * m.M20 + v.W * m.M30;
            res.Y = v.X * m.M01 + v.Y * m.M11 + v.Z * m.M21 + v.W * m.M31;
            res.Z = v.X * m.M02 + v.Y * m.M12 + v.Z * m.M22 + v.W * m.M32;
            res.W = v.X * m.M03 + v.Y * m.M13 + v.Z * m.M23 + v.W * m.M33;
            return res;
        }

        public static Vector4 operator /(Matrix4 m, Vector4 v) => m.Inverse * v;

        public Vector3 MultiplyNormal(Vector3 v)
        {
            Vector3 res = new Vector3();
            res.X = v.X * M00 + v.Y * M10 + v.Z * M20;
            res.Y = v.X * M01 + v.Y * M11 + v.Z * M21;
            res.Z = v.X * M02 + v.Y * M12 + v.Z * M22;
            return res;
        }
        public Vector3 DivideNormal(Vector3 v)
        {
            return Inverse.MultiplyNormal(v);
        }

        //// Transform stuff
        public void Translate(float x, float y, float z)
        {
            Matrix4 translation = Matrix4.Identity;
            translation.M30 = x;
            translation.M31 = y;
            translation.M32 = z;
            this = this * translation;
        }
        public void Translate(Vector3 translation) => Translate(translation.X, translation.Y, translation.Z);

        public void Scale(float x, float y, float z)
        {
            Matrix4 translation = Matrix4.Identity;
            translation.M00 = x;
            translation.M11 = y;
            translation.M22 = z;
            this = this * translation;
        }
        public void Scale(Vector3 scale) => Scale(scale.X, scale.Y, scale.Z);

        public void RotateX(float theta)
        {
            Matrix4 rotation = Matrix4.Identity;
            rotation.M11 = (float)Math.Cos(theta);
            rotation.M12 = (float)Math.Sin(theta);
            rotation.M21 = -rotation.M12;
            rotation.M22 = rotation.M11;
            this = this * rotation;
        }
        public void RotateX(float theta, Vector3 pivot)
        {
            Matrix4 rotation = Matrix4.Identity;
            rotation.Translate(-pivot);
            rotation.RotateX(theta);
            rotation.Translate(pivot);
            this = this * rotation;
        }

        public void RotateY(float theta)
        {
            Matrix4 rotation = Matrix4.Identity;
            rotation.M00 = (float)Math.Cos(theta);
            rotation.M02 = -(float)Math.Sin(theta);
            rotation.M20 = -rotation.M02;
            rotation.M22 = rotation.M00;
            this = this * rotation;
        }
        public void RotateY(float theta, Vector3 pivot)
        {
            Matrix4 rotation = Matrix4.Identity;
            rotation.Translate(-pivot);
            rotation.RotateY(theta);
            rotation.Translate(pivot);
            this = this * rotation;
        }

        public void RotateZ(float theta)
        {
            Matrix4 rotation = Matrix4.Identity;
            rotation.M00 = (float)Math.Cos(theta);
            rotation.M01 = (float)Math.Sin(theta);
            rotation.M10 = -rotation.M01;
            rotation.M11 = rotation.M00;
            this = this * rotation;
        }
        public void RotateZ(float theta, Vector3 pivot)
        {
            Matrix4 rotation = Matrix4.Identity;
            rotation.Translate(-pivot);
            rotation.RotateZ(theta);
            rotation.Translate(pivot);
            this = this * rotation;
        }

        public static bool operator ==(Matrix4 a, Matrix4 b)
        {
            return a.M00 == b.M00 && a.M01 == b.M01 && a.M02 == b.M02 && a.M03 == b.M03 &&
                   a.M10 == b.M11 && a.M11 == b.M11 && a.M12 == b.M12 && a.M13 == b.M13 &&
                   a.M20 == b.M22 && a.M21 == b.M21 && a.M22 == b.M22 && a.M23 == b.M23 &&
                   a.M30 == b.M33 && a.M31 == b.M31 && a.M32 == b.M32 && a.M33 == b.M33;
        }

        public static bool operator !=(Matrix4 a, Matrix4 b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Matrix4))
                return false;
            return ((Matrix4)obj) == this;
        }

        public override int GetHashCode()
        {
            return M00.GetHashCode() ^ M01.GetHashCode() ^ M02.GetHashCode() ^ M03.GetHashCode() ^
                   M10.GetHashCode() ^ M11.GetHashCode() ^ M12.GetHashCode() ^ M13.GetHashCode() ^
                   M20.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode() ^ M23.GetHashCode() ^
                   M30.GetHashCode() ^ M31.GetHashCode() ^ M32.GetHashCode() ^ M33.GetHashCode();
        }

        public override string ToString()
        {
            return $"[{M00}{M01}{M02}{M03}]\r\n[{M10}{M11}{M12}{M13}]\r\n[{M20}{M21}{M22}{M23}]\r\n[{M30}{M31}{M32}{M33}]\r\n";
        }
    }
}
