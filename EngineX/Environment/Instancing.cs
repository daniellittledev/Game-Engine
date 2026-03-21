using System;
using System.Collections.Generic;
using System.Text;

using Microsoft;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using System.Windows.Forms;
using System.Drawing;
using System.IO;

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
            // Mesh will typically use this PositionNormalTextured format
            CustomVertex.PositionNormalTextured[] meshVertices = (CustomVertex.PositionNormalTextured[])meshInfo.ObjMesh.VertexBuffer.Lock
                (0, typeof(CustomVertex.PositionNormalTextured), LockFlags.ReadOnly, meshInfo.ObjMesh.NumberVertices);

            vertexCount = meshVertices.Length * itemCount;

            instanceVertexBuffer = new VertexBuffer(typeof(ShaderInstancingVertex),
                vertexCount, device, Usage.WriteOnly,
                ShaderInstancingVertex.Format, Pool.Default); // items * verts

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


            instanceVertexBuffer.SetData(instanceVertices, 0, LockFlags.None);
            //instanceVertexBuffer.Unlock();

            meshInfo.ObjMesh.VertexBuffer.Unlock();


            // Create Indices //////////////////
            instanceIndexBuffer = new IndexBuffer(typeof(short), itemCount * meshInfo.ObjMesh.NumberFaces * 3,
                device, Usage.WriteOnly, Pool.Default);

            // Create the index buffer
            short[] meshIndices = (short[])meshInfo.ObjMesh.IndexBuffer.Lock(0, typeof(short),
                LockFlags.ReadOnly, meshInfo.ObjMesh.NumberFaces * 3);
            short[] instanceIndices = new short[itemCount * meshIndices.Length];

            primCount = itemCount * meshInfo.ObjMesh.NumberFaces;

            // copy indices from original buffer
            for (int instance = 0; instance < itemCount; instance++)
            {
                for (int index = 0; index < meshIndices.Length; index++)
                {
                    int instanceIndex = instance * meshIndices.Length + index;
                    instanceIndices[instanceIndex] = (short)(meshIndices[index] + instance * meshInfo.ObjMesh.NumberVertices);
                }
            }

            instanceIndexBuffer.SetData(instanceIndices, 0, LockFlags.None);
            //instanceIndexBuffer.Unlock();
            meshInfo.ObjMesh.IndexBuffer.Unlock();

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
            string s;
            instancingEffect = Effect.FromFile(device, file, null, "", ShaderFlags.None, null, out s);
            if (s != "")
            {
                MessageBox.Show(s);
                Application.Exit();
                return;

            }

            instancingEffect.Technique = "ShaderInstancing";

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
            //instanceData
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

            public static readonly VertexFormats Format = VertexFormats.Position | VertexFormats.Normal | VertexFormats.Texture0 | VertexFormats.Texture1;
            public static readonly VertexElement[] Declaration = new VertexElement[]
		{
			new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
			new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
			new VertexElement(0, 24, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
			new VertexElement(0, 32, DeclarationType.Float1, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 1),
			VertexElement.VertexDeclarationEnd
		};

            public static int StrideSize = VertexInformation.GetDeclarationVertexSize(Declaration, 0);

        }
    }
}
