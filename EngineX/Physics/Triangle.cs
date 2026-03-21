using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace EngineX.Physics
{
    /// <summary>
    /// Triangle
    /// </summary>
    public class Triangle
    {

        private Vector3 i;

        public Vector3 I
        {
            get { return i; }
            set 
            { 
                i = value;
            }
        }
        private Vector3 j;

        public Vector3 J
        {
            get { return j; }
            set 
            { 
                j = value;
            }
        }
        private Vector3 k;

        public Vector3 K
        {
            get { return k; }
            set 
            { 
                k = value; 
            }
        }

        private Vector3 normal;

	    public Vector3 Normal
	    {
	      get { return normal;}
	    }
	

        /// <summary>
        /// Build Triangle
        /// </summary>
        public Triangle()
        {
            i = j = k = Vector3.Empty;
        }

        /// <summary>
        /// Build triangle from three points
        /// </summary>
        /// <param name="I"></param>
        /// <param name="J"></param>
        /// <param name="K"></param>
        public Triangle(Vector3 I, Vector3 J, Vector3 K)
        {
            i = I;
            j = J;
            k = K;
        }

        /// <summary>
        /// Tests if a ray has intersected the triangle
        /// </summary>
        /// <param name="ray">The ray</param>
        /// <param name="location">The point of intersection</param>
        /// <returns>Boolean: Collision has occured</returns>
        public bool RayIntersect(Ray ray, out Vector3 location)
        {

            // Parameters
            location = Vector3.Empty;

            // Barycentric
            // u = point in plane , t = distance to plane, v = point in plane 

            Vector3 edge1, edge2, tvec, pvec, qvec;
            float det, inv_det;

            // Find vectors for two edges sharing vertex[0]
            edge1 = j - i;
            edge2 = k - i;

            // Begin calculation of the determinant - Used to find u value
            // Cross ray direction and edge2
            pvec = Vector3.Cross(ray.Direction, edge2);

            // If determint is near zero, ray lies in plane of triangle
            det = Vector3.Dot(edge1, pvec);

            // Culling Branch \\
            if (det < 0.000001f) // Epsilon
                return false;

            // Calculate distance from vertex[0] to ray origin
            tvec = ray.Origin - i;

            // Calculate u value and test bounds
            float u = Vector3.Dot(tvec, pvec);
            if (u < 0.0f || u > det)
                return false;

            // Prepare to test v value
            qvec = Vector3.Cross(tvec, edge1); //Fix

            // Calculate v value and test bounds
            float v = Vector3.Dot(ray.Direction, qvec);
            if (v < 0.0f || u + v > det)
                return false;

            // Calculate t, scale values, interection is true
            float t = Vector3.Dot(edge2, qvec);
            inv_det = 1.0f / det;

            t *= inv_det;
            u *= inv_det;
            v *= inv_det;

            //Vector3 FromBarycentric(float u, float v)
            //Get Position
            location = ((1 - u - v) * i) + (u * j) + (v * k);

            return true;

        }

        public bool TriangleIntersect(Triangle triangle)
        {
            List<Vector3> collisionPoints = new List<Vector3>(3);
            Vector3 outVextor;

            Ray a, b, c;

            a = new Ray(triangle.i, triangle.j);
            b = new Ray(triangle.j, triangle.k);
            c = new Ray(triangle.k, triangle.i);

            if (RayIntersect(a, out outVextor))
            {
                if (MathX.Math3D.RelEqual(MathX.Math3D.LineToPoint(a, outVextor), 0, 6))
                {
                    collisionPoints.Add(outVextor);
                }
            }

            if (RayIntersect(b, out outVextor))
            {
                if (MathX.Math3D.RelEqual(MathX.Math3D.LineToPoint(b, outVextor), 0, 6))
                {
                    collisionPoints.Add(outVextor);
                }
            }

            if (RayIntersect(c, out outVextor))
            {
                if (MathX.Math3D.RelEqual(MathX.Math3D.LineToPoint(c, outVextor), 0, 6))
                {
                    collisionPoints.Add(outVextor);
                }
            }

            if (collisionPoints.Count > 0)
                return true;
            else
                return false;

        }

        public bool TriangleIntersect(Triangle triangle, out List<Vector3> collisionPoints)
        {
            collisionPoints = new List<Vector3>(3);
            Vector3 outVextor;

            Ray a, b, c;

            a = new Ray(triangle.i, triangle.j);
            b = new Ray(triangle.j, triangle.k);
            c = new Ray(triangle.k, triangle.i);

            if (RayIntersect(a, out outVextor))
            {
                if (MathX.Math3D.RelEqual(MathX.Math3D.LineToPoint(a, outVextor), 0, 6))
                {
                    collisionPoints.Add(outVextor);
                }
            }

            if (RayIntersect(b, out outVextor))
            {
                if (MathX.Math3D.RelEqual(MathX.Math3D.LineToPoint(b, outVextor), 0, 6))
                {
                    collisionPoints.Add(outVextor);
                }
            }

            if (RayIntersect(c, out outVextor))
            {
                if (MathX.Math3D.RelEqual(MathX.Math3D.LineToPoint(c, outVextor), 0, 6))
                {
                    collisionPoints.Add(outVextor);
                }
            }

            if (collisionPoints.Count > 0)
                return true;
            else
                return false;

        }

        public void CalculateNormal()
        {
            
            Vector3 p0 = j - i;
            Vector3 p1 = k - i;

            normal = Vector3.Cross(p0, p1);
            normal.Normalize();

        }

    }
}
