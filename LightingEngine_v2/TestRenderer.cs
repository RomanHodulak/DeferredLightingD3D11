using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;

namespace LightingEngine_v2
{
    public class TestRenderer : IDisposable
    {
        Device Device;
        DeviceContext Context
        {
            get
            {
                return Device.ImmediateContext;
            }
        }
        DataStream SampleStream;
        InputLayout SampleLayout;
        Buffer SampleVertices;
        RenderTargetView SampleRenderView;
        DepthStencilView SampleDepthView;
        Effect SampleEffect;
        Texture2D DepthTexture;
        int WindowWidth;
        int WindowHeight;

        public Texture2D SharedTexture
        {
            get;
            set;
        }

        public TestRenderer(int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
            if (this.WindowWidth <= 0)
                this.WindowWidth = 100;
            if (this.WindowHeight <= 0)
                this.WindowHeight = 100;
            InitD3D();
        }

        bool isRendering = false;
        float c;
        bool up = true;
        public void Render(int arg)
        {
            isRendering = true;
            Context.OutputMerger.SetTargets(SampleDepthView, SampleRenderView);
            Context.Rasterizer.SetViewports(new Viewport(0, 0, WindowWidth, WindowHeight, 0.0f, 1.0f));

            Context.ClearDepthStencilView(SampleDepthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            if (c > 1)
                up = false;
            if (c <= 0)
                up = true;
            if (up)
                c += ((float)(arg % 1000)) / 999.0f * 0.005f;
            else
                c -= ((float)(arg % 1000)) / 999.0f * 0.005f;
            Context.ClearRenderTargetView(SampleRenderView, new SlimDX.Color4(1.0f, c, c*2, c*3));

            Context.InputAssembler.InputLayout= (SampleLayout);
            Context.InputAssembler.PrimitiveTopology =(PrimitiveTopology.TriangleList);
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(SampleVertices, 32, 0));

            EffectTechnique technique = SampleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);

            for (int i = 0; i < technique.Description.PassCount; ++i)
            {
                pass.Apply(Context);
                Context.Draw(3, 0);
            }

            Context.Flush();
            isRendering = false;
        }

        public void Resize(int width, int height)
        {
            this.WindowHeight = height;
            this.WindowWidth = width;
            if (this.WindowWidth <= 0)
                this.WindowWidth = 100;
            if (this.WindowHeight <= 0)
                this.WindowHeight = 100;
            
            if (!isRendering)
            {
                SampleRenderView.Dispose();
                SharedTexture.Dispose();
                SampleDepthView.Dispose();
                DepthTexture.Dispose();

                Texture2DDescription colordesc = new Texture2DDescription();
                colordesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
                colordesc.Format = Format.B8G8R8A8_UNorm;
                colordesc.Width = WindowWidth;
                colordesc.Height = WindowHeight;
                colordesc.MipLevels = 1;
                colordesc.SampleDescription = new SampleDescription(1, 0);
                colordesc.Usage = ResourceUsage.Default;
                colordesc.OptionFlags = ResourceOptionFlags.Shared;
                colordesc.CpuAccessFlags = CpuAccessFlags.None;
                colordesc.ArraySize = 1;

                Texture2DDescription depthdesc = new Texture2DDescription();
                depthdesc.BindFlags = BindFlags.DepthStencil;
                depthdesc.Format = Format.D32_Float_S8X24_UInt;
                depthdesc.Width = WindowWidth;
                depthdesc.Height = WindowHeight;
                depthdesc.MipLevels = 1;
                depthdesc.SampleDescription = new SampleDescription(1, 0);
                depthdesc.Usage = ResourceUsage.Default;
                depthdesc.OptionFlags = ResourceOptionFlags.None;
                depthdesc.CpuAccessFlags = CpuAccessFlags.None;
                depthdesc.ArraySize = 1;

                SharedTexture = new Texture2D(Device, colordesc);
                DepthTexture = new Texture2D(Device, depthdesc);
                SampleRenderView = new RenderTargetView(Device, SharedTexture);
                SampleDepthView = new DepthStencilView(Device, DepthTexture);
            }
        }

        void InitD3D()
        {
            Device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0);
            
            Texture2DDescription colordesc = new Texture2DDescription();
            colordesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
            colordesc.Format = Format.B8G8R8A8_UNorm;
            colordesc.Width = WindowWidth;
            colordesc.Height = WindowHeight;
            colordesc.MipLevels = 1;
            colordesc.SampleDescription = new SampleDescription(1, 0);
            colordesc.Usage = ResourceUsage.Default;
            colordesc.OptionFlags = ResourceOptionFlags.Shared;
            colordesc.CpuAccessFlags = CpuAccessFlags.None;
            colordesc.ArraySize = 1;

            Texture2DDescription depthdesc = new Texture2DDescription();
            depthdesc.BindFlags = BindFlags.DepthStencil;
            depthdesc.Format = Format.D32_Float_S8X24_UInt;
            depthdesc.Width = WindowWidth;
            depthdesc.Height = WindowHeight;
            depthdesc.MipLevels = 1;
            depthdesc.SampleDescription = new SampleDescription(1, 0);
            depthdesc.Usage = ResourceUsage.Default;
            depthdesc.OptionFlags = ResourceOptionFlags.None;
            depthdesc.CpuAccessFlags = CpuAccessFlags.None;
            depthdesc.ArraySize = 1;

            SharedTexture = new Texture2D(Device, colordesc);
            DepthTexture = new Texture2D(Device, depthdesc);
            SampleRenderView = new RenderTargetView(Device, SharedTexture);
            SampleDepthView = new DepthStencilView(Device, DepthTexture);
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(@"R:\Users\Rox Cox\Documents\Visual Studio 2013\Projects\LightingEngine_v2\LightingEngine_v2\test.fx", "fx_5_0"))
            {
                try
                {
                    SampleEffect = new Effect(Device, bytecode);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            EffectTechnique technique = SampleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);
            SampleLayout = new InputLayout(Device, pass.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            SampleStream = new DataStream(3 * 32, true, true);
            SampleStream.WriteRange(new[] {
                new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.3f, 1.0f, 0.3f, 1.0f),
                new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.8f, 1.0f, 1.0f)
            });
            SampleStream.Position = 0;

            SampleVertices = new Buffer(Device, SampleStream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 3 * 32,
                Usage = ResourceUsage.Default
            });
        }

        void DestroyD3D()
        {
            if (SampleVertices != null)
            {
                SampleVertices.Dispose();
                SampleVertices = null;
            }

            if (SampleLayout != null)
            {
                SampleLayout.Dispose();
                SampleLayout = null;
            }

            if (SampleEffect != null)
            {
                SampleEffect.Dispose();
                SampleEffect = null;
            }

            if (SampleRenderView != null)
            {
                SampleRenderView.Dispose();
                SampleRenderView = null;
            }

            if (SampleDepthView != null)
            {
                SampleDepthView.Dispose();
                SampleDepthView = null;
            }

            if (SampleStream != null)
            {
                SampleStream.Dispose();
                SampleStream = null;
            }

            if (SampleLayout != null)
            {
                SampleLayout.Dispose();
                SampleLayout = null;
            }

            if (SharedTexture != null)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (DepthTexture != null)
            {
                DepthTexture.Dispose();
                DepthTexture = null;
            }

            if (Device != null)
            {
                Device.Dispose();
                Device = null;
            }
        }

        public void Dispose()
        {
            DestroyD3D();
        }
    }
}