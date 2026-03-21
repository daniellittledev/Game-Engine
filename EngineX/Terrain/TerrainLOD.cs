using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace EngineX
{
    namespace Terrain
    {

        public class TerrainLOD
        {
            Device device;
            TransformsManager transfrom;

            // Textures
            Texture highTexture;
            Texture groundTexture;
            Texture lowTexture;
            Texture detailTexture;

            // Buffers
            VertexBuffer vertexBuffer;
            IndexBuffer indexBuffer;

            //IndexBuffer indexBuffer2; //Remove
            Vertex[] Verticies;

            // Vertix Info
            VertexDeclaration vertexDeclaration;
            int StrideSize;

            // Render Counts
            int vertexCount;
            int indexCount;
            int primCount;

            // Scales
            float groundScale;
            float heightScale;

            // Levels
            float hightLevel;
            float lowLevel;

            // HeightMap
            float[,] heights;
            Vector3[,] normals;

            // Dimentions
            int span;

            // Extra Info
            int spanArray;
            float scaleGInv;
            int displace;
            float realDisplace;
            float realSpan;

            // Effects
            Effect terrainEffect;
            EffectHandle transformMatrix;
            EffectHandle ambientLight;
            EffectHandle lightDirection;

            // LOD
            TriangleNode Root;

            /// <summary>
            /// Initilize brute force terrain object.
            /// </summary>
            /// <param name="device"></param>
            /// <param name="transfrom"></param>
            /// <param name="HeightMap"></param>
            /// <param name="TextureHigh"></param>
            /// <param name="TextureLow"></param>
            /// <param name="TextureGround"></param>
            /// <param name="TextureDetail"></param>
            /// <param name="LowLevel"></param>
            /// <param name="HighLevel"></param>
            /// <param name="ScaleGround"></param>
            /// <param name="ScaleHeight"></param>
            /// <param name="ScaleTexture"></param>
            /// <param name="ScaleDetail"></param>
            /// <param name="TileTex"></param>
            public TerrainLOD(Device device, TransformsManager transfrom,
                // Files
                string HeightMap,
                string TextureHigh,
                string TextureGround,
                string TextureLow,
                string TextureDetail,
                // Levels
                float BaseLevel,
                float LowLevel,
                float HighLevel,
                // Scales
                float ScaleGround,
                float ScaleHeight,
                float ScaleTexture,
                float ScaleDetail,
                // Options
                bool TileTex)
            {

                this.device = device;
                this.transfrom = transfrom;

                heightScale = ScaleHeight;
                groundScale = ScaleGround;


                # region  // Load Terrain Data ==============================================================

                string Splitter = ".";
                string[] Type = HeightMap.Split(Splitter.ToCharArray());
                //float inv256 = 1/256;

                if (string.Compare(Type[1], "raw", true) == 0)
                { // Raw Files

                    FileStream fs = new FileStream(HeightMap, FileMode.Open, FileAccess.Read);
                    BinaryReader r = new BinaryReader(fs);

                    int lenght = (int)System.Math.Sqrt(r.BaseStream.Length);

                    heights = new float[lenght, lenght];

                    for (int i = 0; i < lenght; i++)
                        for (int y = 0; y < lenght; y++)
                        {
                            heights[i, y] = ((float)(r.ReadByte()) / 255) - BaseLevel;
                            vertexCount++;

                        }
                    r.Close();

                    span = lenght;
                    span = lenght;

                }
                else
                { // Image Files

                    Bitmap Map;
                    Map = new Bitmap(HeightMap);

                    span = Map.Width;
                    span = Map.Height;

                    heights = new float[span, span];

                    for (int x = 0; x < span; x++)
                    {

                        for (int y = 0; y < span; y++)
                        {

                            heights[x, y] = (1 - Map.GetPixel(x, y).GetBrightness()) * heightScale - BaseLevel;
                            vertexCount++;

                        }

                    }

                }

                // Extra Info
                realSpan = span * groundScale;
                spanArray = span - 1;
                scaleGInv = 1 / ScaleGround;

                displace = span / 2;
                realDisplace = displace * groundScale;

                # endregion

                # region // Setup Vertex Buffer ============================================================


                float TexX1;
                float TexY1;

                float TexX2;
                float TexY2;

                TexX2 = ScaleDetail;
                TexY2 = ScaleDetail;

                if (TileTex != false)
                {
                    TexX1 = ScaleTexture;
                    TexY1 = ScaleTexture;
                }
                else
                {
                    TexX1 = ScaleTexture / span;
                    TexY1 = ScaleTexture / span;
                }


                int count = 0;


                vertexBuffer = new VertexBuffer(typeof(Vertex), vertexCount,
                        device, Usage.None, Vertex.Format, Pool.Managed);

                Verticies = (Vertex[])vertexBuffer.Lock(0, LockFlags.None);

                for (int z = 0; z <= spanArray; z++)
                {
                    for (int x = 0; x <= spanArray; x++)
                    {

                        Verticies[count++] = new Vertex(
                            new Vector3(x * ScaleGround - realDisplace,
                                        heights[x, z],
                                        z * ScaleGround - realDisplace), //* ScaleHeight

                            new Vector3(0, 1, 0), // FIX THIS
                            x * TexX1, z * TexY1,
                            x * TexX2, z * TexY2,

                            // Blend Low
                            heights[x, z] < lowLevel ? 1 : 0,
                            // Blend Ground
                            (heights[x, z] > lowLevel && heights[x, z] < HighLevel) ? 1 : 0,
                            // Blend High
                            heights[x, z] > HighLevel ? 1 : 0,
                            // Blend Detail
                            0.5f,
                            // Normalise Blending
                            true);
                    }
                }

                // Vertex Normals
                for (int z = 1; z < spanArray - 1; z++)
                {
                    for (int x = 1; x < spanArray - 1; x++)
                    {
                        // Calculate the real normals by using the cross product of the vertex' neighbours

                        Vector3 X = Vector3.Subtract(
                            Verticies[z * span + x + 1].Position,
                            Verticies[z * span + x - 1].Position);

                        Vector3 Z = Vector3.Subtract(
                            Verticies[(z + 1) * span + x].Position,
                            Verticies[(z - 1) * span + x].Position);

                        Vector3 Normal = Vector3.Cross(Z, X);
                        Normal.Normalize();

                        Verticies[z * span + x].Normal = Normal;
                    }
                }

                vertexBuffer.Unlock();

                # endregion

                # region // Setup the index Buffer =========================================================

                indexCount = ((span - 1) * (span - 1)) * 6;
                indexBuffer = new IndexBuffer(typeof(int), indexCount, device, Usage.Dynamic, Pool.SystemMemory);

                // Remove
                //indexBuffer2 = new IndexBuffer(typeof(int), 3, device, Usage.Dynamic, Pool.SystemMemory);

                # endregion

                # region // Load the textures ==============================================================

                highTexture = TextureLoader.FromFile(device, TextureHigh);
                groundTexture = TextureLoader.FromFile(device, TextureGround);
                lowTexture = TextureLoader.FromFile(device, TextureLow);
                detailTexture = TextureLoader.FromFile(device, TextureDetail);

                # endregion

                # region // Load the Effect ================================================================

                string s;
                terrainEffect = Effect.FromFile(device, @"..\..\Resources\shader.fx", null, "", ShaderFlags.None, null, out s);
                if (s != "")
                {
                    MessageBox.Show(s);
                    Application.Exit();
                    return;

                }

                terrainEffect.Technique = "TransformTexture";

                transformMatrix = terrainEffect.GetParameter(null, "WorldViewProj");
                ambientLight = terrainEffect.GetParameter(null, "light");
                lightDirection = terrainEffect.GetParameter(null, "ambient");

                // Effect Options
                terrainEffect.SetValue("ambient", 0.5f);
                terrainEffect.SetValue("light", new float[] { span * 0.5f, 80, span * 0.5f });

                // Effect Textures
                terrainEffect.SetValue("low", lowTexture);
                terrainEffect.SetValue("med", groundTexture);
                terrainEffect.SetValue("hig", highTexture);
                terrainEffect.SetValue("TextureDetail", detailTexture);
                # endregion

                # region // Vertex Declaration =============================================================

                // Create the vertexdeclaration
                VertexElement[] v = new VertexElement[] { 
                    new VertexElement(0,0,DeclarationType.Float3,DeclarationMethod.Default,DeclarationUsage.Position,0),           // Position
					new VertexElement(0,12,DeclarationType.Float3,DeclarationMethod.Default,DeclarationUsage.Normal,0),            // Normal
					new VertexElement(0,24,DeclarationType.Float2,DeclarationMethod.Default,DeclarationUsage.TextureCoordinate,0), // Textures
					new VertexElement(0,32,DeclarationType.Float2,DeclarationMethod.Default,DeclarationUsage.TextureCoordinate,1), // Detail
                    new VertexElement(0,40,DeclarationType.Float2,DeclarationMethod.Default,DeclarationUsage.TextureCoordinate,2), // Blending
                    new VertexElement(0,48,DeclarationType.Float2,DeclarationMethod.Default,DeclarationUsage.TextureCoordinate,3), // Blending
					VertexElement.VertexDeclarationEnd};
                vertexDeclaration = new VertexDeclaration(device, v);

                StrideSize = VertexInformation.GetDeclarationVertexSize(v, 0);

                # endregion

                # region // Initilise LOD ==================================================================

                Root = new TriangleNode(span, ref heights, ScaleGround);

                # endregion

                # region // debug Code Remove ===============================================================

                //vertexCount = 4;

                ////CustomVertex.PositionNormalTextured

                //vertexBuffer = new VertexBuffer(typeof(Vertex), vertexCount,
                //    device, Usage.None, Vertex.Format, Pool.SystemMemory);

                ////CustomVertex.PositionNormalTextured[] Verticies2;
                //Verticies = (Vertex[])vertexBuffer.Lock(0, LockFlags.None);
                //count = 0;

                //Verticies[0] = new Vertex(
                //    new Vector3(0, 0, 0),
                //    new Vector3(0, 1, 0),
                //    0,
                //    0,
                //        0,
                //        0,
                //    0, 1, 0, 1, true);
                ////Verticies2[0] = new CustomVertex.PositionNormalTextured(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 0, 0);

                //Verticies[1] = new Vertex(
                //    new Vector3(50, 0, 0),
                //    new Vector3(0, 1, 0),
                //    1,
                //    0,
                //        1,
                //        0,
                //    .5f, .5f, 0, 1, true);
                ////Verticies2[1] = new CustomVertex.PositionNormalTextured(new Vector3(50, 0, 0), new Vector3(0, 1, 0), 1, 0);

                //Verticies[2] = new Vertex(
                //    new Vector3(50, 0, 50),
                //    new Vector3(0, 1, 0),
                //    0,
                //    1,
                //        0,
                //        1,
                //    1, 0, 0, 1, true);
                ////Verticies2[2] = new CustomVertex.PositionNormalTextured(new Vector3(50, 0, 50), new Vector3(0, 1, 0), 0, 1);

                //Verticies[3] = new Vertex(
                //    new Vector3(0, 0, 50),
                //    new Vector3(0, 1, 0),
                //    1,
                //    1,
                //        1,
                //        1,
                //    0, 0, 1, 1, true);
                ////Verticies2[3] = new CustomVertex.PositionNormalTextured(new Vector3(0, 0, 50), new Vector3(0, 1, 0), 1, 1);

                //vertexBuffer.Unlock();
                //////////// Index

                //indexCount = 6;
                //indexBuffer = new IndexBuffer(typeof(int), indexCount, device, Usage.Dynamic, Pool.SystemMemory);

                //int[] buffer = (int[])indexBuffer.Lock(0, LockFlags.None);
                ////buffer = indices.ToArray();

                //count = 0;
                //buffer[0] = 0;
                //buffer[1] = 2;
                //buffer[2] = 1;

                //buffer[3] = 0;
                //buffer[4] = 2;
                //buffer[5] = 3;

                //primCount = 2;

                //indexBuffer.Unlock();

                # endregion

                //RefreshDeviceProperties();
            }


            /// <summary>
            /// Refresh rendering properties
            /// </summary>
            public void RefreshDeviceProperties()
            {

                device.SamplerState[1].MinFilter = TextureFilter.Linear;
                device.SamplerState[1].MagFilter = TextureFilter.Linear;
                device.SamplerState[1].MipFilter = TextureFilter.Linear;
                device.SamplerState[1].MagFilter = TextureFilter.Linear;
                device.SamplerState[1].MipMapLevelOfDetailBias = 1;

            }

            public void Update(Vector3 viewPosition, float accuracy)
            {
                Root.Update(viewPosition, accuracy);
            }

            /// <summary>
            /// Constructs the updated index list
            /// </summary>
            public void BuildMesh()
            {

                List<int> indices = new List<int>();
                Root.GetIndicies(ref indices, span);
                indexBuffer.SetData(indices.ToArray(), 0, LockFlags.Discard);
                primCount = indices.Count / 3;
                indexCount = indices.Count;

            }

            /// <summary>
            /// Render current mesh static
            /// </summary>
            public void Render()
            {

                device.VertexDeclaration = vertexDeclaration;
                device.SetStreamSource(0, vertexBuffer, 0, StrideSize);

                device.Indices = indexBuffer;

                device.VertexFormat = Vertex.Format;

                // Update the effect
                terrainEffect.SetValue(transformMatrix, transfrom.View * transfrom.Projection);

                // Begin rendering with the effect
                terrainEffect.Begin(FX.None);

                //Render the first pass
                terrainEffect.BeginPass(0);

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primCount);

                terrainEffect.EndPass();

                //Render the first pass
                //terrainEffect.BeginPass(0);

                //device.Indices = indexBuffer2;
                //terrainEffect.SetValue(transformMatrix, transfrom.View * transfrom.Projection); //* Matrix.Translation(0, 0, 0)
                //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, 1);

                //terrainEffect.EndPass();

                terrainEffect.End();

            }

            /// <summary>
            /// Get the height from the closest point on the height map
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            /// <returns>Height</returns>
            public float GetHeightClosest(float X, float Y)
            {
                int x = (int)Math.Round(X * scaleGInv, 0) + displace;
                int y = (int)Math.Round(Y * scaleGInv, 0) + displace;
                if (x < 0 || x > spanArray || y < 0 || y > spanArray)
                    return float.NegativeInfinity;
                return heights[x, y];
            }

            /// <summary>
            /// Gets the visable hight from the current update
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            /// <returns></returns>
            public float GetHieghtReal(float X, float Y)
            {
                float x = X * scaleGInv + displace;
                float y = Y * scaleGInv + displace;
                if (x < 0 || x > spanArray || y < 0 || y > spanArray)
                    return float.NegativeInfinity;

                int[] ind = Root.GetTriangle(x, y, span);

                if (ind.Length != 0)
                {

                    //indexBuffer2.SetData(ind, 0, 0);

                    Plane Plane1 = Plane.FromPoints(
                        Verticies[ind[0]].Position,
                        Verticies[ind[1]].Position,
                        Verticies[ind[2]].Position);

                    // Vector3 u;//x, uy, uz;
                    // Vector3 v;//x, vy, vz;

                    // u = Verticies[ind[1]].Position - Verticies[ind[0]].Position;    
                    // //uy = Verticies[ind[1]].Position.Y - Verticies[ind[0]].Position.Y;    
                    // //uz = Verticies[ind[1]].Position.Z - Verticies[ind[0]].Position.Z;

                    // v = Verticies[ind[2]].Position - Verticies[ind[0]].Position;    
                    // //vy = Verticies[ind[2]].Position.Y - Verticies[ind[0]].Position.Y;    
                    // //vz = Verticies[ind[2]].Position.Z - Verticies[ind[0]].Position.Z;


                    // float Nx = u.Y * v.Z - u.Z * v.Y;
                    // float Ny = u.Z * v.X - u.X * v.Z;
                    // float Nz = u.X * v.Y - u.Y * v.X;
                    // float d = -(Nx * Verticies[ind[0]].Position.X + 
                    //             Ny * Verticies[ind[0]].Position.Y + 
                    //             Nz * Verticies[ind[0]].Position.Z);

                    //return  -( Nx * X +  Nz * Y + d ) / Ny;


                    return -(Plane1.A * X + Plane1.C * Y + Plane1.D) / Plane1.B;
                }
                else
                {
                    return GetHieghtAccurate(X, Y);
                }

            }

            /// <summary>
            /// Gets the height from the height map
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            /// <returns></returns>
            public float GetHieghtAccurate(float X, float Y)
            {
                float x = X * scaleGInv + displace;
                float y = Y * scaleGInv + displace;
                if (x < 0 || x > spanArray || y < 0 || y > spanArray)
                    return float.NegativeInfinity;

                float minX = (float)System.Math.Truncate(x);
                float minY = (float)System.Math.Truncate(y);

                int minXInt = (int)minX;
                int minYInt = (int)minY;

                float decX = (x - minX);
                float decY = (y - minY);



                //topLeft: Verticies[z * span + x ]
                //topRight: Verticies[z * span + x  + 1 ]
                //bottomLeft: Verticies[(z + 1) * span + x ]
                //bottomRight: Verticies[(z + 1) * span + x + 1 ]

                //FIX //Remove
                //Vector3 v0 = new Vector3(minX * groundScale,
                //    heights[minXInt, minYInt],
                //    minY * groundScale);
                //Vector3 v1 = new Vector3((minX + 1) * groundScale,
                //    heights[minXInt + 1, minYInt],
                //    minY * groundScale);
                //Vector3 v2 = new Vector3(minX * groundScale,
                //    heights[minXInt, minYInt + 1],
                //    (minY + 1) * groundScale);
                //Vector3 v3 = new Vector3((minX + 1) * groundScale,
                //    heights[minXInt + 1, minYInt + 1],
                //    (minY + 1) * groundScale);

                Vector3 interpA = Vector3.Lerp(
                    Verticies[(int)(minY * span + minX)].Position,            //topLeft
                    Verticies[(int)(minY * span + minX + 1)].Position,        //topRight
                    decX);

                Vector3 interpB = Vector3.Lerp(
                    Verticies[(int)((minY + 1) * span + minX)].Position,      //bottomLeft
                    Verticies[(int)((minY + 1) * span + minX + 1)].Position,  //bottomRight
                    decX);

                Vector3 interpF = Vector3.Lerp(interpA, interpB, decY);

                return interpF.Y;
            }

            /// <summary>
            /// Grid Ray Test
            /// </summary>
            /// <param name="ray"></param>
            /// <param name="location"></param>
            /// <returns></returns>
            public bool RayIntercect(Physics.Ray ray, out Vector3 location, out float distance)
            {
                // A ray is a line and the heights form a grid so
                // we can test for intersection along a calculated path.

                // Start at line starting point.
                //ray.Origin;
                float x = ray.Origin.X * scaleGInv + displace;
                float y = ray.Origin.Y * scaleGInv + displace;

                // Record current cell (X,Y)
                int currentX, currentY;
                currentX = (int)x;
                currentY = (int)y;

                // Pre compute (int)ed end point
                int endX, endY;
                endX = Math.Abs((int)(ray.Origin.X * scaleGInv + displace));
                endY = Math.Abs((int)(ray.Origin.Y * scaleGInv + displace));

                // Store distance additions
                int additionX, additionY;
                float stepX, stepY;

                // Itit calculations
                // x, y is current point
                // find the distance for next x and next y.

                stepX = x - (float)Math.Truncate(x);
                stepY = y - (float)Math.Truncate(y);

                additionX = Math.Sign(stepX);
                additionY = Math.Sign(stepY);

                stepX = 1 - Math.Abs(stepX);
                stepY = 1 - Math.Abs(stepY);

                // Loop: While current cell <= (int)ed End point
                while (Math.Abs(currentX) < endX && Math.Abs(currentY) < endY)
                {

                    // If Rise < Len (we will rise first)
                    if (stepY < stepX)
                    // Then:
                    {
                        // Cell Y += 1 ;; additionY
                        currentY += additionY;
                        // Calculate distance to next rise
                        stepY = 1.0f - (float)Math.Abs(y - Math.Truncate(y));
                        y += (additionY * stepY);
                    }
                    // Else:
                    else
                    {
                        // Cell X += 1 ;; additionX
                        currentX += additionX;
                        // Calculate distance to next lenght
                        stepX = 1.0f - (float)Math.Abs(x - Math.Truncate(x));
                        x += (additionX * stepX);
                    }

                    // Test For the current cell
                    if (x < 0 || x >= spanArray || y < 0 || y >= spanArray)
                        continue;

                    // Process Collision
                    if (RayTestTile(currentX, currentY, ray, out location, out distance))
                        return true;


                    // Next Loop..
                }
                distance = 0;
                location = Vector3.Empty;
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
            private bool RayTestTile(int X, int Y, Physics.Ray ray, out Vector3 point, out float distance)
            {

                //topLeft: Verticies[z * span + x ]
                //topRight: Verticies[z * span + x  + 1 ]
                //bottomLeft: Verticies[(z + 1) * span + x ]
                //bottomRight: Verticies[(z + 1) * span + x + 1 ]

                int topRight = Y * span + X + 1;
                int bottomLeft = (Y + 1) * span + X;

                //Left
                Physics.Triangle Left = new EngineX.Physics.Triangle(
                    Verticies[Y * span + X].Position,            //topLeft
                    Verticies[topRight].Position,                //topRight
                    Verticies[bottomLeft].Position);             //bottomLeft

                //Right
                Physics.Triangle Right = new EngineX.Physics.Triangle(
                    Verticies[topRight].Position,                //topRight
                    Verticies[bottomLeft].Position,              //bottomLeft
                    Verticies[(Y + 1) * span + X + 1].Position); //bottomRight

                Vector3[] pointResult = new Vector3[2];
                
                bool testA, testB;

                testA = Left.RayIntersect(ray, out pointResult[0]);
                testB = Right.RayIntersect(ray, out pointResult[1]);

                // Need one collision, get closest collision location
                if (testA && testB)
                {
                    distance = MathX.Math3D.DistanceBetweenPoints(ray.Origin, pointResult[0]);
                    float distB = MathX.Math3D.DistanceBetweenPoints(ray.Origin, pointResult[1]);

                    if (distance < distB)
                    {
                        point = pointResult[0];
                    }
                    else
                    {
                        distance = distB;
                        point = pointResult[1];
                    }

                    return true;
                }
                else if (testA)
                {
                    distance = MathX.Math3D.DistanceBetweenPoints(ray.Origin, pointResult[0]);
                    point = pointResult[0];
                    return true;
                }
                else if (testB)
                {
                    distance = MathX.Math3D.DistanceBetweenPoints(ray.Origin, pointResult[1]);
                    point = pointResult[1];
                    return true;
                }
                else
                {
                    distance = 0;
                    point = Vector3.Empty;
                    return false;
                }
            }

            /// <summary>
            /// Gets the triangle from the given position
            /// </summary>
            /// <returns></returns>
            public Physics.Triangle GetTriangle(float X, float Y)
            {
                
                float x = X * scaleGInv + displace;
                float y = Y * scaleGInv + displace;
                if (x < 0 || x > spanArray || y < 0 || y > spanArray)
                    return null;

                int[] ind = Root.GetTriangle(x, y, span);

                if (ind.Length != 0)
                {
                    return new EngineX.Physics.Triangle(
                    Verticies[ind[0]].Position,
                    Verticies[ind[1]].Position,
                    Verticies[ind[2]].Position);  
                }

                return null;

            }

            // TODO: RAY TEST TWO

            /// <summary>
            /// Custom Vertex
            /// </summary>
            public struct Vertex
            {
                public Vector3 Position;
                public Vector3 Normal;
                float tu1, tv1;
                float tu2, tv2;
                float b1, b2;
                float b3, b4;

                public Vertex(Vector3 pos, Vector3 nor,
                    float u1, float v1,
                    float u2, float v2,
                    float BlendLow, float BlendGround,
                    float BlendHigh, float BlendDetail, bool Normalize)
                {
                    Position = pos;
                    Normal = nor;
                    tu1 = u1;
                    tv1 = v1;
                    tu2 = u2;
                    tv2 = v2;
                    b1 = BlendLow;
                    b2 = BlendGround;
                    b3 = BlendHigh;
                    b4 = BlendDetail;


                    if (Normalize)
                    {
                        float total = b1 + b2 + b3 + b4;

                        b1 /= total;
                        b2 /= total;
                        b3 /= total;
                        b4 /= total;
                    }
                }

                public const VertexFormats Format =
                   VertexFormats.Position | VertexFormats.Normal |
                   VertexFormats.Texture0 | VertexFormats.Texture1 |
                   VertexFormats.Texture2 | VertexFormats.Texture3 | VertexFormats.Texture4;

                //  | VertexFormats.Texture3

                public override string ToString()
                {
                    return Position.ToString();
                }

            }

            private class TriangleNode
            {


                private TriangleNode leftChild; // Our Left child
                private TriangleNode rightChild; // Our Right child 
                private TriangleNode baseNeighbor; // Adjacent node, below us     
                private TriangleNode leftNeighbor; // Adjacent node, to our left     
                private TriangleNode rightNeighbor; // Adjacent node, to our right 
                private TriangleNode parent; // Parent node

                //private TriangleNode linkNode;

                private Point tipPoint;
                private Point leftPoint;
                private Point rightPoint;

                private Physics.BoundingVolumes.BoundingSphere sphere;

                private int levels;
                private int level;

                private bool split;
                private float variance;

                private bool set;

                int levelNumber;

                public TriangleNode(int size, ref float[,] heights, float mapScale)
                {
                    //size = 8;

                    // Normal Node
                    this.split = false;

                    int arrayLen = size - 1;

                    levels = (int)Math.Log(arrayLen, 2) * 2;
                    level = 0;

                    this.tipPoint = new Point(0, 0);
                    this.leftPoint = new Point(0, arrayLen);
                    this.rightPoint = new Point(arrayLen, 0);

                    this.baseNeighbor = new TriangleNode(
                        new Point(arrayLen, arrayLen), new Point(arrayLen, 0), new Point(0, arrayLen), levels, level);

                    this.baseNeighbor.baseNeighbor = this;

                    this.leftNeighbor = null;
                    this.rightNeighbor = null;
                    this.parent = null;

                    Generate(ref heights, mapScale);
                    baseNeighbor.Generate(ref heights, mapScale);

                    Link();
                    baseNeighbor.Link();

                    //LinkPath();
                    //baseNeighbor.LinkPath();

                    SumVariance();
                    baseNeighbor.SumVariance();
                }

                private TriangleNode(Point tipPoint, Point leftPoint, Point rightPoint, int levels, int level)
                {
                    // Normal Node
                    this.split = false;

                    this.levels = levels;
                    this.level = level;

                    this.tipPoint = tipPoint;
                    this.leftPoint = leftPoint;
                    this.rightPoint = rightPoint;
                }

                private void Generate(ref float[,] heights, float mapScale)
                {

                    // Cretes and initilises all children

                    int currentLevel = 0;
                    bool inRight = false;
                    int nodeNumber = 0;
                    int totalNodes = (int)(Math.Pow(2, levels) + Math.Pow(2, levels) - 1);

                    int[] levelNumbers = new int[levels + 1];

                    TriangleNode currentNode = this;

                    while (true)
                    {

                        levelNumbers[currentLevel]++;
                        currentNode.levelNumber = levelNumbers[currentLevel];

                        nodeNumber++;

                        currentNode.set = true;

                        // Child tip is centre of leftPoint and rightPoint
                        Point childTip = new Point(
                            GetMiddle(currentNode.leftPoint.X, currentNode.rightPoint.X),
                            GetMiddle(currentNode.leftPoint.Y, currentNode.rightPoint.Y));

                        // Terrain Real Height
                        //heights[childTip.X, childTip.Y];

                        Vector3 left = new Vector3(
                            currentNode.leftPoint.X,
                            heights[currentNode.leftPoint.X, currentNode.leftPoint.Y],
                            currentNode.leftPoint.Y);

                        Vector3 right = new Vector3(
                            currentNode.rightPoint.X,
                            heights[currentNode.rightPoint.X, currentNode.rightPoint.Y],
                            currentNode.rightPoint.Y);

                        // Height Variance
                        float tipHeight = heights[childTip.X, childTip.Y];
                        currentNode.variance = Math.Abs(tipHeight -
                            Vector3.Lerp(left, right, 0.5f).Y);

                        // Compute Sphere
                        CustomVertex.PositionOnly[] points = new CustomVertex.PositionOnly[3];
                        points[0].Position = new Vector3(currentNode.leftPoint.X * mapScale,
                            left.Y, currentNode.leftPoint.Y * mapScale);

                        points[1].Position = new Vector3(currentNode.rightPoint.X * mapScale,
                            right.Y, currentNode.rightPoint.Y * mapScale);

                        points[2].Position = new Vector3(tipPoint.X * mapScale,
                            tipHeight, tipPoint.Y * mapScale);

                        currentNode.sphere = new EngineX.Physics.BoundingVolumes.BoundingSphere(points);


                        // End Here
                        if (totalNodes == nodeNumber) break;

                        // Create Left Nodes, if can but.. when can't
                        if (levels != currentLevel)
                        {

                            // Split Self
                            currentNode.leftChild = new TriangleNode(childTip, currentNode.tipPoint,
                                currentNode.leftPoint, levels, currentLevel + 1);
                            currentNode.leftChild.parent = currentNode;

                            currentNode.rightChild = new TriangleNode(childTip, currentNode.rightPoint,
                                currentNode.tipPoint, levels, currentLevel + 1);
                            currentNode.rightChild.parent = currentNode;

                            // Update
                            currentNode = currentNode.leftChild;
                            currentLevel++;
                            inRight = false;
                        }
                        else
                        //Backup and Create Right Node, (step 1.)
                        {

                            //currentNode.isLeaf = true;
                            if (inRight)
                            {
                                // back up untill you find one not set
                                currentNode = currentNode.parent.parent;
                                while (true)
                                {
                                    if (currentNode.rightChild.set)
                                    {
                                        currentNode = currentNode.parent;
                                    }
                                    else
                                    {
                                        currentNode = currentNode.rightChild;
                                        break;
                                    }
                                }
                                currentLevel = currentNode.level;
                                //inRight = false;
                            }
                            else
                            { currentNode = currentNode.parent.rightChild; }
                            inRight = true;
                        }

                    }

                }

                private void Link()
                {

                    // Loop down to lowest level
                    if (level < levels)
                    {

                        // Fill in the information we can get from the parent (neighbor pointers)
                        leftChild.baseNeighbor = leftNeighbor;
                        leftChild.leftNeighbor = rightChild.rightChild;

                        rightChild.baseNeighbor = rightNeighbor;
                        rightChild.rightNeighbor = leftChild.leftChild;


                        // Link our Left Neighbor to the new children
                        if (this.leftNeighbor != null)
                        {
                            this.leftNeighbor.baseNeighbor = this.leftChild;
                        }

                        // Link our Right Neighbor to the new children
                        if (this.rightNeighbor != null)
                        {
                            this.rightNeighbor.baseNeighbor = this.rightChild;
                        }

                        // Link our Base Neighbor to the new children
                        if (this.baseNeighbor != null)
                        {
                            this.baseNeighbor.leftChild.rightNeighbor = this.rightChild; // Tick
                            this.baseNeighbor.rightChild.leftNeighbor = this.leftChild;  // Tick
                            this.leftChild.rightNeighbor = this.baseNeighbor.rightChild.leftChild; // Added depth one
                            this.rightChild.leftNeighbor = this.baseNeighbor.leftChild.rightChild; // Added depth one
                        }

                        if (levels != level)
                        {
                            leftChild.Link();
                            rightChild.Link();
                        }
                    }

                }

                private float SumVariance()
                {
                    if (this.level < levels)
                    {
                        this.variance = this.leftChild.SumVariance() + this.rightChild.SumVariance();
                    }
                    return this.variance;
                }

                //private void LinkPath()
                //{

                //    TriangleNode currentNode = this;
                //    bool left = true;
                //    int currentLevel = 0;

                //    while (true)
                //    {

                //        // EXIT IF THIS IS THE LAST LEVEL

                //        // CASE 1
                //        // If at the end of the level
                //        // Go to the start of the next level
                //        if (currentNode.levelNumber == (int)Math.Pow(2, currentNode.level))
                //        {

                //            if (currentLevel == levels)
                //                break;

                //            currentLevel++;
                //            // Go from (this) down to current level

                //            TriangleNode Node = this;
                //            for (int i = 0; i < currentLevel; i++)
                //            {
                //                Node = Node.leftChild;
                //            }
                //            currentNode.linkNode = Node;
                //            currentNode = Node;

                //            left = true;

                //            //Console.WriteLine("Case 1 - ||||||||||||||||||||||||");
                //        }

                //        // CASE 2
                //        // If a left child go to right child
                //        // If a right child go to next left child

                //        else if (left)
                //        {
                //            currentNode.linkNode = currentNode.parent.rightChild;
                //            currentNode = currentNode.parent.rightChild;

                //            left = false;
                //        }
                //        else
                //        {

                //            // back up untill you find one not set

                //            int jump = 2;
                //            TriangleNode Node;

                //            while (true)
                //            {

                //                // Go up
                //                Node = currentNode.parent.parent;
                //                for (int i = 2; i < jump; i++)
                //                {
                //                    Node = Node.parent;
                //                }

                //                // Go down
                //                Node = Node.rightChild.leftChild;
                //                for (int i = 2; i < jump; i++)
                //                {
                //                    Node = Node.leftChild;
                //                }

                //                if (Node.linkNode == null && Node.levelNumber > currentNode.levelNumber)
                //                {
                //                    currentNode.linkNode = Node;
                //                    currentNode = Node;
                //                    break; 
                //                }
                //                else
                //                {
                //                    jump++;
                //                }

                //            }
                //            left = true;
                //        }

                //    }

                //}

                private int GetMiddle(int a, int b)
                {
                    int min = Math.Min(a, b);
                    return ((Math.Max(a, b) - min) >> 1) + min;
                }

                /// <summary>
                /// Detail Update
                /// </summary>
                /// <param name="viewPosition"></param>
                /// <param name="accuracy"></param>
                public void Update(Vector3 viewPosition, float accuracy)
                {

                    Adapt(viewPosition, accuracy);
                    baseNeighbor.Adapt(viewPosition, accuracy);

                }

                private void Adapt(Vector3 viewPosition, float accuracy)
                {

                    // if we were smart and had the data
                    // we could set the initial capacity to our max depth
                    Stack<TriangleNode> stack = new Stack<TriangleNode>();

                    // Starting Node
                    for (TriangleNode currentNode = this;
                        // Terminating Condition
                        currentNode != null || stack.Count > 0;
                        // Step forward to the right
                        currentNode = currentNode.rightChild)
                    {


                        // While not at base of tree
                        while (currentNode != null &&

                            // If Need more detail
                            (currentNode.variance /
                                MathX.Math3D.DistanceBetweenPoints(currentNode.sphere.Centre, viewPosition) > accuracy))

                        // Split current node if (a: too close, b: variance high)
                        {
                            // Nodes on the stack need to be split

                            // Store the current Node
                            stack.Push(currentNode);

                            // Move to the left
                            currentNode = currentNode.leftChild;
                        }

                        // Not needed
                        //currentNode.split = false;


                        // Get the next Item to work on
                        if (stack.Count == 0)
                            continue;

                        currentNode = stack.Pop();


                        // Work on the current Node
                        /////////////

                        // If already split; skip
                        if (!currentNode.split)
                        {

                            currentNode.split = true;

                            // Ensure the base is split if there is one
                            if (currentNode.baseNeighbor != null &&
                                currentNode.baseNeighbor.split == false)
                            {
                                stack.Push(currentNode.baseNeighbor);

                                // Ensure that there is a base to be split
                                if (currentNode.baseNeighbor.parent != null &&
                                    currentNode.baseNeighbor.parent.split == false)
                                {
                                    stack.Push(currentNode.baseNeighbor.parent);
                                }

                            }
                        }
                    }
                }

                public int[] GetTriangle(float X, float Y, int mapWidth)
                {
                    int[] result;
                    result = GetTriangleMethod(X, Y, mapWidth);
                    if (result == null)
                    {
                        result = baseNeighbor.GetTriangleMethod(X, Y, mapWidth);
                    }
                    return result;

                }

                private int[] GetTriangleMethod(float X, float Y, int mapWidth)
                {

                    // if we were smart and had the data
                    // we could set the initial capacity to our max depth
                    //Stack<TriangleNode> stack = new Stack<TriangleNode>();

                    if (!MathX.Math2D.IndideTriangle(
                        //Triangle
                            new Vector2(tipPoint.X, tipPoint.Y),
                            new Vector2(rightPoint.X, rightPoint.Y),
                            new Vector2(leftPoint.X, leftPoint.Y),
                        //Point
                            new Vector2(X, Y)))
                        return null; // If not inside return.

                    // Starting Node
                    TriangleNode currentNode = this;

                    // While not at base of tree
                    while (currentNode != null)
                    {

                        // Work on the current Node
                        /////////////

                        // If not split or is last level
                        if (!currentNode.split || currentNode.level == levels)
                        {

                            if (MathX.Math2D.IndideTriangle(
                                new Vector2(currentNode.tipPoint.X, currentNode.tipPoint.Y),
                                new Vector2(currentNode.rightPoint.X, currentNode.rightPoint.Y),
                                new Vector2(currentNode.leftPoint.X, currentNode.leftPoint.Y),
                                new Vector2(X, Y)))
                            {

                                // Return Triangle
                                int[] indicies = new int[3];

                                indicies[0] = currentNode.tipPoint.X + (mapWidth * currentNode.tipPoint.Y);
                                indicies[1] = currentNode.leftPoint.X + (mapWidth * currentNode.leftPoint.Y);
                                indicies[2] = currentNode.rightPoint.X + (mapWidth * currentNode.rightPoint.Y);

                                return indicies;
                            }
                            {
                                return new int[0];
                            }
                        }
                        else // Else
                        {
                            // Narrow Search
                            if (MathX.Math2D.IndideTriangle(
                                new Vector2(currentNode.leftChild.tipPoint.X, currentNode.leftChild.tipPoint.Y),
                                new Vector2(currentNode.leftChild.rightPoint.X, currentNode.leftChild.rightPoint.Y),
                                new Vector2(currentNode.leftChild.leftPoint.X, currentNode.leftChild.leftPoint.Y),
                                new Vector2(X, Y)))
                            {
                                // Point inside Left
                                currentNode = currentNode.leftChild;
                            }
                            else
                            {
                                // Point indise Right
                                currentNode = currentNode.rightChild;
                            }
                            // Point not inside
                            // Not possible

                        }

                    }

                    return null;
                }


                /// <summary>
                /// Constructs a list of triangle points
                /// </summary>
                /// <param name="indicies"></param>
                /// <param name="mapWidth"></param>
                public void GetIndicies(ref List<int> indicies, int mapWidth)
                {

                    RecursiveGetIndicies(ref indicies, mapWidth);

                    if (baseNeighbor != null)
                    {
                        baseNeighbor.RecursiveGetIndicies(ref indicies, mapWidth);
                    }
                }

                private void RecursiveGetIndicies(ref List<int> indicies, int mapWidth)
                {

                    // Grid -> Array
                    if (!split || level == levels)
                    {
                        indicies.Add(tipPoint.X + (mapWidth * tipPoint.Y));
                        indicies.Add(leftPoint.X + (mapWidth * leftPoint.Y));
                        indicies.Add(rightPoint.X + (mapWidth * rightPoint.Y));
                    }
                    else
                    {
                        leftChild.RecursiveGetIndicies(ref indicies, mapWidth);
                        rightChild.RecursiveGetIndicies(ref indicies, mapWidth);
                    }

                    // RESET SPLITTING
                    split = false;

                }

            }

        }
    }
}
