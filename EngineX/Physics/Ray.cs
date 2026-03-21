using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace EngineX.Physics
{
    /// <summary>
    /// Ray
    /// </summary>
    public class Ray
    {
        public enum RayType { finite, infiniteDirection, infinte }

        private RayType type;
        public RayType Type
        {
            get { return type; }
            set { type = value; }
        }

        private Vector3 origin;
        public Vector3 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        private Vector3 direction;
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        /// <summary>
        /// Build Ray
        /// </summary>
        public Ray()
        {
            type = RayType.infiniteDirection;
            origin = direction = Vector3.Empty;
        }

        /// <summary>
        /// Build Ray from points
        /// </summary>
        /// <param name="Origin"></param>
        /// <param name="Direction"></param>
        public Ray(Vector3 Origin, Vector3 Direction)
        {
            type = RayType.infiniteDirection;
            origin = Origin;
            direction = Direction;
        }

    }
}
