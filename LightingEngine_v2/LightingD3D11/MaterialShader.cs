using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX;
using Buffer = SlimDX.Direct3D11.Buffer;
using System.Xml.Serialization;

namespace LightingEngine_v2.LightingD3D11
{
    public class MaterialShader : IDisposable
    {
        public Renderer Renderer { get; set; }

        public HullShader HullShader { get; set; }
        public PixelShader PixelShader { get; set; }
        public DomainShader DomainShader { get; set; }
        public VertexShader VertexShader { get; set; }
        public VertexShader VertexShader_Skinning { get; set; }
        public GeometryShader GeometryShader { get; set; }
        public InputLayout StaticMeshInputLayout { get; set; }
        public InputLayout SkinnedMeshInputLayout { get; set; }

        public VertexDefinition StaticMeshVertexDefinition { get; set; }
        public VertexDefinition SkinnedMeshVertexDefinition
        {
            get
            {
                VertexDefinition vd = new VertexDefinition();
                foreach (VertexParameterType vp in StaticMeshVertexDefinition.Parameters)
                    vd.Parameters.Add(vp);
                vd.Parameters.Add(VertexParameterType.Bones32Bit);
                vd.Parameters.Add(VertexParameterType.Weights);
                return vd;
            }
        }
        public PrimitiveTopology Topology { get; set; }
        public List<SamplerState> Samplers { get; set; }
        public List<ShaderResourceView> ShaderResources { get; set; }
        public List<ConstantBufferWrapper> ConstantBuffers { get; set; }

        public MaterialShader(Renderer Renderer)
        {
            this.Renderer = Renderer;
            Samplers = new List<SamplerState>();
            ShaderResources = new List<ShaderResourceView>();
            ConstantBuffers = new List<ConstantBufferWrapper>();
        }

        static MaterialShader defaultMaterial;

        public static MaterialShader DefaultMaterial(Renderer Renderer)
        {
            if (defaultMaterial == null)
                defaultMaterial = GenerateDefaultMaterial(Renderer);
            return defaultMaterial;
        }

        public static MaterialShader BasicMaterial(Renderer Renderer, Texture2D DiffuseMap)
        {
            MaterialShader m = GenerateDefaultMaterial(Renderer);
            m.ShaderResources[0].Dispose();
            m.ShaderResources[0] = new ShaderResourceView(Renderer.Device, DiffuseMap);
            m.ShaderResources[0].DebugName = DiffuseMap.DebugName;
            m.ShaderResources[1].Dispose();
            m.ShaderResources[1] = m.ShaderResources[0];
            return m;
        }

        public static MaterialShader BasicMaterial(Renderer Renderer, Texture2D DiffuseMap, Texture2D NormalMap)
        {
            MaterialShader m = GenerateDefaultMaterial(Renderer);
            m.ShaderResources[0].Dispose();
            m.ShaderResources[0] = new ShaderResourceView(Renderer.Device, DiffuseMap);
            m.ShaderResources[0].DebugName = DiffuseMap.DebugName;
            m.ShaderResources[1].Dispose();
            m.ShaderResources[1] = new ShaderResourceView(Renderer.Device, NormalMap);
            m.ShaderResources[1].DebugName = DiffuseMap.DebugName;
            return m;
        }

        static MaterialShader GenerateDefaultMaterial(Renderer Renderer)
        {
            string path = @"R:\Users\Rox Cox\Documents\Visual Studio 2013\Projects\LightingEngine_v2\LightingEngine_v2\LightingD3D11\HLSL\";
            ShaderFlags flags = ShaderFlags.Debug | ShaderFlags.EnableStrictness | ShaderFlags.PackMatrixRowMajor;
            MaterialShader m = new MaterialShader(Renderer);
            m.StaticMeshVertexDefinition = new VertexDefinition()
            {
                Parameters = new List<VertexParameterType>()
                {
                    VertexParameterType.Position,
                    VertexParameterType.Normal,
                    VertexParameterType.TextureCoordinate,
                    VertexParameterType.Tangent,
                    VertexParameterType.Binormal,
                }
            };
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "materialVS.hlsl", "VS", "vs_5_0", flags, EffectFlags.None))
            {
                m.VertexShader = new VertexShader(Renderer.Device, bytecode);
                m.StaticMeshInputLayout = m.StaticMeshVertexDefinition.GetInputLayout(bytecode);
            }
            /*using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "materialPS.hlsl", "ps_5_0"))
            {
                m.PixelShader = new PixelShader(Renderer.Device, bytecode);
            }*/
            m.Topology = PrimitiveTopology.TriangleList;
            m.Samplers.Add(SamplerState.FromDescription(Renderer.Device, new SamplerDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = new Color4(1, 1, 1, 1),
                ComparisonFunction = Comparison.Never,
                Filter = Filter.Anisotropic,
                MaximumAnisotropy = 16,
                MaximumLod = float.MaxValue,
                MinimumLod = float.MinValue,
                MipLodBias = 0,
            }));
            ConstantBufferWrapper bf0 = new ConstantBufferWrapper(Renderer, sizeof(float) * 64, ShaderType.VertexShader, 0);
            bf0.Semantics.Add(Semantic.World);
            bf0.Semantics.Add(Semantic.ViewProj);
            bf0.Semantics.Add(Semantic.View);
            bf0.Semantics.Add(Semantic.WorldView);
            m.ConstantBuffers.Add(bf0);
            using (Texture2D tex = ContentHelper.TextureFromBitmap(Properties.Resources.notexture))
            {
                m.ShaderResources.Add(new ShaderResourceView(Renderer.Device, tex));
            }
            using (Texture2D tex = ContentHelper.TextureFromBitmap(Properties.Resources.notexture_NORM))
            {
                m.ShaderResources.Add(new ShaderResourceView(Renderer.Device, tex));
            }

            return m;
        }

        public void Begin()
        {
            Begin(Renderer.Context);
        }

        public void Begin(DeviceContext Context)
        {
            Context.VertexShader.Set(VertexShader);
            //Context.PixelShader.Set(PixelShader);
        }

        public void SetBuffers(GeometricPrimitive Primitive)
        {
            SetBuffers(Primitive, Renderer.Context);
        }

        public void SetBuffers(GeometricPrimitive Primitive, DeviceContext Context)
        {
            if (Primitive is SkinnedMeshPrimitive)
            {
                if (VertexShader_Skinning == null)
                    return;
                else
                    Context.VertexShader.Set(VertexShader_Skinning);
            }
            foreach (ConstantBufferWrapper cbuffer in ConstantBuffers)
            {
                cbuffer.FillBuffer(Context, Primitive);
                if (cbuffer.ShaderType == ShaderType.VertexShader)
                    Context.VertexShader.SetConstantBuffer(cbuffer.CBuffer, cbuffer.ResourceSlot);
                if (cbuffer.ShaderType == ShaderType.PixelShader)
                    Context.PixelShader.SetConstantBuffer(cbuffer.CBuffer, cbuffer.ResourceSlot);
            }
            if (Primitive is SkinnedMeshPrimitive)
            {
                if ((Primitive as SkinnedMeshPrimitive).IsPlaying == false)
                    (Primitive as SkinnedMeshPrimitive).PlayAnimation(0, true);
                (Primitive as SkinnedMeshPrimitive).StepAnimation(0.002f);
                (Primitive as SkinnedMeshPrimitive).SetBoneMatrices(Renderer);
                Context.VertexShader.SetConstantBuffer((Primitive as SkinnedMeshPrimitive).BoneBuffer, 1);
            }
            for (int i = 0; i < Samplers.Count; i++)
            {
                Context.PixelShader.SetSampler(Samplers[i], i);
            }
            for (int i = 0; i < ShaderResources.Count; i++)
            {
                Context.PixelShader.SetShaderResource(ShaderResources[i], i);
            }
        }

        public void Draw(GeometricPrimitive Primitive)
        {
            Draw(Primitive, Renderer.Context);
        }

        public void Draw(GeometricPrimitive Primitive, DeviceContext Context)
        {
            Context.InputAssembler.PrimitiveTopology = Topology;
            if (Primitive is SkinnedMeshPrimitive)
                Context.InputAssembler.InputLayout = SkinnedMeshInputLayout;
            else
                Context.InputAssembler.InputLayout = StaticMeshInputLayout;
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(Primitive.GeometryData.VertexBuffer, Primitive.GeometryData.VertexStride, 0));
            Context.InputAssembler.SetIndexBuffer(Primitive.GeometryData.IndexBuffer, Format.R16_UInt, 0);

            Context.DrawIndexed(Primitive.GeometryData.IndexCount, 0, 0);
        }

        public void Dispose()
        {
            Helper.Dispose(HullShader, PixelShader, DomainShader, VertexShader, VertexShader_Skinning, GeometryShader, StaticMeshInputLayout, SkinnedMeshInputLayout);
            Helper.Dispose(ConstantBuffers.ToArray());
            Helper.Dispose(Samplers.ToArray());
            Helper.Dispose(ShaderResources.ToArray());
        }
    }
}
