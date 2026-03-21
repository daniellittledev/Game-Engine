using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct3D9;

namespace EngineX.Drawing
{
    class Sprite : IDisposable
    {

        public enum location
        {
            TopLeft = 0,
            TopRight = 1,
            BottomLeft = 2,
            BottomRight = 3,
            Centre = 4
        }

        private Device device;
        private VertexBuffer VertexB;
        private Texture Texture1;
        private Vector3 Roation1;
        private Matrix RenderMatrix1;
        private Matrix RenderMatrix2;

        public Vector3 Position
        {
            get
            {
                return new Vector3(
                    RenderMatrix1.M41,
                    RenderMatrix1.M42,
                    RenderMatrix1.M43);
            }
            set
            {
                RenderMatrix1.M41 = value.X;
                RenderMatrix1.M42 = value.Y;
                RenderMatrix1.M43 = value.Z;
            }
        }

        public Vector3 Scale
        {
            get
            {
                return new Vector3(
                    RenderMatrix1.M11,
                    RenderMatrix1.M22,
                    RenderMatrix1.M33);
            }
            set
            {
                RenderMatrix1.M11 = value.X;
                RenderMatrix1.M22 = value.Y;
                RenderMatrix1.M33 = value.Z;
            }
        }

        public Vector3 Roation
        {
            get { return Roation1; }
            set { Roation1 = value; }
        }

        public Sprite(Device device, location PositionPoint, EngineX.Structures.Size size, Texture texture)
        {
            this.device = device;
            Texture1 = texture;

            int stride = Marshal.SizeOf(typeof(CustomVertex.PositionTextured));
            VertexB = new VertexBuffer(device, 4 * stride, Usage.None, CustomVertex.PositionTextured.Format, Pool.Managed);

            var Vexticies = new CustomVertex.PositionTextured[4];

            switch (PositionPoint)
            {
                case location.TopLeft:
                    Vexticies[0].Position = new Vector3(0, 0, 0);
                    Vexticies[1].Position = new Vector3(size.X, 0, 0);
                    Vexticies[2].Position = new Vector3(0, -size.Y, 0);
                    Vexticies[3].Position = new Vector3(size.X, -size.Y, 0);
                    break;

                case location.TopRight:
                    Vexticies[0].Position = new Vector3(-size.X, 0, 0);
                    Vexticies[1].Position = new Vector3(0, 0, 0);
                    Vexticies[2].Position = new Vector3(-size.X, -size.Y, 0);
                    Vexticies[3].Position = new Vector3(0, -size.Y, 0);
                    break;

                case location.BottomLeft:
                    Vexticies[0].Position = new Vector3(0, size.Y, 0);
                    Vexticies[1].Position = new Vector3(size.X, size.Y, 0);
                    Vexticies[2].Position = new Vector3(0, 0, 0);
                    Vexticies[3].Position = new Vector3(size.X, 0, 0);
                    break;

                case location.BottomRight:
                    Vexticies[0].Position = new Vector3(-size.X, size.Y, 0);
                    Vexticies[1].Position = new Vector3(0, size.Y, 0);
                    Vexticies[2].Position = new Vector3(-size.X, 0, 0);
                    Vexticies[3].Position = new Vector3(0, 0, 0);
                    break;

                case location.Centre:
                    Vexticies[0].Position = new Vector3(-size.X * 0.5f, size.Y * 0.5f, 0);
                    Vexticies[1].Position = new Vector3(size.X * 0.5f, size.Y * 0.5f, 0);
                    Vexticies[2].Position = new Vector3(-size.X * 0.5f, -size.Y * 0.5f, 0);
                    Vexticies[3].Position = new Vector3(size.X * 0.5f, -size.Y * 0.5f, 0);
                    break;

                default:
                    break;
            }

            Vexticies[0].Tu = 0; Vexticies[0].Tv = 0;
            Vexticies[1].Tu = 1; Vexticies[1].Tv = 0;
            Vexticies[2].Tu = 0; Vexticies[2].Tv = 1;
            Vexticies[3].Tu = 1; Vexticies[3].Tv = 1;

            var ds = VertexB.Lock(0, 0, LockFlags.None);
            ds.WriteRange(Vexticies);
            VertexB.Unlock();

            RenderMatrix1 = Matrix.Identity;
        }

        public void Update()
        {
            RenderMatrix2 = Matrix.RotationYawPitchRoll(Roation1.Y, Roation1.X, Roation1.Z) * RenderMatrix1;
        }

        public void Render()
        {
            device.SetTransform(TransformState.World, RenderMatrix2);
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, VertexB, 0, Marshal.SizeOf(typeof(CustomVertex.PositionTextured)));
            device.SetTexture(0, Texture1);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        public void Dispose()
        {
            VertexB.Dispose();
            VertexB = null;

            Texture1.Dispose();
            Texture1 = null;
        }

    }
}
