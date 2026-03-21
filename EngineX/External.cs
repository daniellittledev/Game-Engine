using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D9;

namespace EngineX
{
    /// <summary>
    /// Custom vertex formats not covered by the built-in CustomVertex types.
    /// </summary>
    public static class CustomVertex2
    {
        /// <summary>
        /// Custom vertex with two texture coordinate sets.
        /// Replaces old Microsoft.DirectX.Direct3D.CustomVertex2.PositionTextured2.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PositionTextured2
        {
            /// <summary>Position coordinate X</summary>
            public float X;
            /// <summary>Position coordinate Y</summary>
            public float Y;
            /// <summary>Position coordinate Z</summary>
            public float Z;
            /// <summary>Texture one coordinate u</summary>
            public float Tu1;
            /// <summary>Texture one coordinate v</summary>
            public float Tv1;
            /// <summary>Texture two coordinate u</summary>
            public float Tu2;
            /// <summary>Texture two coordinate v</summary>
            public float Tv2;

            /// <summary>Vertex Format (Position + 2 texture sets)</summary>
            public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Texture2;

            /// <summary>
            /// Vertex Elements for use with VertexDeclaration
            /// </summary>
            public static readonly VertexElement[] Declarator = new VertexElement[]
            {
                new VertexElement(0, 0,  DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
                new VertexElement(0, 12, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
                new VertexElement(0, 20, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 1),
                VertexElement.VertexDeclarationEnd
            };

            /// <summary>Vertex stride in bytes</summary>
            public static readonly int StrideSize = Marshal.SizeOf(typeof(PositionTextured2));

            /// <summary>Creates a vertex with a position and two texture coordinates.</summary>
            public PositionTextured2(float x, float y, float z, float u1, float v1, float u2, float v2)
            {
                X = x; Y = y; Z = z;
                Tu1 = u1; Tv1 = v1;
                Tu2 = u2; Tv2 = v2;
            }

            /// <summary>Creates a vertex with a position and two texture coordinates.</summary>
            public PositionTextured2(Vector3 position, float u1, float v1, float u2, float v2)
            {
                X = position.X; Y = position.Y; Z = position.Z;
                Tu1 = u1; Tv1 = v1;
                Tu2 = u2; Tv2 = v2;
            }

            /// <summary>Gets and sets the position</summary>
            public Vector3 Position
            {
                get { return new Vector3(X, Y, Z); }
                set { X = value.X; Y = value.Y; Z = value.Z; }
            }
        }
    }
}
