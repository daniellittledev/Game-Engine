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
            private System.Drawing.Color Color1;
            /// <summary>
            /// Rendering Vertex Buffer
            /// </summary>
            private VertexBuffer Buffer1;

            /// <summary>
            /// Initilize Billboard
            /// </summary>
            public BeamBillboard(Device device, String texture, System.Drawing.Color color)
            {
                Device1 = device;
                Texture1 = Texture.FromFile(Device1, texture);
                Color1 = color;

                int stride = Marshal.SizeOf(typeof(CustomVertex.PositionColoredTextured));
                Buffer1 = new VertexBuffer(Device1, 4 * stride, Usage.Dynamic, CustomVertex.PositionColoredTextured.Format, Pool.SystemMemory);
            }

            /// <summary>
            /// Update Billboard
            /// </summary>
            public void Update(Vector3 origin, Vector3 dest, Vector3 camera_pos, float radius)
            {
                Vector3 vector1 = camera_pos - origin;
                Vector3 vector2 = dest - origin;
                Vector3 normal = Vector3.Cross(vector1, vector2);
                normal = Vector3.Normalize(normal);

                var Vertex = new CustomVertex.PositionColoredTextured[4];

                // first corner
                Vertex[0].Tu = 0; Vertex[0].Tv = 0;
                Vertex[0].Color = Color1.ToArgb();
                Vertex[0].X = origin.X - (radius * normal.X);
                Vertex[0].Y = origin.Y - (radius * normal.Y);
                Vertex[0].Z = origin.Z - (radius * normal.Z);

                // second corner
                Vertex[1].Tu = 1; Vertex[1].Tv = 0;
                Vertex[1].Color = Color1.ToArgb();
                Vertex[1].X = origin.X + (radius * normal.X);
                Vertex[1].Y = origin.Y + (radius * normal.Y);
                Vertex[1].Z = origin.Z + (radius * normal.Z);

                // third corner
                Vertex[2].Tu = 0; Vertex[2].Tv = 1;
                Vertex[2].Color = Color1.ToArgb();
                Vertex[2].X = dest.X + (radius * normal.X);
                Vertex[2].Y = dest.Y + (radius * normal.Y);
                Vertex[2].Z = dest.Z + (radius * normal.Z);

                // forth corner
                Vertex[3].Tu = 1; Vertex[3].Tv = 1;
                Vertex[3].Color = Color1.ToArgb();
                Vertex[3].X = dest.X - (radius * normal.X);
                Vertex[3].Y = dest.Y - (radius * normal.Y);
                Vertex[3].Z = dest.Z - (radius * normal.Z);

                var ds = Buffer1.Lock(0, 0, LockFlags.Discard);
                ds.WriteRange(Vertex);
                Buffer1.Unlock();
            }

            /// <summary>
            /// Render Billboard
            /// </summary>
            public void Render()
            {
                Device1.SetTransform(TransformState.World, Matrix.Identity);
                Device1.VertexFormat = CustomVertex.PositionColoredTextured.Format;
                Device1.SetStreamSource(0, Buffer1, 0, Marshal.SizeOf(typeof(CustomVertex.PositionColoredTextured)));
                Device1.SetTexture(0, Texture1);
                Device1.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

    }
}
