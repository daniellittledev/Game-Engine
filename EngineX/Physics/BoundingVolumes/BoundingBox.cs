using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace EngineX.Physics.BoundingVolumes
{
    /// <summary>
    /// Axis Aligned Bounding Box
    /// </summary>
    public class BoundingBox : BoundingVolume
    {
        private Vector3 min;
        private Vector3 max;
        private Vector3[] bounds;
        private Vector3 currentMin;
        private Vector3 currentMax;
        private Vector3 position;
        private Vector3 extents;
        private Vector3 currentPosition;

        /// <summary>
        /// The corner point of the bounding box
        /// </summary>
        public Vector3[] Bounds
        {
            get { return bounds; }
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
        /// Minimum Coordinate
        /// </summary>
        public Vector3 Min
        {
            get { return currentMin; }
        }

        /// <summary>
        /// Maximum Coordinate
        /// </summary>
        public Vector3 Max
        {
            get { return currentMax; }
        }

        /// <summary>
        /// Create new axis aligned bounding box
        /// </summary>
        /// <param name="objMesh">The mesh to create the bounding box from</param>
        public BoundingBox(Mesh objMesh)
            : base()
        {

            // Compute bounding box min and max
            using (VertexBuffer buffer = objMesh.VertexBuffer)
            {
                GraphicsStream GStream = buffer.Lock(0, 0, LockFlags.None);
                Geometry.ComputeBoundingBox(GStream, objMesh.NumberVertices, objMesh.VertexFormat, out min, out max);
                buffer.Unlock();
            }

            // Get object bounds
            bounds = new Vector3[8];
            bounds[0] = new Vector3(min.X, min.Y, min.Z);
            bounds[1] = new Vector3(max.X, min.Y, min.Z);
            bounds[2] = new Vector3(min.X, max.Y, min.Z);
            bounds[3] = new Vector3(max.X, max.Y, min.Z);
            bounds[4] = new Vector3(min.X, min.Y, max.Z);
            bounds[5] = new Vector3(max.X, min.Y, max.Z);
            bounds[6] = new Vector3(min.X, max.Y, max.Z);
            bounds[7] = new Vector3(max.X, max.Y, max.Z);

            // Initilise
            currentMin = min;
            currentMax = max;

            position = Vector3.Multiply(max + min, 0.5f);
            extents = Vector3.Multiply(max - min, 0.5f);

            currentPosition = position;

        }

        /// <summary>
        /// Get the updated axis aligned box for a given translation
        /// </summary>
        /// <param name="transform">The transformation matrix as world space transformer</param>
        public override void Transform(Matrix transform)
        {
            currentPosition = Vector3.TransformCoordinate(position, transform);
            Vector3[] currentBounds = Vector3.TransformCoordinate(bounds, transform);

            currentMin.X = currentMax.X = bounds[0].X;
            currentMin.Y = currentMax.Y = bounds[0].Y;
            currentMin.Z = currentMax.Z = bounds[0].Z;

            for (int i = 1; i < bounds.Length; i++)
            {

                // X coordinate
                if (bounds[i].X < currentMin.X)
                    currentMin.X = bounds[i].X;
                if (bounds[i].X < currentMax.X)
                    currentMax.X = bounds[i].X;

                // Y coordinate
                if (bounds[i].Y < currentMin.Y)
                    currentMin.Y = bounds[i].Y;
                if (bounds[i].Y < currentMax.Y)
                    currentMax.Y = bounds[i].Y;

                // Z coordinate
                if (bounds[i].Z < currentMin.Z)
                    currentMin.Z = bounds[i].Z;
                if (bounds[i].Z < currentMax.Z)
                    currentMax.Z = bounds[i].Z;

            }

        }
    }
}
