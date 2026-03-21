using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;
using SharpDX.Direct3D9;

using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace EngineX.Environment
{

    public class InstanceGroup
    {

        Device device;
        TransformsManager transfrom;
        Texture texture;

        private Effect instancingEffect;
        private EffectHandle viewProjection;
        private EffectHandle instanceData;
        private EffectHandle instanceAlpha;
        private EffectHandle textureData;

        IndexBuffer instanceIndexBuffer;
        VertexBuffer instanceVertexBuffer;

        Matrix[] instanceMatrixData;
        float[] aplha;

        public float[] Aplha
        {
            get { return aplha; }
            set { aplha = value; }
        }

        int vertexCount;
        int primCount;

        bool alphaEnabled;

        public VertexDeclaration Vertex;

        public Matrix[] InstanceMatrixData
        {
            get { return instanceMatrixData; }
            set { instanceMatrixData = value; }
        }

        public InstanceGroup(Device device, TransformsManager transfrom, int itemCount, Objects.MeshInformation meshInfo, bool enableAlpha)
        {
            alphaEnabled = enableAlpha;

            SetUpEffect(device, transfrom, itemCount);
            texture = meshInfo.Textures[0];

            // Create Vertices /////////////////
            // Lock the mesh vertex buffer and read PositionNormalTextured data
            int meshVertexCount = meshInfo.ObjMesh.NumberVertices;
            int meshStride = Marshal.SizeOf(typeof(CustomVertex.PositionNormalTextured));
            var meshVBData = meshInfo.ObjMesh.VertexBuffer.Lock(0, meshVertexCount * meshStride, LockFlags.ReadOnly);
            CustomVertex.PositionNormalTextured[] meshVertices = meshVBData.ReadRange<CustomVertex.PositionNormalTextured>(meshVertexCount);
            meshInfo.ObjMesh.VertexBuffer.Unlock();

            vertexCount = meshVertices.Length * itemCount;

            int instanceStride = ShaderInstancingVertex.StrideSize;
            instanceVertexBuffer = new VertexBuffer(device, vertexCount * instanceStride,
                Usage.WriteOnly, ShaderInstancingVertex.Format, Pool.Default);

            ShaderInstancingVertex[] instanceVertices = new ShaderInstancingVertex[vertexCount];

            // create new vertices for batch with instance index
            for (int instance = 0; instance < itemCount; instance++)
            {
                for (int vertex = 0; vertex < meshVertices.Length; vertex++)
                {
                    int instanceVertex = instance * meshVertices.Length + vertex;
                    instanceVertices[instanceVertex] = new ShaderInstancingVertex(meshVertices[vertex], instance);
                }
            }

            var dsVB = instanceVertexBuffer.Lock(0, 0, LockFlags.None);
            dsVB.WriteRange(instanceVertices);
            instanceVertexBuffer.Unlock();


            // Create Indices //////////////////
            int meshFaceCount = meshInfo.ObjMesh.NumberFaces;
            int meshIndexCount = meshFaceCount * 3;
            var meshIBData = meshInfo.ObjMesh.IndexBuffer.Lock(0, meshIndexCount * sizeof(short), LockFlags.ReadOnly);
            short[] meshIndices = meshIBData.ReadRange<short>(meshIndexCount);
            meshInfo.ObjMesh.IndexBuffer.Unlock();

            short[] instanceIndices = new short[itemCount * meshIndices.Length];
            primCount = itemCount * meshFaceCount;

            instanceIndexBuffer = new IndexBuffer(device, instanceIndices.Length * sizeof(short),
                Usage.WriteOnly, Pool.Default, true);

            // copy indices from original buffer
            for (int instance = 0; instance < itemCount; instance++)
            {
                for (int index = 0; index < meshIndices.Length; index++)
                {
                    int instanceIndex = instance * meshIndices.Length + index;
                    instanceIndices[instanceIndex] = (short)(meshIndices[index] + instance * meshInfo.ObjMesh.NumberVertices);
                }
            }

            var dsIB = instanceIndexBuffer.Lock(0, 0, LockFlags.None);
            dsIB.WriteRange(instanceIndices);
            instanceIndexBuffer.Unlock();
        }

        private void SetUpEffect(Device device, TransformsManager transfrom, int itemCount)
        {
            this.device = device;
            this.transfrom = transfrom;

            Vertex = new VertexDeclaration(device, ShaderInstancingVertex.Declaration);

            instanceMatrixData = new Matrix[itemCount];

            string file;
            if (alphaEnabled)
            {
                aplha = new float[itemCount];
                file = @"..\..\Resources\InstancingAlpha.fx";
            }
            else
            {
                file = @"..\..\Resources\Instancing.fx";
            }

            // Reset
            for (int index = 0; index < itemCount; index++)
            {
                instanceMatrixData[index] = Matrix.Identity;
            }

            // Load Effect
            instancingEffect = Effect.FromFile(device, file, null, ShaderFlags.None, null);
            instancingEffect.Technique = new EffectHandle("ShaderInstancing");

            viewProjection = instancingEffect.GetParameter(null, "ViewProjection");
            instanceData = instancingEffect.GetParameter(null, "InstanceData");
            textureData = instancingEffect.GetParameter(null, "Texture");

            if (alphaEnabled)
            {
                instanceAlpha = instancingEffect.GetParameter(null, "InstanceAlpha");
            }
        }

        public void SetMatrixArray()
        {
            instancingEffect.SetValue(instanceData, instanceMatrixData);
        }

        public void SetAlphaArray()
        {
            if (!alphaEnabled)
            { throw new Exception("Aplha not enabled"); }
            instancingEffect.SetValue(instanceAlpha, aplha);
        }

        public void Render()
        {

            device.SetStreamSource(0, instanceVertexBuffer, 0, ShaderInstancingVertex.StrideSize);
            device.Indices = instanceIndexBuffer;
            device.VertexFormat = ShaderInstancingVertex.Format;
            device.VertexDeclaration = Vertex;

            // Update the effect
            instancingEffect.SetValue(viewProjection, transfrom.View * transfrom.Projection);
            instancingEffect.SetValue(textureData, texture);

            // Begin rendering with the effect
            instancingEffect.Begin(FX.None);

            //Render the first pass
            instancingEffect.BeginPass(0);

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primCount);

            instancingEffect.EndPass();

            instancingEffect.End();

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderInstancingVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public float Tu;
            public float Tv;
            public float Instance;

            public ShaderInstancingVertex(CustomVertex.PositionNormalTextured vertex, float inst)
            {
                this.Position = vertex.Position;
                this.Normal = vertex.Normal;
                this.Tu = vertex.Tu;
                this.Tv = vertex.Tv;
                this.Instance = inst;
            }

            public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1 | VertexFormat.Texture2;
            public static readonly VertexElement[] Declaration = new VertexElement[]
	        {
		        new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
		        new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
		        new VertexElement(0, 24, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
		        new VertexElement(0, 32, DeclarationType.Float1, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 1),
		        VertexElement.VertexDeclarationEnd
	        };

            public static int StrideSize = Marshal.SizeOf(typeof(ShaderInstancingVertex));

        }
    }
}
