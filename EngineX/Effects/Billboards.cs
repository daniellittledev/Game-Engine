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
}
