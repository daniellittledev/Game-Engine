using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace EngineX.Physics.BoundingVolumes
{
    /// <summary>
    /// Orientated Bounding Box
    /// </summary>
    public class BoundingOrientatedBox : BoundingVolume
    {

        private Vector3 position;
        private Vector3 extents;

        private Vector3 currentPosition;

        private Vector3 xAxis;
        private Vector3 yAxis;
        private Vector3 zAxis;

        /// <summary>
        /// X Axis
        /// </summary>
        public Vector3 XAxis
        {
            get { return xAxis; }
        }

        /// <summary>
        /// Y Axis
        /// </summary>
        public Vector3 YAxis
        {
            get { return yAxis; }
        }

        /// <summary>
        /// Z Axis
        /// </summary>
        public Vector3 ZAxis
        {
            get { return zAxis; }
        }

        /// <summary>
        /// The dimensions of the box along the three axes X, Y and Z respectively.
        /// </summary>
        public Vector3 Extents
        {
            get { return extents; }
        }

        /// <summary>
        /// The box centre
        /// </summary>
        public Vector3 Position
        {
            get { return currentPosition; }
        }

        /// <summary>
        /// Create new axis aligned bounding box
        /// </summary>
        /// <param name="objMesh">The mesh to create the bounding box from</param>
        public BoundingOrientatedBox(Mesh objMesh)
            : base()
        {
            // Box minimum and maximum, used to calculate the extents and position
            Vector3 min, max;

            // Compute bounding box min and max
            using (VertexBuffer buffer = objMesh.VertexBuffer)
            {
                GraphicsStream GStream = buffer.Lock(0, 0, LockFlags.None);
                Geometry.ComputeBoundingBox(GStream, objMesh.NumberVertices, objMesh.VertexFormat, out min, out max);
                buffer.Unlock();
            }


            // Initilise
            position = Vector3.Multiply(max + min, 0.5f);
            extents = Vector3.Multiply(max - min, 0.5f);

            currentPosition = position;

        }


        /// <summary>
        /// Get the updated orientated box for a given translation
        /// </summary>
        /// <param name="transform">The transformation matrix as world space transformer</param>
        public override void Transform(Matrix transform)
        {

            currentPosition = Vector3.TransformCoordinate(position, transform);
            //currentExtents = Vector3.TransformCoordinate(extents, transform);

            //xaxis
            xAxis.X = transform.M11;
            xAxis.Y = transform.M21;
            xAxis.Z = transform.M31;

            //yaxis
            yAxis.X = transform.M12;
            yAxis.Y = transform.M22;
            yAxis.Z = transform.M32;

            //zaxis
            zAxis.X = transform.M13;
            zAxis.Y = transform.M23;
            zAxis.Z = transform.M33;

        }

    }
}
