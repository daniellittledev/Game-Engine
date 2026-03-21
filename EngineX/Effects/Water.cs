using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using System.Windows.Forms;
using System.Drawing;

namespace EngineX.Effects
{
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
                Matrix.Multiply(TransformsManager.View, TransformsManager.Projection));

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
