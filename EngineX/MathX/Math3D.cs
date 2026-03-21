using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using System.Windows.Forms;
using System.Drawing;

namespace EngineX.MathX
{

    /// <summary>
    /// 3D Math
    /// </summary>
    public static class Math3D
    {

        /// <summary>
        /// Calculate Tangent, Binormal, Normal for a Polygon. w = width
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="pos3"></param>
        /// <param name="w0"></param>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <param name="Normal"></param>
        /// <param name="Tangent"></param>
        /// <param name="Binormal"></param>
        public static void GenerateTBN(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector2 w0, Vector2 w1, Vector2 w2, ref Vector3 Normal, ref Vector3 Tangent, ref Vector3 Binormal)
        {
            Vector3 vNorm = Vector3.Empty;

            Vector3 p0 = pos2 - pos1;
            Vector3 p1 = pos3 - pos1;

            vNorm = Vector3.Cross(p0, p1);
            vNorm.Normalize();

            float x1 = (float)(pos2.X - pos1.X);
            float x2 = (float)(pos3.X - pos1.X);
            float y1 = (float)(pos2.Y - pos1.Y);
            float y2 = (float)(pos3.Y - pos1.Y);
            float z1 = (float)(pos2.Z - pos1.Z);
            float z2 = (float)(pos3.Z - pos1.Z);

            float s1 = (float)(w1.X - w0.X);
            float s2 = (float)(w2.X - w0.X);
            float t1 = (float)(w1.Y - w0.Y);
            float t2 = (float)(w2.Y - w0.Y);

            float r = (float)((s1 * t2) - (s2 * t1));
            if (r == 0)
            {
                r = 1F;
            }
            else
            {
                r = 1 / r;
            }

            Vector3 sDir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tDir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            Vector3 vTan = sDir;
            Vector3 vBi = tDir;

            vTan = (vTan - vNorm * Vector3.Dot(vNorm, vTan));
            vTan.Normalize();

            if (Vector3.Dot(Vector3.Cross(vNorm, vTan), vBi) < 0)
            {
                vTan.Scale(-1);
            }

            vBi = Vector3.Cross(vTan, vNorm);
            vBi.Normalize();

            Normal = vNorm;
            Tangent = vTan;
            Binormal = vBi;
        }

        /// <summary>
        /// Calculate Polygon Normal. p = position
        /// </summary>
        /// <param name="p0">Vertex One</param>
        /// <param name="p1">Vertex Two</param>
        /// <param name="p2">Vertex Three</param>
        /// <returns></returns>
        public static Vector3 GenerateNormal(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3 vNorm = Vector3.Empty;

            Vector3 v0 = p1 - p0;
            Vector3 v1 = p2 - p0;

            vNorm = Vector3.Cross(v0, v1);
            vNorm.Normalize();

            return vNorm;
        }

        /// <summary>
        /// Approximate Equals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static bool RelEqual(float a, float b, int decimals)
        {
            if (a == b) return true;
            if (a == 0) return (Math.Round(b, decimals) == 0);
            if (b == 0) return (Math.Round(a, decimals) == 0);
            return (Math.Round(a / b, decimals) == 1);
        }

        /// <summary>
        /// Gets a point on a 3D Sphere
        /// </summary>
        /// <param name="radius">Radius of the sphere</param>
        /// <param name="angleXZ">Angle on the XZ plane</param>
        /// <param name="angleY">Angle on the vertical plane</param>
        /// <returns>Point on the shpere</returns>
        public static Vector3 PointOnSphere(float radius, float angleXZ, float angleY)
        {

            Vector3 result;
            float newRadius = (float)Math.Cos(angleY) * radius;

            result.Y = (float)Math.Sin(angleY) * radius;
            result.X = (float)(Math.Cos(angleXZ) * newRadius);
            result.Z = (float)(Math.Sin(angleXZ) * newRadius);

            return result;
        }

        /// <summary>
        /// Interpolate Color 
        /// </summary>
        /// <param name="c1">Color one</param>
        /// <param name="c2">Color two</param>
        /// <param name="f">Factor from: 0.0f to 1.0f</param>
        /// <returns>The interpolated color</returns>
        public static System.Drawing.Color Lerp(System.Drawing.Color c1, System.Drawing.Color c2, float f)
        {
            byte a = (byte)(c1.A + (c2.A - c1.A) * f);
            byte r = (byte)(c1.R + (c2.R - c1.R) * f);
            byte g = (byte)(c1.G + (c2.G - c1.G) * f);
            byte b = (byte)(c1.B + (c2.B - c1.B) * f);
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// interpolate float
        /// </summary>
        /// <param name="f1">Float one</param>
        /// <param name="f2">Float two</param>
        /// <param name="f">Factor from: 0.0f to 1.0f</param>
        /// <returns>The interpolated float</returns>
        public static float Lerp(float f1, float f2, float f)
        {
            return f1 + (f2 - f1) * f;
        }

        /// <summary>
        /// Gets the closest distance from a ray to a point
        /// </summary>
        /// <param name="Origin">Line Start</param>
        /// <param name="Direction">Line Direction</param>
        /// <param name="Point">The point</param>
        /// <returns>Distance to the point</returns>
        public static float LineToPoint(Vector3 Origin, Vector3 Direction, Vector3 Point)
        {
            Vector3 u = DevideVec(Vector3.Dot((Point - Origin), Direction), Abs(Direction));
            return DistanceBetweenPoints(Point, Point - (Origin + Multiply(Direction, u))); // Taken abs from second param
        }

        /// <summary>
        /// Gets the closest distance from a ray to a point
        /// </summary>
        /// <param name="Origin">Line Start</param>
        /// <param name="Direction">Line Direction</param>
        /// <param name="Point">The point</param>
        /// <returns>Closes point to the point</returns>
        public static float LineToPoint(Physics.Ray ray, Vector3 Point)
        {

            Vector3 AP = Point - ray.Origin;
            Vector3 AB = ray.Direction - ray.Origin;

            float ab2 = AB.X * AB.X + AB.Y * AB.Y + AB.Z * AB.Z;
            float ap_ab = AP.X * AB.X + AP.Y * AB.Y + AP.Z * AB.Z;
            float t = ap_ab / ab2;

            switch (ray.Type)
            {
                case EngineX.Physics.Ray.RayType.finite:
                    if (t < 0.0f) t = 0.0f;
                    else if (t > 1.0f) t = 1.0f;
                    break;
                case EngineX.Physics.Ray.RayType.infiniteDirection:
                    if (t < 0.0f) t = 0.0f;
                    break;
            }

            return DistanceBetweenPoints(ray.Origin + AB * t, Point);
        }

        /// <summary>
        /// Gets the closest distance from a ray to a point
        /// </summary>
        /// <param name="Origin">Line Start</param>
        /// <param name="Direction">Line Direction</param>
        /// <param name="Point">The point</param>
        /// <returns>Distance to the point</returns>
        public static Vector3 LineToPointClosestPoint(Physics.Ray ray, Vector3 Point)
        {

            Vector3 AP = Point - ray.Origin;
            Vector3 AB = ray.Direction - ray.Origin;

            float ab2 = AB.X * AB.X + AB.Y * AB.Y + AB.Z * AB.Z;
            float ap_ab = AP.X * AB.X + AP.Y * AB.Y + AP.Z * AB.Z;
            float t = ap_ab / ab2;

            switch (ray.Type)
            {
                case EngineX.Physics.Ray.RayType.finite:
                    if (t < 0.0f) t = 0.0f;
                    else if (t > 1.0f) t = 1.0f;
                    break;
                case EngineX.Physics.Ray.RayType.infiniteDirection:
                    if (t < 0.0f) t = 0.0f;
                    break;
            }

            return ray.Origin + AB * t;
        }

        /// <summary>
        /// Absolute value of a Vector3
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Abs(Vector3 vector)
        {
            return new Vector3(
                Math.Abs(vector.X),
                Math.Abs(vector.Y),
                Math.Abs(vector.Z));
        }

        /// <summary>
        /// Multiply Vector3
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Multiply(Vector3 vectorA, Vector3 vectorB)
        {
            return new Vector3(
                vectorA.X * vectorB.X,
                vectorA.Y * vectorB.Y,
                vectorA.Z * vectorB.Z);
        }

        /// <summary>
        /// Divide Vector3
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 DevideVec(float value, Vector3 vector)
        {
            Vector3 result = Vector3.Empty;
            if (vector.X != 0)
                result.X = value / vector.X;
            if (vector.Y != 0)
                result.Y = value / vector.Y;
            if (vector.Z != 0)
                result.Z = value / vector.Z;

            return result;
        }


        /// <summary>
        /// Gets the distance between two points in 3D space
        /// </summary>
        /// <param name="PointA"></param>
        /// <param name="PointB"></param>
        /// <returns></returns>
        public static float DistanceBetweenPoints(Vector3 PointA, Vector3 PointB)
        {
            return (float)Math.Sqrt((PointA.X - PointB.X) * (PointA.X - PointB.X) +
                (PointA.Y - PointB.Y) * (PointA.Y - PointB.Y) +
                (PointA.Z - PointB.Z) * (PointA.Z - PointB.Z));
        }

    }


    public static class Math2D
    {

        public const float Epsilon = 0.0000000001f;
        public const float FloatElipsonPlusOne = 1 + Epsilon;
        public const float NegitiveElipson = -Epsilon;

        public static bool IndideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 pointT)
        {
        
            // Compute vectors        
		    Vector2 v0 = C - A;
		    Vector2 v1 = B - A;
            Vector2 v2 = pointT - A;
    		
		    // Compute dot products
            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);
    		
		    // Compute barycentric coordinates
		    float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
		    float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
		    float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
    		
		    // Check if point is in triangle
            return (u > NegitiveElipson) && (v > NegitiveElipson) && (u + v < FloatElipsonPlusOne);

        }

    }
}
