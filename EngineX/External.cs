using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft
{
    namespace DirectX
    {
        namespace Direct3D
        {
            /// <summary>
            /// Custom vertex formatts
            /// </summary>
            public class CustomVertex2
            {

                /// <summary>
                /// Custom vertex with two textures
                /// </summary>
                public struct PositionTextured2
                {
                    /// <summary>
                    /// Position coordinate X
                    /// </summary>
                    public float X;
                    /// <summary>
                    /// Position coordinate Y
                    /// </summary>
                    public float Y;
                    /// <summary>
                    /// Position coordinate Z
                    /// </summary>
                    public float Z;
                    /// <summary>
                    /// Texture one coordinate u
                    /// </summary>
                    public float Tu1;
                    /// <summary>
                    /// Texture one coordinate v
                    /// </summary>
                    public float Tv1;
                    /// <summary>
                    /// Texture two coordinate u
                    /// </summary>
                    public float Tu2;
                    /// <summary>
                    /// Texture two coordinate v
                    /// </summary>
                    public float Tv2;

                    /// <summary>
                    /// Vertix Format
                    /// </summary>
                    public static readonly VertexFormats Format = VertexFormats.Position | VertexFormats.Texture2;
                    
                    /// <summary>
                    /// Vertex Elements
                    /// </summary>
                    public static readonly VertexElement[] Declarator = new VertexElement[]
                    {
                        new VertexElement( 0, 0, DeclarationType.Float3, //Pos
                            DeclarationMethod.Default, DeclarationUsage.Position, 0 ),

                        new VertexElement( 0, 12, DeclarationType.Float2, //Tex1
                            DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0 ),

                        new VertexElement( 0, 20, DeclarationType.Float2, //Tex2
                            DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0 ),

                        VertexElement.VertexDeclarationEnd 
                    };

                    /// <summary>
                    /// Vertix Stride Size
                    /// </summary>
                    public static readonly int StrideSize =
                        VertexInformation.GetDeclarationVertexSize(Declarator, 0);

                    /// <summary>Creates a vertex with a position and two texture coordinates.</summary>
                    /// <param name="x">X position</param>
                    /// <param name="y">Y position</param>
                    /// <param name="z">Z position</param>
                    /// <param name="u1">First texture coordinate U</param>
                    /// <param name="v1">First texture coordinate V</param>
                    /// <param name="u2">Second texture coordinate U</param>
                    /// <param name="v2">Second texture coordinate V</param>
                    public PositionTextured2(float x, float y, float z, float u1, float v1, float u2, float v2)
                    {
                        X = x;
                        Y = y;
                        Z = z;
                        Tu1 = u1;
                        Tv1 = v1;
                        Tu2 = u2;
                        Tv2 = v2;
                    }

                    /// <summary>Creates a vertex with a position and two texture coordinates.</summary>
                    /// <param name="position">Position</param>
                    /// <param name="u1">First texture coordinate U</param>
                    /// <param name="v1">First texture coordinate V</param>
                    /// <param name="u2">Second texture coordinate U</param>
                    /// <param name="v2">Second texture coordinate V</param>
                    public PositionTextured2(Vector3 position, float u1, float v1, float u2, float v2)
                    {
                        X = position.X;
                        Y = position.Y;
                        Z = position.Z;
                        Tu1 = u1;
                        Tv1 = v1;
                        Tu2 = u2;
                        Tv2 = v2;
                    }

                    /// <summary>Gets and sets the position</summary>
                    public Vector3 Position
                    {
                        get
                        {
                            return new Vector3(X, Y, Z);
                        }
                        set
                        {
                            X = value.X;
                            Y = value.Y;
                            Z = value.Z;
                        }
                    }
                }

            }
        }
    }
}
