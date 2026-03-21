using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace EngineX.Physics
{
    public class Polyhedron
    {
        public class Vertex
        {
            public ArrayList edges = new ArrayList();
            public ArrayList faces = new ArrayList();

            public Vector3 position;
            public Vector3 normal;

            public void Initialize()
            {
                // compute the interpolated normal
                foreach (Face currentFace in faces)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (currentFace.vertex[i] == this)
                        {
                            // weight the contribution of each face's normal by the angle that face
                            // subtends with respect this vertex
                            if (currentFace.edge[i] != null && currentFace.edge[(i + 2) % 3] != null) // HACK to make holey meshes work
                            {
                                float angle = (float)Math.Acos(Math.Abs(Vector3.Dot(currentFace.edge[i].normal, currentFace.edge[(i + 2) % 3].normal)));
                                normal += new Vector3(currentFace.plane.A, currentFace.plane.B, currentFace.plane.C) * angle;
                            }
                            break;
                        }
                    }
                }

                normal.Normalize();
            }

            public void Transform(Matrix matrix)
            {
                position.TransformCoordinate(matrix);
                normal.TransformNormal(matrix);
            }
        }

        public class Edge
        {
            public Vertex[] vertex = new Vertex[2];
            public Face[] face = new Face[2];

            public Vector3 normal;

            public void Initialize()
            {
                normal = Vector3.Normalize(vertex[0].position - vertex[1].position);
            }

            public void Transform(Matrix matrix)
            {
                normal.TransformNormal(matrix);
            }
        }

        public class Face
        {
            public Vertex[] vertex = new Vertex[3];
            public Edge[] edge = new Edge[3];

            public Plane plane;

            // for distance calculations
            private Vector3 edge0, edge1;
            private float fA00, fA01, fA11, fDet;

            public void Initialize()
            {
                plane = Plane.Normalize(Plane.FromPoints(vertex[0].position, vertex[1].position, vertex[2].position));

                // for distance calculations
                edge0 = vertex[1].position - vertex[0].position;
                edge1 = vertex[2].position - vertex[0].position;
                fA00 = edge0.LengthSq();
                fA01 = Vector3.Dot(edge0, edge1);
                fA11 = edge1.LengthSq();
                fDet = Math.Abs(fA00 * fA11 - fA01 * fA01);
            }

            public Vector3 PointFromST(float s, float t)
            {
                return (1 - (s + t)) * vertex[0].position + s * vertex[1].position + t * vertex[2].position;
            }

            public Vector3 NormalFromST(float s, float t)
            {
                return (1 - (s + t)) * vertex[0].normal + s * vertex[1].normal + t * vertex[2].normal;
            }

            public void Transform(Matrix matrix)
            {
                plane.Transform(matrix);
                edge0.TransformNormal(matrix);
                edge1.TransformNormal(matrix);
            }

            // this code was adapted from Magic Software's point-triangle distance
            // code which can be found at:
            // http://www.magic-software.com
            public void SqrDistance(Vector3 point, out float fSqrDist, out float fS, out float fT)
            {
                Vector3 kDiff = vertex[0].position - point;
                float dotDiffEdge0 = Vector3.Dot(kDiff, edge0);
                float dotDiffEdge1 = Vector3.Dot(kDiff, edge1);
                float fC = kDiff.LengthSq();
                fS = fA01 * dotDiffEdge1 - fA11 * dotDiffEdge0;
                fT = fA01 * dotDiffEdge0 - fA00 * dotDiffEdge1;
                fSqrDist = (float)0.0;

                if (fS + fT <= fDet)
                {
                    if (fS < (float)0.0)
                    {
                        if (fT < (float)0.0)  // region 4
                        {
                            if (dotDiffEdge0 < (float)0.0)
                            {
                                fT = (float)0.0;
                                if (-dotDiffEdge0 >= fA00)
                                {
                                    fS = (float)1.0;
                                    fSqrDist = fA00 + ((float)2.0) * dotDiffEdge0 + fC;
                                }
                                else
                                {
                                    fS = -dotDiffEdge0 / fA00;
                                    fSqrDist = dotDiffEdge0 * fS + fC;
                                }
                            }
                            else
                            {
                                fS = (float)0.0;
                                if (dotDiffEdge1 >= (float)0.0)
                                {
                                    fT = (float)0.0;
                                    fSqrDist = fC;
                                }
                                else if (-dotDiffEdge1 >= fA11)
                                {
                                    fT = (float)1.0;
                                    fSqrDist = fA11 + ((float)2.0) * dotDiffEdge1 + fC;
                                }
                                else
                                {
                                    fT = -dotDiffEdge1 / fA11;
                                    fSqrDist = dotDiffEdge1 * fT + fC;
                                }
                            }
                        }
                        else  // region 3
                        {
                            fS = (float)0.0;
                            if (dotDiffEdge1 >= (float)0.0)
                            {
                                fT = (float)0.0;
                                fSqrDist = fC;
                            }
                            else if (-dotDiffEdge1 >= fA11)
                            {
                                fT = (float)1.0;
                                fSqrDist = fA11 + ((float)2.0) * dotDiffEdge1 + fC;
                            }
                            else
                            {
                                fT = -dotDiffEdge1 / fA11;
                                fSqrDist = dotDiffEdge1 * fT + fC;
                            }
                        }
                    }
                    else if (fT < (float)0.0)  // region 5
                    {
                        fT = (float)0.0;
                        if (dotDiffEdge0 >= (float)0.0)
                        {
                            fS = (float)0.0;
                            fSqrDist = fC;
                        }
                        else if (-dotDiffEdge0 >= fA00)
                        {
                            fS = (float)1.0;
                            fSqrDist = fA00 + ((float)2.0) * dotDiffEdge0 + fC;
                        }
                        else
                        {
                            fS = -dotDiffEdge0 / fA00;
                            fSqrDist = dotDiffEdge0 * fS + fC;
                        }
                    }
                    else  // region 0
                    {
                        // minimum at interior point
                        float fInvDet = ((float)1.0) / fDet;
                        fS *= fInvDet;
                        fT *= fInvDet;
                        fSqrDist = fS * (fA00 * fS + fA01 * fT + ((float)2.0) * dotDiffEdge0) +
                            fT * (fA01 * fS + fA11 * fT + ((float)2.0) * dotDiffEdge1) + fC;
                    }
                }
                else
                {
                    float fTmp0, fTmp1, fNumer, fDenom;

                    if (fS < (float)0.0)  // region 2
                    {
                        fTmp0 = fA01 + dotDiffEdge0;
                        fTmp1 = fA11 + dotDiffEdge1;
                        if (fTmp1 > fTmp0)
                        {
                            fNumer = fTmp1 - fTmp0;
                            fDenom = fA00 - 2.0f * fA01 + fA11;
                            if (fNumer >= fDenom)
                            {
                                fS = (float)1.0;
                                fT = (float)0.0;
                                fSqrDist = fA00 + ((float)2.0) * dotDiffEdge0 + fC;
                            }
                            else
                            {
                                fS = fNumer / fDenom;
                                fT = (float)1.0 - fS;
                                fSqrDist = fS * (fA00 * fS + fA01 * fT + 2.0f * dotDiffEdge0) +
                                    fT * (fA01 * fS + fA11 * fT + ((float)2.0) * dotDiffEdge1) + fC;
                            }
                        }
                        else
                        {
                            fS = (float)0.0;
                            if (fTmp1 <= (float)0.0)
                            {
                                fT = (float)1.0;
                                fSqrDist = fA11 + ((float)2.0) * dotDiffEdge1 + fC;
                            }
                            else if (dotDiffEdge1 >= (float)0.0)
                            {
                                fT = (float)0.0;
                                fSqrDist = fC;
                            }
                            else
                            {
                                fT = -dotDiffEdge1 / fA11;
                                fSqrDist = dotDiffEdge1 * fT + fC;
                            }
                        }
                    }
                    else if (fT < (float)0.0)  // region 6
                    {
                        fTmp0 = fA01 + dotDiffEdge1;
                        fTmp1 = fA00 + dotDiffEdge0;
                        if (fTmp1 > fTmp0)
                        {
                            fNumer = fTmp1 - fTmp0;
                            fDenom = fA00 - ((float)2.0) * fA01 + fA11;
                            if (fNumer >= fDenom)
                            {
                                fT = (float)1.0;
                                fS = (float)0.0;
                                fSqrDist = fA11 + ((float)2.0) * dotDiffEdge1 + fC;
                            }
                            else
                            {
                                fT = fNumer / fDenom;
                                fS = (float)1.0 - fT;
                                fSqrDist = fS * (fA00 * fS + fA01 * fT + ((float)2.0) * dotDiffEdge0) +
                                    fT * (fA01 * fS + fA11 * fT + ((float)2.0) * dotDiffEdge1) + fC;
                            }
                        }
                        else
                        {
                            fT = (float)0.0;
                            if (fTmp1 <= (float)0.0)
                            {
                                fS = (float)1.0;
                                fSqrDist = fA00 + ((float)2.0) * dotDiffEdge0 + fC;
                            }
                            else if (dotDiffEdge0 >= (float)0.0)
                            {
                                fS = (float)0.0;
                                fSqrDist = fC;
                            }
                            else
                            {
                                fS = -dotDiffEdge0 / fA00;
                                fSqrDist = dotDiffEdge0 * fS + fC;
                            }
                        }
                    }
                    else  // region 1
                    {
                        fNumer = fA11 + dotDiffEdge1 - fA01 - dotDiffEdge0;
                        if (fNumer <= (float)0.0)
                        {
                            fS = (float)0.0;
                            fT = (float)1.0;
                            fSqrDist = fA11 + ((float)2.0) * dotDiffEdge1 + fC;
                        }
                        else
                        {
                            fDenom = fA00 - 2.0f * fA01 + fA11;
                            if (fNumer >= fDenom)
                            {
                                fS = (float)1.0;
                                fT = (float)0.0;
                                fSqrDist = fA00 + ((float)2.0) * dotDiffEdge0 + fC;
                            }
                            else
                            {
                                fS = fNumer / fDenom;
                                fT = (float)1.0 - fS;
                                fSqrDist = fS * (fA00 * fS + fA01 * fT + ((float)2.0) * dotDiffEdge0) +
                                    fT * (fA01 * fS + fA11 * fT + ((float)2.0) * dotDiffEdge1) + fC;
                            }
                        }
                    }
                }

                fSqrDist = Math.Abs(fSqrDist);
            }
        }

        // this code was adapted from Brian Mirtich's polyhedral mass
        // calculation code which can be found at:
        // http://www.acm.org/jgt
        private class PolyhedralMassProperties
        {
            private const int X = 0;
            private const int Y = 1;
            private const int Z = 2;

            private int A;   /* alpha */
            private int B;   /* beta */
            private int C;   /* gamma */

            /* projection integrals */
            private float P1, Pa, Pb, Paa, Pab, Pbb, Paaa, Paab, Pabb, Pbbb;

            /* face integrals */
            private float Fa, Fb, Fc, Faa, Fbb, Fcc, Faaa, Fbbb, Fccc, Faab, Fbbc, Fcca;

            /* volume integrals */
            private float T0;
            private float[] T1 = new float[3];
            private float[] T2 = new float[3];
            private float[] TP = new float[3];

            private static float SQR(float x) { return x * x; }
            private static float CUBE(float x) { return x * x * x; }

            public void Compute(Polyhedron p, float density,
                out float mass, out Vector3 r, out Matrix3 J)
            {
                ComputeVolumeIntegrals(p);

                //                Console.WriteLine("T0 = " + T0);
                //                Console.WriteLine("T1 = ({0},{1},{2})", T1[0], T1[1], T1[2]);
                //                Console.WriteLine("T2 = ({0},{1},{2})", T2[0], T2[1], T2[2]);
                //                Console.WriteLine("TP = ({0},{1},{2})", TP[0], TP[1], TP[2]);

                mass = density * T0;

                /* compute center of mass */
                r.X = T1[X] / T0;
                r.Y = T1[Y] / T0;
                r.Z = T1[Z] / T0;

                /* compute inertia tensor */
                J = Matrix3.Zero;
                J.M11 = density * (T2[Y] + T2[Z]);
                J.M22 = density * (T2[Z] + T2[X]);
                J.M33 = density * (T2[X] + T2[Y]);
                J.M12 = J.M21 = -density * TP[X];
                J.M23 = J.M32 = -density * TP[Y];
                J.M31 = J.M13 = -density * TP[Z];

                /* translate inertia tensor to center of mass */
                J.M11 -= mass * (r.Y * r.Y + r.Z * r.Z);
                J.M22 -= mass * (r.Z * r.Z + r.X * r.X);
                J.M33 -= mass * (r.X * r.X + r.Y * r.Y);
                J.M12 = J.M21 += mass * r.X * r.Y;
                J.M23 = J.M32 += mass * r.Y * r.Z;
                J.M31 = J.M13 += mass * r.Z * r.X;
            }

            private void ComputeVolumeIntegrals(Polyhedron p)
            {
                T0 = T1[X] = T1[Y] = T1[Z]
                    = T2[X] = T2[Y] = T2[Z]
                    = TP[X] = TP[Y] = TP[Z] = 0;

                float[] norm = new float[3];
                foreach (Face f in p.faces)
                {
                    norm[X] = f.plane.A;
                    norm[Y] = f.plane.B;
                    norm[Z] = f.plane.C;

                    float nx = Math.Abs(norm[X]);
                    float ny = Math.Abs(norm[Y]);
                    float nz = Math.Abs(norm[Z]);
                    if (nx > ny && nx > nz) C = X;
                    else C = (ny > nz) ? Y : Z;
                    A = (C + 1) % 3;
                    B = (A + 1) % 3;

                    ComputeFaceIntegrals(p, f, norm);

                    T0 += norm[X] * ((A == X) ? Fa : ((B == X) ? Fb : Fc));

                    T1[A] += norm[A] * Faa;
                    T1[B] += norm[B] * Fbb;
                    T1[C] += norm[C] * Fcc;
                    T2[A] += norm[A] * Faaa;
                    T2[B] += norm[B] * Fbbb;
                    T2[C] += norm[C] * Fccc;
                    TP[A] += norm[A] * Faab;
                    TP[B] += norm[B] * Fbbc;
                    TP[C] += norm[C] * Fcca;
                }

                T1[X] /= 2; T1[Y] /= 2; T1[Z] /= 2;
                T2[X] /= 3; T2[Y] /= 3; T2[Z] /= 3;
                TP[X] /= 2; TP[Y] /= 2; TP[Z] /= 2;
            }

            private void ComputeFaceIntegrals(Polyhedron p, Polyhedron.Face f, float[] n)
            {
                float w = f.plane.D;
                float k1, k2, k3, k4;

                ComputeProjectionIntegrals(p, f);

                k1 = 1 / n[C]; k2 = k1 * k1; k3 = k2 * k1; k4 = k3 * k1;

                Fa = k1 * Pa;
                Fb = k1 * Pb;
                Fc = -k2 * (n[A] * Pa + n[B] * Pb + w * P1);

                Faa = k1 * Paa;
                Fbb = k1 * Pbb;
                Fcc = k3 * (SQR(n[A]) * Paa + 2 * n[A] * n[B] * Pab + SQR(n[B]) * Pbb
                    + w * (2 * (n[A] * Pa + n[B] * Pb) + w * P1));

                Faaa = k1 * Paaa;
                Fbbb = k1 * Pbbb;
                Fccc = -k4 * (CUBE(n[A]) * Paaa + 3 * SQR(n[A]) * n[B] * Paab
                    + 3 * n[A] * SQR(n[B]) * Pabb + CUBE(n[B]) * Pbbb
                    + 3 * w * (SQR(n[A]) * Paa + 2 * n[A] * n[B] * Pab + SQR(n[B]) * Pbb)
                    + w * w * (3 * (n[A] * Pa + n[B] * Pb) + w * P1));

                Faab = k1 * Paab;
                Fbbc = -k2 * (n[A] * Pabb + n[B] * Pbbb + w * Pbb);
                Fcca = k3 * (SQR(n[A]) * Paaa + 2 * n[A] * n[B] * Paab + SQR(n[B]) * Pabb
                    + w * (2 * (n[A] * Paa + n[B] * Pab) + w * Pa));
            }

            private void ComputeProjectionIntegrals(Polyhedron p, Polyhedron.Face f)
            {
                float a0, a1, da;
                float b0, b1, db;
                float a0_2, a0_3, a0_4, b0_2, b0_3, b0_4;
                float a1_2, a1_3, b1_2, b1_3;
                float C1, Ca, Caa, Caaa, Cb, Cbb, Cbbb;
                float Cab, Kab, Caab, Kaab, Cabb, Kabb;

                P1 = Pa = Pb = Paa = Pab = Pbb = Paaa = Paab = Pabb = Pbbb = 0.0f;

                float[,] v = new float[3, 3];
                for (int i = 0; i < 3; i++)
                {
                    v[i, 0] = f.vertex[i].position.X;
                    v[i, 1] = f.vertex[i].position.Y;
                    v[i, 2] = f.vertex[i].position.Z;
                }
                for (int i = 0; i < 3; i++)
                {
                    a0 = v[i, A];
                    b0 = v[i, B];
                    a1 = v[(i + 1) % 3, A];
                    b1 = v[(i + 1) % 3, B];
                    da = a1 - a0;
                    db = b1 - b0;
                    a0_2 = a0 * a0; a0_3 = a0_2 * a0; a0_4 = a0_3 * a0;
                    b0_2 = b0 * b0; b0_3 = b0_2 * b0; b0_4 = b0_3 * b0;
                    a1_2 = a1 * a1; a1_3 = a1_2 * a1;
                    b1_2 = b1 * b1; b1_3 = b1_2 * b1;

                    C1 = a1 + a0;
                    Ca = a1 * C1 + a0_2; Caa = a1 * Ca + a0_3; Caaa = a1 * Caa + a0_4;
                    Cb = b1 * (b1 + b0) + b0_2; Cbb = b1 * Cb + b0_3; Cbbb = b1 * Cbb + b0_4;
                    Cab = 3 * a1_2 + 2 * a1 * a0 + a0_2; Kab = a1_2 + 2 * a1 * a0 + 3 * a0_2;
                    Caab = a0 * Cab + 4 * a1_3; Kaab = a1 * Kab + 4 * a0_3;
                    Cabb = 4 * b1_3 + 3 * b1_2 * b0 + 2 * b1 * b0_2 + b0_3;
                    Kabb = b1_3 + 2 * b1_2 * b0 + 3 * b1 * b0_2 + 4 * b0_3;

                    P1 += db * C1;
                    Pa += db * Ca;
                    Paa += db * Caa;
                    Paaa += db * Caaa;
                    Pb += da * Cb;
                    Pbb += da * Cbb;
                    Pbbb += da * Cbbb;
                    Pab += db * (b1 * Cab + b0 * Kab);
                    Paab += db * (b1 * Caab + b0 * Kaab);
                    Pabb += da * (a1 * Cabb + a0 * Kabb);
                }

                P1 /= 2.0f;
                Pa /= 6.0f;
                Paa /= 12.0f;
                Paaa /= 20.0f;
                Pb /= -6.0f;
                Pbb /= -12.0f;
                Pbbb /= -20.0f;
                Pab /= 24.0f;
                Paab /= 60.0f;
                Pabb /= -60.0f;
            }

        }


        public ArrayList verts;
        public ArrayList edges;
        public ArrayList faces;

        public Polyhedron(Mesh mesh)
        {
            /* clone the mesh so as to get at it's faces and vertices */
            using (Mesh tempMesh = mesh.Clone(MeshFlags.Managed, VertexFormats.Position, mesh.Device))
            {
                CustomVertex.PositionOnly[] vertList = (CustomVertex.PositionOnly[])tempMesh.LockVertexBuffer(typeof(CustomVertex.PositionOnly), LockFlags.ReadOnly, tempMesh.NumberVertices);
                short[] faceList = (short[])tempMesh.LockIndexBuffer(typeof(short), LockFlags.ReadOnly, tempMesh.NumberFaces * 3);

                System.Diagnostics.Debug.Assert(vertList.Length == tempMesh.NumberVertices);
                System.Diagnostics.Debug.Assert(faceList.Length == tempMesh.NumberFaces * 3);

                verts = new ArrayList();
                for (int i = 0; i < tempMesh.NumberVertices; i++)
                {
                    Vertex v = new Vertex();
                    v.position = vertList[i].Position;
                    verts.Add(v);
                }

                faces = new ArrayList();
                for (int i = 0; i < tempMesh.NumberFaces; i++)
                {
                    Face f = new Face();
                    for (int j = 0; j < 3; j++)
                        f.vertex[j] = (Vertex)verts[faceList[i * 3 + j]];
                    faces.Add(f);
                }

                tempMesh.UnlockVertexBuffer();
                tempMesh.UnlockIndexBuffer();
            }

            Initialize();
        }

        private void Initialize()
        {
            // backlink each vertex to all the faces it is in
            foreach (Face face in faces)
                foreach (Vertex vertex in face.vertex)
                    vertex.faces.Add(face);

            // clean the mesh of duplicate vertices
            // for speed would want to sort the vertices in some way
            for (int i = 0; i < verts.Count; i++)
            {
                for (int j = i + 1; j < verts.Count; )
                {
                    Vertex v1 = (Vertex)verts[i];
                    Vertex v2 = (Vertex)verts[j];
                    if (v1.position.X == v2.position.X && v1.position.Y == v2.position.Y && v1.position.Z == v2.position.Z)
                    {
                        foreach (Face f in v2.faces)
                        {
                            // replace v2 in face f with v1
                            for (int k = 0; k < 3; k++)
                                if (f.vertex[k] == v2)
                                    f.vertex[k] = v1;

                            // add f to v1's list of faces
                            v1.faces.Add(f);
                        }

                        // eliminate v2 from the list of vertices
                        verts.RemoveAt(j);
                    }
                    else
                    {
                        // only move on when didn't delete something
                        j++;
                    }
                }
            }

            // identify all edges
            edges = new ArrayList();
            int numMissingEdges = 0;
            foreach (Face f1 in faces)
            {
                for (int i = 0; i < 3; i++)
                {
                    // check if edge already created
                    if (f1.edge[i] != null) continue;

                    foreach (Face f2 in f1.vertex[i].faces)
                    {
                        if (f1 == f2) continue;

                        for (int j = 0; j < 3; j++)
                        {
                            if (f1.vertex[i] == f2.vertex[(j + 1) % 3] && f1.vertex[(i + 1) % 3] == f2.vertex[j])
                            {
                                Edge e = new Edge();
                                e.vertex[0] = f1.vertex[i];
                                e.vertex[1] = f1.vertex[(i + 1) % 3];
                                e.face[0] = f1;
                                e.face[1] = f2;
                                f1.edge[i] = e;
                                f2.edge[j] = e;
                                f1.vertex[i].edges.Add(e);
                                f2.vertex[j].edges.Add(e);
                                edges.Add(e);
                            }
                        }
                    }

                    // make sure edge was found
                    if (f1.edge[i] == null)
                    {
                        numMissingEdges++;
                    }
                }
            }

            //            if (numMissingEdges > 0)
            //                throw new Exception("Unable to construct polyhedron b/c of hole in mesh: " + numMissingEdges + " missing edges\n");
            //
            foreach (Face f in faces)
            {
                f.Initialize();
            }

            foreach (Edge e in edges)
            {
                e.Initialize();
            }

            foreach (Vertex v in verts)
            {
                v.Initialize();
            }
        }

        public void ComputeMassProperties(float density, out float mass, out Vector3 centerOfMass, out Matrix3 inertiaTensor)
        {
            PolyhedralMassProperties massProps = new PolyhedralMassProperties();
            massProps.Compute(this, density, out mass, out centerOfMass, out inertiaTensor);
        }

        public void Transform(Matrix m)
        {
            foreach (Vertex v in verts)
            {
                v.Transform(m);
            }

            foreach (Face f in faces)
            {
                f.Transform(m);
            }

            foreach (Edge e in edges)
            {
                e.Transform(m);
            }
        }

        // determines the squared distance from the given point to this polyhedron
        // also gives the closest point on the polyhedron and the interpolated normal at the point
        public void SqrDistance(Vector3 pt, out float sqrDist, out Vector3 closestPt, out Vector3 normal)
        {
            float bestDist = float.PositiveInfinity;
            float bestS = 0, bestT = 0;
            Face bestFace = null;

            foreach (Face curFace in faces)
            {
                float curDist;
                float curS, curT;

                curFace.SqrDistance(pt, out curDist, out curS, out curT);
                if (curDist < bestDist)
                {
                    bestDist = curDist;
                    bestS = curS;
                    bestT = curT;
                    bestFace = curFace;
                }
            }

            sqrDist = bestDist;
            closestPt = bestFace.PointFromST(bestS, bestT);
            normal = bestFace.NormalFromST(bestS, bestT);
        }
    }
}
