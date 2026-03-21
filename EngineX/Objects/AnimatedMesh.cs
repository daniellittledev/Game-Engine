using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


using SharpDX;
using SharpDX.Direct3D9;

namespace EngineX.Objects
{
    public class AnimatedMesh
    {
        private AnimationRootFrame rootFrame;
        private Device device;
        private TransformsManager transfrom;

        public enum SkinningMethod
        {
            D3DIndexed,
            D3DNonIndexed,
            HLSL
        }

        private SkinningMethod  m_SkinningMethod;
        private int             m_NumBoneMatricesMax;
        private Matrix[]        m_BoneMatrices;
        private Effect          m_SkinningEffect;

        public AnimatedMesh(Device device, TransformsManager transfrom, string meshFilePath, string meshFileName)
        {
            this.device = device;
            this.transfrom = transfrom;

            m_SkinningMethod = SkinningMethod.D3DIndexed;

            if (m_SkinningMethod == SkinningMethod.HLSL)
            {
                // Load effect
                m_SkinningEffect = Effect.FromFile(device, @"..\..\Resources\SimpleAnimation.fx", null, ShaderFlags.None, null);

                m_SkinningEffect.OnResetDevice();
                m_SkinningEffect.Technique = new EffectHandle("t0");
            }

            AnimationAllocation alloc = new AnimationAllocation(this, meshFilePath);
            rootFrame = Mesh.LoadHierarchyFromFile(meshFilePath + meshFileName, MeshFlags.Managed, device, alloc, null);

            // Setup the matrices for animation
            SetupBoneMatrices(rootFrame.FrameHierarchy as AnimationFrame);

        }

        /// <summary>
        /// Generate the skinned mesh information
        /// </summary>
        public void GenerateSkinnedMesh(AnimationMeshContainer mesh)
        {
            if (mesh.SkinInformation == null)
                throw new ArgumentException();  // There is nothing to generate


            Caps caps = mesh.MeshData.Mesh.Device.DeviceCaps;

            if (this.m_SkinningMethod == SkinningMethod.D3DIndexed)
            {

                MeshFlags flags = MeshFlags.OptimizeVertexCache;

                if (caps.VertexShaderVersion >= new Version(1, 1))
                {
                    flags |= MeshFlags.Managed;
                    mesh.UseSoftwareVP = false;
                }
                else
                {
                    mesh.UseSoftwareVP = true;

                    flags |= MeshFlags.SystemMemory;
                }

                int numMaxFaceInfl;
                using (IndexBuffer ib = mesh.MeshData.Mesh.IndexBuffer)
                {
                    numMaxFaceInfl = mesh.SkinInformation.GetMaxFaceInfluences(ib,
                        mesh.MeshData.Mesh.NumberFaces);
                }
                // 12 entry palette guarantees that any triangle (4 independent 
                // influences per vertex of a tri) can be handled
                numMaxFaceInfl = (int)Math.Min(numMaxFaceInfl, 12);

                if (caps.MaxVertexBlendMatrixIndex + 1 >= numMaxFaceInfl)
                {
                    mesh.NumPalette = (int)Math.Min((
                        caps.MaxVertexBlendMatrixIndex + 1) / 2,
                        mesh.SkinInformation.NumberBones);

                    flags |= MeshFlags.Managed;
                }

                int influences = 0;
                BoneCombination[] bones = null;

                // Use ConvertToBlendedMesh to generate a drawable mesh
                MeshData data = mesh.MeshData;
                data.Mesh = mesh.SkinInformation.ConvertToIndexedBlendedMesh(data.Mesh, flags,
                    mesh.GetAdjacencyStream(), mesh.NumPalette, out influences,
                    out bones);

                // Store this info
                mesh.NumInfluences = influences;
                mesh.Bones = bones;

                // Get the number of attributes
                mesh.NumAttributes = bones.Length;

                mesh.MeshData = data;
            }
            else if (m_SkinningMethod == SkinningMethod.HLSL)
            {
                // Get palette size
                // First 9 constants are used for other data.  Each 4x3 matrix takes up 3 constants.
                // (96 - 9) /3 i.e. Maximum constant count - used constants 
                uint MaxMatrices = 26;
                mesh.NumPalette = (int)Math.Min(MaxMatrices, mesh.SkinInformation.NumberBones);

                MeshFlags flags = MeshFlags.OptimizeVertexCache;

                if (caps.VertexShaderVersion >= new Version(1, 1))
                {
                    mesh.UseSoftwareVP = false;
                    flags |= MeshFlags.Managed;
                }
                else
                {
                    mesh.UseSoftwareVP = true;
                    flags |= MeshFlags.SystemMemory;
                }

                int influences = 0;
                BoneCombination[] bones = null;

                // Use ConvertToBlendedMesh to generate a drawable mesh
                MeshData data = mesh.MeshData;
                data.Mesh = mesh.SkinInformation.ConvertToIndexedBlendedMesh(data.Mesh, flags,
                    mesh.GetAdjacencyStream(), mesh.NumPalette, out influences,
                    out bones);

                mesh.MeshData = data;

                // Store this info
                mesh.NumInfluences = influences;
                mesh.Bones = bones;

                // Get the number of attributes
                mesh.NumAttributes = bones.Length;


                // FVF has to match our declarator. Vertex shaders are not as forgiving as FF pipeline
                VertexFormat NewFVF = (mesh.MeshData.Mesh.VertexFormat & VertexFormat.PositionMask) | VertexFormat.Normal | VertexFormat.Texture1 | VertexFormat.LastBetaUByte4;
                if (NewFVF != mesh.MeshData.Mesh.VertexFormat)
                {

                    Mesh pMesh = mesh.MeshData.Mesh.Clone(MeshFlags.Managed, NewFVF, device);
                    MeshData pData = mesh.MeshData;
                    pData.Mesh = pMesh;

                    mesh.MeshData = pData;
                }

                VertexElement[] v = mesh.MeshData.Mesh.Declaration;

                // the vertex shader is expecting to interpret the UBYTE4 as a D3DCOLOR, so update the type 
                //   NOTE: this cannot be done with CloneMesh, that would convert the UBYTE4 data to float and then to D3DCOLOR
                //          this is more of a "cast" operation
                for (int iDecl = 0; iDecl < v.GetLength(0); iDecl++)
                {
                    if ((v[iDecl].DeclarationUsage == DeclarationUsage.BlendIndices) && (v[iDecl].UsageIndex == 0))
                        v[iDecl].DeclarationType = DeclarationType.Color;
                }
                mesh.MeshData.Mesh.UpdateSemantics(v);

                // allocate a buffer for bone matrices, but only if another mesh has not allocated one of the same size or larger
                if (m_NumBoneMatricesMax < mesh.SkinInformation.NumberBones)
                {
                    m_NumBoneMatricesMax = mesh.SkinInformation.NumberBones;
                    // Allocate space for blend matrices
                    m_BoneMatrices = new Matrix[m_NumBoneMatricesMax];
                }

            }
        }

        /// <summary>This method will set the bone matrices for a frame</summary>
        private void SetupBoneMatrices(AnimationFrame frame)
        {
            // First do the mesh container this frame contains (if it does)
            if (frame.MeshContainer != null)
            {
                SetupBoneMatrices(frame.MeshContainer as AnimationMeshContainer);
            }
            // Next do any siblings this frame may contain
            if (frame.FrameSibling != null)
            {
                SetupBoneMatrices(frame.FrameSibling as AnimationFrame);
            }
            // Finally do the children of this frame
            if (frame.FrameFirstChild != null)
            {
                SetupBoneMatrices(frame.FrameFirstChild as AnimationFrame);
            }
        }

        /// <summary>Sets the bone matrices for a mesh container</summary>
        private void SetupBoneMatrices(AnimationMeshContainer mesh)
        {
            // Is there skin information?  If so, setup the matrices
            if (mesh.SkinInformation != null)
            {
                int numberBones = mesh.SkinInformation.NumberBones;

                AnimationFrame[] frameMatrices = new AnimationFrame[numberBones];
                for (int i = 0; i < numberBones; i++)
                {
                    AnimationFrame frame = Frame.Find(rootFrame.FrameHierarchy,
                        mesh.SkinInformation.GetBoneName(i)) as AnimationFrame;

                    if (frame == null)
                        throw new InvalidOperationException("Could not find valid bone.");

                    frameMatrices[i] = frame;
                }
                mesh.FrameMatrices = frameMatrices;
            }
        }

        /// <summary>
        /// Render the mesh
        /// </summary>
        public void Render()
        {
            if (m_SkinningMethod == SkinningMethod.HLSL)
            {
                this.m_SkinningEffect.SetValue("mViewProj", transfrom.View * transfrom.Projection);
                
                // Set Light for vertex shader
                Vector4 vLightDir = new Vector4(0.0f, 1.0f, -1.0f, 0.0f);
                vLightDir = Vector4.Normalize(vLightDir);

                m_SkinningEffect.SetValue("lhtDir", vLightDir);
            }

            // Render the animation
            DrawFrame(rootFrame.FrameHierarchy as AnimationFrame);
            device.SetRenderState(RenderState.IndexedVertexBlendEnable, false);

        }

        /// <summary>Update the frames matrices and combine it with it's parents</summary>
        private void UpdateFrameMatrices(AnimationFrame frame, Matrix parentMatrix)
        {
            frame.CombinedTransformationMatrix = frame.TransformationMatrix *
                parentMatrix;

            if (frame.FrameSibling != null)
            {
                UpdateFrameMatrices(frame.FrameSibling as AnimationFrame, parentMatrix);
            }

            if (frame.FrameFirstChild != null)
            {
                UpdateFrameMatrices(frame.FrameFirstChild as AnimationFrame,
                    frame.CombinedTransformationMatrix);
            }
        }

        /// <summary>Draw a frame and all child and sibling frames</summary>
        private void DrawFrame(AnimationFrame frame)
        {
            AnimationMeshContainer mesh = frame.MeshContainer as AnimationMeshContainer;
            while (mesh != null)
            {
                DrawMeshContainer(mesh, frame);

                mesh = mesh.NextContainer as AnimationMeshContainer;
            }

            if (frame.FrameSibling != null)
            {
                DrawFrame(frame.FrameSibling as AnimationFrame);
            }

            if (frame.FrameFirstChild != null)
            {
                DrawFrame(frame.FrameFirstChild as AnimationFrame);
            }
        }

        /// <summary>Render a mesh container</summary>
        private void DrawMeshContainer(AnimationMeshContainer mesh, AnimationFrame parent)
        {



            // first check for skinning
            if (mesh.SkinInformation != null)
            {
                if (this.m_SkinningMethod == SkinningMethod.D3DIndexed)
                {

                    // if hw doesn't support indexed vertex processing, switch to software vertex processing
                    if (mesh.UseSoftwareVP == true)
                    {
                        // If hw or pure hw vertex processing is forced, we can't render the
                        // mesh, so just exit out.  Typical applications should create
                        // a device with appropriate vertex processing capability for this
                        // skinning method.

                        device.SoftwareVertexProcessing = true;
                    }

                    if (mesh.NumInfluences == 1)
                        device.SetRenderState(RenderState.VertexBlend, VertexBlend.ZeroWeights);
                    else
                        device.SetRenderState(RenderState.VertexBlend, (VertexBlend)(mesh.NumInfluences - 1));

                    if (mesh.NumInfluences > 0)
                        device.SetRenderState(RenderState.IndexedVertexBlendEnable, true);

                    BoneCombination[] bones = mesh.Bones;

                    for (int iAttrib = 0; iAttrib < mesh.NumAttributes; iAttrib++)
                    {
                        // first, get world matrices
                        for (int iPaletteEntry = 0; iPaletteEntry < mesh.NumPalette;
                            ++iPaletteEntry)
                        {
                            int iMatrixIndex = bones[iAttrib].BoneId[iPaletteEntry];
                            if (iMatrixIndex != -1)
                            {
                                device.SetTransform(TransformState.World + iPaletteEntry,
                                    mesh.OffsetMatrices[iMatrixIndex] *
                                    mesh.FrameMatrices[iMatrixIndex].
                                    CombinedTransformationMatrix);

                            }
                        }

                        // Setup the material
                        device.Material = mesh.GetMaterials()[bones[iAttrib].AttributeId].MaterialD3D;

                        Texture tex = mesh.MeshTextures[bones[iAttrib].AttributeId];

                        device.SetTexture(0, tex);

                        // Finally draw the subset
                        mesh.MeshData.Mesh.DrawSubset(iAttrib);

                    }
                }
                else
                {
                    // Standard mesh, just draw it using FF
                    device.SetRenderState(RenderState.VertexBlend, VertexBlend.Disable);

                    // Set up transforms
                    device.SetTransform(TransformState.World, parent.CombinedTransformationMatrix);

                    ExtendedMaterial[] materials = mesh.GetMaterials();
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        device.Material = materials[i].MaterialD3D;
                        device.SetTexture(0, mesh.MeshTextures[i]);
                        mesh.MeshData.Mesh.DrawSubset(i);
                    }
                }
            }
            else if (this.m_SkinningMethod == SkinningMethod.HLSL)
            {

                // if hw doesn't support indexed vertex processing, switch to software vertex processing
                if (mesh.UseSoftwareVP == true)
                {
                    // If hw or pure hw vertex processing is forced, we can't render the
                    // mesh, so just exit out.  Typical applications should create
                    // a device with appropriate vertex processing capability for this
                    // skinning method.

                    device.SoftwareVertexProcessing = true;
                }


                if (mesh.NumInfluences == 1)
                    device.SetRenderState(RenderState.VertexBlend, VertexBlend.ZeroWeights);
                else
                    device.SetRenderState(RenderState.VertexBlend, (VertexBlend)(mesh.NumInfluences - 1));

                if (mesh.NumInfluences > 0)
                    device.SetRenderState(RenderState.IndexedVertexBlendEnable, true);

                BoneCombination[] bones = mesh.Bones;

                for (int iAttrib = 0; iAttrib < mesh.NumAttributes; iAttrib++)
                {
                    // first calculate all the world matrices
                    for (int iPaletteEntry = 0; iPaletteEntry < mesh.NumPalette; ++iPaletteEntry)
                    {
                        int iMatrixIndex = bones[iAttrib].BoneId[iPaletteEntry];
                        if (iMatrixIndex != -1)
                        {
                            Matrix matTemps = mesh.OffsetMatrices[iMatrixIndex] * mesh.FrameMatrices[iMatrixIndex].CombinedTransformationMatrix;

                            m_BoneMatrices[iPaletteEntry] = matTemps;

                        }
                    }
                    Matrix[] clippedBoneMatrices = new Matrix[mesh.NumPalette];
                    Array.Copy(m_BoneMatrices, clippedBoneMatrices, mesh.NumPalette);
                    m_SkinningEffect.SetValue("mWorldMatrixArray", clippedBoneMatrices);

                    // Sum of all ambient and emissive contribution
                    ExtendedMaterial[] materials = mesh.GetMaterials();
                    Color4 color1 = materials[bones[iAttrib].AttributeId].MaterialD3D.Ambient;
                    Color4 color2 = new Color4(1.0f, 64f/255f, 64f/255f, 64f/255f);
                    Color4 color3 = materials[bones[iAttrib].AttributeId].MaterialD3D.Emissive;
                    Color4 ambEmm;
                    ambEmm = new Color4(color1.Alpha * color2.Alpha, color1.Red * color2.Red, color1.Green * color2.Green, color1.Blue * color2.Blue);
                    ambEmm = new Color4(ambEmm.Alpha + color3.Alpha, ambEmm.Red + color3.Red, ambEmm.Green + color3.Green, ambEmm.Blue + color3.Blue);

                    Color4 color4 = materials[bones[iAttrib].AttributeId].MaterialD3D.Diffuse;

                    // set material color properties
                    m_SkinningEffect.SetValue("MaterialDiffuse", color4);
                    m_SkinningEffect.SetValue("MaterialAmbient", ambEmm);

                    // setup the material of the mesh subset - REMEMBER to use the original pre-skinning attribute id to get the correct material id
                    device.SetTexture(0, mesh.MeshTextures[bones[iAttrib].AttributeId]);

                    // Set CurNumBones to select the correct vertex shader for the number of bones
                    int num = mesh.NumInfluences - 1;
                    m_SkinningEffect.SetValue("CurNumBones", mesh.NumInfluences - 1);


                    // Start the effect now all parameters have been updated
                    int numPasses;
                    numPasses = m_SkinningEffect.Begin(FX.DoNotSaveShaderState);
                    for (int iPass = 0; iPass < numPasses; iPass++)
                    {
                        m_SkinningEffect.BeginPass(iPass);

                        // draw the subset with the current world matrix palette and material state
                        mesh.MeshData.Mesh.DrawSubset(iAttrib);

                        m_SkinningEffect.EndPass();
                    }

                    m_SkinningEffect.End();

                }

                // remember to reset back to hw vertex processing if software was required
                if (mesh.UseSoftwareVP == true)
                {
                    device.SoftwareVertexProcessing = false;
                }
            }
        }

        public void Update(float elapsedTime, Matrix worldMatrix)
        {
            // Has any time elapsed?
            if (elapsedTime > 0.0f)
            {
                if (rootFrame.AnimationController != null)
                    rootFrame.AnimationController.AdvanceTime(elapsedTime);

                UpdateFrameMatrices(rootFrame.FrameHierarchy as AnimationFrame, worldMatrix);
            }
        }

    }

    // Animation Frame ----------------------------------------------------------------------------

    /// <summary>
    /// The frame that will hold mesh animation translation matrix
    /// </summary>
    public class AnimationFrame : Frame
    {
        // Store the combined transformation matrix
        private Matrix combined = Matrix.Identity;

        /// <summary>
        /// The combined transformation matrix
        /// </summary>
        public Matrix CombinedTransformationMatrix
        {
            get { return combined; }
            set { combined = value; }
        }
    }

    // Animation Mesh Container -------------------------------------------------------------------

    /// <summary>
    /// The mesh container class that will hold the animation data
    /// </summary>
    public class AnimationMeshContainer : MeshContainer
    {
        // Array data
        private Texture[] meshTextures;
        private BoneCombination[] bones;
        private Matrix[] offsetMatrices;
        private AnimationFrame[] frameMatrices;

        // Instance data
        private int numAttributes = 0;
        private int numInfluences = 0;
        private int numPalette = 0;

        private bool useSoftwareVP;


        // Public properties

        public bool UseSoftwareVP
        {
            get { return useSoftwareVP; }
            set { useSoftwareVP = value; }
        }

        /// <summary>
        /// Textures used for this container
        /// </summary>
        public Texture[] MeshTextures
        {
            get { return meshTextures; }
            set { meshTextures = value; }
        }

        /// <summary>
        /// Bone combinations used for this container
        /// </summary>
        public BoneCombination[] Bones
        {
            get { return bones; }
            set { bones = value; }
        }

        /// <summary>
        /// Animation frames used for this container
        /// </summary>
        public Matrix[] OffsetMatrices
        {
            get { return offsetMatrices; }
            set { offsetMatrices = value; }
        }

        /// <summary>
        /// Offset matrices used for this container
        /// </summary>
        public AnimationFrame[] FrameMatrices
        {
            get { return frameMatrices; }
            set { frameMatrices = value; }
        }

        /// <summary>
        /// Total number of attributes this mesh container contains
        /// </summary>
        public int NumAttributes
        {
            get { return numAttributes; }
            set { numAttributes = value; }
        }

        /// <summary>
        /// Total number of influences this mesh container contains
        /// </summary>
        public int NumInfluences
        {
            get { return numInfluences; }
            set { numInfluences = value; }
        }

        /// <summary>
        /// Total number of palette entries this mesh container contains
        /// </summary>
        public int NumPalette
        {
            get { return numPalette; }
            set { numPalette = value; }
        }
    }

    // Animation Allocation -----------------------------------------------------------------------

    /// <summary>
    /// AllocateHierarchy derived class
    /// </summary>
    public class AnimationAllocation : AllocateHierarchy
    {
        AnimatedMesh parent = null;
        string filePath;

        /// <summary>Create new instance of this class</summary>
        public AnimationAllocation(AnimatedMesh p, string meshFilePath)
        {
            parent = p;
            filePath = meshFilePath;
        }

        /// <summary>
        /// Create a new frame
        /// </summary>
        public override Frame CreateFrame(string name)
        {
            AnimationFrame frame = new AnimationFrame();
            frame.Name = name;
            frame.TransformationMatrix = Matrix.Identity;
            frame.CombinedTransformationMatrix = Matrix.Identity;

            return frame;
        }

        /// <summary>
        /// Create a new mesh container
        /// </summary>
        public override MeshContainer CreateMeshContainer(
            string name, MeshData meshData, ExtendedMaterial[] materials,
            EffectInstance[] effectInstances, int[] adjacency,
            SkinInformation skinInfo)
        {
            // We only handle meshes here
            if (meshData.Mesh == null)
                throw new ArgumentException();

            // We must have a vertex format mesh
            if (meshData.Mesh.VertexFormat == VertexFormat.None)
                throw new ArgumentException();

            AnimationMeshContainer mesh = new AnimationMeshContainer();

            mesh.Name = name;
            int numFaces = meshData.Mesh.NumberFaces;
            Device dev = meshData.Mesh.Device;



            // Make sure there are normals
            if ((meshData.Mesh.VertexFormat & VertexFormat.Normal) == 0)
            {
                // Clone the mesh
                Mesh tempMesh = meshData.Mesh.Clone(MeshFlags.Managed,
                    meshData.Mesh.VertexFormat | VertexFormat.Normal, dev);

                // Destroy current mesh, use the new one
                meshData.Mesh.Dispose();
                meshData.Mesh = tempMesh;
                meshData.Mesh.ComputeNormals();
            }




            // Store the materials
            mesh.SetMaterials(materials);
            mesh.SetAdjacency(adjacency);

            mesh.MeshData = meshData;

            // If there is skinning info, save any required data
            if (skinInfo != null)
            {
                mesh.SkinInformation = skinInfo;
                int numBones = skinInfo.NumberBones;
                Matrix[] offsetMatrices = new Matrix[numBones];

                for (int i = 0; i < numBones; i++)
                    offsetMatrices[i] = skinInfo.GetBoneOffsetMatrix(i);

                mesh.OffsetMatrices = offsetMatrices;

                parent.GenerateSkinnedMesh(mesh);

            }

            Texture[] meshTextures = new Texture[materials.Length];

            // Create any textures
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].TextureFileName != null)
                {
                    meshTextures[i] = Texture.FromFile(dev, filePath + materials[i].TextureFileName);
                }
            }

            mesh.MeshTextures = meshTextures;

            return mesh;
        }

    }

}
