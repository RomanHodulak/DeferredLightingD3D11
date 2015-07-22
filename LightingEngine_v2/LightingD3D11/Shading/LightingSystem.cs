using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    public abstract class LightingSystem : IDisposable
    {
        public Renderer Renderer { get; set; }
        public List<Light> Lights { get; set; }

        public LightingSystem(Renderer Renderer)
        {
            this.Renderer = Renderer;
            this.Lights = new List<Light>();
        }

        public virtual void Initialize()
        {
        }

        public virtual void OnBackBufferSampleDescChanged()
        {
        }

        public virtual void OnBackBufferSizeChanged()
        {
        }

        public virtual void Begin(DeviceContext Context)
        {
        }

        public virtual void End(List<Model> submittedModels)
        {
        }

        public virtual void Dispose()
        {
        }
    }

    public enum PixelShaderOutputComponent
    {
        Empty,
        Zero,
        One,
        ColorR,
        ColorG,
        ColorB,
        ColorA,
        NormalR,
        NormalG,
        NormalB,
        SpecularR,
        wPositionX,
        wPositionY,
        wPositionZ,
        wPositionW,
        Depth,
        RoughnessR,
        RoughnessB,
    }

    public class PixelShaderOutput
    {
        public PixelShaderOutputComponent R { get; set; }
        public PixelShaderOutputComponent G { get; set; }
        public PixelShaderOutputComponent B { get; set; }
        public PixelShaderOutputComponent A { get; set; }
        public Format RenderTargetFormat { get; set; }

        public int ComponentsCount
        {
            get
            {
                int c = 0;
                if (R != PixelShaderOutputComponent.Empty)
                    c++;
                if (G != PixelShaderOutputComponent.Empty)
                    c++;
                if (B != PixelShaderOutputComponent.Empty)
                    c++;
                if (A != PixelShaderOutputComponent.Empty)
                    c++;
                return c;
            }
        }

        public PixelShaderOutput(PixelShaderOutputComponent R, PixelShaderOutputComponent G, PixelShaderOutputComponent B, PixelShaderOutputComponent A, Format Format)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
            this.RenderTargetFormat = Format;
        }
    }
}
