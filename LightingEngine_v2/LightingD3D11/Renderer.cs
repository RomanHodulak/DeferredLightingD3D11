using LightingEngine_v2.LightingD3D11.Shading;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SlimDX.Direct3D11.Device;

namespace LightingEngine_v2.LightingD3D11
{
    public class Renderer : IDisposable
    {
        public static Device Device
        {
            get
            {
                if (device == null)
                    device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0);
                else if (device.Disposed)
                    device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0);
                return device;
            }
        }
        public DeviceContext Context { get { return Device.ImmediateContext; } }
        public SampleDescription BufferSampleDescription { get { return samples; } set { samples = value; SetupRenderTargets(); LightingSystem.OnBackBufferSampleDescChanged(); } }
        public RenderTargetView BackBufferColor { get { return backBufferColor; } }
        public DepthStencilView BackBufferDepth { get { return backBufferDepth; } }
        public Texture2D SharedTexture { get { if (BufferSampleDescription.Count > 1) return sharedTexture; return outputTexture; } }
        public Texture2D OutputTexture { get { return outputTexture; } }
        public Texture2D OutputDepth { get { return outputDepth; } }
        public int BufferWidth { get { return bufferWidth; } }
        public int BufferHeight { get { return bufferHeight; } }
        public Camera Camera { get; set; }
        IntPtr? OutputHandle { get { if (swapChain == null) return null; return swapChain.Description.OutputHandle; } }
        public LightingSystem LightingSystem;

        static Device device;
        static int deviceUsers = 0;
        SwapChain swapChain;
        bool hasHandle = false;
        RenderTargetView backBufferColor;
        DepthStencilView backBufferDepth;
        Texture2D sharedTexture;
        Texture2D outputTexture;
        Texture2D outputDepth;
        int bufferWidth;
        int bufferHeight;
        bool isRendering = false;
        int renderTargetsCount = 1;
        SampleDescription samples = new SampleDescription(1, 0);
        private MaterialShader ActiveMaterial;

        public Renderer(int Width, int Height) : this(Width, Height, null)
        {
        }

        public Renderer(int Width, int Height, IntPtr? OutputHandle)
        {
            if (Width < 1)
                Width = 1;
            if (Height < 1)
                Height = 1;
            bufferWidth = Width;
            bufferHeight = Height;
            deviceUsers++;
            if (device == null)
                device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0);
            if (OutputHandle.HasValue)
            {
                SwapChainDescription swapChainDesc = new SwapChainDescription()
                {
                    BufferCount = 1,
                    ModeDescription = new ModeDescription(BufferWidth, BufferHeight, new Rational(120, 1), Format.R8G8B8A8_UNorm),
                    IsWindowed = true,
                    OutputHandle = OutputHandle.Value,
                    SampleDescription = BufferSampleDescription,
                    SwapEffect = SwapEffect.Discard,
                    Usage = Usage.RenderTargetOutput,
                    Flags = SwapChainFlags.AllowModeSwitch,
                };
                swapChain = new SwapChain(device.Factory, Device, swapChainDesc);
                using (var factory = swapChain.GetParent<Factory>())
                    factory.SetWindowAssociation(OutputHandle.Value, WindowAssociationFlags.IgnoreAltEnter);
            }
            LightingSystem = new ForwardLighting(this);
            SetupRenderTargets();
            LightingSystem.Initialize();
        }

        public void Resize(int BufferWidth, int BufferHeight)
        {
            if (BufferWidth > 0 & BufferHeight > 0 & !isRendering)
            {
                bufferWidth = BufferWidth;
                bufferHeight = BufferHeight;
                Camera.OutputSize = new Vector2(BufferWidth, BufferHeight);
                if (hasHandle)
                    swapChain.ResizeBuffers(1, BufferWidth, BufferHeight, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
                SetupRenderTargets();
                LightingSystem.OnBackBufferSizeChanged();
            }
        }

        public void Begin()
        {
            isRendering = true;
            Context.OutputMerger.SetTargets(BackBufferDepth, BackBufferColor);
            Context.Rasterizer.SetViewports(new Viewport(0, 0, bufferWidth, bufferHeight, 0.0f, 1.0f));
            LightingSystem.Begin(Context);
        }

        public void DrawPrimitive(TestTriangle prim)
        {
            Context.InputAssembler.InputLayout = prim.SampleLayout;
            Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(prim.SampleVertices, 32, 0));

            EffectTechnique technique = prim.SampleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);

            for (int i = 0; i < technique.Description.PassCount; ++i)
            {
                pass.Apply(Context);
                Context.Draw(3, 0);
            }
        }

        public void DrawPrimitive(GeometricPrimitive primitive)
        {
            if (ActiveMaterial != primitive.MaterialShader)
            {
                ActiveMaterial = primitive.MaterialShader;
                ActiveMaterial.Begin();
            }
            ActiveMaterial.SetBuffers(primitive);
            primitive.Draw(Context);
        }

        public void End()
        {
            LightingSystem.End(null);
            ActiveMaterial = null;
            isRendering = false;
        }

        public void Present()
        {
            if (hasHandle)
                swapChain.Present(0, PresentFlags.None);
            else
                Context.Flush();
            if (!hasHandle)
                if (BufferSampleDescription.Count > 1)
                    Context.ResolveSubresource(outputTexture, 0, sharedTexture, 0, sharedTexture.Description.Format);
        }

        void SetupRenderTargets()
        {
            Helper.Dispose(backBufferColor, backBufferDepth, outputTexture, outputDepth, sharedTexture);
            Texture2DDescription dsViewTex = new Texture2DDescription()
            {
                Width = bufferWidth,
                Height = bufferHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = samples.Count > 1 ? Format.R32G8X24_Typeless : Format.R32_Typeless,
                SampleDescription = BufferSampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            DepthStencilViewDescription dsDesc = new DepthStencilViewDescription()
            {
                Format = samples.Count > 1 ? Format.D32_Float_S8X24_UInt : Format.D32_Float,
                Dimension = dsViewTex.SampleDescription.Count > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                MipSlice = 0,
            };
            
            if (OutputHandle != null)
            {
                outputTexture = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0);
            }
            else
            {
                Texture2DDescription colordesc = new Texture2DDescription()
                {
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = bufferWidth,
                    Height = bufferHeight,
                    MipLevels = 1,
                    SampleDescription = BufferSampleDescription,
                    Usage = ResourceUsage.Default,
                    OptionFlags = BufferSampleDescription.Count > 1 ? ResourceOptionFlags.None : ResourceOptionFlags.Shared,
                    CpuAccessFlags = CpuAccessFlags.None,
                    ArraySize = 1,
                };
                outputTexture = new Texture2D(device, colordesc);
                if (BufferSampleDescription.Count > 1)
                {
                    Texture2DDescription colordesc2 = colordesc;
                    colordesc2.SampleDescription = new SampleDescription(1, 0);
                    colordesc2.OptionFlags = ResourceOptionFlags.Shared;
                    sharedTexture = new Texture2D(device, colordesc2);
                }
            }
            outputDepth = new Texture2D(device, dsViewTex);
            backBufferColor = new RenderTargetView(device, outputTexture);
            backBufferDepth = new DepthStencilView(device, outputDepth, dsDesc);
        }

        public void Clear(Color4 color)
        {
            RenderTargetView[] rt = Context.OutputMerger.GetRenderTargets(renderTargetsCount);
            for (int i = 0; i < rt.Length; i++)
                if (rt[i] != null)
                    Context.ClearRenderTargetView(rt[i], color);
            DepthStencilView ds = Context.OutputMerger.GetDepthStencilView();
            if (ds != null)
                Context.ClearDepthStencilView(ds, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 0, 0);
        }

        public void ClearShaders()
        {
            Context.ComputeShader.Set(null);
            Context.VertexShader.Set(null);
            Context.GeometryShader.Set(null);
            Context.HullShader.Set(null);
            Context.DomainShader.Set(null);
            Context.PixelShader.Set(null);
            for (int i = 0; i < 13; i++)
            {
                Context.ComputeShader.SetConstantBuffer(null, i);
                Context.VertexShader.SetConstantBuffer(null, i);
                Context.GeometryShader.SetConstantBuffer(null, i);
                Context.HullShader.SetConstantBuffer(null, i);
                Context.DomainShader.SetConstantBuffer(null, i);
                Context.PixelShader.SetConstantBuffer(null, i);
            }
            for (int i = 0; i < 16; i++)
            {
                Context.ComputeShader.SetSampler(null, i);
                Context.VertexShader.SetSampler(null, i);
                Context.GeometryShader.SetSampler(null, i);
                Context.HullShader.SetSampler(null, i);
                Context.DomainShader.SetSampler(null, i);
                Context.PixelShader.SetSampler(null, i);
            }
            for (int i = 0; i < 128; i++)
            {
                Context.ComputeShader.SetShaderResource(null, i);
                Context.VertexShader.SetShaderResource(null, i);
                Context.GeometryShader.SetShaderResource(null, i);
                Context.HullShader.SetShaderResource(null, i);
                Context.DomainShader.SetShaderResource(null, i);
                Context.PixelShader.SetShaderResource(null, i);
            }
        }

        public void Dispose()
        {
            Helper.Dispose(outputTexture, outputDepth, backBufferColor, backBufferDepth, LightingSystem, sharedTexture);
            if (device != null)
            {
                deviceUsers--;
                if (deviceUsers < 1)
                {
                    device.Dispose();
                    device = null;
                }
            }
        }
    }
}
