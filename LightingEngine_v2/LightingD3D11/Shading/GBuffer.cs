using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    public class GBuffer : IDisposable
    {
        public Renderer Renderer { get; set; }
        public PixelShader PixelShader, PixelShaderMS;
        public ShaderResourceView[] ShaderResourceViews;
        public RenderTargetView[] RenderTargets;

        public GBuffer(Renderer Renderer)
        {
            this.Renderer = Renderer;
            RenderTargets = new RenderTargetView[3];
            ShaderResourceViews = new ShaderResourceView[4];
        }

        public void Initialize()
        {
            Helper.Dispose(RenderTargets);
            Helper.Dispose(ShaderResourceViews);
            Helper.Dispose(PixelShader, PixelShaderMS);

            string path = @"LightingD3D11\HLSL\";
            ShaderMacro[] macros = new[] { new ShaderMacro("MSAA_SAMPLES", Renderer.BufferSampleDescription.Count.ToString()) };
            ShaderFlags flags = ShaderFlags.Debug | ShaderFlags.EnableStrictness | ShaderFlags.PackMatrixRowMajor;

            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "gbuffer.hlsl", "GBufferPS", "ps_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                PixelShader = new PixelShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "gbuffer.hlsl", "RequiresPerSampleShadingPS", "ps_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                PixelShaderMS = new PixelShader(Renderer.Device, bytecode);
            }

            Texture2DDescription gbuffer1Desc = new Texture2DDescription()
            {
                Width = Renderer.BufferWidth,
                Height = Renderer.BufferHeight,
                SampleDescription = Renderer.BufferSampleDescription,
                Format = SlimDX.DXGI.Format.R16G16B16A16_Float,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
            };
            Texture2DDescription gbuffer2Desc = gbuffer1Desc;
            gbuffer2Desc.Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm;
            Texture2DDescription gbuffer3Desc = gbuffer1Desc;
            gbuffer3Desc.Format = SlimDX.DXGI.Format.R16G16_Float;

            using (Texture2D normal_specular = new Texture2D(Renderer.Device, gbuffer1Desc))
            {
                RenderTargets[0] = new RenderTargetView(Renderer.Device, normal_specular);
                ShaderResourceViews[0] = new ShaderResourceView(Renderer.Device, normal_specular, new ShaderResourceViewDescription()
                {
                    Dimension = normal_specular.Description.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
                    Format = normal_specular.Description.Format,
                    ArraySize = 1,
                    MipLevels = 1,
                    FirstArraySlice = 0,
                    MostDetailedMip = 0
                });
            }
            using (Texture2D albedo = new Texture2D(Renderer.Device, gbuffer2Desc))
            {
                RenderTargets[1] = new RenderTargetView(Renderer.Device, albedo);
                ShaderResourceViews[1] = new ShaderResourceView(Renderer.Device, albedo, new ShaderResourceViewDescription()
                {
                    Dimension = albedo.Description.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
                    Format = albedo.Description.Format,
                    ArraySize = 1,
                    MipLevels = 1,
                    FirstArraySlice = 0,
                    MostDetailedMip = 0
                });
            }
            //This buffer affects only deferred MSAA and is optional
            using (Texture2D positionZgrad = new Texture2D(Renderer.Device, gbuffer3Desc))
            {
                RenderTargets[2] = new RenderTargetView(Renderer.Device, positionZgrad);
                ShaderResourceViews[2] = new ShaderResourceView(Renderer.Device, positionZgrad, new ShaderResourceViewDescription()
                {
                    Dimension = positionZgrad.Description.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
                    Format = positionZgrad.Description.Format,
                    ArraySize = 1,
                    MipLevels = 1,
                    FirstArraySlice = 0,
                    MostDetailedMip = 0
                });
            }
            ShaderResourceViews[3] = new ShaderResourceView(Renderer.Device, Renderer.OutputDepth, new ShaderResourceViewDescription()
            {
                Dimension = Renderer.OutputDepth.Description.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
                Format = Renderer.BufferSampleDescription.Count > 1 ? SlimDX.DXGI.Format.R32_Float_X8X24_Typeless : SlimDX.DXGI.Format.R32_Float,
                ArraySize = 1,
                MipLevels = 1,
                FirstArraySlice = 0,
                MostDetailedMip = 0
            });
            Renderer.Context.ClearRenderTargetView(RenderTargets[2], new Color4(1, 1, 1, 1));
        }

        public void Dispose()
        {
            Helper.Dispose(RenderTargets);
            Helper.Dispose(ShaderResourceViews);
            Helper.Dispose(PixelShader, PixelShaderMS);
        }
    }
}
