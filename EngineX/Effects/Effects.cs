using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using System.Windows.Forms;
using System.Drawing;

namespace EngineX
{
    namespace Effects
    {

        /// <summary>
        /// Beam Billboard
        /// </summary>
        public class BeamBillboard
        {
            /// <summary>
            /// Rendering Device
            /// </summary>
            private Device Device1;
            /// <summary>
            /// Rendering Texture
            /// </summary>
            private Texture Texture1;
            /// <summary>
            /// Rendering colour
            /// </summary>
            private Color Color1;
            /// <summary>
            /// Rendering Vertex Buffer
            /// </summary>
            private VertexBuffer Buffer1;

            /// <summary>
            /// Initilize Billboard
            /// </summary>
            /// <param name="device"></param>
            /// <param name="texture"></param>
            /// <param name="color"></param>
            public BeamBillboard(Device device, String texture, Color color)
            {
                Device1 = device;
                Texture1 = TextureLoader.FromFile(Device1, texture);
                Color1 = color;

                Buffer1 = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured), 4, Device1, Usage.Dynamic, CustomVertex.PositionColoredTextured.Format, Pool.SystemMemory);
            }

            /// <summary>
            /// Update Billboard
            /// </summary>
            /// <param name="origin"></param>
            /// <param name="dest"></param>
            /// <param name="camera_pos"></param>
            /// <param name="radius"></param>
            public void Update(Vector3 origin, Vector3 dest, Vector3 camera_pos, float radius)
            {
                //Variables
                Vector3 vector1;
                Vector3 vector2;
                Vector3 normal;

                //float magnitude;

                Vector3[] corners;
                corners = new Vector3[3];


                // vector1 is the vector from origin to the camera
                vector1 = camera_pos - origin;

                // vector2 is the vector from the origin to the destination
                vector2 = dest - origin;

                // get the normal or cross product between vector1 and vector 2
                // we will use this vector to create a rectangle which runs along vector 2
                // and as facing a direction that give maximum surface area to the camera.

                normal = Vector3.Cross(vector1, vector2);

                //normal.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
                //normal.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
                //normal.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;

                normal.Normalize();

                // now create the corners for the rectangle

                CustomVertex.PositionColoredTextured[] Vertex;
                Vertex = (CustomVertex.PositionColoredTextured[])Buffer1.Lock(0, LockFlags.Discard);

                // first corner
                Vertex[0].Tu = 0;
                Vertex[0].Tv = 0;
                Vertex[0].Color = Color1.ToArgb();
                Vertex[0].X = origin.X - (radius * normal.X);
                Vertex[0].Y = origin.Y - (radius * normal.Y);
                Vertex[0].Z = origin.Z - (radius * normal.Z);

                // second corner
                Vertex[1].Tu = 1;
                Vertex[1].Tv = 0;
                Vertex[1].Color = Color1.ToArgb();
                Vertex[1].X = origin.X + (radius * normal.X);
                Vertex[1].Y = origin.Y + (radius * normal.Y);
                Vertex[1].Z = origin.Z + (radius * normal.Z);

                // third corner
                Vertex[2].Tu = 0;
                Vertex[2].Tv = 1;
                Vertex[2].Color = Color1.ToArgb();
                Vertex[2].X = dest.X + (radius * normal.X);
                Vertex[2].Y = dest.Y + (radius * normal.Y);
                Vertex[2].Z = dest.Z + (radius * normal.Z);

                // forth corner
                Vertex[3].Tu = 1;
                Vertex[3].Tv = 1;
                Vertex[3].Color = Color1.ToArgb();
                Vertex[3].X = dest.X - (radius * normal.X);
                Vertex[3].Y = dest.Y - (radius * normal.Y);
                Vertex[3].Z = dest.Z - (radius * normal.Z);

                Buffer1.Unlock();

            }

            /// <summary>
            /// Render Billboard
            /// </summary>
            public void Render()
            {

                // Set the Matricies
                Device1.Transform.World = Matrix.Identity;

                // Set the vertex format
                Device1.VertexFormat = CustomVertex.PositionColoredTextured.Format;

                // The Vertices
                Device1.SetStreamSource(0, Buffer1, 0);

                // Set the Texture
                Device1.SetTexture(0, Texture1);

                // Rendering
                Device1.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            }

        }

        /// <summary>
        /// Water
        /// </summary>
        public class Water
        {
            /// <summary>
            /// Rendering Effect
            /// </summary>
            public Effect Shader;

            /// <summary>
            /// Transforms Manager
            /// </summary>
            public TransformsManager TransformsManager;

            /// <summary>
            /// Rendering Device
            /// </summary>
            private Device device;
            /// <summary>
            /// Reflection Texture
            /// </summary>
            private Texture ReflectionTex;
            /// <summary>
            /// Refraction Texure
            /// </summary>
            private Texture RefractionTex;
            /// <summary>
            /// Image Texture
            /// </summary>
            private Texture ImageTex;
            /// <summary>
            /// Render to surface
            /// </summary>
            private RenderToSurface RTT;
            /// <summary>
            /// Water hight
            /// </summary>
            private float waterheight = 1;
            /// <summary>
            /// Noise Volume Texture
            /// </summary>
            public VolumeTexture NoiseTex;
            /// <summary>
            /// Texture Projection Matrix
            /// </summary>
            private Matrix TexProj;
            /// <summary>
            /// Animation Tick
            /// </summary>
            private float Tick = 0.0f;
            /// <summary>
            /// Resource file location
            /// </summary>
            private string files;
            /// <summary>
            /// Water Verticies
            /// </summary>
            CustomVertex.PositionColoredTextured[] verticies;
            /// <summary>
            /// Water plane
            /// </summary>
            Plane waterplane;


            /// <summary>
            /// Initilize Water effect
            /// </summary>
            /// <param name="Files"></param>
            /// <param name="WaterHeight"></param>
            /// <param name="device"></param>
            /// <param name="transformsManager"></param>
            public Water(string Files, Device device, ref TransformsManager transformsManager, Vector3 Centre, Vector2 Size)
            {
                this.device = device;
                this.TransformsManager = transformsManager;
                this.waterheight = Centre.Y;

                // Load effects and images
                files = Files;
                Shader = Effect.FromFile(device, Files + "\\WaterShader.fx", null, null, ShaderFlags.None, null);
                ImageTex = TextureLoader.FromFile(device, Files + "\\Textures\\Water.png");
                NoiseTex = TextureLoader.FromVolumeFile(device, Files + "\\Textures\\NoiseVolume.dds", 0, 0, 0, 0, Usage.None, Format.Unknown, Pool.Default, Filter.Linear, Filter.Linear, 0);

                // Setup Textures
                Vector2 RTSize = new Vector2(512, 512);
                RTT = new RenderToSurface(device, (int)RTSize.X, (int)RTSize.Y, Format.X8R8G8B8, true, DepthFormat.D24S8);
                ReflectionTex = new Texture(device, (int)RTSize.X, (int)RTSize.Y, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
                RefractionTex = new Texture(device, (int)RTSize.X, (int)RTSize.Y, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

                // Setup Texture Projection
                TexProj.M11 = 0.5f;
                TexProj.M22 = -0.5f;
                TexProj.M33 = 0.5f;

                TexProj.M41 = 0.5f;
                TexProj.M42 = 0.5f;
                TexProj.M43 = 0.5f;
                TexProj.M44 = 1.0f;

                // Construct the rendering quad
                verticies = new CustomVertex.PositionColoredTextured[4];
                verticies[0] = new CustomVertex.PositionColoredTextured(Centre.X - Size.X, waterheight, Centre.Z - Size.Y, Color.White.ToArgb(), 0, 0);
                verticies[1] = new CustomVertex.PositionColoredTextured(Centre.X + Size.X, waterheight, Centre.Z - Size.Y, Color.White.ToArgb(), 1, 0);
                verticies[2] = new CustomVertex.PositionColoredTextured(Centre.X + Size.X, waterheight, Centre.Z + Size.Y, Color.White.ToArgb(), 1, 1);
                verticies[3] = new CustomVertex.PositionColoredTextured(Centre.X - Size.X, waterheight, Centre.Z + Size.Y, Color.White.ToArgb(), 0, 1);

                // Construct the water plane
                waterplane = Plane.FromPoints(verticies[0].Position, verticies[1].Position, verticies[2].Position);
                waterplane.D += 1;
                waterplane.Normalize();

            }

            /// <summary>
            /// Render Refraction
            /// </summary>
            public void RenderRefraction()
            {
                // Render
                device.EndScene();

                //Matrix tempProj = TransformsManager.Projection;

                RTT.BeginScene(RefractionTex.GetSurfaceLevel(0));
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Blue, 1, 0);

            }

            /// <summary>
            /// Render Reflection
            /// </summary>
            public void RenderReflection()
            {

                TransformsManager.World = Matrix.Identity;
                TransformsManager.SetWorld();
                TransformsManager.SetView();

                RTT.EndScene(Filter.None);

                // RenderReflectionMap //

                //Reflect
                Matrix matReflect = Matrix.Identity;
                matReflect.Reflect(waterplane);
                device.Transform.World = matReflect;

                RTT.BeginScene(ReflectionTex.GetSurfaceLevel(0));
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Blue, 1, 0);

                //Clip
                device.ClipPlanes[0].Enabled = true;
                device.ClipPlanes[0].Plane = waterplane;
                device.RenderState.Clipping = true;
                
                device.RenderState.CullMode = Cull.Clockwise;

            }

            /// <summary>
            /// Render Water
            /// </summary>
            /// <param name="ElapsedTime"></param>
            /// <param name="CamPos"></param>
            public void RenderFinalise(float elapsedTime, Vector3 cameraPosition)
            {

                Tick += elapsedTime;

                device.RenderState.CullMode = Cull.CounterClockwise;

                device.RenderState.Clipping = false;
                device.ClipPlanes[0].Enabled = false;

                TransformsManager.World = Matrix.Identity;
                TransformsManager.SetWorld();

                RTT.EndScene(Filter.None);

                ////////////////////////////////////////////////////////

                device.BeginScene();

                device.VertexFormat = CustomVertex.PositionColoredTextured.Format;

                Shader.Technique = "RenderWater";

                Shader.SetValue(EffectHandle.FromString("viewProjection"), 
                    Matrix.Multiply(TransformsManager.View, TransformsManager.Projection ));

                Shader.SetValue(EffectHandle.FromString("elapsedTime"), Tick);

                Shader.SetValue(EffectHandle.FromString("textureProjection"), 
                    Matrix.Multiply(TransformsManager.View, TransformsManager.Projection) * TexProj);

                Shader.SetValue(EffectHandle.FromString("cameraPosition"), 
                    new Vector4(cameraPosition.X, cameraPosition.Y, cameraPosition.Z, 1));

                Shader.SetValue(EffectHandle.FromString("sunDirection"), 
                    Vector4.Normalize(new Vector4(0.5f, 1, 0, 1)));

                Shader.SetValue(EffectHandle.FromString("voltex"), NoiseTex);

                Shader.SetValue(EffectHandle.FromString("fresnelbias"), 
                    System.Convert.ToSingle(0.15)); //  15 / 1

                Shader.SetValue(EffectHandle.FromString("fresnelpow"), 
                    System.Convert.ToSingle(4)); //  400 / 1

                Shader.Begin(FX.None);
                Shader.BeginPass(0);
                device.SetTexture(0, ImageTex);

                if (cameraPosition.Y < waterheight)
                {
                    device.SetTexture(2, ReflectionTex); // 2
                    device.SetTexture(1, RefractionTex); // 1
                }
                else
                {
                    device.SetTexture(1, ReflectionTex); // 1 Reflect
                    device.SetTexture(2, RefractionTex); // 2 Refraction
                }

                device.RenderState.CullMode = Cull.None;
                device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, verticies);
                device.RenderState.CullMode = Cull.CounterClockwise;

                Shader.EndPass();
                Shader.End();

            }

            /// <summary>
            /// Release objects
            /// </summary>
            public void Dispose()
            {
                RTT.Dispose();
                RTT = null;

                ReflectionTex.Dispose();
                ReflectionTex = null;

                RefractionTex.Dispose();
                RefractionTex = null;

                NoiseTex.Dispose();
                NoiseTex = null;
            }

        }
    }
}
