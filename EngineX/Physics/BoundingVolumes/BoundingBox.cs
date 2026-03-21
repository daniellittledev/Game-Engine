using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;

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
                DataStream GStream = buffer.Lock(0, 0, LockFlags.None);
                int stride = GetFVFStride(objMesh.VertexFormat);
                ComputeBoundingBox(GStream, objMesh.NumberVertices, stride, out min, out max);
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
            Vector3[] currentBounds = new Vector3[bounds.Length];
            for (int i = 0; i < bounds.Length; i++)
                currentBounds[i] = Vector3.TransformCoordinate(bounds[i], transform);

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

        /// <summary>
        /// Computes the FVF vertex stride in bytes from a VertexFormat.
        /// </summary>
        internal static int GetFVFStride(VertexFormat format)
        {
            int size = 0;
            if ((format & VertexFormat.Position) != 0) size += 12;
            if ((format & VertexFormat.Normal) != 0) size += 12;
            if ((format & VertexFormat.PointSize) != 0) size += 4;
            if ((format & VertexFormat.Diffuse) != 0) size += 4;
            if ((format & VertexFormat.Specular) != 0) size += 4;
            int numTex = ((int)format >> 8) & 0xF;
            size += numTex * 8;
            return size;
        }

        /// <summary>
        /// Manually computes a bounding box by reading vertex positions from a DataStream.
        /// Replaces Geometry.ComputeBoundingBox which is not available in SharpDX.
        /// </summary>
        internal static void ComputeBoundingBox(DataStream stream, int numVertices, int stride, out Vector3 outMin, out Vector3 outMax)
        {
            long start = stream.Position;
            Vector3 first = stream.Read<Vector3>();
            outMin = outMax = first;
            for (int i = 1; i < numVertices; i++)
            {
                stream.Position = start + i * stride;
                Vector3 v = stream.Read<Vector3>();
                if (v.X < outMin.X) outMin.X = v.X;
                if (v.Y < outMin.Y) outMin.Y = v.Y;
                if (v.Z < outMin.Z) outMin.Z = v.Z;
                if (v.X > outMax.X) outMax.X = v.X;
                if (v.Y > outMax.Y) outMax.Y = v.Y;
                if (v.Z > outMax.Z) outMax.Z = v.Z;
            }
            stream.Position = start;
        }
    }
}
