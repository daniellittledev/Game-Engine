using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct3D9;

namespace EngineX.Environment
{
    public class Sky
    {

        private Device device;
        private VertexBuffer vertexBuffer;
        private int vertexCount;

        public Sky(Device device)
        {
            this.device = device;

            vertexCount = 3 * 4 * 8 * 8 + 4 * 3;
            int stride = Marshal.SizeOf(typeof(CustomVertex.PositionColored));

            vertexBuffer = new VertexBuffer(device, vertexCount * stride, Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.SystemMemory);

            var verts = new CustomVertex.PositionColored[vertexCount];

            System.Drawing.Color topColor = System.Drawing.Color.LightBlue;
            System.Drawing.Color bottomColor = System.Drawing.Color.Blue;

            int count = 0;

            CreateLayers(verts, 1, topColor, 2, bottomColor, ref count);

            var ds = vertexBuffer.Lock(0, 0, LockFlags.None);
            ds.WriteRange(verts);
            vertexBuffer.Unlock();
        }

        private void CreateLayers(CustomVertex.PositionColored[] verts, float topRadius, System.Drawing.Color topColor, float bottomRadius, System.Drawing.Color bottomColor, ref int offset)
        {
            float topHeight = 1;
            float bottomHeight = -1;

            Vector3 topHeightC = new Vector3(0, 2, 0);
            Vector3 bottomHeightC = new Vector3(0, -2, 0);

            float parts = 8;
            float gap = (float)(Math.PI * 2) / parts;

            float bodyBegin = 1;
            float bodyEnd = -1;

            for (int i = 0; i < parts; i++)
            {
                for (int j = 0; j < parts; j++)
                {
                    System.Drawing.Color C1 = Lerp(bottomColor, topColor, (parts - j) / parts);
                    System.Drawing.Color C2 = Lerp(bottomColor, topColor, (parts - (j + 1)) / parts);

                    Vector3 leftTop = MathX.Math3D.PointOnSphere(j / (parts + 1), gap * i, 0);
                    Vector3 rightTop = MathX.Math3D.PointOnSphere(j / (parts + 1), gap * (i + 1), 0);

                    Vector3 leftBottom = MathX.Math3D.PointOnSphere((j + 1) / (parts + 1), gap * i, 0);
                    Vector3 rightBottom = MathX.Math3D.PointOnSphere((j + 1) / (parts + 1), gap * (i + 1), 0);

                    if (j == 0)
                    {
                        //Top
                        verts[offset].Color = topColor.ToArgb();
                        verts[offset++].Position = topHeightC;
                        verts[offset].Color = C2.ToArgb();
                        verts[offset++].Position = leftBottom + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - (j + 1) / (parts + 1))), 0);
                        verts[offset].Color = C2.ToArgb();
                        verts[offset++].Position = rightBottom + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - (j + 1) / (parts + 1))), 0);
                    }
                    else
                    {
                        //Face
                        verts[offset].Color = C1.ToArgb();
                        verts[offset++].Position = rightTop + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - j / (parts + 1))), 0);
                        verts[offset].Color = C1.ToArgb();
                        verts[offset++].Position = leftTop + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - j / (parts + 1))), 0);
                        verts[offset].Color = C2.ToArgb();
                        verts[offset++].Position = leftBottom + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - (j + 1) / (parts + 1))), 0);

                        verts[offset].Color = C1.ToArgb();
                        verts[offset++].Position = rightTop + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - j / (parts + 1))), 0);
                        verts[offset].Color = C2.ToArgb();
                        verts[offset++].Position = leftBottom + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - (j + 1) / (parts + 1))), 0);
                        verts[offset].Color = C2.ToArgb();
                        verts[offset++].Position = rightBottom + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - (j + 1) / (parts + 1))), 0);

                        if (j == (parts - 1))
                        {
                            //Base
                            verts[offset].Color = bottomColor.ToArgb();
                            verts[offset++].Position = bottomHeightC;
                            verts[offset].Color = C2.ToArgb();
                            verts[offset++].Position = rightBottom + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - (j + 1) / (parts + 1))), 0);
                            verts[offset].Color = C2.ToArgb();
                            verts[offset++].Position = leftBottom + new Vector3(0, Lerp(bodyEnd, bodyBegin, (1 - (j + 1) / (parts + 1))), 0);
                        }
                    }
                }
            }
        }

        private System.Drawing.Color Lerp(System.Drawing.Color c1, System.Drawing.Color c2, float f)
        {
            byte a = (byte)(c1.A + (c2.A - c1.A) * f);
            byte r = (byte)(c1.R + (c2.R - c1.R) * f);
            byte g = (byte)(c1.G + (c2.G - c1.G) * f);
            byte b = (byte)(c1.B + (c2.B - c1.B) * f);
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        private float Lerp(float f1, float f2, float f)
        {
            return f1 + (f2 - f1) * f;
        }

        public void Render(Vector3 CameraPosition)
        {
            device.SetRenderState(RenderState.ZEnable, false);
            device.SetRenderState(RenderState.Lighting, false);

            device.SetTexture(0, null);
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.SetStreamSource(0, vertexBuffer, 0, Marshal.SizeOf(typeof(CustomVertex.PositionColored)));
            device.SetTransform(TransformState.World, Matrix.Translation(CameraPosition));
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 5 * 8 * 8);

            device.SetRenderState(RenderState.Lighting, true);
            device.SetRenderState(RenderState.ZEnable, true);
        }

    }
}
