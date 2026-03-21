using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DX = Microsoft.DirectX.Direct3D;

using EngineX.Physics.BoundingVolumes;

namespace EngineX.Physics.RigidBody
{

    public enum Dimention
    {
        X,
        Y,
        Z
    }

    /// <summary>
    /// Describes a pysical ridged body
    /// </summary>
    class PhysicsBody : IDisposable
    {

        // Physics
        public enum CollisionType { polyhedron, alignedBox, orientatedBox, circle, hybrid }

        private Quaternion roation;

        public Quaternion Roation
        {
            get { return roation; }
            set { roation = value; }
        }
        private Vector3 position;

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        private Matrix scale;

        protected Vector3 verlocity;
        protected Vector3 rotationVerlocity;

        private float frictionCoefficient;
        private float constantFriction;
        private float mass = 1;
        private float invMass = 1; //Inverse of mass
        private CollisionType type;

        private Polyhedron polyhedron;

        // Mesh
        private Device device;
        private Matrix renderMatrix;
        private Mesh objMesh;
        private Texture[] texture;
        private Material[] material;
        private int subsetCount;
        protected EngineX.TransformsManager transforms;

        // Extra
        private Vector3 look, up, right;

        // Collision Detection
        private BoundingBox AABB;
        private BoundingOrientatedBox OBB;
        private BoundingSphere BS;

        public Vector3 Right
        {
            get { return right; }
        }

        public Vector3 Up
        {
            get { return up; }
        }

        public Vector3 Look
        {
            get { return look; }
        }

        /// <summary>
        /// Create a new physics body
        /// </summary>
        /// <param name="device"></param>
        /// <param name="Path"></param>
        /// <param name="Mesh"></param>
        public PhysicsBody(Device device, string Path, string Mesh, EngineX.TransformsManager Transformations, CollisionType collisionType)
        {

            roation = Quaternion.Identity;
            position = Vector3.Empty;
            scale = Matrix.Identity;
            frictionCoefficient = 1;

            //Object Settings
            this.type = collisionType;
            this.device = device;
            this.transforms = Transformations;
            this.constantFriction = 0.01f;


            try
            {
                // Load mesh =====================================================================================================
                GraphicsStream outputAdjacency;
                ExtendedMaterial[] materials;
                EffectInstance[] effects;
                objMesh = DX.Mesh.FromFile(Path + Mesh,
                    MeshFlags.Managed, device, out outputAdjacency, out materials, out effects);

                // Not using effects
                effects = null;

                // Add normals if it doesn't have any
                if ((objMesh.VertexFormat & VertexFormats.PositionNormal) != VertexFormats.PositionNormal)
                {
                    Mesh tempMesh = objMesh.Clone(objMesh.Options.Value,
                        objMesh.VertexFormat | VertexFormats.PositionNormal | VertexFormats.Texture1, device);
                    tempMesh.ComputeNormals();
                    objMesh.Dispose();
                    objMesh = tempMesh;
                }
                outputAdjacency.Dispose();
                outputAdjacency = null;
                subsetCount = objMesh.GetAttributeTable().Length;

                // Extract the material properties and texture names.
                texture = new Texture[materials.Length];
                material = new Material[materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    material[i] = materials[i].Material3D;

                    // Create the texture.
                    if (materials[i].TextureFilename != null && materials[i].TextureFilename.Length > 0)
                    {
                        string temp = materials[i].TextureFilename;
                        texture[i] = TextureLoader.FromFile(device, Path + temp);
                    }
                    else
                    {
                        texture[i] = null;
                    }
                }

                // Physics ==================================================================================================

                polyhedron = new Polyhedron(objMesh);

                Vector3 centerOfMass;

                float volume;
                Matrix3 inertiaTensor;
                polyhedron.ComputeMassProperties(1.0f, out volume, out centerOfMass, out inertiaTensor);

                // Mass
                mass = 1 / volume; // Density / Volume
                invMass = volume; // 1 / mass

                // Transform vertices in original mesh to make the center of mass (0,0,0)
                CustomVertex.PositionNormalTextured[] vertList = (CustomVertex.PositionNormalTextured[])objMesh.LockVertexBuffer(
                    typeof(CustomVertex.PositionNormalTextured), LockFlags.None, objMesh.NumberVertices);
                // For each Vertex
                for (int i = 0; i < vertList.Length; i++)
                { vertList[i].Position = (vertList[i].Position - centerOfMass); }
                objMesh.UnlockVertexBuffer();

                // Transform the polyhedron as well
                polyhedron.Transform(Matrix.Translation(-1 * centerOfMass));

                // Collision Detection ======================================================================================

                switch (collisionType)
                {
                    case CollisionType.polyhedron:
                        //TODO
                        break;
                    case CollisionType.alignedBox:
                        AABB = new BoundingBox(objMesh);
                        break;
                    case CollisionType.orientatedBox:
                        OBB = new BoundingOrientatedBox(objMesh);
                        break;
                    case CollisionType.circle:
                        BS = new BoundingSphere(objMesh);
                        break;
                    case CollisionType.hybrid:
                        OBB = new BoundingOrientatedBox(objMesh);
                        BS = new BoundingSphere(objMesh);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("The mesh could not load", ex);
            }

        }

        /// <summary>
        /// Get's the mesh edges
        /// </summary>
        public int edges
        {
            get { return 0; /* this.polyhedron.edges; */ }
        }

        /// <summary>
        /// Tests for a collision and updates the body accordingly
        /// </summary>
        /// <param name="body"></param>
        public bool TestCollide(PhysicsBody body)
        {
            if (body.type != type)
                throw new Exception("Body type mismatch");

            // Friction
            // Maybe not?

            // Find position and force
            switch (type)
            {
                case CollisionType.polyhedron:
                    break;
                case CollisionType.alignedBox:
                    break;
                case CollisionType.orientatedBox:
                    break;
                case CollisionType.circle:
                    break;
                case CollisionType.hybrid:

                    // Collision Information
                    Vector3 normal;
                    Vector3 point;
                    float depth;

                    if (BoundingVolume.BoundingSphereIntersection(BS, body.BS, out normal, out depth, out point))
                    {
                        if (BoundingVolume.BoundingOrientatedBoxIntersect(OBB, body.OBB, out normal, out depth))
                        {
                            //Get Projection
                            Vector3 projection = Vector3.Multiply(normal, depth);

                            // Move
                            this.position += projection;
                            body.position -= projection;

                            // Generate Phsics Impulse

                            //TODO, GET CONTACT POINTS!!!

                            //Fake Impulse
                            this.GenerateImpulse(projection);
                            body.GenerateImpulse(-projection);

                            //this.GenerateImpulse 

                        }
                    }

                    break;
                default:
                    break;
            }
            // Move out of collision

            // Respond
            return false;
        }

        /// <summary>
        /// Tests for a collision and updates the body accordingly
        /// </summary>
        /// <param name="body"></param>
        public bool Probe(Vector3 rayStart, Vector3 rayDir, out List<Vector3> hit)
        {
            hit = new List<Vector3>();
            bool result = false;

            //Test
            foreach (Polyhedron.Face face in polyhedron.faces)
            {
                // u = point in plane , t = distance to plane, v = point in plane 

                Vector3 edge1, edge2, tvec, pvec, qvec;
                float det, inv_det;

                // Find vectors for two edges sharing vertex[0]
                edge1 = face.vertex[1].position - face.vertex[0].position;
                edge2 = face.vertex[2].position - face.vertex[0].position;

                // Begin calculation of the determinant - Used to find u value
                // Cross ray direction and edge2
                pvec = Vector3.Cross(rayDir, edge2);

                // If determint is near zero, ray lies in plane of triangle
                det = Vector3.Dot(edge1, pvec);

                // Culling Branch \\
                if (det < 0.000001f) // Epsilon
                    continue;

                // Calculate distance from vertex[0] to ray origin
                tvec = rayStart - face.vertex[0].position;

                // Calculate u value and test bounds
                float u = Vector3.Dot(tvec, pvec);
                if (u < 0.0f || u > det)
                    continue;

                // Prepare to test v value
                qvec = Vector3.Cross(tvec, edge1); //Fix

                // Calculate v value and test bounds
                float v = Vector3.Dot(rayDir, qvec);
                if (v < 0.0f || u + v > det)
                    continue;

                // Calculate t, scale values, interection is true
                float t = Vector3.Dot(edge2, qvec);
                inv_det = 1.0f / det;

                t *= inv_det;
                u *= inv_det;
                v *= inv_det;

                //Vector3 FromBarycentric(float u, float v)
                //Get Position

                hit.Add(
                    ((1 - u - v) * face.vertex[0].position) +
                    (u * face.vertex[1].position) +
                    (v * face.vertex[2].position));

                result = true;

            }
            return result;
        }

        /// <summary>
        /// Update the body
        /// </summary>
        /// <param name="elapsedTime"></param>
        public void Update(float elapsedTime)
        {
            // Physics ================================================================================================

            //Friction
            rotationVerlocity = rotationVerlocity - CalcuateFriction(rotationVerlocity);
            verlocity = verlocity - CalcuateFriction(verlocity);

            // Update the rotation
            Vector3 updateVelocity = Vector3.Multiply(rotationVerlocity, elapsedTime);
            roation *= Quaternion.RotationYawPitchRoll(
                updateVelocity.X,
                updateVelocity.Y,
                updateVelocity.Z);

            // Update the position
            position += verlocity * elapsedTime;

            // Set the Rendering matrix
            roation.Normalize();
            Matrix rotationalDisplacement = Matrix.RotationQuaternion(Quaternion.Conjugate(roation));
            renderMatrix = rotationalDisplacement * Matrix.Translation(position); //Matrix.RotationQuaternion(roation)

            // Get Vectors ============================================================================================

            //xaxis
            right.X = rotationalDisplacement.M11;
            right.Y = rotationalDisplacement.M21;
            right.Z = rotationalDisplacement.M31;

            //yaxis
            up.X = rotationalDisplacement.M12;
            up.Y = rotationalDisplacement.M22;
            up.Z = rotationalDisplacement.M32;

            //zaxis
            look.X = rotationalDisplacement.M13;
            look.Y = rotationalDisplacement.M23;
            look.Z = rotationalDisplacement.M33;

            // Normalise
            //TODO

            // Collision Detection ===================================================================================

            switch (type)
            {
                case CollisionType.polyhedron:
                    break;
                case CollisionType.alignedBox:
                    AABB.Transform(renderMatrix);
                    break;
                case CollisionType.orientatedBox:
                    OBB.Transform(renderMatrix);
                    break;
                case CollisionType.circle:
                    BS.Translate(position);
                    break;
                case CollisionType.hybrid:
                    BS.Translate(position);
                    OBB.Transform(renderMatrix);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// Friction simulation
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Vector3 CalcuateFriction(Vector3 value)
        {
            Vector3 result = Vector3.Empty;
            if (value.X != 0)
                result.X = (value.X * value.X * constantFriction) / value.X;
            if (value.Y != 0)
                result.Y = (value.Y * value.Y * constantFriction) / value.Y;
            if (value.Z != 0)
                result.Z = (value.Z * value.Z * constantFriction) / value.Z;

            return result;
        }

        /// <summary>
        /// Renders the body
        /// </summary>
        public void Render()
        {
            if (device == null || objMesh == null)
            {
                return;
            }

            // Set Transformations
            transforms.World = renderMatrix;
            transforms.SetWorld();

            // Set Effect Parameters
            // TODO: Effects

            // Call to grahics to draw
            device.VertexFormat = objMesh.VertexFormat;

            for (int i = 0; i < subsetCount; i++)
            {
                // Set the material and texture for this subset.
                if (material != null && material[i] != null)
                    device.Material = material[i];

                if (texture != null && texture[i] != null)
                    device.SetTexture(0, texture[i]);

                // Draw the mesh subset.
                objMesh.DrawSubset(i);
            }
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (objMesh != null)
            {
                objMesh.Dispose();
                objMesh = null;
            }
            material = null;
            if (texture != null)
            {
                for (int i = 0; i < texture.Length; i++)
                {
                    if (texture[i] != null)
                    {
                        texture[i].Dispose();
                        texture[i] = null;
                    }
                }
                texture = null;
            }
        }

        /// <summary>
        /// Generates a physics impulse using the bodys centre of gravity as the effect point using the objects relative rotation
        /// </summary>
        /// <param name="direction">The direction along which the force is applied</param>
        /// <param name="Force">The force in Newtons</param>
        public void GenerateImpulse(Dimention direction, float force)
        {
            // Update verlocity
            switch (direction)
            {
                case Dimention.X:
                    verlocity += (right * force) * invMass;
                    break;
                case Dimention.Y:
                    verlocity += (up * force) * invMass;
                    break;
                case Dimention.Z:
                    verlocity += (look * force) * invMass;
                    break;
                default:
                    break;
            }

            // Friction

            // Centre of Mass inertia
        }

        /// <summary>
        /// Generates a physics impulse to rotate the body
        /// </summary>
        /// <param name="direction">The direction along which the force is applied</param>
        /// <param name="Force">The force in Newtons</param>
        public void GenerateRotationImpulse(Dimention direction, float force)
        {
            // Update verlocity
            switch (direction)
            {
                case Dimention.X:
                    // Update spin
                    rotationVerlocity.X += force * invMass;
                    break;
                case Dimention.Y:
                    // Update spin
                    rotationVerlocity.Y += force * invMass;
                    break;
                case Dimention.Z:
                    // Update spin
                    rotationVerlocity.Z += force * invMass;
                    break;
                default:
                    break;
            }

            // Friction

            // Centre of Mass inertia
        }

        /// <summary>
        /// Generates a physics impulse using the bodys centre of gravity as the effect point
        /// </summary>
        /// <param name="Force">The force in Newtons</param>
        public void GenerateImpulse(Vector3 force)
        {
            // Update verlocity
            verlocity += force * invMass;

            // Friction

            // Centre of Mass inertia
        }

        /// <summary>
        /// Generates a physics impulse
        /// </summary>
        /// <param name="Position">Point of effect</param>
        /// <param name="Force">The force in Newtons</param>
        public void GenerateImpulse(Vector3 position, Vector3 force)
        {
            float distance = DistanceBetweenPoints(this.position, position);
            float torque = LineToPoint(position, force, this.position);

            // Update spin
            rotationVerlocity += (Vector3.Scale(force, distance) *
                torque * 0.5f) * invMass;

            // Update verlocity
            if (this.position != position)
            {
                float multiplyer = (float)Math.Pow(0.5f, torque);
                verlocity += Vector3.Scale(force, distance * multiplyer) * invMass;
            }
            else
            {
                verlocity += force * invMass;
            }

            // Friction

            // Centre of Mass inertia
        }

        # region Math Calculations

        /// <summary>
        /// Gets the closest distance from a ray to a point
        /// </summary>
        /// <param name="Origin">Line Start</param>
        /// <param name="Direction">Line Direction</param>
        /// <param name="Point">The point</param>
        /// <returns>Distance to the point</returns>
        private float LineToPoint(Vector3 Origin, Vector3 Direction, Vector3 Point)
        {
            Vector3 u = DevideVec(Vector3.Dot((Point - Origin), Direction), Abs(Direction));
            return DistanceBetweenPoints(Point, Abs(Point - (Origin + Multiply(Direction, u))));
        }

        /// <summary>
        /// Absolute value of a Vector3
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private Vector3 Abs(Vector3 vector)
        {
            return new Vector3(
                Math.Abs(vector.X),
                Math.Abs(vector.Y),
                Math.Abs(vector.Z));
        }

        /// <summary>
        /// Divide Vector3
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private Vector3 DevideVal(Vector3 vector, float value)
        {
            if (value == 0)
                return Vector3.Empty;

            float invVal = 1 / value;
            return new Vector3(
                vector.X * invVal,
                vector.Y * invVal,
                vector.Z * invVal);
        }

        /// <summary>
        /// Multiply Vector3
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private Vector3 Multiply(Vector3 vectorA, Vector3 vectorB)
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
        private Vector3 DevideVec(float value, Vector3 vector)
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
        private float DistanceBetweenPoints(Vector3 PointA, Vector3 PointB)
        {
            return (float)Math.Sqrt((PointA.X - PointB.X) * (PointA.X - PointB.X) +
                (PointA.Y - PointB.Y) * (PointA.Y - PointB.Y) +
                (PointA.Z - PointB.Z) * (PointA.Z - PointB.Z));
        }

        # endregion

    }
}

