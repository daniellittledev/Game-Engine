using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D9;

namespace EngineX
{
    /// <summary>
    /// Replacement for Microsoft.DirectX.Direct3D.CustomVertex.
    /// Defines vertex formats compatible with SharpDX.Direct3D9.
    /// </summary>
    public static class CustomVertex
    {
        /// <summary>
        /// Position (XYZ) + one texture coordinate (Tu, Tv).
        /// Equivalent to old CustomVertex.PositionTextured.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PositionTextured
        {
            public float X, Y, Z;
            public float Tu, Tv;

            public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Texture1;
            public static readonly int Stride = Marshal.SizeOf(typeof(PositionTextured));

            public PositionTextured(float x, float y, float z, float tu, float tv)
            { X = x; Y = y; Z = z; Tu = tu; Tv = tv; }

            public PositionTextured(Vector3 pos, float tu, float tv)
            { X = pos.X; Y = pos.Y; Z = pos.Z; Tu = tu; Tv = tv; }

            public Vector3 Position
            {
                get { return new Vector3(X, Y, Z); }
                set { X = value.X; Y = value.Y; Z = value.Z; }
            }
        }

        /// <summary>
        /// Position (XYZ) + diffuse color (ARGB int).
        /// Equivalent to old CustomVertex.PositionColored.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PositionColored
        {
            public float X, Y, Z;
            public int Color;

            public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Diffuse;
            public static readonly int Stride = Marshal.SizeOf(typeof(PositionColored));

            public PositionColored(float x, float y, float z, int color)
            { X = x; Y = y; Z = z; Color = color; }

            public PositionColored(Vector3 pos, int color)
            { X = pos.X; Y = pos.Y; Z = pos.Z; Color = color; }

            public Vector3 Position
            {
                get { return new Vector3(X, Y, Z); }
                set { X = value.X; Y = value.Y; Z = value.Z; }
            }
        }

        /// <summary>
        /// Position (XYZ) + diffuse color (ARGB int) + one texture coordinate.
        /// Equivalent to old CustomVertex.PositionColoredTextured.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PositionColoredTextured
        {
            public float X, Y, Z;
            public int Color;
            public float Tu, Tv;

            public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Texture1;
            public static readonly int Stride = Marshal.SizeOf(typeof(PositionColoredTextured));

            public PositionColoredTextured(float x, float y, float z, int color, float tu, float tv)
            { X = x; Y = y; Z = z; Color = color; Tu = tu; Tv = tv; }

            public PositionColoredTextured(Vector3 pos, int color, float tu, float tv)
            { X = pos.X; Y = pos.Y; Z = pos.Z; Color = color; Tu = tu; Tv = tv; }

            public Vector3 Position
            {
                get { return new Vector3(X, Y, Z); }
                set { X = value.X; Y = value.Y; Z = value.Z; }
            }
        }

        /// <summary>
        /// Position (XYZ) + normal (XYZ) + one texture coordinate (Tu, Tv).
        /// Equivalent to old CustomVertex.PositionNormalTextured.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PositionNormalTextured
        {
            public float X, Y, Z;
            public float Nx, Ny, Nz;
            public float Tu, Tv;

            public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1;
            public static readonly int Stride = Marshal.SizeOf(typeof(PositionNormalTextured));

            public PositionNormalTextured(float x, float y, float z, float nx, float ny, float nz, float tu, float tv)
            { X = x; Y = y; Z = z; Nx = nx; Ny = ny; Nz = nz; Tu = tu; Tv = tv; }

            public Vector3 Position
            {
                get { return new Vector3(X, Y, Z); }
                set { X = value.X; Y = value.Y; Z = value.Z; }
            }

            public Vector3 Normal
            {
                get { return new Vector3(Nx, Ny, Nz); }
                set { Nx = value.X; Ny = value.Y; Nz = value.Z; }
            }
        }

        /// <summary>
        /// Pre-transformed position (XYZW) + diffuse color.
        /// Equivalent to old CustomVertex.TransformedColored.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TransformedColored
        {
            public float X, Y, Z, Rhw;
            public int Color;

            public static readonly VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Diffuse;
            public static readonly int Stride = Marshal.SizeOf(typeof(TransformedColored));

            public TransformedColored(float x, float y, float z, float rhw, int color)
            { X = x; Y = y; Z = z; Rhw = rhw; Color = color; }

            public Vector4 Position
            {
                get { return new Vector4(X, Y, Z, Rhw); }
                set { X = value.X; Y = value.Y; Z = value.Z; Rhw = value.W; }
            }
        }

        /// <summary>
        /// Position only (XYZ). Used for bounding volume computation and physics.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PositionOnly
        {
            public float X, Y, Z;

            public static readonly VertexFormat Format = VertexFormat.Position;
            public static readonly int Stride = Marshal.SizeOf(typeof(PositionOnly));
            public static readonly int StrideSize = Marshal.SizeOf(typeof(PositionOnly));

            public PositionOnly(float x, float y, float z)
            { X = x; Y = y; Z = z; }

            public PositionOnly(Vector3 pos)
            { X = pos.X; Y = pos.Y; Z = pos.Z; }

            public Vector3 Position
            {
                get { return new Vector3(X, Y, Z); }
                set { X = value.X; Y = value.Y; Z = value.Z; }
            }
        }
    }
}
