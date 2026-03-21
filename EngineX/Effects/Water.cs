using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct3D9;

using System.Windows.Forms;
using System.Drawing;

namespace EngineX.Effects
{
    /// <summary>
    /// Water effect with reflection and refraction
    /// </summary>
    public class Water
    {
        /// <summary>Rendering Effect</summary>
        public Effect Shader;

        /// <summary>Transforms Manager</summary>
        public TransformsManager TransformsManager;

        private Device device;
        private Texture ReflectionTex;
        private Texture RefractionTex;
        private Texture ImageTex;
        private RenderToSurface RTT;
        private float waterheight = 1;
        public VolumeTexture NoiseTex;
        private Matrix TexProj;
        private float Tick = 0.0f;
        private string files;
        CustomVertex.PositionColoredTextured[] verticies;
        Plane waterplane;

        /// <summary>
        /// Initialize Water effect
        /// </summary>
        public Water(string Files, Device device, ref TransformsManager transformsManager, Vector3 Centre, Vector2 Size)
        {
            this.device = device;
            this.TransformsManager = transformsManager;
            this.waterheight = Centre.Y;

            files = Files;
            Shader = Effect.FromFile(device, Files + "\\WaterShader.fx", null, ShaderFlags.None, null);
            ImageTex = Texture.FromFile(device, Files + "\\Textures\\Water.png");
            NoiseTex = VolumeTexture.FromFile(device, Files + "\\Textures\\NoiseVolume.dds");

            Vector2 RTSize = new Vector2(512, 512);
            RTT = new RenderToSurface(device, (int)RTSize.X, (int)RTSize.Y, Format.X8R8G8B8, true, Format.D24S8);
            ReflectionTex = new Texture(device, (int)RTSize.X, (int)RTSize.Y, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
            RefractionTex = new Texture(device, (int)RTSize.X, (int)RTSize.Y, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            TexProj.M11 = 0.5f;
            TexProj.M22 = -0.5f;
            TexProj.M33 = 0.5f;
            TexProj.M41 = 0.5f;
            TexProj.M42 = 0.5f;
            TexProj.M43 = 0.5f;
            TexProj.M44 = 1.0f;

            int white = System.Drawing.Color.White.ToArgb();
            verticies = new CustomVertex.PositionColoredTextured[4];
            verticies[0] = new CustomVertex.PositionColoredTextured(Centre.X - Size.X, waterheight, Centre.Z - Size.Y, white, 0, 0);
            verticies[1] = new CustomVertex.PositionColoredTextured(Centre.X + Size.X, waterheight, Centre.Z - Size.Y, white, 1, 0);
            verticies[2] = new CustomVertex.PositionColoredTextured(Centre.X + Size.X, waterheight, Centre.Z + Size.Y, white, 1, 1);
            verticies[3] = new CustomVertex.PositionColoredTextured(Centre.X - Size.X, waterheight, Centre.Z + Size.Y, white, 0, 1);

            waterplane = Plane.FromPoints(
                new Vector3(verticies[0].X, verticies[0].Y, verticies[0].Z),
                new Vector3(verticies[1].X, verticies[1].Y, verticies[1].Z),
                new Vector3(verticies[2].X, verticies[2].Y, verticies[2].Z));
            waterplane.D += 1;
            waterplane = Plane.Normalize(waterplane);
        }

        /// <summary>Render Refraction</summary>
        public void RenderRefraction()
        {
            device.EndScene();
            RTT.BeginScene(RefractionTex.GetSurfaceLevel(0));
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new RawColorBGRA(0, 0, 255, 255), 1, 0);
        }

        /// <summary>Render Reflection</summary>
        public void RenderReflection()
        {
            TransformsManager.World = Matrix.Identity;
            TransformsManager.SetWorld();
            TransformsManager.SetView();

            RTT.EndScene(Filter.None);

            Matrix matReflect = Matrix.Reflection(waterplane);
            device.SetTransform(TransformState.World, matReflect);

            RTT.BeginScene(ReflectionTex.GetSurfaceLevel(0));
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new RawColorBGRA(0, 0, 255, 255), 1, 0);

            device.SetRenderState(RenderState.ClipPlaneEnable, 1);
            device.SetClipPlane(0, waterplane);
            device.SetRenderState(RenderState.Clipping, true);
            device.SetRenderState(RenderState.CullMode, Cull.Clockwise);
        }

        /// <summary>Render Water</summary>
        public void RenderFinalise(float elapsedTime, Vector3 cameraPosition)
        {
            Tick += elapsedTime;

            device.SetRenderState(RenderState.CullMode, Cull.CounterClockwise);
            device.SetRenderState(RenderState.Clipping, false);
            device.SetRenderState(RenderState.ClipPlaneEnable, 0);

            TransformsManager.World = Matrix.Identity;
            TransformsManager.SetWorld();

            RTT.EndScene(Filter.None);

            device.BeginScene();

            device.VertexFormat = CustomVertex.PositionColoredTextured.Format;

            Shader.Technique = new EffectHandle("RenderWater");

            Shader.SetValue("viewProjection",
                Matrix.Multiply(TransformsManager.View, TransformsManager.Projection));

            Shader.SetValue("elapsedTime", Tick);

            Shader.SetValue("textureProjection",
                Matrix.Multiply(TransformsManager.View, TransformsManager.Projection) * TexProj);

            Shader.SetValue("cameraPosition",
                new Vector4(cameraPosition.X, cameraPosition.Y, cameraPosition.Z, 1));

            Shader.SetValue("sunDirection",
                Vector4.Normalize(new Vector4(0.5f, 1, 0, 1)));

            Shader.SetValue("voltex", NoiseTex);

            Shader.SetValue("fresnelbias", System.Convert.ToSingle(0.15));
            Shader.SetValue("fresnelpow", System.Convert.ToSingle(4));

            Shader.Begin(FX.None);
            Shader.BeginPass(0);
            device.SetTexture(0, ImageTex);

            if (cameraPosition.Y < waterheight)
            {
                device.SetTexture(2, ReflectionTex);
                device.SetTexture(1, RefractionTex);
            }
            else
            {
                device.SetTexture(1, ReflectionTex);
                device.SetTexture(2, RefractionTex);
            }

            device.SetRenderState(RenderState.CullMode, Cull.None);
            device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, verticies);
            device.SetRenderState(RenderState.CullMode, Cull.CounterClockwise);

            Shader.EndPass();
            Shader.End();
        }

        /// <summary>Release objects</summary>
        public void Dispose()
        {
            RTT.Dispose(); RTT = null;
            ReflectionTex.Dispose(); ReflectionTex = null;
            RefractionTex.Dispose(); RefractionTex = null;
            NoiseTex.Dispose(); NoiseTex = null;
        }
    }
}
