using System;
using System.Collections.Generic;
using System.Text;
using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Security;
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
    /// Direct3D Initilazation
    /// </summary>
    public class Direct3D
    {

        /// <summary>
        /// Create Present Parameters
        /// </summary>
        /// <param name="Device"></param>
        public static PresentParameters CreatePrams(System.Windows.Forms.Control Target, bool Windowed, Structures.Size Resolution)
        {
            // Device, Size, Buffer format, Full screen, Window

            //Is App Full Screen/Windowed
            PresentParameters PPrams;
            PPrams = new PresentParameters();

            int adapterOrdinal = Manager.Adapters.Default.Adapter;
            DisplayMode DP = Manager.Adapters.Default.CurrentDisplayMode;
            Caps DevCaps = new Caps();
            DevCaps = Manager.GetDeviceCaps(adapterOrdinal, DeviceType.Hardware);
            //DisplayMode DP;

            //Presentation Parameters//
            PPrams = new PresentParameters();
            PPrams.DeviceWindow = Target;
            PPrams.Windowed = Windowed;
            PPrams.SwapEffect = SwapEffect.Discard;
            PPrams.PresentationInterval = PresentInterval.Default;
            PPrams.PresentFlag = PresentFlag.None;
            PPrams.BackBufferCount = 1;
            PPrams.EnableAutoDepthStencil = true;
            //PPrams.FullScreenRefreshRateInHz = 60;

            if (Windowed == true)
            {
                //DP = new DisplayMode();
                //DP = Manager.Adapters.Default.CurrentDisplayMode;
                PPrams.BackBufferHeight = (int)Resolution.Y;
                PPrams.BackBufferWidth = (int)Resolution.X;
            }
            else
            {
                //DP = new DisplayMode();
                //DP = Manager.Adapters.Default.CurrentDisplayMode;
                PPrams.BackBufferHeight = (int)Resolution.Y;
                PPrams.BackBufferWidth = (int)Resolution.X;
            }


            //Find out how to choose.
            //PPrams.AutoDepthStencilFormat = DepthFormat.D16; //low
            //PPrams.AutoDepthStencilFormat = DepthFormat.D32; //High

            //PPrams.BackBufferFormat = DP.Format;

            //Creation Flags//
            CreateFlags CFlag = new CreateFlags();
            if (DevCaps.DeviceCaps.SupportsHardwareRasterization == true)
            {
                if (DevCaps.DeviceCaps.SupportsPureDevice == true)
                { CFlag = CreateFlags.HardwareVertexProcessing | CreateFlags.PureDevice; }
                else
                { CFlag = CreateFlags.HardwareVertexProcessing; }
            }
            else
            { CFlag = CreateFlags.SoftwareVertexProcessing; }

            //Device Type//
            DeviceType DType;
            DType = DevCaps.DeviceType;

            //Find backBufferFormats //Find depthStencilFormats

            Format adaptorFormat = (Windowed) ? DP.Format : Format.X8R8G8B8;
            if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D24S8))
            {
                PPrams.AutoDepthStencilFormat = DepthFormat.D24S8;
            }
            else if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D24X8))
            {
                PPrams.AutoDepthStencilFormat = DepthFormat.D24X8;
            }
            else if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D16))
            {
                PPrams.AutoDepthStencilFormat = DepthFormat.D16;
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
        /// <param name="Device"></param>
        public static bool CreateDevice(ref Device Device, System.Windows.Forms.Control Target, bool Windowed, Structures.Size Resolution)
        {
            // Device, Size, Buffer format, Full screen, Window

            //Is App Full Screen/Windowed
            PresentParameters PPrams;
            PPrams = new PresentParameters();

            int adapterOrdinal = Manager.Adapters.Default.Adapter;
            DisplayMode DP = Manager.Adapters.Default.CurrentDisplayMode;
            Caps DevCaps = new Caps();
            DevCaps = Manager.GetDeviceCaps(adapterOrdinal, DeviceType.Hardware);
            //DisplayMode DP;

            //Presentation Parameters//
            PPrams = new PresentParameters();
            PPrams.DeviceWindow = Target;
            PPrams.Windowed = Windowed;
            PPrams.SwapEffect = SwapEffect.Discard;
            PPrams.PresentationInterval = PresentInterval.Default;
            PPrams.PresentFlag = PresentFlag.None;
            PPrams.BackBufferCount = 1;
            PPrams.EnableAutoDepthStencil = true;
            //PPrams.FullScreenRefreshRateInHz = 60;

            if (Windowed == true)
            {
                //DP = new DisplayMode();
                //DP = Manager.Adapters.Default.CurrentDisplayMode;
                PPrams.BackBufferHeight = (int)Resolution.Y;
                PPrams.BackBufferWidth = (int)Resolution.X;
            }
            else
            {
                //DP = new DisplayMode();
                //DP = Manager.Adapters.Default.CurrentDisplayMode;
                PPrams.BackBufferHeight = (int)Resolution.Y;
                PPrams.BackBufferWidth = (int)Resolution.X;
            }


            //Find out how to choose.
            //PPrams.AutoDepthStencilFormat = DepthFormat.D16; //low
            //PPrams.AutoDepthStencilFormat = DepthFormat.D32; //High

            //PPrams.BackBufferFormat = DP.Format;

            //Creation Flags//
            CreateFlags CFlag = new CreateFlags();
            if (DevCaps.DeviceCaps.SupportsHardwareRasterization == true)
            {
                if (DevCaps.DeviceCaps.SupportsPureDevice == true)
                { CFlag = CreateFlags.HardwareVertexProcessing | CreateFlags.PureDevice; }
                else
                { CFlag = CreateFlags.HardwareVertexProcessing; }
            }
            else
            { CFlag = CreateFlags.SoftwareVertexProcessing; }

            //Device Type//
            DeviceType DType;
            DType = DevCaps.DeviceType;

            //Find backBufferFormats //Find depthStencilFormats

            Format adaptorFormat = (Windowed) ? DP.Format : Format.X8R8G8B8;
            if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D24S8))
            {
                PPrams.AutoDepthStencilFormat = DepthFormat.D24S8;
            }
            else if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D24X8))
            {
                PPrams.AutoDepthStencilFormat = DepthFormat.D24X8;
            }
            else if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, adaptorFormat, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D16))
            {
                PPrams.AutoDepthStencilFormat = DepthFormat.D16;
            }
            else
            {
                return false;
            }

            PPrams.BackBufferFormat = adaptorFormat;

            //PPrams.BackBufferFormat = Format.R5G6B5;
            //PPrams.AutoDepthStencilFormat = DepthFormat.D24X8 ;


            //    DepthFormat[] depthStencilFormats = new DepthFormat[]
            //    {
            //DepthFormat.D16,
            //DepthFormat.D15S1,
            //DepthFormat.D24X8,
            //DepthFormat.D24S8,
            //DepthFormat.D24X4S4,
            //DepthFormat.D32,
            //    };

            //    DeviceType[] deviceTypes = new DeviceType[] 
            //    { 
            //DeviceType.Hardware, 
            //DeviceType.Software,
            //DeviceType.Reference 
            //    };

            //    Format[] backBufferFormats = new Format[] 
            //    {
            //Format.A8R8G8B8, 
            //Format.X8R8G8B8, 
            //Format.A2R10G10B10, 
            //Format.R5G6B5, 
            //Format.A1R5G5B5, 
            //Format.X1R5G5B5
            //    };

            //    DepthFormat[] DFormats;
            //    DFormats = new DepthFormat[20];
            //    int Count2 = 0;

            //    Format[] Formats;
            //    Formats = new Format[20];
            //    int Count1 = 0;

            //    foreach (Format backBufferFormat in backBufferFormats)
            //    {
            //        if (Manager.CheckDeviceType(
            //            Manager.Adapters.Default.Adapter, DType, Manager.Adapters.Default.CurrentDisplayMode.Format,
            //            backBufferFormat, Windowed))
            //        {
            //            //////////////////////////////////////
            //            //Formats[Count1++] = backBufferFormat;
            //            // *** This combination is valid!
            //            foreach (DepthFormat depthStencilFormat in depthStencilFormats)
            //            {
            //                if (Manager.CheckDeviceFormat(Manager.Adapters.Default.Adapter, DType, Manager.Adapters.Default.CurrentDisplayMode.Format,
            //                        Usage.DepthStencil, ResourceType.Surface, depthStencilFormat))
            //                {
            //                    if (Manager.CheckDepthStencilMatch(Manager.Adapters.Default.Adapter, DType, Manager.Adapters.Default.CurrentDisplayMode.Format,
            //                        backBufferFormat, depthStencilFormat))
            //                    {
            //                        // This depth stencil format is compatible

            //                        Formats[Count1++] = backBufferFormat;
            //                        DFormats[Count2++] = depthStencilFormat;

            //                    }
            //                }
            //            }
            //            //////////////////////////////////////
            //        }
            //    } // backBufferFormat

            //Formats
            //DFormats

            //String temp = "";

            ////int i;
            //for (int i = 0; i <= 5; i++)
            //{
            //    temp += Formats[i] + ", ";
            //}

            //temp += "/r";

            //for (int i = 0; i <= 5; i++)
            //{
            //    temp += DFormats[i] + ", ";
            //}


            //MessageBox.Show(temp);

            // PPrams.AutoDepthStencilFormat = DFormats[DFormats.Length - 2]; //low
            //PPrams.EnableAutoDepthStencil = true;
            //.BackBufferFormat = Formats[Formats.Length - 2];

            try
            {
                //Create Device
                Device = new Device(0, DType, Target, CFlag, PPrams);
                return true;
            }
            catch (DirectXException e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

        }

        /// <summary>
        /// Common Device Preferences
        /// </summary>
        /// <param name="Device"></param>
        public static void SetUpDevice(Device Device)
        {
            //SetUp Device
            Device.RenderState.ZBufferEnable = true;

            //We want it to be rendered solid. You can also do Wireframe and Points
            Device.RenderState.FillMode = FillMode.Solid;

            //Remove the pixilatedness!
            Device.SamplerState[0].MinFilter = TextureFilter.Linear;
            Device.SamplerState[0].MagFilter = TextureFilter.Linear;
            Device.SamplerState[0].MipFilter = TextureFilter.Linear;
            Device.SamplerState[0].MagFilter = TextureFilter.Linear;
            Device.SamplerState[0].MipMapLevelOfDetailBias = 1;
            //Device.SamplerState[0].SrgbTexture = true;

            //Turn on some low level ambient light
            Device.RenderState.Ambient = System.Drawing.Color.LightGray;
            Device.RenderState.AntiAliasedLineEnable = true;
            Device.RenderState.SpecularEnable = true;
            Device.RenderState.ColorVertex = true;
            Device.RenderState.LocalViewer = true;
            Device.RenderState.Lighting = true;

        }

        /// <summary>
        /// Set sampler max mip level
        /// </summary>
        /// <param name="Device"></param>
        /// <param name="level"></param>
        public static void SetSmoothing(Device Device, int level)
        {
            Device.SamplerState[0].MaxMipLevel = level;
        }

        /// <summary>
        /// Terminate Device Hold
        /// </summary>
        /// <param name="Device"></param>
        public static void Terminate(Device Device)
        {

            Device = null;
            //Exit
            System.Windows.Forms.Application.Exit();
            //FORCE an exit if it didnt exit
            System.Environment.Exit(System.Environment.ExitCode);

        }

        /// <summary>
        /// Begin the rendering Proccess
        /// </summary>
        /// <param name="Device"></param>
        /// <param name="CFlag"></param>
        /// <param name="Color"></param>
        /// <param name="StencilClear"></param>
        public static bool BeginRender(Device Device, ClearFlags CFlag, Color Color, StencilBitDepth StencilClear, bool WinActive, bool DeviceLost)
        {

            if (DeviceLost == true)
            {

                try
                {
                    // Test the cooperative level to see if it's okay to render
                    Device.TestCooperativeLevel();
                }
                catch (DeviceLostException)
                {
                    // If the device was lost, do not render until we get it back
                    return false;
                }
                catch (DeviceNotResetException)
                {
                    // Check if the device needs to be resized.
                    // If we are windowed, read the desktop mode and use the same format for
                    // the back buffer
                    if (WinActive == true)
                    {
                        Device.Reset(Device.PresentationParameters);
                    }
                }

            }

            Device.Clear(CFlag, Color, 1.0f, (int)StencilClear);
            Device.BeginScene();

            return true;
        }

        /// <summary>
        /// Display the Rendered Scene
        /// </summary>
        /// <param name="Device"></param>
        /// <returns></returns>
        public static bool EndRender(Device Device)
        {

            try
            {
                // Show the frame on the primary surface.
                Device.EndScene();
                Device.Present();
                return true;
            }
            catch (DeviceLostException)
            {
                return false;
            }

        }

        /// <summary>
        /// Rendering Opacity
        /// </summary>
        /// <param name="Device"></param>
        /// <param name="Value"></param>
        public static void Opacity(Device Device, Byte Value)
        {
            Device.RenderState.AlphaBlendEnable = true;
            if (Value == 255)
            {
                Device.RenderState.SourceBlend = Blend.One;
                Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
                Device.RenderState.BlendFactor = Color.White;
            }
            else
            {
                Device.RenderState.SourceBlend = Blend.BlendFactor;
                Device.RenderState.DestinationBlend = Blend.InvBlendFactor;
                Device.RenderState.BlendOperation = BlendOperation.Add;
                Device.RenderState.BlendFactor = Color.FromArgb(Value, Value, Value);
            }
        }

        /// <summary>
        /// Save ScreenShot
        /// </summary>
        /// <param name="Device"></param>
        /// <returns></returns>
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
                    //Unable to create directory
                    return false;
                }
            }
            pSurf = Device.GetBackBuffer(0, 0, BackBufferType.Mono);
            pFilename = Application.StartupPath + "\\ScreenShots\\" + DateTime.Now.ToLongDateString() + ".jpg";
            if (System.IO.File.Exists(pFilename))
            {
                try
                {
                    System.IO.File.Delete(pFilename);
                }
                catch
                {
                    //Unable to delete
                    return false;
                }
            }

            try
            {
                SurfaceLoader.Save(pFilename, ImageFileFormat.Jpg, pSurf);
                return true;
            }
            catch
            {
                //Unable to create file
                return false;
            }
        }

    }

    /// <summary>
    /// Holds Device information
    /// </summary>
    public class TransformsManager
    {
        /// <summary>
        /// Rendering Device
        /// </summary>
        Device Device1;

        /// <summary>
        /// Initilize Transforms Manager
        /// </summary>
        /// <param name="device"></param>
        public TransformsManager(Device device)
        { Device1 = device; }

        /// <summary>
        /// View matrix
        /// </summary>
        private Matrix tView = Matrix.Identity;
        /// <summary>
        /// Projection Matrix
        /// </summary>
        private Matrix tProj = Matrix.Identity;
        /// <summary>
        /// World Matrix
        /// </summary>
        private Matrix tWorld = Matrix.Identity;
        /// <summary>
        /// Texture Matrix
        /// </summary>
        private Matrix tTex = Matrix.Identity;

        /// <summary>
        /// View Matix
        /// </summary>
        public Matrix View
        {
            get
            { return tView; }
            set
            { tView = value; }
        }
        /// <summary>
        /// Projection Matrix
        /// </summary>
        public Matrix Projection
        {
            get
            { return tProj; }
            set
            { tProj = value; }
        }
        /// <summary>
        /// World Matirx
        /// </summary>
        public Matrix World
        {
            get
            { return tWorld; }
            set
            { tWorld = value; }
        }
        /// <summary>
        /// Texture Matrix
        /// </summary>
        public Matrix Texture
        {
            get
            { return tTex; }
            set
            { tTex = value; }
        }

        /// <summary>
        /// Set rendering device view matrix.
        /// </summary>
        public void SetView()
        { Device1.Transform.View = tView; }

        /// <summary>
        /// Set rendering device Projection matrix.
        /// </summary>
        public void SetProjection()
        { Device1.Transform.Projection = tProj; }

        /// <summary>
        /// Set rendering device World matrix.
        /// </summary>
        public void SetWorld()
        { Device1.Transform.World = tWorld; }

        /// <summary>
        /// Set rendering device Texture matrix.
        /// </summary>
        public void SetTexture()
        { Device1.Transform.Texture0 = tTex; }

        /// <summary>
        /// Set rendering device view, projection and world matrix.
        /// </summary>
        public void SetVPW()
        {
            Device1.Transform.View = tWorld;
            Device1.Transform.Projection = tProj;
            Device1.Transform.View = tView;
        }

        /// <summary>
        /// Set rendering device view, projection, world and texture matrix.
        /// </summary>
        public void SetAllTransforms()
        {
            Device1.Transform.Texture0 = tTex;
            Device1.Transform.View = tWorld;
            Device1.Transform.Projection = tProj;
            Device1.Transform.View = tView;
        }

    }

    /// <summary>
    /// Stencil Clear Depth
    /// </summary>
    public enum StencilBitDepth
    {
        /// <summary>
        /// Nothing
        /// </summary>
        B0 = 0,
        /// <summary>
        /// 16 Bits
        /// </summary>
        B16 = 31,
        /// <summary>
        /// 32 Bits
        /// </summary>
        B32 = 63,
    }


}