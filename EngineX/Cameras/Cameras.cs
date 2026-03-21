using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;

namespace EngineX
{

    namespace Cameras
    {

        /// <summary>
        /// Camera Base
        /// </summary>
        public abstract class Camera
        {

            /// <summary>
            /// Rendering Matrices
            /// </summary>
            protected TransformsManager Transforms;

            /// <summary>
            /// Camera View Matrix.
            /// </summary>
            protected Matrix ViewMatrix;

            /// <summary>
            /// Camera Projection Matrix
            /// </summary>
            protected Matrix ProjectionMatrix;


            /// <summary>
            /// Camera Right
            /// </summary>
            protected Vector3 Right;
            /// <summary>
            /// Camera Up
            /// </summary>
            protected Vector3 Up;
            /// <summary>
            /// Camera Look
            /// </summary>
            protected Vector3 Look;
            /// <summary>
            /// Camera Position
            /// </summary>
            protected Vector3 Position;
            /// <summary>
            /// Camera Look in world space.
            /// </summary>
            protected Vector3 LookAt;

            /// <summary>
            /// Camera Look in world space.
            /// </summary>
            public Vector3 LookAtPos
            {
                get { return Position + Look * FarPlane; }
            }

            /// <summary>
            /// Camera field of view in radians.
            /// </summary>
            private float FieldOfView = (float)System.Math.PI / 4.0f;
            /// <summary>
            /// Screen ascpect ratio.
            /// </summary>
            private float AspectRatio = 1.33f;
            /// <summary>
            /// Near clipping plane
            /// </summary>
            private float NearPlane = 0.1f;
            /// <summary>
            /// Far clipping plane;
            /// </summary>
            private float FarPlane = 1000.0f;


            /// <summary>
            /// Camera field of view in radians.
            /// </summary>
            public float ProjFieldOfView
            {
                get { return FieldOfView; }
                set { FieldOfView = value; 
                      RefreshProjection();}
            }
            /// <summary>
            /// Screen ascpect ratio.
            /// </summary>
            public float ProjAspectRatio
            {
                get { return AspectRatio; }
                set
                {
                    AspectRatio = value;
                    RefreshProjection();
                }
            }
            /// <summary>
            /// Near clipping plane
            /// </summary>
            public float ProjNearPlane
            {
                get { return NearPlane; }
                set
                {
                    NearPlane = value;
                    RefreshProjection();
                }
            }
            /// <summary>
            /// Far clipping plane;
            /// </summary>
            public float ProjFarPlane
            {
                get { return FarPlane; }
                set
                {
                    FarPlane = value;
                    RefreshProjection();
                }
            }


            /// <summary>
            /// Rebuild the projection matrix with new parameters
            /// </summary>
            /// <param name="fieldOfView"></param>
            /// <param name="aspectRatio"></param>
            /// <param name="nearPlane"></param>
            /// <param name="farPlane"></param>
            public void RefreshFieldOfView(float fieldOfView)
            {

                FieldOfView = fieldOfView;
                RefreshProjection();
            }

            /// <summary>
            /// View Frustum Corners.
            /// </summary>
            private Vector3[] FrustumCorners;

            /// <summary>
            /// View Frustum Planes
            /// </summary>
            private Plane[] FrustumPlanes;
           
            /// <summary>
            /// Initilize the camera.
            /// </summary>
            /// <param name="TransformsManager"></param>
            public Camera( ref TransformsManager TransformsManager)
            {

                Transforms = TransformsManager;

                ViewMatrix = Matrix.Identity;
                RefreshProjection();
                
                Up = new Vector3(0.0f, 1.0f, 0.0f);
                Position = new Vector3(0.0f, 1.0f, 0.0f);
                Look = new Vector3(0.0f, 1.0f, 1.0f);

                FrustumCorners = new Vector3[8];
                FrustumPlanes = new Plane[6];

                Render();
            }

            /// <summary>
            /// Rebuild the projection matrix with new parameters
            /// </summary>
            /// <param name="fieldOfView"></param>
            /// <param name="aspectRatio"></param>
            /// <param name="nearPlane"></param>
            /// <param name="farPlane"></param>
            public void RefreshProjectionMatrix(float fieldOfView, float aspectRatio, float nearPlane, float farPlane)
            {

                FieldOfView = fieldOfView;
                AspectRatio = aspectRatio;
                NearPlane = nearPlane;
                FarPlane = farPlane;

                RefreshProjection();
            }

            /// <summary>
            /// Rebuild the projection matrix.
            /// </summary>
            private void RefreshProjection()
            {
                ProjectionMatrix = Matrix.PerspectiveFovLH(FieldOfView, AspectRatio, NearPlane, FarPlane);
                Transforms.Projection = ProjectionMatrix;
                Transforms.SetProjection();
            }

            /// <summary>
            /// Rebuild the projection matrix.
            /// </summary>
            public void RefreshProjectionMatrix()
            {
                Transforms.Projection = ProjectionMatrix;
                Transforms.SetProjection();
            }

            /// <summary>
            /// Test is a point is inside the view frustum.
            /// </summary>
            /// <param name="unitToCheck"></param>
            /// <param name="Radius"></param>
            /// <returns></returns>
            public bool ViewTestPoint(Vector3 unitToCheck, float Radius)
            {
                foreach (Plane plane in FrustumPlanes)
                {
                    if (plane.A * unitToCheck.X + plane.B * unitToCheck.Y + plane.C * unitToCheck.Z + plane.D <= (-Radius))
                        return false;
                }

                return true;
            }

            /// <summary>
            /// Test is a point is inside the view frustum.
            /// </summary>
            /// <param name="unitToCheck"></param>
            /// <param name="Radius"></param>
            /// <returns></returns>
            public bool ViewTestPoint2(Vector3 unitToCheck, float Radius)
            {
                float distance = 0.0f;

                foreach (Plane plane in FrustumPlanes)
                {
                    distance = Plane.DotCoordinate(plane, unitToCheck);

                    if (distance <= (-Radius))
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Test is a box is inside the view frustum.
            /// </summary>
            /// <param name="Min"></param>
            /// <param name="Max"></param>
            /// <param name="Bias"></param>
            /// <returns></returns>
            public bool ViewTestBox(Vector3 Min, Vector3 Max, float Bias)
            {
                Vector3 Center = Vector3.Add(Min, Max);
                Center *= (1.0f / 2);
                Vector3 HalfDiag = Vector3.Subtract(Max, Center);

                float M = 0F;
                float N = 0F;

                foreach (Plane plane in FrustumPlanes)
                {
                    M = (Center.X * plane.A) + (Center.Y * plane.B) + (Center.Z * plane.C) + plane.D;
                    N = (HalfDiag.X * plane.A) + (HalfDiag.Y * plane.B) + (HalfDiag.Z * plane.C);
                    if (M + N <= -Bias)
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Test is a polygon is inside the view frustum.
            /// </summary>
            /// <param name="V"></param>
            /// <param name="VCount"></param>
            /// <returns></returns>
            public bool ViewTestPoly(Vector3[] V, int VCount)
            {

                foreach (Plane plane in FrustumPlanes)
                {
                    bool bIn = false;
                    int J = 0;

                    for (J = 0; J < VCount; J++)
                    {
                        if (Plane.DotCoordinate(plane, V[J]) > 0)
                        {
                            bIn = true;
                        }
                    }

                    if (!bIn)
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Build the camera virtual view frustum for object visablility tests.
            /// </summary>
            public void ComputeViewFrustum()
            {
                Matrix matrix = ViewMatrix * ProjectionMatrix;
                matrix = Matrix.Invert(matrix);

                FrustumCorners[0] = new Vector3(-1.0f, -1.0f, 0.0f); // xyz
                FrustumCorners[1] = new Vector3(1.0f, -1.0f, 0.0f); // Xyz
                FrustumCorners[2] = new Vector3(-1.0f, 1.0f, 0.0f); // xYz
                FrustumCorners[3] = new Vector3(1.0f, 1.0f, 0.0f); // XYz
                FrustumCorners[4] = new Vector3(-1.0f, -1.0f, 1.0f); // xyZ
                FrustumCorners[5] = new Vector3(1.0f, -1.0f, 1.0f); // XyZ
                FrustumCorners[6] = new Vector3(-1.0f, 1.0f, 1.0f); // xYZ
                FrustumCorners[7] = new Vector3(1.0f, 1.0f, 1.0f); // XYZ

                for (int i = 0; i < FrustumCorners.Length; i++)
                    FrustumCorners[i] = Vector3.TransformCoordinate(FrustumCorners[i], matrix);

                // Calculate the planes
                FrustumPlanes[0] = Plane.FromPoints(FrustumCorners[0], FrustumCorners[1], FrustumCorners[2]); // Near
                FrustumPlanes[1] = Plane.FromPoints(FrustumCorners[6], FrustumCorners[7], FrustumCorners[5]); // Far
                FrustumPlanes[2] = Plane.FromPoints(FrustumCorners[2], FrustumCorners[6], FrustumCorners[4]); // Left
                FrustumPlanes[3] = Plane.FromPoints(FrustumCorners[7], FrustumCorners[3], FrustumCorners[5]); // Right
                FrustumPlanes[4] = Plane.FromPoints(FrustumCorners[2], FrustumCorners[3], FrustumCorners[6]); // Top
                FrustumPlanes[5] = Plane.FromPoints(FrustumCorners[1], FrustumCorners[0], FrustumCorners[4]); // Bottom
            }

            /// <summary>
            /// Retrieve or set view matrix.
            /// </summary>
            public Matrix View
            {
                get { return ViewMatrix; }
                set { ViewMatrix = value; }
            }

            /// <summary>
            /// Retrieve or set view matrix.
            /// </summary>
            public virtual Vector3 Location
            {
                get { return Position; }
                set { Position = value; }
            }

            /// <summary>
            /// Retrieve last built Projection matrix.
            /// </summary>
            public Matrix Projection
            {
                get { return ProjectionMatrix; }
            }

            /// <summary>
            /// Update the camera heading.
            /// </summary>
            /// <param name="Factor"></param>
            public abstract void UpdateHeading(Single Factor);
            /// <summary>
            /// Update the camera roll.
            /// </summary>
            /// <param name="Factor"></param>
            public abstract void UpdateRoll(Single Factor);
            /// <summary>
            /// Update the camera pitch.
            /// </summary>
            /// <param name="Factor"></param>
            public abstract void UpdatePitch(Single Factor);

            /// <summary>
            /// Update the camera and appy the view matrix to the scene.
            /// </summary>
            public abstract void Render();
        }

        /// <summary>
        /// Direct Access Camera
        /// </summary>
        public class CameraDA : Camera
        {
            
            /// <summary>
            /// Initilize the camera.
            /// </summary>
            /// <param name="TransformsManager"></param>
            public CameraDA(ref TransformsManager TransformsManager)
                : base(ref TransformsManager) { }

            /// <summary>
            /// Not implimented for AD
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateHeading(float Factor)
            {
                throw new Exception("Not Implimented");
            }

            /// <summary>
            /// Not implimented for AD
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateRoll(float Factor)
            {
                throw new Exception("Not Implimented");
            }

            /// <summary>
            /// Not implimented for AD
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdatePitch(float Factor)
            {
                throw new Exception("Not Implimented");
            }

            /// <summary>
            /// Sets the orientation and position of the camera
            /// </summary>
            /// <param name="Look">Normal: Camera direction</param>
            /// <param name="Up">Normal: Camera up</param>
            /// <param name="Right">Normal: Camera right</param>
            /// <param name="Position">Vector Position</param>
            public void Update(Vector3 Look, Vector3 Up, Vector3 Right, Vector3 Position)
            {

                this.Look = Look;
                this.Up = Up;
                this.Right = Right;
                this.Position = Position;

                LookAt = Look + Position;

            }

            /// <summary>
            /// Update the camera and appy the view matrix to the scene.
            /// </summary>
            public override void Render()
            {

                //Render
                ViewMatrix = Matrix.LookAtLH(Position, LookAt, Up);

                Transforms.View = ViewMatrix;
                Transforms.SetView();

                ComputeViewFrustum();

            }

        }

        /// <summary>
        /// First Person Camera
        /// </summary>
        public class CameraFP : Camera
        {
            /// <summary>
            /// Camera rotation around the vector (0,1,0) : world up.
            /// </summary>
            public float Heading = 0;

            /// <summary>
            /// Camera rotation around the vector (1,0,0) : world left.
            /// </summary>
            public float Pitch = 0;

            /// <summary>
            /// Camera rotation around the vector (0,0,1) : world forward.
            /// </summary>
            public float Roll = 0;

            /// <summary>
            /// The next psoition to move to
            /// </summary>
            private Vector3 NewPos;

            /// <summary>
            /// Ground Height
            /// </summary>
            private float GroundHeight;

            /// <summary>
            /// The hieght of the camera from the ground.
            /// </summary>
            private float StandHeight = 10;

            /// <summary>
            /// Initilize the camera.
            /// </summary>
            /// <param name="TransformsManager"></param>
            public CameraFP(ref TransformsManager TransformsManager)
                : base(ref TransformsManager) { }

            /// <summary>
            /// The hieght of the camera from the ground.
            /// </summary>
            private float StandingHeight
            {
                get { return StandHeight; }
                set { StandHeight = value; }
            }

            /// <summary>
            /// Update the camera heading.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateHeading(Single Factor)
            {
                Heading -= Factor;

                if (Heading > Constants.DoublePI)
                {
                    Heading = Heading - (float)Constants.DoublePI;
                }
                else if (Heading < 0)
                {
                    Heading = (float)Constants.DoublePI + Heading;
                }
            }

            /// <summary>
            /// Update the camera roll.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateRoll(Single Factor)
            {
                Roll -= Factor;

                if (Roll > Constants.DoublePI)
                {
                    Roll = Roll - (float)Constants.DoublePI;
                }
                else if (Roll < 0)
                {
                    Roll = (float)Constants.DoublePI + Roll;
                }
            }

            /// <summary>
            /// Update the camera pitch.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdatePitch(Single Factor)
            {
                
                Pitch -= Factor;

                if (Pitch > Constants.HalfPI)
                {
                    Pitch = 1.57f;
                }
                else if (Pitch < -Constants.HalfPI)
                {
                    Pitch = -1.57f;
                }
            }

            /// <summary>
            /// Move the camera relativly forwards or backwards.
            /// </summary>
            /// <param name="Factor"></param>
            /// <param name="ElapsedTime"></param>
            public void Move(Single ForwardFactor, Single StrafeFactor, double ElapsedTime)
            {
                Vector3 Forward = new Vector3();
                Forward.X = (float)System.Math.Cos(Heading);
                Forward.Z = (float)System.Math.Sin(Heading);
                Look = Vector3.Normalize(Look);

                NewPos = Position + Forward * (ForwardFactor * (float)ElapsedTime);
                NewPos += new Vector3(Right.X, 0, Right.Z) * (StrafeFactor * (float)ElapsedTime);
                
                NewPos.Y = Position.Y;

                Time = (float)ElapsedTime;
            
            }

          
            /// <summary>
            /// Set the hight of the ground and calculate real position
            /// </summary>
            /// <param name="Height"></param>
            public void Update(float groundHeight)
            {
                GroundHeight = groundHeight;
                HeightInAir = Position.Y - GroundHeight;

                //Console.WriteLine(HeightInAir);

                Vector3 Diff = NewPos - Position;
                if (HeightInAir < 0.5f & Verlocity.Y < 30)
                {////ON GROUND////

                    Jumped = false;
                    //Move
                    Vector3 pos = NewPos;
                    pos.Y = groundHeight;
                    pos = pos - Position;
                    pos = Vector3.Normalize(pos);

                    //Console.WriteLine(Diff.Length());

                    pos *= Diff.Length();

                    Position += pos;

                    Verlocity.X = pos.X / Time;
                    Verlocity.Y = 0;//pos.Y / Time;
                    Verlocity.Z = pos.Z / Time;
                    //Console.WriteLine("On Ground " + Verlocity.X + " : " + Verlocity.Z);

                    Position.Y = groundHeight;
                                        
                }
                else
                {////IN AIR////
                    
                    //Move
                    //Position.Y = 0.05f;
                    Verlocity.Y -= Gravity * Time;
                    //Verlocity += Vector3.Scale(Diff, 0.1f);
                    Position += Verlocity * Time;
                    //Console.WriteLine("In Air");
                    
                }

                if (Position.Y - GroundHeight < 0)
                {
                    Position.Y = groundHeight;
                }



            }

            /// <summary>
            /// Makes the Camera Jump
            /// </summary>
            public void Jump()
            {

                if (Jumped == false)
                {
                    Verlocity.Y += JumpPower;
                    Jumped = true;
                }
            
            }

            /// <summary>
            /// Threshold
            /// </summary>
            const float GroundExtent = -0.1f;

            /// <summary>
            /// Camera jump status
            /// </summary>
            private bool Jumped = false ;

            ///// <summary>
            ///// Camera on ground status
            ///// </summary>
            //private bool OnGround = true;

            /// <summary>
            /// The gap from the ground to the bottom of the camera stand.
            /// </summary>
            private float HeightInAir = 0;

            /// <summary>
            /// Gravity
            /// </summary>
            private float Gravity = 99.0f;

            /// <summary>
            /// Jump Power
            /// </summary>
            private float JumpPower = 100;

            /// <summary>
            /// Last Elpsed time
            /// </summary>
            private float Time;

            /// <summary>
            /// Flight Verlocity
            /// </summary>
            private Vector3 Verlocity;

            /// <summary>
            /// Update the camera and appy the view matrix to the scene.
            /// </summary>
            public override void Render()
            {

                //Update
                Look.Y = (float)Math.Sin(Pitch);
                Single temp = (float)Math.Cos(Pitch);
                Look.X = (float)Math.Cos(Heading) * temp;
                Look.Z = (float)Math.Sin(Heading) * temp;
                Look = Vector3.Normalize(Look);

                //Roll
                Up.X = (float)Math.Sin(Roll);
                Up.Y = (float)Math.Cos(Roll);
                Up.Z = 0;

                Right = Vector3.Cross(Look, Up);
                Right = Vector3.Normalize(Right);

                CamPos = Position;
                CamPos.Y += StandHeight;

                LookAt = CamPos + Look;

                //Render
                ViewMatrix = Matrix.LookAtLH(CamPos, LookAt, Up);

                Transforms.View = ViewMatrix;
                Transforms.SetView();

                ComputeViewFrustum();

            }

            Vector3 CamPos;

            public override Vector3 Location
            {
                get { return CamPos; }
                set { Position = value; }
            }


        }

        /// <summary>
        /// First Person Spectator Camera
        /// </summary>
        public class CameraFPS : Camera
        {
            /// <summary>
            /// Camera rotation around the vector (0,1,0) : world up.
            /// </summary>
            public float Heading = 0;

            /// <summary>
            /// Camera rotation around the vector (1,0,0) : world left.
            /// </summary>
            public float Pitch = 0;

            /// <summary>
            /// Camera rotation around the vector (0,0,1) : world forward.
            /// </summary>
            public float Roll = 0;

            /// <summary>
            /// Initilize the camera.
            /// </summary>
            /// <param name="TransformsManager"></param>
            public CameraFPS( ref TransformsManager TransformsManager)
                : base( ref TransformsManager){}

            /// <summary>
            /// Update the camera heading.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateHeading(Single Factor)
            {
                Heading -= Factor;

                if (Heading > Constants.DoublePI)
                {
                    Heading = Heading - (float)Constants.DoublePI;
                }
                else if (Heading < 0)
                {
                    Heading = (float)Math.PI + Heading;
                }
            }

            /// <summary>
            /// Update the camera roll.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateRoll(Single Factor)
            { 
                Roll -= Factor;

                if (Roll > Constants.DoublePI)
                {
                    Roll = Roll - (float)Constants.DoublePI;
                }
                else if (Roll < 0)
                {
                    Roll = (float)Constants.DoublePI + Roll;
                }
            }

            /// <summary>
            /// Update the camera pitch.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdatePitch(Single Factor)
            { 
                Pitch -= Factor;

                if (Pitch > Constants.DoublePI)
                {
                    Pitch = Pitch - (float)Constants.DoublePI;
                }
                else if (Pitch < 0)
                {
                    Pitch = (float)Constants.DoublePI + Pitch;
                }
            }
            

            /// <summary>
            /// Move the camera relativly forwards or backwards.
            /// </summary>
            /// <param name="Factor"></param>
            /// <param name="ElapsedTime"></param>
            public void UpdateDistMain(Single Factor, double ElapsedTime)
            {Position += Look * (Factor * (float)ElapsedTime);}

            /// <summary>
            /// Move the camera relativly left or right.
            /// </summary>
            /// <param name="Factor"></param>
            /// <param name="ElapsedTime"></param>
            public void UpdateDistStrafe(Single Factor, double ElapsedTime)
            {Position += Right * (Factor * (float)ElapsedTime);}

            /// <summary>
            /// Move the camera relativly up or down.
            /// </summary>
            /// <param name="Factor"></param>
            /// <param name="ElapsedTime"></param>
            public void UpdateDistHeight(Single Factor, double ElapsedTime)
            {Position += Up * (Factor * (float)ElapsedTime);}

            /// <summary>
            /// Update the camera and appy the view matrix to the scene.
            /// </summary>
            public override void Render()
            {

                //Update
                Look.Y = (float)Math.Sin(Pitch);
                Single temp = (float)Math.Cos(Pitch);
                Look.X = (float)Math.Cos(Heading) * temp;
                Look.Z = (float)Math.Sin(Heading) * temp;
                Look = Vector3.Normalize(Look);

                //Roll
                Up.X = (float)Math.Sin(Roll);
                Up.Y = (float)Math.Cos(Roll);
                Up.Z = 0;

                Right = Vector3.Cross(Look, Up);
                Right = Vector3.Normalize(Right);

                LookAt = Position + Look;

                //Render
                View = Matrix.LookAtLH(Position, LookAt, Up);
                Transforms.View = View;
                Transforms.SetView();

                ComputeViewFrustum();

            }

        }

        /// <summary>
        /// First Person Camera
        /// </summary>
        public class CameraFS : Camera
        {

            /// <summary>
            /// Rotaion Matrix
            /// </summary>
            private Matrix CamRotationM;
            /// <summary>
            /// Rotation Quaternion
            /// </summary>
            private Quaternion CamRotationQ;
            /// <summary>
            /// World Up Vector.
            /// </summary>
            private Vector3 WorldUp;

            /// <summary>
            /// Initilize the camera.
            /// </summary>
            /// <param name="TransformsManager"></param>
            public CameraFS(ref TransformsManager TransformsManager)
                : base(ref TransformsManager)
            {
                CamRotationM = Matrix.Identity;
                CamRotationQ = Quaternion.Identity;

                WorldUp = new Vector3(0, 1, 0);
            }

            /// <summary>
            /// World Up
            /// </summary>
            public Vector3 worldUp
            {
                get { return WorldUp; }
                set { WorldUp = value; }
            }

            /// <summary>
            /// Levels Camera
            /// </summary>
            public void CameraLevel(double ElapsedTime)
            {
                float  dotProduct;//, angle;
                Vector3 worldXAxis;
                Vector3 perpAxis;

                // Determine world x-axis relative to camera's orientation.
                worldXAxis = Vector3.Cross(WorldUp, Look);//, /
                worldXAxis = Vector3.Normalize(worldXAxis);

                // Determine shortest rotation to level camera with horizon.
                perpAxis = Vector3.Cross(Right, worldXAxis);
                //angle = (float)Math.Acos(Vector3.Dot(Right, worldXAxis));
                //angle = (float)RadiansToDegrees(angle);
                dotProduct = Vector3.Dot(Look, perpAxis);

                // Camera is already close enough to being level with horizon!
                if (dotProduct < 0.01f & dotProduct > -0.01f) //dotProduct
                    return;

                if (dotProduct > 0.0f)
                {
                    // Counterclockwise rotation about camera z-axis will level camera.
                    UpdateRoll((float)ElapsedTime * dotProduct); // or just dotProduct
                }
                else
                {
                    // Clockwise rotation about camera z-axis will level camera.
                    UpdateRoll((float)ElapsedTime * dotProduct); // or just dotProduct
                }
            }

            /// <summary>
            /// Update the camera heading.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateHeading(float Factor)
            {CamRotationQ = Quaternion.Multiply(CamRotationQ,
                    Quaternion.RotationAxis(Up, Factor));}

            /// <summary>
            /// Update the camera roll.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateRoll(float Factor)
            {CamRotationQ = Quaternion.Multiply(CamRotationQ,
                    Quaternion.RotationAxis(Look, Factor));}

            /// <summary>
            /// Update the camera pitch.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdatePitch(float Factor)
            {CamRotationQ = Quaternion.Multiply(CamRotationQ,
                    Quaternion.RotationAxis(Right, Factor));}

            /// <summary>
            /// Move the camera relativly forwards or backwards.
            /// </summary>
            /// <param name="Factor"></param>
            /// <param name="ElapsedTime"></param>
            public void UpdateDistMain(float Factor, double ElapsedTime)
            {Position += Look * (Factor * (float)ElapsedTime);}

            /// <summary>
            /// Move the camera relativly left or right.
            /// </summary>
            /// <param name="Factor"></param>
            /// <param name="ElapsedTime"></param>
            public void UpdateDistStrafe(float Factor, double ElapsedTime)
            {Position += Right * (Factor * (float)ElapsedTime);}

            /// <summary>
            /// Move the camera relativly up or down.
            /// </summary>
            /// <param name="Factor"></param>
            /// <param name="ElapsedTime"></param>
            public void UpdateDistHeight(float Factor, double ElapsedTime)
            {Position += Up * (Factor * (float)ElapsedTime);}

            /// <summary>
            /// Render the camera and appy the view matrix to the scene.
            /// </summary>
            public override void Render()
            {


                //Update
                CamRotationM = Matrix.RotationQuaternion(
                Quaternion.Conjugate(CamRotationQ));

                CamRotationQ = Quaternion.Normalize(CamRotationQ);

                //Matrix.LookAtRH - Define
                //zaxis = normal(cameraPosition - cameraTarget)
                //xaxis = normal(cross(cameraUpVector, zaxis))
                //yaxis = cross(zaxis, xaxis)

                // xaxis.x           yaxis.x           zaxis.x          0
                // xaxis.y           yaxis.y           zaxis.y          0
                // xaxis.z           yaxis.z           zaxis.z          0
                //-dot(xaxis, cameraPosition)  -dot(yaxis, cameraPosition)  -dot(zaxis, cameraPosition)  1

                //xaxis
                Right.X = CamRotationM.M11;
                Right.Y = CamRotationM.M21;
                Right.Z = CamRotationM.M31;

                //yaxis
                Up.X = CamRotationM.M12;
                Up.Y = CamRotationM.M22;
                Up.Z = CamRotationM.M32;

                //zaxis
                Look.X = CamRotationM.M13;
                Look.Y = CamRotationM.M23;
                Look.Z = CamRotationM.M33;

                View = Matrix.Translation(-Position) * CamRotationM;

                //Render
                Transforms.View = View; 
                Transforms.SetView();

                ComputeViewFrustum();

            }

        }

        /// <summary>
        /// First Person Camera
        /// </summary>
        public class CameraTP : Camera
        {
            /// <summary>
            /// Camera rotation around the vector (0,1,0) : world up.
            /// </summary>
            private float Heading = 0;

            /// <summary>
            /// Camera rotation around the vector (1,0,0) : world left.
            /// </summary>
            private float Pitch = 0;

            /// <summary>
            /// Camera rotation around the vector (0,0,1) : world forward.
            /// </summary>
            private float Roll = 0;
            
            /// <summary>
            /// Distance From Target
            /// </summary>
            private float Distance = 10;

            /// <summary>
            /// Current Heading
            /// </summary>
            private float CurrentHeading = 0;

            /// <summary>
            /// Turning speed
            /// </summary>
            private float TurningSpeed = 0.5f;
            
            /// <summary>
            /// Camera Target
            /// </summary>
            private Vector3 Target;

            /// <summary>
            /// Initilize the camera.
            /// </summary>
            /// <param name="TransformsManager"></param>
            public CameraTP(ref TransformsManager TransformsManager)
                : base(ref TransformsManager) { }

            /// <summary>
            /// Set camera target
            /// </summary>
            /// <param name="target"></param>
            public void SetTarget(Vector3 target)
            {
                Target = target;
            }

            /// <summary>
            /// Update the camera heading.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateHeading(Single Factor)
            {
                Heading -= Factor;

                if (Heading > Constants.DoublePI)
                {
                    Heading = Heading - (float)Constants.DoublePI;
                }
                else if (Heading < 0)
                {
                    Heading = (float)Math.PI + Heading;
                }
            }

            /// <summary>
            /// Update the camera roll.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdateRoll(Single Factor)
            {
                Roll -= Factor;

                if (Roll > Constants.DoublePI)
                {
                    Roll = Roll - (float)Constants.DoublePI;
                }
                else if (Roll < 0)
                {
                    Roll = (float)Constants.DoublePI + Roll;
                }
            }

            /// <summary>
            /// Update the camera pitch.
            /// </summary>
            /// <param name="Factor"></param>
            public override void UpdatePitch(Single Factor)
            {
                Pitch -= Factor;

                if (Pitch > Constants.DoublePI)
                {
                    Pitch = Pitch - (float)Constants.DoublePI;
                }
                else if (Pitch < 0)
                {
                    Pitch = (float)Constants.DoublePI + Pitch;
                }
            }

            /// <summary>
            /// Update the camera and appy the view matrix to the scene.
            /// </summary>
            public override void Render()
            {

                CurrentHeading += (Roll - CurrentHeading) * TurningSpeed;

                //Update
                Position.Y = (float)Math.Sin(Pitch) * Distance;
                float adjacent = (float)Math.Cos(Pitch) * Distance;
                Position.X = (float)Math.Cos(CurrentHeading) * adjacent;
                Position.Z = (float)Math.Sin(CurrentHeading) * adjacent;
                //Look.Normalize();

                //Roll
                Up.X = (float)Math.Sin(Roll);
                Up.Y = (float)Math.Cos(Roll);
                Up.Z = 0;

                Right = Vector3.Cross(Target, Up);
                Right = Vector3.Normalize(Right);


                //Render
                View = Matrix.LookAtLH(Position, Target, Up);
                Transforms.View = View;
                Transforms.SetView();

                ComputeViewFrustum();

            }

        }



    }
}