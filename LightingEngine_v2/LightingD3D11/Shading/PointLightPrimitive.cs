using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.D3DCompiler;
using System.IO;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    public class PointLightPrimitive : LightPrimitive
    {
        public PointLightPrimitive(Renderer Renderer)
        {
            this.Renderer = Renderer;
        }

        public override void SetBuffers(RenderableLightPrimitive primitive, Light light)
        {
            DeviceContext Context = Renderer.Context;
            PointLight pLight = light as PointLight;

            Context.InputAssembler.InputLayout = primitive.Shader.InputLayout;
            Context.InputAssembler.PrimitiveTopology = primitive.Shader.Topology;
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(primitive.VertexBuffer, 3 * sizeof(float), 0));
            Context.InputAssembler.SetIndexBuffer(primitive.IndexBuffer, Format.R16_UInt, 0);

            primitive.Shader.ConstantBuffers[0].FillBuffer(Context, Matrix.Scaling(pLight.Radius, pLight.Radius, pLight.Radius) * Matrix.Translation(pLight.Position));
            //primitive.Shader.ConstantBuffers[1].FillBuffer(Context, Matrix.Identity, pLight.Position, new Vector4(pLight.Color, pLight.Intensity));

            foreach (ConstantBufferWrapper cbuff in primitive.Shader.ConstantBuffers)
            {
                if (cbuff.ShaderType == ShaderType.VertexShader)
                    Context.VertexShader.SetConstantBuffer(cbuff.CBuffer, cbuff.ResourceSlot);
                if (cbuff.ShaderType == ShaderType.PixelShader)
                    Context.PixelShader.SetConstantBuffer(cbuff.CBuffer, cbuff.ResourceSlot);
            }

            //DeferredLighting LightingSystem = Renderer.LightingSystem as DeferredLighting;

            //Context.PixelShader.SetShaderResource(LightingSystem.PositionSRV, 0);
            //Context.PixelShader.SetShaderResource(LightingSystem.NormalSRV, 1);
            //Context.PixelShader.SetShaderResource(LightingSystem.LinearDepthSRV, 2);
        }

        public override void Draw(RenderableLightPrimitive primitive)
        {
            Renderer.Context.DrawIndexed(primitive.IndexCount, 0, 0);
        }

        public override RenderableLightPrimitive GetRenderablePrimitive()
        {
            LightShader shader = new LightShader(Renderer);

            using (ShaderBytecode bytecode = new ShaderBytecode(new DataStream(File.ReadAllBytes("Shaders\\DeferredPointLight.vs"), true, false)))
            {
                shader.VertexShader = new VertexShader(Renderer.Device, bytecode);
                shader.InputLayout = new InputLayout(Renderer.Device, bytecode, new[]
                {
                    new InputElement("Position", 0, Format.R32G32B32_Float, sizeof(float) * 0, 0),
                });
            }
            using (ShaderBytecode bytecode = new ShaderBytecode(new DataStream(File.ReadAllBytes("Shaders\\DeferredPointLightShadowless.ps"), true, false)))
            {
                shader.PixelShader = new PixelShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = new ShaderBytecode(new DataStream(File.ReadAllBytes("Shaders\\DeferredPointLightShadowless.ps"), true, false)))
            {
                shader.PixelShaderShadowless = new PixelShader(Renderer.Device, bytecode);
            }
            shader.Topology = PrimitiveTopology.TriangleList;

            ConstantBufferWrapper MatricesCBuffer = new ConstantBufferWrapper(Renderer, sizeof(float) * 32, ShaderType.VertexShader, 0);
            ConstantBufferWrapper LightCBuffer = new ConstantBufferWrapper(Renderer, sizeof(float) * 64, ShaderType.PixelShader, 0);
            ConstantBufferWrapper ShadowCBuffer = new ConstantBufferWrapper(Renderer, sizeof(float) * 16 * 4, ShaderType.PixelShader, 1);
            MatricesCBuffer.Semantics.Add(Semantic.WorldViewProj);
            MatricesCBuffer.Semantics.Add(Semantic.World);
            LightCBuffer.Semantics.Add(Semantic.View);
            LightCBuffer.Semantics.Add(Semantic.ViewInverse);
            LightCBuffer.Semantics.Add(Semantic.ViewProjInverse);
            LightCBuffer.Semantics.Add(Semantic.CameraPosition);
            shader.ConstantBuffers.Add(MatricesCBuffer);
            shader.ConstantBuffers.Add(LightCBuffer);
            shader.ConstantBuffers.Add(ShadowCBuffer);

            GeometricPrimitive prim = new SpherePrimitive(Renderer, 1, 16);

            DataStream str = new DataStream(prim.GeometryData.Positions, true, false);
            Buffer vertexBuffer = new Buffer(Renderer.Device, str, new BufferDescription()
            {
                SizeInBytes = (int)str.Length,
                BindFlags = BindFlags.VertexBuffer,
                StructureByteStride = 3 * sizeof(float),
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            });
            int vertexCount = prim.GeometryData.VertexCount;

            DataStream IndicesStream = new DataStream(prim.GeometryData.Indices.ToArray(), true, true);

            int indexCount = prim.GeometryData.IndexCount;
            Buffer indexBuffer = new Buffer(Renderer.Device, IndicesStream, sizeof(ushort) * indexCount,
                ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, sizeof(ushort));

            prim.Dispose();

            return new RenderableLightPrimitive(shader, vertexBuffer, vertexCount, indexBuffer, indexCount);
        }
    }
}
