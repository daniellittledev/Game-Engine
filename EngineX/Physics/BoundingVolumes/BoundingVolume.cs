using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;

namespace EngineX.Physics.BoundingVolumes
{
    /// <summary>
    /// Bounding Volume
    /// </summary>
    public abstract class BoundingVolume
    {

        public abstract void Transform(Matrix transform);

        public override string ToString()
        {
            return "Bounding Volume";
        }

        #region Bounding Box Collision Detection Orientated

        /// <summary>
        /// Calculate the collision between two orientated bounding boxes
        /// </summary>
        /// <param name="BoxA">Box A</param>
        /// <param name="BoxB">Box B</param>
        /// <param name="normal">The normal of the collision towards box A</param>
        /// <param name="depth">The depth of depitration due to te collision</param>
        /// <returns>Boolean: Collision has occured</returns>
        public static bool BoundingOrientatedBoxIntersect(BoundingOrientatedBox BoxA, BoundingOrientatedBox BoxB, out Vector3 normal, out float depth)
        {
            normal = Vector3.Zero;
            depth = 0.0f;

            float p0, r0, p1, r1;
            float[] minA = new float[15], maxA = new float[15],
                    minB = new float[15], maxB = new float[15];

            // Posible Deviding Axis
            Vector3[] Axis = { 
                BoxA.XAxis, BoxA.YAxis, BoxA.ZAxis, // 1-3
                BoxB.XAxis, BoxB.YAxis, BoxB.ZAxis, // 4-6

                Vector3.Cross(BoxA.XAxis, BoxB.XAxis), // 07
                Vector3.Cross(BoxA.XAxis, BoxB.YAxis), // 08
                Vector3.Cross(BoxA.XAxis, BoxB.ZAxis), // 09
                Vector3.Cross(BoxA.YAxis, BoxB.XAxis), // 10
                Vector3.Cross(BoxA.YAxis, BoxB.YAxis), // 11
                Vector3.Cross(BoxA.YAxis, BoxB.ZAxis), // 12
                Vector3.Cross(BoxA.ZAxis, BoxB.XAxis), // 13
                Vector3.Cross(BoxA.ZAxis, BoxB.YAxis), // 14
                Vector3.Cross(BoxA.ZAxis, BoxB.ZAxis)  // 15
            };

            // 15 Tests
            for (int i = 0; i < 15; i++)
            {

                // BoxA Span Over Axis
                p0 = Vector3.Dot(Axis[i], BoxA.Position);
                r0 = Math.Abs(Vector3.Dot(Axis[i], BoxA.XAxis)) * BoxA.Extents.X +
                     Math.Abs(Vector3.Dot(Axis[i], BoxA.YAxis)) * BoxA.Extents.Y +
                     Math.Abs(Vector3.Dot(Axis[i], BoxA.ZAxis)) * BoxA.Extents.Z;

                // BoxB Span Over Axis
                p1 = Vector3.Dot(Axis[i], BoxB.Position);
                r1 = Math.Abs(Vector3.Dot(Axis[i], BoxB.XAxis)) * BoxB.Extents.X +
                     Math.Abs(Vector3.Dot(Axis[i], BoxB.YAxis)) * BoxB.Extents.Y +
                     Math.Abs(Vector3.Dot(Axis[i], BoxB.ZAxis)) * BoxB.Extents.Z;

                // Min             // Max
                minA[i] = p0 - r0; maxA[i] = p0 + r0;
                minB[i] = p1 - r1; maxB[i] = p1 + r1;

                // Span Overlap
                if (minA[i] > maxA[i] || maxA[i] < maxB[i])
                    return false;

            }

            depth = 1.0E8f;
            float d = 0.0f;

            // 15 Tests
            for (int i = 0; i < 15; i++)
            {
                float min = (minB[i] > minA[i]) ? minB[i] : minA[i];
                float max = (maxB[i] < maxA[i]) ? maxB[1] : maxA[i];

                d = max - min;

                float Alength = Axis[i].Length();

                if (Alength < 1.0E-8f)
                    continue;

                // normalise the penetration depth, are axes are not normalised
                d /= Alength;

                if (d < depth)
                {
                    depth = d;
                    normal = Vector3.Multiply(Axis[i], 1 / Alength);
                }
            }

            // make sure the normal points towards the BoxA
            Vector3 Diff = BoxB.Position - BoxA.Position;
            if (Vector3.Dot(Diff, normal) > 0.0f)
                normal *= -1.0f;

            return true;

        }

        #endregion

        #region Bounding Sphere Collision Detection

        /// <summary>
        /// Calculate the collision between two spheres
        /// </summary>
        /// <param name="SphereA">Sphere A</param>
        /// <param name="SphereB">Sphere B</param>
        /// <param name="normal">The normal of the collision towards sphere A</param>
        /// <param name="depth">The depth of depitration due to te collision</param>
        /// <param name="point">The point of intersection</param>
        /// <returns>Boolean: Collision has occured</returns>
        public static bool BoundingSphereIntersection(BoundingSphere SphereA, BoundingSphere SphereB, out Vector3 normal, out float depth, out Vector3 point)
        {
            point = Vector3.Zero;
            normal = Vector3.Zero;
            depth = 0.0f;

            // Distance
            float Distance = (float)Math.Sqrt(
                (SphereA.Centre.X - SphereB.Centre.X) * (SphereA.Centre.X - SphereB.Centre.X) +
                (SphereA.Centre.Y - SphereB.Centre.Y) * (SphereA.Centre.Y - SphereB.Centre.Y) +
                (SphereA.Centre.Z - SphereB.Centre.Z) * (SphereA.Centre.Z - SphereB.Centre.Z)) -
                (SphereA.Radius + SphereB.Radius);

            if (Distance > 0.0f)
                return false;

            // Calculate depth and normal
            depth = Math.Abs(Distance);
            normal = SphereA.Centre - SphereB.Centre;
            normal = Vector3.Normalize(normal);

            // Approx point of intersection
            point = SphereA.Centre + (normal * SphereA.Radius);

            return true;
        }

        /// <summary>
        /// Calculate the collision between two spheres
        /// </summary>
        /// <param name="SphereA">Sphere A</param>
        /// <param name="SphereB">Sphere B</param>
        /// <returns>Boolean: Collision has occured</returns>
        public static bool BoundingSphereIntersection(BoundingSphere SphereA, BoundingSphere SphereB)
        {

            // Distance
            float Distance = (float)Math.Sqrt(
                (SphereA.Centre.X - SphereB.Centre.X) * (SphereA.Centre.X - SphereB.Centre.X) +
                (SphereA.Centre.Y - SphereB.Centre.Y) * (SphereA.Centre.Y - SphereB.Centre.Y) +
                (SphereA.Centre.Z - SphereB.Centre.Z) * (SphereA.Centre.Z - SphereB.Centre.Z)) -
                (SphereA.Radius + SphereB.Radius);

            if (Distance > 0.0f)
                return false;

            return true;
        }

        #endregion

        #region Bounding Box Collision Detection

        /// <summary>
        /// Calculate the collision between two axis aligned bounding boxes
        /// </summary>
        /// <param name="BoxA">Box A</param>
        /// <param name="BoxB">Box B</param>
        /// <param name="projection">The vector representing the deth and direction the the collision</param>
        /// <returns>Boolean: Collision has occured</returns>
        public static bool BoundingBoxIntersection(BoundingBox BoxA, BoundingBox BoxB, out Vector3 penetration)
        {
            // out parameters
            penetration = Vector3.Zero;

            // local variables
            float length;

            // X Axis
            // Distance Between Boxes - Box Length /2 
            length = BoxA.Position.X - BoxB.Position.X;
            penetration.X = Math.Abs(length) - (BoxA.Extents.X + BoxB.Extents.X) * 0.5f * Math.Sign(length);
            if (penetration.X > 0)
                return false;

            // Y Axis
            // Distance Between Boxes - Box Length /2 
            length = BoxA.Position.Y - BoxB.Position.Y;
            penetration.Y = Math.Abs(length) - (BoxA.Extents.Y + BoxB.Extents.Y) * 0.5f * Math.Sign(length);
            if (penetration.Y > 0)
                return false;

            // Z Axis
            // Distance Between Boxes - Box Length /2 
            length = BoxA.Position.Z - BoxB.Position.Z;
            penetration.Z = Math.Abs(length) - (BoxA.Extents.Z + BoxB.Extents.Z) * 0.5f * Math.Sign(length);
            if (penetration.Z > 0)
                return false;

            return true;

        }

        /// <summary>
        /// Calculate the collision between two axis aligned bounding boxes
        /// </summary>
        /// <param name="BoxA">Box A</param>
        /// <param name="BoxB">Box B</param>
        /// <param name="projection">The vector representing the deth and direction the the collision</param>
        /// <param name="point">The point of intersection</param>
        /// <returns>Boolean: Collision has occured</returns>
        public static bool BoundingBoxIntersection(BoundingBox BoxA, BoundingBox BoxB, out Vector3 penetration, out Vector3 point)
        {
            // out parameters
            penetration = Vector3.Zero;
            point = Vector3.Zero;

            // local variables
            float length;

            // X Axis
            // Distance Between Boxes - Box Length /2 
            length = BoxA.Position.X - BoxB.Position.X;
            penetration.X = Math.Abs(length) - (BoxA.Extents.X + BoxB.Extents.X) * 0.5f * Math.Sign(length);
            if (penetration.X > 0)
                return false;

            // Y Axis
            // Distance Between Boxes - Box Length /2 
            length = BoxA.Position.Y - BoxB.Position.Y;
            penetration.Y = Math.Abs(length) - (BoxA.Extents.Y + BoxB.Extents.Y) * 0.5f * Math.Sign(length);
            if (penetration.Y > 0)
                return false;

            // Z Axis
            // Distance Between Boxes - Box Length /2 
            length = BoxA.Position.Z - BoxB.Position.Z;
            penetration.Z = Math.Abs(length) - (BoxA.Extents.Z + BoxB.Extents.Z) * 0.5f * Math.Sign(length);
            if (penetration.Z > 0)
                return false;

            // Average of all points inside other
            Vector3 total = Vector3.Zero;
            int count = 0;

            // Box A points
            foreach (Vector3 pointX in BoxA.Bounds)
            {
                if (BoundingBoxIntersection(BoxB, point))
                {
                    total += pointX;
                    count++;
                }
            }

            // Box B points
            foreach (Vector3 pointX in BoxB.Bounds)
            {
                if (BoundingBoxIntersection(BoxA, point))
                {
                    total += pointX;
                    count++;
                }
            }

            // Find points average
            if (count > 0)
            {
                float inverse = 1 / count;
                point.X = total.X * inverse;
                point.Y = total.Y * inverse;
                point.Z = total.Z * inverse;
            }

            return true;

        }

        /// <summary>
        /// Determindes if a point is inside an axis aligned bounding box
        /// </summary>
        /// <param name="BoxA">Box A</param>
        /// <param name="point">Point to test</param>
        /// <returns>Boolean: Point inside Box</returns>
        public static bool BoundingBoxIntersection(BoundingBox BoxA, Vector3 point)
        {

            // out parameters
            Vector3 penetration = Vector3.Zero;

            // local variables
            float length;

            // X Axis
            // Distance Between Box and point - Box Length /2 
            length = BoxA.Position.X - point.X;
            penetration.X = Math.Abs(length) - BoxA.Extents.X * 0.5f * Math.Sign(length);
            if (penetration.X > 0)
                return false;

            // Y Axis
            // Distance Between Box and point - Box Length /2 
            length = BoxA.Position.Y - point.Y;
            penetration.Y = Math.Abs(length) - BoxA.Extents.Y * 0.5f * Math.Sign(length);
            if (penetration.Y > 0)
                return false;

            // Z Axis
            // Distance Between Box and point - Box Length /2 
            length = BoxA.Position.Z - point.Z;
            penetration.Z = Math.Abs(length) - BoxA.Extents.Z * 0.5f * Math.Sign(length);
            if (penetration.Z > 0)
                return false;

            return true;

        }

        #endregion

        #region Math

        /// <summary>
        /// Vector3 Multiply
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private static Vector3 Multiply(Vector3 A, Vector3 B)
        {
            return new Vector3(A.X * B.X, A.Y * B.Y, A.Z * B.Z);
        }

        /// <summary>
        /// Vector3 Absolute
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        private static Vector3 Abs(Vector3 A)
        {
            return new Vector3(Math.Abs(A.X), Math.Abs(A.Y), Math.Abs(A.Z));
        }

        #endregion

    }
}
