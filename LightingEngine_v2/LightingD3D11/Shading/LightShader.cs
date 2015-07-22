using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;

namespace LightingEngine_v2.LightingD3D11
{
    public class LightShader : IDisposable
    {
        public Renderer Renderer { get; set; }

        public PixelShader PixelShader { get; set; }
        public PixelShader PixelShaderShadowless { get; set; }
        public VertexShader VertexShader { get; set; }
        public InputLayout InputLayout { get; set; }

        public VertexDefinition VertexDefinition
        {
            get
            {
                VertexDefinition def = new VertexDefinition();
                def.Parameters.Add(VertexParameterType.Position);
                return def;
            }
        }
        public PrimitiveTopology Topology { get; set; }
        public List<SamplerState> Samplers { get; set; }
        public List<ShaderResourceView> ShaderResources { get; set; }
        public List<ConstantBufferWrapper> ConstantBuffers { get; set; }

        public LightShader(Renderer Renderer)
        {
            this.Renderer = Renderer;
            Samplers = new List<SamplerState>();
            ShaderResources = new List<ShaderResourceView>();
            ConstantBuffers = new List<ConstantBufferWrapper>();
        }

        public void Begin(bool shadowless)
        {
            DeviceContext Context = Renderer.Context;
            Context.VertexShader.Set(VertexShader);
            if (shadowless)
                Context.PixelShader.Set(PixelShaderShadowless);
            else
                Context.PixelShader.Set(PixelShader);
            Context.HullShader.Set(null);
            Context.DomainShader.Set(null);
            Context.GeometryShader.Set(null);
        }

        public void Dispose()
        {
            Helper.Dispose(PixelShader, PixelShaderShadowless, VertexShader);
            Helper.Dispose(ShaderResources);
            Helper.Dispose(Samplers);
            Helper.Dispose(ConstantBuffers);
            Helper.Dispose(InputLayout);
        }
    }
}
