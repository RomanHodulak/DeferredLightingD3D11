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
    public class DeferredQuadLighting : LightingSystem
    {
        int maxLights = 1;
        PointLightStruct[] plStruct;
        public StructuredBuffer<PointLightStruct> pLightBuffer;
        VertexShader VS_FullscreenTriangle;

        GBuffer GBuffer;
        DepthStencilView ReadOnlyDepth;
        ConstantBufferWrapper cBuffer;
        private BlendState mGeometryBlendState;
        private BlendState mLightingBlendState;
        private DepthStencilState mEqualStencilState;
        private DepthStencilState mDepthState;
        private RasterizerState mRasterizerState;
        private DepthStencilState mWriteStencilState;
        private PixelShader mGPUQuadPS;
        private PixelShader mGPUQuadPerSamplePS;
        private VertexShader mGPUQuadVS;
        private GeometryShader mGPUQuadGS;
        ShaderResourceView litBuffer;
        PixelShader simplePS;
        RenderTargetView litTarget;

        public DeferredQuadLighting(Renderer Renderer)
            : base(Renderer)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            plStruct = new PointLightStruct[maxLights];
            pLightBuffer = new StructuredBuffer<PointLightStruct>(maxLights, false, true);
            cBuffer = new ConstantBufferWrapper(Renderer, sizeof(float) * 32, ShaderType.PixelShader, 0);
            cBuffer.Semantics.Add(Semantic.Projection);
            cBuffer.Semantics.Add(Semantic.CameraNearFar);
            GBuffer = new GBuffer(Renderer);

            BlendStateDescription disabledBlendDesc = States.BlendDefault();
            BlendStateDescription additiveDesc = States.BlendDefault();
            additiveDesc.RenderTargets[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = BlendOption.One,
                DestinationBlendAlpha = BlendOption.One,
                BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
            };
            DepthStencilStateDescription defaultDepthDesc = States.DepthDefault();
            defaultDepthDesc.DepthComparison = Comparison.GreaterEqual;
            DepthStencilStateDescription equalDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.GreaterEqual,
                IsStencilEnabled = true,
                StencilWriteMask = 0xFF,
                StencilReadMask = 0xFF,
                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Equal,
                },
                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Equal,
                }
            };
            DepthStencilStateDescription writeDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.GreaterEqual,
                IsStencilEnabled = true,
                StencilWriteMask = 0xFF,
                StencilReadMask = 0xFF,
                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Replace,
                    DepthFailOperation = StencilOperation.Replace,
                    PassOperation = StencilOperation.Replace,
                    Comparison = Comparison.Always,
                },
                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Replace,
                    DepthFailOperation = StencilOperation.Replace,
                    PassOperation = StencilOperation.Replace,
                    Comparison = Comparison.Always,
                }
            };
            RasterizerStateDescription rsDesc = States.RasterizerDefault();

            mGeometryBlendState = BlendState.FromDescription(Renderer.Device, disabledBlendDesc);
            mLightingBlendState = BlendState.FromDescription(Renderer.Device, additiveDesc);
            mDepthState = DepthStencilState.FromDescription(Renderer.Device, defaultDepthDesc);
            mEqualStencilState = DepthStencilState.FromDescription(Renderer.Device, equalDesc);
            mWriteStencilState = DepthStencilState.FromDescription(Renderer.Device, writeDesc);
            mRasterizerState = RasterizerState.FromDescription(Renderer.Device, rsDesc);

            InitializeBuffers();
            InitializeShaders();
        }

        public override void OnBackBufferSampleDescChanged()
        {
            base.OnBackBufferSampleDescChanged();
            InitializeBuffers();
            InitializeShaders();
        }

        public override void OnBackBufferSizeChanged()
        {
            base.OnBackBufferSizeChanged();
            InitializeBuffers();
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
            cBuffer.FillBuffer(Context);
            Context.PixelShader.Set(GBuffer.PixelShader);

            // Set up render GBuffer render targets
            Context.OutputMerger.SetTargets(Renderer.BackBufferDepth, GBuffer.RenderTargets);
            Context.OutputMerger.DepthStencilState = mDepthState;
            Context.OutputMerger.BlendState = mGeometryBlendState;
        }

        public override void End(List<Model> submittedModels)
        {
            base.End(submittedModels);
            DeviceContext Context = Renderer.Context;
            Context.OutputMerger.SetTargets();
            Context.ClearRenderTargetView(litTarget, new Color4(0, 0, 0, 0));
        
            if (Renderer.BufferSampleDescription.Count > 1) 
            {
                // Full screen triangle setup
                Context.InputAssembler.InputLayout = null;
                Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));

                Context.VertexShader.Set(VS_FullscreenTriangle);
                Context.GeometryShader.Set(null);

                Context.Rasterizer.State = mRasterizerState;
                Context.Rasterizer.SetViewports(new Viewport(0, 0, Renderer.BufferWidth, Renderer.BufferHeight));
                
                Context.PixelShader.SetConstantBuffer(cBuffer.CBuffer, 0);
                Context.PixelShader.SetShaderResources(GBuffer.ShaderResourceViews, 0, 4);
                Context.PixelShader.SetShaderResource(pLightBuffer.ShaderResource, 5);

                // Set stencil mask for samples that require per-sample shading
                Context.PixelShader.Set(GBuffer.PixelShaderMS);
                Context.OutputMerger.DepthStencilState = mWriteStencilState;
                Context.OutputMerger.DepthStencilReference = 1;
                Context.OutputMerger.SetTargets(ReadOnlyDepth);
                Context.Draw(3, 0);
            }

            // Point primitives expanded into quads in the geometry shader
            Context.InputAssembler.InputLayout = null;
            Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
            
            Context.VertexShader.SetConstantBuffer(cBuffer.CBuffer, 0);
            Context.VertexShader.SetShaderResource(pLightBuffer.ShaderResource, 5);
            Context.VertexShader.Set(mGPUQuadVS);
            
            Context.GeometryShader.Set(mGPUQuadGS);
            
            Context.Rasterizer.State = mRasterizerState;
            Context.Rasterizer.SetViewports(new Viewport(0, 0, Renderer.BufferWidth, Renderer.BufferHeight));
            
            Context.PixelShader.SetConstantBuffer(cBuffer.CBuffer, 0);
            Context.PixelShader.SetShaderResources(GBuffer.ShaderResourceViews, 0, 4);
            Context.PixelShader.SetShaderResource(pLightBuffer.ShaderResource, 5);

            // Additively blend into back buffer
            Context.OutputMerger.SetTargets(ReadOnlyDepth, litTarget);
            Context.OutputMerger.BlendState = mLightingBlendState;
            Context.OutputMerger.BlendFactor = new Color4(0, 0, 0, 0);
            Context.OutputMerger.BlendSampleMask = int.MaxValue;
        
            // Dispatch one point per light

            // Do pixel frequency shading
            Context.PixelShader.Set(mGPUQuadPS);
            Context.OutputMerger.DepthStencilState = mEqualStencilState;
            Context.OutputMerger.DepthStencilReference = 0;
            Context.Draw(Lights.Count(), 0);
        
            if (Renderer.BufferSampleDescription.Count > 1) 
            {
                // Do sample frequency shading
                Context.PixelShader.Set(mGPUQuadPerSamplePS);
                Context.OutputMerger.DepthStencilState = mEqualStencilState;
                Context.OutputMerger.DepthStencilReference = 1;
                Context.Draw(Lights.Count(), 0);
            }

            Context.OutputMerger.BlendState = mGeometryBlendState;
            Context.OutputMerger.DepthStencilState = mDepthState;
            Context.OutputMerger.DepthStencilReference = 0;
            Context.GeometryShader.Set(null);

            //Final pass
            Context.OutputMerger.SetTargets(ReadOnlyDepth, Renderer.BackBufferColor);

            Context.InputAssembler.InputLayout = null;
            Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));

            Context.VertexShader.Set(VS_FullscreenTriangle);

            Context.PixelShader.Set(simplePS);
            Context.PixelShader.SetShaderResource(litBuffer, 7);
            Context.Draw(3, 0);
        }

        public void InitializeShaders()
        {
            Helper.Dispose(VS_FullscreenTriangle, simplePS, mGPUQuadGS, mGPUQuadPerSamplePS, mGPUQuadPS, mGPUQuadVS);
            string path = @"LightingD3D11\HLSL\";
            ShaderMacro[] macros = new[] { new ShaderMacro("MSAA_SAMPLES", Renderer.BufferSampleDescription.Count.ToString()) };
            ShaderFlags flags = ShaderFlags.Debug | ShaderFlags.EnableStrictness | ShaderFlags.PackMatrixRowMajor;

            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "fullscreenVS.hlsl", "FullScreenTriangleVS", "vs_5_0", flags, EffectFlags.None, null, new IncludeFX(path)))
            {
                VS_FullscreenTriangle = new VertexShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "deferredQuad.hlsl", "GPUQuadPS", "ps_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                mGPUQuadPS = new PixelShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "deferredQuad.hlsl", "GPUQuadPerSamplePS", "ps_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                mGPUQuadPerSamplePS = new PixelShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "deferredQuad.hlsl", "GPUQuadVS", "vs_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                mGPUQuadVS = new VertexShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "deferredQuad.hlsl", "GPUQuadGS", "gs_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                mGPUQuadGS = new GeometryShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "simple.hlsl", "SkyboxPS", "ps_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                simplePS = new PixelShader(Renderer.Device, bytecode);
            }
        }

        public void InitializeBuffers()
        {
            Helper.Dispose(ReadOnlyDepth, litBuffer, litTarget);
            DepthStencilViewDescription dsvDesc = Renderer.BackBufferDepth.Description;
            dsvDesc.Flags = DepthStencilViewFlags.ReadOnlyDepth;
            ReadOnlyDepth = new DepthStencilView(Renderer.Device, Renderer.OutputDepth, dsvDesc);
            Texture2DDescription litBufferDesc = Renderer.OutputTexture.Description;
            //TODO: This format
            litBufferDesc.Format = SlimDX.DXGI.Format.R11G11B10_Float;
            using (Texture2D tex = new Texture2D(Renderer.Device, litBufferDesc))
            {
                litTarget = new RenderTargetView(Renderer.Device, tex);
                litBuffer = new ShaderResourceView(Renderer.Device, tex, new ShaderResourceViewDescription()
                {
                    Dimension = tex.Description.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
                    Format = tex.Description.Format,
                    ArraySize = 1,
                    MipLevels = 1,
                    FirstArraySlice = 0,
                    MostDetailedMip = 0
                });
            }
            GBuffer.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
            Helper.Dispose(pLightBuffer, GBuffer, cBuffer, litBuffer, litTarget, ReadOnlyDepth, VS_FullscreenTriangle, simplePS, mGPUQuadGS, mGPUQuadPerSamplePS, mGPUQuadPS, mGPUQuadVS, 
                mWriteStencilState, mRasterizerState, mGeometryBlendState, mLightingBlendState, mDepthState, mEqualStencilState);
        }
    }
}
