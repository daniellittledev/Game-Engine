using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace EngineX.Environment
{
    public class Sky
    {

        private Device device;
        private VertexBuffer vertexBuffer;

        public Sky(Device device)
        {
            this.device = device;

            // Create the VB
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3 * 4 * 8 * 8 + 4 * 3, device, Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.SystemMemory);
            // Create a vertex buffer (100 customervertex)
            CustomVertex.PositionColored[] verts = (CustomVertex.PositionColored[])vertexBuffer.Lock(0, 0); // Lock the buffer (which will return our structs)

            System.Drawing.Color topColor = System.Drawing.Color.LightBlue;
            System.Drawing.Color bottomColor = System.Drawing.Color.Blue;

            int count = 0;

            Vector3 Top = new Vector3(0, 2, 0);
            Vector3 Base = new Vector3(0, -2, 0);

            //Dome
            CreateLayers(verts, 1, topColor, 2, bottomColor, ref count);

            // Unlock (and copy) the data
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

            Vector3 top = new Vector3(0, topHeight, 0);
            Vector3 bottom = new Vector3(0, bottomHeight, 0);

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

            device.RenderState.ZBufferEnable = false;
            device.RenderState.Lighting = false;
            //device.RenderState.FillMode = FillMode.WireFrame;

            device.SetTexture(0, null);
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.SetStreamSource(0, vertexBuffer, 0);
            device.Transform.World = Matrix.Translation(CameraPosition);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 5 * 8 * 8);

            //device.RenderState.FillMode = FillMode.Solid;
            device.RenderState.Lighting = true;
            device.RenderState.ZBufferEnable = true;

        }

    }
}
