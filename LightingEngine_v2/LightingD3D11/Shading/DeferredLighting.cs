#undef frustumCull
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
    public class DeferredLighting : LightingSystem
    {
        int maxLights = 1;
        PointLightStruct[] plStruct;
        public StructuredBuffer<PointLightStruct> pLightBuffer;
        VertexShader VS_FullscreenTriangle;
        PixelShader PS_PointLights, PS_PointLights_MS;

        GBuffer GBuffer;
        DepthStencilView ReadOnlyDepth;
        ConstantBufferWrapper cBuffer;
        private BlendState mGeometryBlendState;
        private BlendState mLightingBlendState;
        private DepthStencilState mEqualStencilState;
        private DepthStencilState mDepthState;
        private RasterizerState mRasterizerState;
        private DepthStencilState mWriteStencilState;

        public DeferredLighting(Renderer Renderer)
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
#if frustumCull
            maxLights = 0;
            List<PointLightStruct> pl = new List<PointLightStruct>();
            for (int i = 0; i < Lights.Count(); i++)
            {
                if (Lights[i] is PointLight)
                {
                    PointLight l = Lights[i] as PointLight;
                    Vector3 rad = new Vector3(l.Radius, l.Radius, l.Radius);
                    if (Renderer.Camera.ViewFrustum.Intersects(BoundingBox.FromPoints(new[] { l.Position, l.Position + rad, l.Position - rad })))
                    {
                        PointLightStruct s;
                        s.positionView = MathHelper.ToVector3(Vector3.Transform(l.Position, Renderer.Camera.MatrixView));
                        s.attenuationBegin = 0.1f;
                        s.color = l.Color;
                        s.attenuationEnd = l.Radius;
                        pl.Add(s);
                    }
                }
            }
            maxLights = pl.Count();
            if (pLightBuffer.ElementsCount != maxLights & maxLights != 0)
            {
                pLightBuffer.Resize(maxLights);
            }
            pLightBuffer.FillBuffer(Renderer.Context, pl.ToArray());
#else
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
#endif
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
            //Targets still set to GBuffer. Therefore depth would get cleard when set to shader SRV
            Context.OutputMerger.SetTargets();
            Context.ClearRenderTargetView(Renderer.BackBufferColor, new Color4(0, 0, 0, 0));

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

            if (Renderer.BufferSampleDescription.Count > 1)
            {
                // Set stencil mask for samples that require per-sample shading
                Context.PixelShader.Set(GBuffer.PixelShaderMS);
                Context.OutputMerger.DepthStencilState = mWriteStencilState;
                Context.OutputMerger.DepthStencilReference = 1;
                Context.OutputMerger.SetTargets(ReadOnlyDepth);
                Context.Draw(3, 0);
            }

            // Additively blend into back buffer
            Context.OutputMerger.SetTargets(ReadOnlyDepth, Renderer.BackBufferColor);
            Context.OutputMerger.BlendState = mLightingBlendState;
            Context.OutputMerger.BlendFactor = new Color4(0, 0, 0, 0);
            Context.OutputMerger.BlendSampleMask = int.MaxValue;

            // Do pixel frequency shading
            Context.PixelShader.Set(PS_PointLights);
            Context.OutputMerger.DepthStencilState = mEqualStencilState;
            Context.OutputMerger.DepthStencilReference = 0;
            Context.Draw(3, 0);

            if (Renderer.BufferSampleDescription.Count > 1)
            {
                // Do sample frequency shading
                Context.PixelShader.Set(PS_PointLights_MS);
                Context.OutputMerger.DepthStencilState = mEqualStencilState;
                Context.OutputMerger.DepthStencilReference = 1;
                Context.Draw(3, 0);
            }

            Context.OutputMerger.DepthStencilState = mDepthState;
            Context.OutputMerger.BlendState = mGeometryBlendState;
        }

        public void InitializeShaders()
        {
            Helper.Dispose(VS_FullscreenTriangle, PS_PointLights, PS_PointLights_MS);
            string path = @"LightingD3D11\HLSL\";
            ShaderMacro[] macros = new[] { new ShaderMacro("MSAA_SAMPLES", Renderer.BufferSampleDescription.Count.ToString()) };
            ShaderFlags flags = ShaderFlags.Debug | ShaderFlags.EnableStrictness | ShaderFlags.PackMatrixRowMajor;

            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "fullscreenVS.hlsl", "FullScreenTriangleVS", "vs_5_0", flags, EffectFlags.None, null, new IncludeFX(path)))
            {
                VS_FullscreenTriangle = new VertexShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "deferred.hlsl", "DeferredPS", "ps_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                PS_PointLights = new PixelShader(Renderer.Device, bytecode);
            }
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "deferred.hlsl", "DeferredPerSamplePS", "ps_5_0", flags, EffectFlags.None, macros, new IncludeFX(path)))
            {
                PS_PointLights_MS = new PixelShader(Renderer.Device, bytecode);
            }
        }

        public void InitializeBuffers()
        {
            Helper.Dispose(ReadOnlyDepth);
            DepthStencilViewDescription dsvDesc = Renderer.BackBufferDepth.Description;
            dsvDesc.Flags = DepthStencilViewFlags.ReadOnlyDepth;
            ReadOnlyDepth = new DepthStencilView(Renderer.Device, Renderer.OutputDepth, dsvDesc);
            GBuffer.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
            Helper.Dispose(pLightBuffer, GBuffer, cBuffer, ReadOnlyDepth, VS_FullscreenTriangle, PS_PointLights, PS_PointLights_MS, 
                mWriteStencilState, mRasterizerState, mGeometryBlendState, mLightingBlendState, mDepthState, mEqualStencilState);
        }
    }
}
