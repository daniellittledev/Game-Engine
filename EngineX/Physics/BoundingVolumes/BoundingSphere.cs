using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;

namespace EngineX.Physics.BoundingVolumes
{
    /// <summary>
    /// Bounding Sphere
    /// </summary>
    public class BoundingSphere : BoundingVolume
    {

        private Vector3 currentCentre;
        private Vector3 centre;
        private float radius;

        /// <summary>
        /// Sphere Radius
        /// </summary>
        public float Radius
        {
            get { return radius; }
        }

        /// <summary>
        /// Sphere Centre
        /// </summary>
        public Vector3 Centre
        {
            get { return currentCentre; }
        }

        /// <summary>
        /// Create new bounding sphere
        /// </summary>
        /// <param name="objMesh">The mesh to create the bounding spherefrom</param>
        public BoundingSphere(Mesh objMesh)
            : base()
        {

            // Compute bounding sphere
            using (VertexBuffer buffer = objMesh.VertexBuffer)
            {
                DataStream GStream = buffer.Lock(0, 0, LockFlags.None);
                int stride = BoundingBox.GetFVFStride(objMesh.VertexFormat);
                radius = ComputeBoundingSphere(GStream, objMesh.NumberVertices, stride, out centre);
                buffer.Unlock();
            }

            // Initilise
            currentCentre = centre;

        }


        /// <summary>
        /// Create new bounding sphere
        /// </summary>
        /// <param name="points">The points from which to create the bounding sphere</param>
        public BoundingSphere(CustomVertex.PositionOnly[] points)
            : base()
        {

            // Compute bounding sphere
            radius = ComputeBoundingSphereFromPoints(points, out centre);

            // Initilise
            currentCentre = centre;

        }

        /// <summary>
        /// Get the updated bounding sphere for a given translation
        /// </summary>
        /// <param name="translation">The translation vector as world space position</param>
        public void Translate(Vector3 translation)
        {
            currentCentre = centre + translation;
        }

        /// <summary>
        /// Transform 'Not Implimente'
        /// </summary>
        /// <param name="transform"></param>
        public override void Transform(Matrix transform)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Computes a bounding sphere from a DataStream of vertices.
        /// Replaces Geometry.ComputeBoundingSphere which is not available in SharpDX.
        /// </summary>
        private static float ComputeBoundingSphere(DataStream stream, int numVertices, int stride, out Vector3 center)
        {
            long start = stream.Position;
            // Use bounding box to find center
            BoundingBox.ComputeBoundingBox(stream, numVertices, stride, out Vector3 bMin, out Vector3 bMax);
            center = (bMin + bMax) * 0.5f;
            float radius = 0;
            for (int i = 0; i < numVertices; i++)
            {
                stream.Position = start + i * stride;
                Vector3 v = stream.Read<Vector3>();
                float d = Vector3.Distance(center, v);
                if (d > radius) radius = d;
            }
            stream.Position = start;
            return radius;
        }

        /// <summary>
        /// Computes a bounding sphere from an array of position-only vertices.
        /// Replaces Geometry.ComputeBoundingSphere overload for arrays.
        /// </summary>
        private static float ComputeBoundingSphereFromPoints(CustomVertex.PositionOnly[] points, out Vector3 center)
        {
            Vector3 bMin = points[0].Position;
            Vector3 bMax = points[0].Position;
            foreach (var p in points)
            {
                Vector3 pos = p.Position;
                if (pos.X < bMin.X) bMin.X = pos.X;
                if (pos.Y < bMin.Y) bMin.Y = pos.Y;
                if (pos.Z < bMin.Z) bMin.Z = pos.Z;
                if (pos.X > bMax.X) bMax.X = pos.X;
                if (pos.Y > bMax.Y) bMax.Y = pos.Y;
                if (pos.Z > bMax.Z) bMax.Z = pos.Z;
            }
            center = (bMin + bMax) * 0.5f;
            float radius = 0;
            foreach (var p in points)
            {
                float d = Vector3.Distance(center, p.Position);
                if (d > radius) radius = d;
            }
            return radius;
        }
    }

}
