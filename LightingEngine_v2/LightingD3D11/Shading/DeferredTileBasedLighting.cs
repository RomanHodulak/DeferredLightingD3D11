using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    public class DeferredTileBasedLighting : LightingSystem
    {
        int maxLights = 1;
        PointLightStruct[] plStruct;
        public StructuredBuffer<PointLightStruct> pLightBuffer;
        PixelShader PS_GBuffer;

        public Texture2D GBuffer1TEX, GBuffer2TEX, GBuffer3TEX;
        ShaderResourceView GBuffer1SRV, GBuffer2SRV, GBuffer3SRV;
        RenderTargetView GBuffer1, GBuffer2, GBuffer3;

        public DeferredTileBasedLighting(Renderer Renderer)  : base(Renderer)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            plStruct = new PointLightStruct[maxLights];
            pLightBuffer = new StructuredBuffer<PointLightStruct>(maxLights, false, true);
            string path = @"R:\Users\Rox Cox\Documents\Visual Studio 2013\Projects\LightingEngine_v2\LightingEngine_v2\LightingD3D11\HLSL\";
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "gbuffer.hlsl", "GBufferPS", "ps_5_0", ShaderFlags.Debug, EffectFlags.None, null, new IncludeFX(path)))
            {
                PS_GBuffer = new PixelShader(Renderer.Device, bytecode);
            }
            InitializeGBuffer();
        }

        public override void OnBackBufferSampleDescChanged()
        {
            base.OnBackBufferSampleDescChanged();
            InitializeGBuffer();
        }

        public override void Begin(DeviceContext Context)
        {
            base.Begin(Context);
            if (pLightBuffer.ElementsCount != Lights.Count() & Lights.Count() != 0)
            {
                maxLights = Lights.Count();
                pLightBuffer.Resize(Lights.Count());
                plStruct = new PointLightStruct[maxLights];
            }
            for (int i = 0; i < maxLights; i++)
            {
                if (Lights.Count() - 1 < i)
                    plStruct[i] = default(PointLightStruct);
                else if (Lights[i] is PointLight)
                {
                    PointLight l = Lights[i] as PointLight;
                    plStruct[i].positionView = MathHelper.ToVector3(Vector3.Transform(l.Position, Renderer.Camera.MatrixView));
                    plStruct[i].attenuationBegin = 0.1f;
                    plStruct[i].color = l.Color;
                    plStruct[i].attenuationEnd = l.Radius;
                }
            }
            pLightBuffer.FillBuffer(Renderer.Context, plStruct);
            Context.PixelShader.Set(PS_GBuffer);
            Context.PixelShader.SetShaderResource(pLightBuffer.ShaderResource, 5);

            // Set up render GBuffer render targets
            //Context.OutputMerger.DepthStencilState = mDepthState;
            Context.OutputMerger.SetTargets(Renderer.BackBufferDepth, GBuffer1, GBuffer2, GBuffer3);
            //Context.OutputMerger.BlendState = (mGeometryBlendState, 0, 0xFFFFFFFF);
        }

        ComputeShader computeShader;
        public override void End(List<Model> submittedModels)
        {
            base.End(submittedModels);
            // No need to clear, we write all pixels
        
            // Compute shader setup (always does all the lights at once)
            //d3dDeviceContext->CSSetConstantBuffers(0, 1, &mPerFrameConstants);
            //d3dDeviceContext->CSSetShaderResources(0, static_cast<UINT>(mGBufferSRV.size()), &mGBufferSRV.front());
            //d3dDeviceContext->CSSetShaderResources(5, 1, &lightBufferSRV);

            //ID3D11UnorderedAccessView *litBufferUAV = mLitBufferCS->GetUnorderedAccess();
            //d3dDeviceContext->CSSetUnorderedAccessViews(0, 1, &litBufferUAV, 0);
           // d3dDeviceContext->CSSetShader(mComputeShaderTileCS->GetShader(), 0, 0);

            // Dispatch
            //unsigned int dispatchWidth = (mGBufferWidth + COMPUTE_SHADER_TILE_GROUP_DIM - 1) / COMPUTE_SHADER_TILE_GROUP_DIM;
            //unsigned int dispatchHeight = (mGBufferHeight + COMPUTE_SHADER_TILE_GROUP_DIM - 1) / COMPUTE_SHADER_TILE_GROUP_DIM;
            //d3dDeviceContext->Dispatch(dispatchWidth, dispatchHeight, 1);

            //DeviceContext Context = Renderer.Context;
            //Context.ComputeShader.Set(computeShader);
            //Context.ComputeShader.SetConstantBuffer(, 0);
        }

        public void InitializeGBuffer()
        {
            Helper.Dispose(GBuffer1, GBuffer1SRV, GBuffer1TEX, GBuffer2, GBuffer2SRV, GBuffer2TEX, GBuffer3, GBuffer3SRV, GBuffer3TEX);
            //normal_specular
            GBuffer1TEX = new Texture2D(Renderer.Device, new Texture2DDescription()
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
            });
            //albedo
            GBuffer2TEX = new Texture2D(Renderer.Device, new Texture2DDescription()
            {
                Width = Renderer.BufferWidth,
                Height = Renderer.BufferHeight,
                SampleDescription = Renderer.BufferSampleDescription,
                Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
            });
            //positionZgrad
            GBuffer3TEX = new Texture2D(Renderer.Device, new Texture2DDescription()
            {
                Width = Renderer.BufferWidth,
                Height = Renderer.BufferHeight,
                SampleDescription = Renderer.BufferSampleDescription,
                Format = SlimDX.DXGI.Format.R16G16_Float,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
            });
            GBuffer1SRV = new ShaderResourceView(Renderer.Device, GBuffer1TEX, new ShaderResourceViewDescription()
            {
                Dimension = GBuffer1TEX.Description.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
                Format = GBuffer1TEX.Description.Format,
                ArraySize = 1,
                MipLevels = 1,
                FirstArraySlice = 0,
                MostDetailedMip = 0
            });
            GBuffer2SRV = new ShaderResourceView(Renderer.Device, GBuffer1TEX, new ShaderResourceViewDescription()
            {
                Dimension = GBuffer1TEX.Description.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
                Format = GBuffer1TEX.Description.Format,
                ArraySize = 1,
                MipLevels = 1,
                FirstArraySlice = 0,
                MostDetailedMip = 0
            });
            GBuffer3SRV = new ShaderResourceView(Renderer.Device, GBuffer1TEX, new ShaderResourceViewDescription()
            {
                Dimension = GBuffer1TEX.Description.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
                Format = GBuffer1TEX.Description.Format,
                ArraySize = 1,
                MipLevels = 1,
                FirstArraySlice = 0,
                MostDetailedMip = 0
            });
            GBuffer1 = new RenderTargetView(Renderer.Device, GBuffer1TEX);
            GBuffer2 = new RenderTargetView(Renderer.Device, GBuffer2TEX);
            GBuffer3 = new RenderTargetView(Renderer.Device, GBuffer3TEX);
        }

        public override void Dispose()
        {
            base.Dispose();
            Helper.Dispose(pLightBuffer, PS_GBuffer, GBuffer1, GBuffer1SRV, GBuffer1TEX, GBuffer2, GBuffer2SRV, GBuffer2TEX, GBuffer3, GBuffer3SRV, GBuffer3TEX);
        }
    }
}
