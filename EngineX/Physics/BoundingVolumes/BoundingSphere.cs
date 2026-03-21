using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

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
                GraphicsStream GStream = buffer.Lock(0, 0, LockFlags.None);
                radius = Geometry.ComputeBoundingSphere(GStream, objMesh.NumberVertices, objMesh.VertexFormat, out centre);
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
            radius = Geometry.ComputeBoundingSphere(points, CustomVertex.PositionOnly.StrideSize, out centre);

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

    }

}
