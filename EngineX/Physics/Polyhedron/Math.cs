using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace EngineX.Physics
{
    public struct Matrix3
    {
        /// <summary>
        /// Matrix Row one, two and three
        /// </summary>
        private Vector3 row1, row2, row3;

        /// <summary>
        /// Inverts a three by three matrix
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Matrix InvertMatrix3(Matrix m)
        {
            m.M44 = 1;
            m.Invert();
            m.M44 = 0;
            return m;
        }

        public Matrix3(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            this.row1 = v1;
            this.row2 = v2;
            this.row3 = v3;
        }

        // row major
        public float M11 { get { return row1.X; } set { row1.X = value; } }
        public float M12 { get { return row1.Y; } set { row1.Y = value; } }
        public float M13 { get { return row1.Z; } set { row1.Z = value; } }
        public float M21 { get { return row2.X; } set { row2.X = value; } }
        public float M22 { get { return row2.Y; } set { row2.Y = value; } }
        public float M23 { get { return row2.Z; } set { row2.Z = value; } }
        public float M31 { get { return row3.X; } set { row3.X = value; } }
        public float M32 { get { return row3.Y; } set { row3.Y = value; } }
        public float M33 { get { return row3.Z; } set { row3.Z = value; } }

        public static Matrix3 operator *(Matrix3 m, float f)
        {
            return new Matrix3(
                m.row1 * f,
                m.row2 * f,
                m.row3 * f);
        }

        public static Matrix3 operator *(float f, Matrix3 m)
        {
            return new Matrix3(
                m.row1 * f,
                m.row2 * f,
                m.row3 * f);
        }

        public static Vector3 operator *(Matrix3 m, Vector3 v)
        {
            return new Vector3(
                Vector3.Dot(m.row1, v),
                Vector3.Dot(m.row2, v),
                Vector3.Dot(m.row3, v));
        }

        public static Vector3 operator *(Vector3 v, Matrix3 m)
        {
            return new Vector3(
                v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31,
                v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32,
                v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33);
        }

        public static Matrix3 operator +(Matrix3 m1, Matrix3 m2)
        {
            return new Matrix3(
                m1.row1 + m2.row1,
                m1.row2 + m2.row2,
                m1.row3 + m2.row3);
        }

        public static Matrix3 operator -(Matrix3 m1, Matrix3 m2)
        {
            return new Matrix3(
                m1.row1 - m2.row1,
                m1.row2 - m2.row2,
                m1.row3 - m2.row3);
        }

        public static Matrix3 operator *(Matrix3 m1, Matrix3 m2)
        {
            return new Matrix3(
                m1.row1 * m2,
                m1.row2 * m2,
                m1.row3 * m2);
        }

        public Matrix3 Inverse()
        {
            return Matrix3.FromMatrix4(Matrix.Invert(Matrix3.ToMatrix4(this)));
        }

        public Matrix3 Transpose()
        {
            return new Matrix3(new Vector3(M11, M21, M31), new Vector3(M12, M22, M32), new Vector3(M13, M23, M33));
        }

        public static Matrix3 CrossProductMatrix(Vector3 v)
        {
            Matrix3 m = Matrix3.Zero;
            m.M12 = -v.Z;
            m.M13 = v.Y;
            m.M21 = v.Z;
            m.M23 = -v.X;
            m.M31 = -v.Y;
            m.M32 = v.X;

            return m;
        }

        public static Matrix ToMatrix4(Matrix3 m3)
        {
            Matrix m4 = Matrix.Identity;
            m4.M11 = m3.M11;
            m4.M21 = m3.M12;
            m4.M31 = m3.M13;
            m4.M12 = m3.M21;
            m4.M22 = m3.M22;
            m4.M32 = m3.M23;
            m4.M13 = m3.M31;
            m4.M23 = m3.M32;
            m4.M33 = m3.M33;
            return m4;
        }

        public static Matrix3 FromMatrix4(Matrix m4)
        {
            Matrix3 m3 = Matrix3.Zero;
            m3.M11 = m4.M11;
            m3.M21 = m4.M12;
            m3.M31 = m4.M13;
            m3.M12 = m4.M21;
            m3.M22 = m4.M22;
            m3.M32 = m4.M23;
            m3.M13 = m4.M31;
            m3.M23 = m4.M32;
            m3.M33 = m4.M33;
            return m3;
        }


        // static stuff

        public static readonly Matrix3 Identity;
        public static readonly Matrix3 Zero;

        static Matrix3()
        {
            Identity = new Matrix3(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));
            Zero = new Matrix3(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        }
    }
}
