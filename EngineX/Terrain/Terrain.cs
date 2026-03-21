using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;

using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace EngineX
{

    namespace Terrain
    {

        public class TerrainBF
        {

            /// <summary>
            /// Rendering Device
            /// </summary>
            Device Device1;

            /// <summary>
            /// Height Map
            /// </summary>
            float[,] Heights; // Height Map

            /// <summary>
            /// Surface Texture
            /// </summary>
            Texture Surface; // SurfaceTexture
            /// <summary>
            /// Detail Texture 
            /// </summary>
            Texture Detail; // Detail Texture

            /// <summary>
            /// Vertex Buffer
            /// </summary>
            VertexBuffer VertexB; // Vertex Buffer
            /// <summary>
            /// Index Buffer
            /// </summary>
            IndexBuffer IndexB; // Index Buffer

            /// <summary>
            /// Number of Points in terrain.
            /// </summary>
            int VertexCount; // Number of Points in map
            /// <summary>
            /// Number of Points of triangles in terrain.
            /// </summary>
            int IndexCount; // Vertex build structure
            /// <summary>
            /// The number of Indicies to be rendered.
            /// </summary>
            int IndexToRender; // The number of Indicies to be rendered
            /// <summary>
            /// The number of triangles Rendered.
            /// </summary>
            int PrimCount; // The number of triangles Rendered

            /// <summary>
            /// Vertex across count
            /// </summary>
            int Width; // Vertex across count
            /// <summary>
            /// Vertex along count
            /// </summary>
            int Height; // Vertex along count

            /// <summary>
            /// Terrain hieght scale.
            /// </summary>
            float YScale;
            /// <summary>
            /// Terrain map scale.
            /// </summary>
            float MapScale;

            /// <summary>
            /// Detail texture filepath.
            /// </summary>
            String TexDetail;
            /// <summary>
            /// Detail surface filepath.
            /// </summary>
            String TexSurface;

            /// <summary>
            /// Initilize brute force terrain object.
            /// </summary>
            /// <param name="device"></param>
            /// <param name="HeightMap"></param>
            /// <param name="SurfaceTex"></param>
            /// <param name="Detailtex"></param>
            /// <param name="Yscale"></param>
            /// <param name="Mapscale"></param>
            public TerrainBF(Device device, String HeightMap, String SurfaceTex, String Detailtex, Single Yscale, Single Mapscale)
            {

                Device1 = device;

                YScale = Yscale;
                MapScale = Mapscale;

                string Splitter = ".";
                string[] Type = HeightMap.Split(Splitter.ToCharArray());
                //float inv256 = 1/256;

                if (string.Compare(Type[1], "raw", true) == 0)
                { // Raw Files

                    FileStream fs = new FileStream(HeightMap, FileMode.Open, FileAccess.Read);
                    BinaryReader r = new BinaryReader(fs);

                    int lenght = (int)System.Math.Sqrt(r.BaseStream.Length);

                    Heights = new float[lenght, lenght];

                    for (int i = 0; i < lenght; i++)
                        for (int y = 0; y < lenght; y++)
                        {
                            Heights[i, y] = (float)(r.ReadByte()) / 255;
                            VertexCount++;

                        }
                    r.Close();

                    Width = lenght;
                    Height = lenght;

                }
                else
                { // Image Files

                    Bitmap Map;
                    Map = new Bitmap(HeightMap);

                    Width = Map.Width;
                    Height = Map.Height;

                    Heights = new float[Width, Height];

                    for (int x = 0; x < Width; x++)
                    {

                        for (int y = 0; y < Height; y++)
                        {

                            Heights[x, y] = (1 - Map.GetPixel(x, y).GetBrightness());
                            VertexCount++;

                        }

                    }

                }


                IndexCount = ((Width - 1) * (Height - 1)) * 6;

                TexSurface = SurfaceTex;
                TexDetail = Detailtex;

                Surface = Texture.FromFile(Device1, SurfaceTex); //, 0,0, 1, Usage.AutoGenerateMipMap , Format.R5G6B5 , Pool.SystemMemory , Filter.None, Filter.None, 0 );
                Detail = Texture.FromFile(Device1, Detailtex); //, 0, 0, 1, Usage.AutoGenerateMipMap, Format.R5G6B5, Pool.SystemMemory, Filter.None, Filter.None, 0);

            }

            /// <summary>
            /// Reload textures from file
            /// </summary>
            public void ReloadTextures()
            {
                Surface = Texture.FromFile(Device1, TexSurface);
                Detail = Texture.FromFile(Device1, TexDetail);
            }

            /// <summary>
            /// Refresh rendering properties
            /// </summary>
            public void RefreshDeviceProperties()
            {

                Device1.SetSamplerState(1, SamplerState.MinFilter, TextureFilter.Linear);
                Device1.SetSamplerState(1, SamplerState.MagFilter, TextureFilter.Linear);
                Device1.SetSamplerState(1, SamplerState.MipFilter, TextureFilter.Linear);
                Device1.SetSamplerState(1, SamplerState.MagFilter, TextureFilter.Linear);
                Device1.SetSamplerState(1, SamplerState.MipMapLodBias, 1.0f);
                //Device1.SetSamplerState(1, SamplerState.SrgbTexture, true);

            }

            /// <summary>
            /// Setup terrain and build verticies
            /// </summary>
            /// <param name="TexScale"></param>
            /// <param name="TileTex"></param>
            /// <param name="DetailScale"></param>
            public void Initilize(Single TexScale, bool TileTex, Single DetailScale)
            {

                // Set up the vertex buffer
                VertexB = new VertexBuffer(Device1, VertexCount * Marshal.SizeOf(typeof(CustomVertex2.PositionTextured2)), Usage.None, VertexFormat.None, Pool.Managed);
                CustomVertex2.PositionTextured2[] Verticies;
                Verticies = new CustomVertex2.PositionTextured2[VertexCount];


                Single TexX1;
                Single TexY1;

                Single TexX2;
                Single TexY2;

                TexX2 = DetailScale;
                TexY2 = DetailScale;

                if (TileTex != false)
                {
                    TexX1 = TexScale;
                    TexY1 = TexScale;
                }
                else
                {
                    TexX1 = TexScale / Height;
                    TexY1 = TexScale / Height;
                }

                int i = 0;

                int W1 = Width - 1;
                int H1 = Height - 1;

                for (int x = 0; x <= W1; x++)
                {

                    for (int z = 0; z <= H1; z++)
                    {

                        Verticies[i].X = x * MapScale;
                        Verticies[i].Y = Heights[x, z] * YScale;
                        Verticies[i].Z = z * MapScale;

                        Verticies[i].Tu1 = x * TexX1;
                        Verticies[i].Tv1 = z * TexY1;

                        Verticies[i].Tu2 = x * TexX2;
                        Verticies[i].Tv2 = z * TexY2;

                        i++;

                    }
                }



                DataStream dsVB = VertexB.Lock(0, 0, LockFlags.None);
                dsVB.WriteRange(Verticies);
                VertexB.Unlock();

                // Setup the index Buffer
                IndexB = new IndexBuffer(Device1, IndexCount * sizeof(int), Usage.Dynamic, Pool.SystemMemory, false);

            }

            /// <summary>
            /// Build Index Buffer
            /// </summary>
            public void Update()
            {


                int[] Indicies;
                Indicies = new int[IndexCount];
                DataStream dsIB = IndexB.Lock(0, 0, LockFlags.Discard);


                int TL, BR, BL, TR;

                IndexToRender = 0;
                PrimCount = 0;

                int W1 = Width - 2;
                int H1 = Height - 2;

                for (int x = 0; x <= W1; x++)
                {

                    for (int y = 0; y <= H1; y++)
                    {

                        TL = x + y * Width;
                        BR = x + 1 + (y + 1) * Width;
                        TR = x + 1 + y * Width;
                        BL = x + (y + 1) * Width;

                        Indicies[IndexToRender] = BR;
                        Indicies[IndexToRender + 1] = BL;
                        Indicies[IndexToRender + 2] = TL;

                        Indicies[IndexToRender + 3] = TR;
                        Indicies[IndexToRender + 4] = BR;
                        Indicies[IndexToRender + 5] = TL;

                        IndexToRender += 6;
                        PrimCount += 2;

                    }
                }

                dsIB.WriteRange(Indicies);
                IndexB.Unlock();

            }

            /// <summary>
            /// Tests if a ray intersects the terrain
            /// </summary>
            /// <param name="ray">Ray to test</param>
            /// <param name="point">Point of intersection</param>
            /// <returns>Boolean: Intersection Detected</returns>
            public bool RayIntersect(Physics.Ray ray, out Vector3 point)
            {
                int deltaX = GetTileX(ray.Direction.X - ray.Origin.X);
                int deltaY = GetTileY(ray.Direction.Z - ray.Origin.Z);

                float absDeltaX = Math.Abs(deltaX);     //We're comparing lengths, so we must take the
                float absDeltaY = Math.Abs(deltaY);     //  absolute value to ensure correct decisions.
                int sx = Math.Sign(deltaX);                   //The sign of the delta values tell us if we need to
                int sy = Math.Sign(deltaY);                   //  add or subtract 1 to traverse the axes of the line.

                int x = GetTileX(ray.Origin.X);
                int y = GetTileY(ray.Origin.Z);

                int x2 = GetTileX(ray.Direction.X);
                int y2 = GetTileY(ray.Direction.Z);

                float numerator;

                // Test
                if (RayTestTile(x, y, ray, out point))
                    return true;

                if (absDeltaX > absDeltaY)
                {
                    numerator = absDeltaY / 2;                  //Initialize numerator

                    while (x != x2)
                    {
                        x = x + sx;                             //Move along the 'long' axis
                        numerator = numerator + absDeltaY;      //Update the numerator

                        if (numerator > absDeltaX)              //Check to see if num>denom
                        {
                            numerator = numerator - absDeltaX;  //If so, subtract 1 from the fraction...
                            y = y + sy;                         //...and add 1 to the 'short' axis
                        }

                        // Test
                        if (RayTestTile(x, y, ray, out point))
                            return true;

                    }
                }
                else
                {
                    numerator = absDeltaX / 2;                  //Initialize numerator 

                    while (y != y2)
                    {
                        y = y + sy;                             //Move along the 'long' axis
                        numerator = numerator + absDeltaX;      //Update the numerator

                        if (numerator > absDeltaY)              //Check to see if num>denom
                        {
                            numerator = numerator - absDeltaY;  //If so, subtract 1 from the fraction...
                            x = x + sx;                         //...and add 1 to the 'short' axis
                        }

                        // Test
                        if (RayTestTile(x, y, ray, out point))
                            return true;
                    }
                }

                return false;

            }

            /// <summary>
            /// Test a tile against a ray
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            /// <param name="ray"></param>
            /// <param name="point"></param>
            /// <returns></returns>
            private bool RayTestTile(int X, int Y, Physics.Ray ray, out Vector3 point)
            {
                Vector3 topLeft = new Vector3(X * Width, GetGridHeight(X, Y), Y * Height);
                Vector3 topRight = new Vector3((X + 1) * Width, GetGridHeight(X + 1, Y), Y * Height);
                Vector3 bottomLeft = new Vector3(X * Width, GetGridHeight(X, Y + 1), (Y + 1) * Height);
                Vector3 bottomRight = new Vector3((X + 1) * Width, GetGridHeight(X + 1, Y + 1), (Y + 1) * Height);

                Physics.Triangle Left = new EngineX.Physics.Triangle(topLeft, topRight, bottomLeft);
                Physics.Triangle Right = new EngineX.Physics.Triangle(topRight, bottomLeft, bottomRight);


                //Fix
                if (Left.RayIntersect(ray, out point))
                    return true;
                else
                    if (Right.RayIntersect(ray, out point))
                        return true;
                    else
                        return false;
            }

            /// <summary>
            /// Get tile X coordinate from a real position
            /// </summary>
            /// <param name="X"></param>
            /// <returns></returns>
            private int GetTileX(float X)
            {
                return (int)(X / Width);
            }

            /// <summary>
            /// Get tile Y coordinate from a real position
            /// </summary>
            /// <param name="Y"></param>
            /// <returns></returns>
            private int GetTileY(float Y)
            {
                return (int)(Y / Height);
            }

            /// <summary>
            /// Get the real height from tile coordinates
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            /// <returns></returns>
            private float GetGridHeight(int X, int Y)
            {
                return Heights[X, Y] * YScale;
            }

            /// <summary>
            /// Render terrain
            /// </summary>
            public void Render()
            {

                // Set the Matricies
                // Device1.Transform.World = Matrix.Identity;

                // Set the vertex format
                Device1.VertexFormat = CustomVertex2.PositionTextured2.Format;

                // The Vertices
                Device1.SetStreamSource(0, VertexB, 0, CustomVertex2.PositionTextured2.StrideSize);

                Device1.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
                Device1.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.SelectArg1);
                Device1.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Current);
                Device1.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Texture);
                Device1.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.AddSigned);

                // The Indices
                Device1.Indices = IndexB;

                // Set the Texture
                Device1.SetTexture(0, Surface);
                Device1.SetTexture(1, Detail);

                // Rendering
                Device1.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimCount);

            }

            /// <summary>
            /// Get height from postion
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Z"></param>
            /// <returns></returns>
            public float GetHeight(float X, float Z)
            {

                float X1, Y1;

                X1 = X / MapScale;
                Y1 = Z / MapScale;

                if (X1 > Width - 1 | X1 < 0)
                {
                    return 0;
                }
                if (Y1 > Height - 1 | Y1 < 0)
                {
                    return 0;
                }

                float minX = (float)System.Math.Truncate(X1);
                float minY = (float)System.Math.Truncate(Y1);

                Plane Plane1;
                Vector3 P1, P2, P3;
                if ((Y1 - (int)minY) < (X1 - (int)minX))
                {
                    P1.X = minX * MapScale; // TL
                    P1.Y = minY * MapScale;
                    P1.Z = Heights[(int)minX, (int)minY] * YScale;

                    P2.X = minX * MapScale; // BL
                    P2.Y = (minY + 1) * MapScale;
                    P2.Z = Heights[(int)minX, (int)minY + 1] * YScale;

                    P3.X = ((int)minX + 1) * MapScale; // BR
                    P3.Y = ((int)minY + 1) * MapScale;
                    P3.Z = Heights[(int)minX + 1, (int)minY + 1] * YScale;

                    Plane1 = Plane.FromPoints(P1, P2, P3);

                }
                else
                {

                    P1.X = minX * MapScale; // TL
                    P1.Y = minY * MapScale;
                    P1.Z = Heights[(int)minX, (int)minY] * YScale;

                    P2.X = minX * MapScale; // BR
                    P2.Y = (minY + 1) * MapScale;
                    P2.Z = Heights[(int)minX, (int)minY + 1] * YScale;

                    P3.X = (minX + 1) * MapScale; // TR
                    P3.Y = (minY + 1) * MapScale;
                    P3.Z = Heights[(int)minX + 1, (int)minY + 1] * YScale;

                    Plane1 = Plane.FromPoints(P1, P2, P3);

                }

                float H1 = Plane1.B / (Plane1.A * X + Plane1.C * Z + Plane1.D);

                //Console.WriteLine(Plane1.ToString());
                //Console.WriteLine(H1);

                return H1;

            }

            /// <summary>
            /// Get height from postion
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Z"></param>
            /// <returns></returns>
            public float GetHeight2(float X, float Z)
            {

                float X1, Y1;

                X1 = X / MapScale;
                Y1 = Z / MapScale;

                if (X1 > Width - 1 | X1 < 0)
                {
                    return 0;
                }
                if (Y1 > Height - 1 | Y1 < 0)
                {
                    return 0;
                }

                float minX = (float)System.Math.Truncate(X1);
                float minY = (float)System.Math.Truncate(Y1);

                float decX = (float)(X1 - minX);
                float decY = (float)(Y1 - minY);

                Vector3 v0 = new Vector3(minX * MapScale,
                    Heights[(int)minX, (int)minY] * YScale,
                    minY * MapScale);
                Vector3 v1 = new Vector3((minX + 1) * MapScale,
                    Heights[(int)minX + 1, (int)minY] * YScale,
                    minY * MapScale);
                Vector3 v2 = new Vector3(minX * MapScale,
                    Heights[(int)minX, (int)minY + 1] * YScale,
                    (minY + 1) * MapScale);
                Vector3 v3 = new Vector3((minX + 1) * MapScale,
                    Heights[(int)minX + 1, (int)minY + 1] * YScale,
                    (minY + 1) * MapScale);

                Vector3 interpA = Vector3.Lerp(v0, v1, decX);
                Vector3 interpB = Vector3.Lerp(v2, v3, decX);
                Vector3 interpF = Vector3.Lerp(interpA, interpB, decY);

                return interpF.Y;

            }

            /// <summary>
            /// Get Tangent, Binormal and Normal from position.
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Z"></param>
            /// <param name="Tangent"></param>
            /// <param name="Binormal"></param>
            /// <param name="Normal"></param>
            public void GetTBN(float X, float Z, ref Vector3 Tangent, ref Vector3 Binormal, ref Vector3 Normal)
            {
                Vector3 v0 = new Vector3(X, GetHeight(X, Z), Z);
                Vector3 v1 = new Vector3(X + 1, GetHeight(X + 1, Z), Z);
                Vector3 v2 = new Vector3(X, GetHeight(X, Z + 1), Z + 1);

                MathX.Math3D.GenerateTBN(v0, v1, v2, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), ref Normal, ref Tangent, ref Binormal);
            }

        }

    }

}
