using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;
using SharpDX.Direct3D9;
using System.Windows.Forms;
using System.Drawing;

namespace EngineX
{

    /// <summary>
    /// Constants
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// PI / 2 in radians
        /// </summary>
        public const double HalfPI = (System.Math.PI / 2);
        /// <summary>
        /// PI x 2 in radians
        /// </summary>
        public const double DoublePI = (System.Math.PI * 2);
        /// <summary>
        /// 1 / 90
        /// </summary>
        public const double Inv90 = 1 / 90;
        /// <summary>
        /// 1 / 180
        /// </summary>
        public const double Inv180 = 1 / 180;
        /// <summary>
        /// One Degree in Radians
        /// </summary>
        public const double OneDeg = 0.0174532925;
    }

    /// <summary>
    /// Direct3D Initialization
    /// </summary>
    public class Direct3D
    {
        /// <summary>
        /// Shared Direct3D factory instance
        /// </summary>
        private static SharpDX.Direct3D9.Direct3D d3d;

        /// <summary>
        /// Get (or create) the shared Direct3D factory
        /// </summary>
        public static SharpDX.Direct3D9.Direct3D Factory
        {
            get
            {
                if (d3d == null)
                    d3d = new SharpDX.Direct3D9.Direct3D();
                return d3d;
            }
        }

        /// <summary>
        /// Create Present Parameters
        /// </summary>
        public static PresentParameters CreatePrams(System.Windows.Forms.Control Target, bool Windowed, Structures.Size Resolution)
        {
            var factory = Factory;
            DisplayMode DP = factory.GetAdapterDisplayMode(0);
            Capabilities DevCaps = factory.GetDeviceCaps(0, DeviceType.Hardware);

            PresentParameters PPrams = new PresentParameters();
            PPrams.DeviceWindowHandle = Target.Handle;
            PPrams.Windowed = Windowed;
            PPrams.SwapEffect = SwapEffect.Discard;
            PPrams.PresentationInterval = PresentInterval.Default;
            PPrams.PresentFlags = PresentFlags.None;
            PPrams.BackBufferCount = 1;
            PPrams.EnableAutoDepthStencil = true;

            PPrams.BackBufferHeight = (int)Resolution.Y;
            PPrams.BackBufferWidth = (int)Resolution.X;

            Format adaptorFormat = Windowed ? DP.Format : Format.X8R8G8B8;

            if (factory.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, Format.D24S8))
            {
                PPrams.AutoDepthStencilFormat = Format.D24S8;
            }
            else if (factory.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, Format.D24X8))
            {
                PPrams.AutoDepthStencilFormat = Format.D24X8;
            }
            else if (factory.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, Format.D16))
            {
                PPrams.AutoDepthStencilFormat = Format.D16;
            }
            else
            {
                return null;
            }

            PPrams.BackBufferFormat = adaptorFormat;

            return PPrams;
        }

        /// <summary>
        /// Create New D3D Device
        /// </summary>
        public static bool CreateDevice(ref Device Device, System.Windows.Forms.Control Target, bool Windowed, Structures.Size Resolution)
        {
            var factory = Factory;
            DisplayMode DP = factory.GetAdapterDisplayMode(0);
            Capabilities DevCaps = factory.GetDeviceCaps(0, DeviceType.Hardware);

            PresentParameters PPrams = new PresentParameters();
            PPrams.DeviceWindowHandle = Target.Handle;
            PPrams.Windowed = Windowed;
            PPrams.SwapEffect = SwapEffect.Discard;
            PPrams.PresentationInterval = PresentInterval.Default;
            PPrams.PresentFlags = PresentFlags.None;
            PPrams.BackBufferCount = 1;
            PPrams.EnableAutoDepthStencil = true;

            PPrams.BackBufferHeight = (int)Resolution.Y;
            PPrams.BackBufferWidth = (int)Resolution.X;

            CreateFlags CFlag;
            if (DevCaps.DeviceCaps.HasFlag(DeviceCaps.HWRasterization))
            {
                if (DevCaps.DeviceCaps.HasFlag(DeviceCaps.PureDevice))
                    CFlag = CreateFlags.HardwareVertexProcessing | CreateFlags.PureDevice;
                else
                    CFlag = CreateFlags.HardwareVertexProcessing;
            }
            else
            {
                CFlag = CreateFlags.SoftwareVertexProcessing;
            }

            DeviceType DType = DeviceType.Hardware;

            Format adaptorFormat = Windowed ? DP.Format : Format.X8R8G8B8;

            if (factory.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, Format.D24S8))
            {
                PPrams.AutoDepthStencilFormat = Format.D24S8;
            }
            else if (factory.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, Format.D24X8))
            {
                PPrams.AutoDepthStencilFormat = Format.D24X8;
            }
            else if (factory.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, Format.D16))
            {
                PPrams.AutoDepthStencilFormat = Format.D16;
            }
            else
            {
                return false;
            }

            PPrams.BackBufferFormat = adaptorFormat;

            try
            {
                Device = new Device(factory, 0, DType, Target.Handle, CFlag, PPrams);
                return true;
            }
            catch (SharpDXException e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Common Device Preferences
        /// </summary>
        public static void SetUpDevice(Device Device)
        {
            Device.SetRenderState(RenderState.ZEnable, true);
            Device.SetRenderState(RenderState.FillMode, FillMode.Solid);

            Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Linear);
            Device.SetSamplerState(0, SamplerState.MipMapLodBias, 1.0f);

            Device.SetRenderState(RenderState.Ambient, new ColorBGRA(211, 211, 211, 255).ToRgba());
            Device.SetRenderState(RenderState.AntialiasedLineEnable, true);
            Device.SetRenderState(RenderState.SpecularEnable, true);
            Device.SetRenderState(RenderState.ColorVertex, true);
            Device.SetRenderState(RenderState.LocalViewer, true);
            Device.SetRenderState(RenderState.Lighting, true);
        }

        /// <summary>
        /// Set sampler max mip level
        /// </summary>
        public static void SetSmoothing(Device Device, int level)
        {
            Device.SetSamplerState(0, SamplerState.MaxMipLevel, level);
        }

        /// <summary>
        /// Terminate Device Hold
        /// </summary>
        public static void Terminate(Device Device)
        {
            Device.Dispose();
            Device = null;
            if (d3d != null)
            {
                d3d.Dispose();
                d3d = null;
            }
            System.Windows.Forms.Application.Exit();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        /// <summary>
        /// Begin the rendering Process
        /// </summary>
        public static bool BeginRender(Device Device, ClearFlags CFlag, System.Drawing.Color Color, StencilBitDepth StencilClear, bool WinActive, bool DeviceLost)
        {
            if (DeviceLost)
            {
                try
                {
                    Device.TestCooperativeLevel();
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode == ResultCode.DeviceLost)
                    {
                        return false;
                    }
                    else if (ex.ResultCode == ResultCode.DeviceNotReset)
                    {
                        if (WinActive)
                        {
                            Device.Reset(Device.PresentationParameters);
                        }
                    }
                }
            }

            Device.Clear(CFlag, new RawColorBGRA(Color.B, Color.G, Color.R, Color.A), 1.0f, (int)StencilClear);
            Device.BeginScene();

            return true;
        }

        /// <summary>
        /// Display the Rendered Scene
        /// </summary>
        public static bool EndRender(Device Device)
        {
            try
            {
                Device.EndScene();
                Device.Present();
                return true;
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode == ResultCode.DeviceLost)
                    return false;
                throw;
            }
        }

        /// <summary>
        /// Rendering Opacity
        /// </summary>
        public static void Opacity(Device Device, Byte Value)
        {
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            if (Value == 255)
            {
                Device.SetRenderState(RenderState.SourceBlend, Blend.One);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                Device.SetRenderState(RenderState.BlendFactor, unchecked((int)0xFFFFFFFF));
            }
            else
            {
                Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseBlendFactor);
                Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
                Device.SetRenderState(RenderState.BlendFactor, new ColorBGRA(Value, Value, Value, 255).ToRgba());
            }
        }

        /// <summary>
        /// Save ScreenShot
        /// </summary>
        public static bool SaveScreenShot(Device Device)
        {
            Surface pSurf;
            String pFilename;

            if (System.IO.Directory.Exists(Application.StartupPath + "\\ScreenShots") == false)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(Application.StartupPath + "\\ScreenShots");
                }
                catch
                {
                    return false;
                }
            }

            pSurf = Device.GetBackBuffer(0, 0);
            pFilename = Application.StartupPath + "\\ScreenShots\\" + DateTime.Now.ToLongDateString() + ".jpg";

            if (System.IO.File.Exists(pFilename))
            {
                try { System.IO.File.Delete(pFilename); }
                catch { return false; }
            }

            try
            {
                Surface.ToFile(pSurf, pFilename, ImageFileFormat.Jpg);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }

    /// <summary>
    /// Holds Device information and manages transforms
    /// </summary>
    public class TransformsManager
    {
        Device Device1;

        public TransformsManager(Device device)
        { Device1 = device; }

        private Matrix tView = Matrix.Identity;
        private Matrix tProj = Matrix.Identity;
        private Matrix tWorld = Matrix.Identity;
        private Matrix tTex = Matrix.Identity;

        public Matrix View
        {
            get { return tView; }
            set { tView = value; }
        }

        public Matrix Projection
        {
            get { return tProj; }
            set { tProj = value; }
        }

        public Matrix World
        {
            get { return tWorld; }
            set { tWorld = value; }
        }

        public Matrix Texture
        {
            get { return tTex; }
            set { tTex = value; }
        }

        public void SetView()
        { Device1.SetTransform(TransformState.View, tView); }

        public void SetProjection()
        { Device1.SetTransform(TransformState.Projection, tProj); }

        public void SetWorld()
        { Device1.SetTransform(TransformState.World, tWorld); }

        public void SetTexture()
        { Device1.SetTransform(TransformState.Texture0, tTex); }

        public void SetVPW()
        {
            Device1.SetTransform(TransformState.World, tWorld);
            Device1.SetTransform(TransformState.Projection, tProj);
            Device1.SetTransform(TransformState.View, tView);
        }

        public void SetAllTransforms()
        {
            Device1.SetTransform(TransformState.Texture0, tTex);
            Device1.SetTransform(TransformState.World, tWorld);
            Device1.SetTransform(TransformState.Projection, tProj);
            Device1.SetTransform(TransformState.View, tView);
        }
    }

    /// <summary>
    /// Stencil Clear Depth
    /// </summary>
    public enum StencilBitDepth
    {
        /// <summary>Nothing</summary>
        B0 = 0,
        /// <summary>16 Bits</summary>
        B16 = 31,
        /// <summary>32 Bits</summary>
        B32 = 63,
    }

}
