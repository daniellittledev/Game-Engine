using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DX = Microsoft.DirectX.Direct3D;

namespace EngineX
{
    namespace Objects // Up to date
    {
        
        /// <summary>
        /// World matrix transformations container
        /// </summary>
        public class WorldTransform
        {
            /// <summary>
            /// Translation Matrix
            /// </summary>
            private Matrix Translate1;
            /// <summary>
            /// Rotaion Matrix
            /// </summary>
            private Matrix Rotate1;
            /// <summary>
            /// Scaleing Matrix
            /// </summary>
            private Matrix Scale1;
            /// <summary>
            /// Rotaion components
            /// </summary>
            float RotaionX, RotaionY, RotaionZ;

            /// <summary>
            /// Creates a new WorldTransform
            /// </summary>
            public WorldTransform()
            {
                Reset();
            }

            /// <summary>
            /// Reset the matrices to default position.
            /// </summary>
            public void Reset()
            {
                Translate1 = Matrix.Identity;
                Rotate1 = Matrix.Identity;
                Scale1 = Matrix.Identity;
                RotaionX = 0.0f;
                RotaionY = 0.0f;
                RotaionZ = 0.0f;
            }

            /// <summary>
            /// Absolute translation
            /// </summary>
            /// <param name="x">X</param>
            /// <param name="y">Y</param>
            /// <param name="z">Z</param>
            public void TranslateAbs(float x, float y, float z)
            {
                Translate1.M41 = x;
                Translate1.M42 = y;
                Translate1.M43 = z;
            }

            /// <summary>
            /// Absolute translation
            /// </summary>
            /// <param name="translation">Translations vector</param>
            public void TranslateAbs(Vector3 translation)
            {
                TranslateAbs(translation.X, translation.Y, translation.Z);
            }

            /// <summary>
            /// Relative translation
            /// </summary>
            /// <param name="x">X</param>
            /// <param name="y">Y</param>
            /// <param name="z">Z</param>
            public void TranslateRel(float x, float y, float z)
            {
                Translate1.M41 += x;
                Translate1.M42 += y;
                Translate1.M43 += z;
            }

            /// <summary>
            /// Relative translation
            /// </summary>
            /// <param name="translation">Translations vector</param>
            public void TranslateRel(Vector3 translation)
            {
                TranslateRel(translation.X, translation.Y, translation.Z);
            }

            /// <summary>
            /// Absolute rotation
            /// </summary>
            /// <param name="x">X radians</param>
            /// <param name="y">Y radians</param>
            /// <param name="z">Z radians</param>
            public void RotateAbs(float x, float y, float z)
            {
                RotaionX = x;
                RotaionY = y;
                RotaionZ = z;
                Rotate1 = Matrix.RotationYawPitchRoll(y, x, z);
            }

            /// <summary>
            /// Relative rotation
            /// </summary>
            /// <param name="x">X radians</param>
            /// <param name="y">Y radians</param>
            /// <param name="z">Z radians</param>
            public void RotateRel(float x, float y, float z)
            {
                RotaionX += x;
                RotaionY += y;
                RotaionZ += z;
                Rotate1 = Matrix.RotationYawPitchRoll(RotaionY, RotaionX, RotaionZ);
            }

            /// <summary>
            /// Absolute Scale
            /// </summary>
            /// <param name="x">X</param>
            /// <param name="y">Y</param>
            /// <param name="z">Z</param>
            public void Scale1Abs(float x, float y, float z)
            {
                Scale1.M11 = x;
                Scale1.M22 = y;
                Scale1.M33 = z;
            }

            /// <summary>
            /// Relative Scale
            /// </summary>
            /// <param name="x">X</param>
            /// <param name="y">Y</param>
            /// <param name="z">Z</param>
            public void Scale1Rel(float x, float y, float z)
            {
                Scale1.M11 += x;
                Scale1.M22 += y;
                Scale1.M33 += z;
            }

            /// <summary>
            /// The combined transformation matrix.
            /// </summary>
            public Matrix Transform
            {
                get
                {
                    return Scale1 * Rotate1 * Translate1;
                }
            }

            /// <summary>
            /// Gets and sets the position vector
            /// </summary>
            public Vector3 Position
            {
                get { return new Vector3(Translate1.M41, Translate1.M42, Translate1.M43); }
                set
                {
                    Translate1.M41 = value.X;
                    Translate1.M42 = value.Y;
                    Translate1.M43 = value.Z;
                }
            }

            /// <summary>
            /// Absolute x position.
            /// </summary>
            public float XPosition
            {
                get { return Translate1.M41; }
                set { Translate1.M41 = value; }
            }

            /// <summary>
            /// Absolute y position.
            /// </summary>
            public float YPosition
            {
                get { return Translate1.M42; }
                set { Translate1.M42 = value; }
            }

            /// <summary>
            /// Absolute z position.
            /// </summary>
            public float ZPosition
            {
                get { return Translate1.M43; }
                set { Translate1.M43 = value; }
            }

            /// <summary>
            /// Absolute x rotation.
            /// </summary>
            public float XRotation
            {
                get { return RotaionX; }
                set { RotateAbs(value, RotaionY, RotaionZ); }
            }

            /// <summary>
            /// Absolute y rotation.
            /// </summary>
            public float YRotation
            {
                get { return RotaionY; }
                set { RotateAbs(RotaionX, value, RotaionZ); }
            }

            /// <summary>
            /// Absolute z rotation.
            /// </summary>
            public float ZRotation
            {
                get { return RotaionZ; }
                set { RotateAbs(RotaionX, RotaionY, value); }
            }

            /// <summary>
            /// Absolute x Scale1.
            /// </summary>
            public float XScale1
            {
                get { return Scale1.M11; }
                set { Scale1.M11 = value; }
            }

            /// <summary>
            /// Absolute y Scale1.
            /// </summary>
            public float YScale1
            {
                get { return Scale1.M22; }
                set { Scale1.M22 = value; }
            }

            /// <summary>
            /// Absolute z Scale1.
            /// </summary>
            public float ZScale1
            {
                get { return Scale1.M33; }
                set { Scale1.M33 = value; }
            }
        }

        /// <summary>
        /// Mesh Information Class
        /// </summary>
        public class MeshInformation
        {
            /// <summary>
            /// Mesh Materials
            /// </summary>
            public Material[] Materials;
            /// <summary>
            /// Mesh Textures
            /// </summary>
            public Texture[] Textures;
            /// <summary>
            /// Mesh Information
            /// </summary>
            public Mesh ObjMesh;
            /// <summary>
            /// Number of mesh subsets
            /// </summary>
            private int Subsets;

            /// <summary>
            /// gets the subset count for a mesh.
            /// </summary>
            public int SubsetCount
            {
                get { return Subsets;  }
            }

            /// <summary>
            /// Load plain mesh
            /// </summary>
            /// <param name="objMesh"></param>
            /// <param name="materials"></param>
            /// <param name="textures"></param>
            public MeshInformation(Mesh objMesh)
            {
                ObjMesh = objMesh;
                Materials = null;
                Textures = null;
                Subsets = objMesh.NumberAttributes;

                if (objMesh != null && Subsets == 0)
                {
                    Subsets = 1;
                }
            }

            /// <summary>
            /// Load mesh from given data.
            /// </summary>
            /// <param name="objMesh"></param>
            /// <param name="materials"></param>
            /// <param name="textures"></param>
            public MeshInformation(Mesh objMesh, Material[] materials, Texture[] textures)
            {

                    ObjMesh = objMesh;
                    Materials = materials;
                    Textures = textures;
                    Subsets = objMesh.NumberAttributes;

                    if (objMesh != null && Subsets == 0)
                    {
                        Subsets = 1;
                    }
            }
            
            /// <summary>
            /// Load mesh information from file.
            /// </summary>
            /// <param name="device">Direct3D Device</param>
            /// <param name="file">File Name</param>
            public MeshInformation(Device device, string location, string file)
            {
  
                GraphicsStream outputAdjacency;
                ExtendedMaterial[] materials;
                EffectInstance[] effects;
                ObjMesh = DX.Mesh.FromFile(location + file, MeshFlags.Managed, device, out outputAdjacency, out materials, out effects);

                // Not using effects
                effects = null;

                // Add normals if it doesn't have any
                if ((ObjMesh.VertexFormat & VertexFormats.PositionNormal) != VertexFormats.PositionNormal)
                {
                    Mesh tempMesh = ObjMesh.Clone(ObjMesh.Options.Value, ObjMesh.VertexFormat | VertexFormats.PositionNormal | VertexFormats.Texture1, device);
                    tempMesh.ComputeNormals();
                    ObjMesh.Dispose();
                    ObjMesh = tempMesh;
                }

                // Attribute sort the mesh to enhance Mesh.DrawSubset performance

                //Mesh1.GenerateAdjacency(0.1f, Mesh1.ConvertAdjacencyToPointReps(outputAdjacency));

                ObjMesh.OptimizeInPlace(Microsoft.DirectX.Direct3D.MeshFlags.OptimizeAttributeSort, outputAdjacency);

                outputAdjacency.Dispose();
                outputAdjacency = null;

                // Extract the material properties and texture names.
                Textures = new Texture[materials.Length];
                Materials = new Material[materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    Materials[i] = materials[i].Material3D;

                    //Console.WriteLine(materials[0].TextureFilename);

                    // Create the texture.
                    if (materials[i].TextureFilename != null && materials[i].TextureFilename.Length > 0)
                    {
                        string temp = materials[i].TextureFilename;
                        Textures[i] = TextureLoader.FromFile(device, location + temp);
                    }
                    else
                    {
                        Textures[i] = null;
                    }
                }

                Subsets = ObjMesh.GetAttributeTable().Length;
            }
        }

        /// <summary>
        /// Mesh class
        /// </summary>
        public class MeshStatic : WorldTransform
        {
            /// <summary>
            /// Mesh Rendering Information
            /// </summary>
            MeshInformation Mesh1;
            /// <summary>
            /// Direct3D Device
            /// </summary>
            Device Device1;

            /// <summary>Creates a new static mesh</summary>
            /// <param name="device">Direct3D Device</param>
            /// <param name="file">File Name</param>
            public MeshStatic(Device device, MeshInformation meshInformation)
            {
                Device1 = device;
                Mesh1 = meshInformation;
  
            }

            /// <summary>
            /// Clean up resources
            /// </summary>
            public void Dispose()
            {
                if (Mesh1.ObjMesh != null)
                {
                    Mesh1.ObjMesh.Dispose();
                    Mesh1.ObjMesh = null;
                }
                Mesh1.Materials = null;
                if (Mesh1.Textures != null)
                {
                    for (int i = 0; i < Mesh1.Textures.Length; i++)
                    {
                        if (Mesh1.Textures[i] != null)
                        {
                            Mesh1.Textures[i].Dispose();
                            Mesh1.Textures[i] = null;
                        }
                    }
                    Mesh1.Textures = null;
                }
            }

            /// <summary>
            /// Gets the source Microsoft.DirectX.Direct3D.Mesh
            /// </summary>
            public Mesh SourceMesh
            {
                get { return Mesh1.ObjMesh; }
            }

            /// <summary>
            /// Gets the Mesh's Materials.
            /// </summary>
            public Material[] Materials
            {
                get { return Mesh1.Materials; }
            }

            /// <summary>
            /// Gets the Mesh's Textures.
            /// </summary>
            public Texture[] Textures
            {
                get { return Mesh1.Textures; }
            }

            /// <summary>
            /// Render the mesh
            /// </summary>
            public void Render()
            {
                if (Device1 == null || Mesh1.ObjMesh == null)
                {
                    return;
                }
                Device1.Transform.World = Transform;
                Device1.VertexFormat = SourceMesh.VertexFormat;
                
                for (int i = 0; i < Mesh1.SubsetCount ; i++)
                {
                    // Set the material and texture for this subset.
                    if ( Mesh1.Materials != null && Mesh1.Materials[i] != null )
                        Device1.Material = Mesh1.Materials[i];

                    if (Mesh1.Textures != null && Mesh1.Textures[i] != null)
                        Device1.SetTexture(0, Mesh1.Textures[i]);

                    // Draw the mesh subset.
                    Mesh1.ObjMesh.DrawSubset(i);
                }
            }

            /// <summary>
            /// Render the mesh
            /// </summary>
            public void RenderBasic()
            {
                if (Device1 == null || Mesh1.ObjMesh == null)
                {
                    return;
                }
                Device1.VertexFormat = SourceMesh.VertexFormat;
                for (int i = 0; i < Mesh1.SubsetCount; i++)
                {
                    // Set the material and texture for this subset.
                    if (Mesh1.Materials != null && Mesh1.Materials[i] != null)
                        Device1.Material = Mesh1.Materials[i];

                    if (Mesh1.Textures != null && Mesh1.Textures[i] != null)
                        Device1.SetTexture(0, Mesh1.Textures[i]);

                    // Draw the mesh subset.
                    Mesh1.ObjMesh.DrawSubset(i);
                }

            }

        }

    }

}