using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightingEngine_v2
{
    public class TestTriangle : IDisposable
    {
        public DataStream SampleStream;
        public InputLayout SampleLayout;
        public SlimDX.Direct3D11.Buffer SampleVertices;
        public Effect SampleEffect;
        public VertexShader VS;

        public TestTriangle(SlimDX.Direct3D11.Device Device)
        {
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
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(@"R:\Users\Rox Cox\Documents\Visual Studio 2013\Projects\LightingEngine_v2\LightingEngine_v2\test.fx", "VSTri", "vs_5_0", ShaderFlags.None, EffectFlags.None))
            {
                VS = new VertexShader(Device, bytecode);
            }
            EffectTechnique technique = SampleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);
            SampleLayout = new InputLayout(Device, pass.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            SampleStream = new DataStream(4 * 32, true, true);
            SampleStream.WriteRange(new[] {
                new Vector4(1.0f, 1.0f, 0.5f, 1.0f), new Vector4(0.0f, 0.8f, 1.0f, 1.0f),
                new Vector4(1.0f, -1.0f, 0.5f, 1.0f), new Vector4(0.3f, 1.0f, 0.3f, 1.0f),
                new Vector4(-1.0f, -1.0f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, 1.0f, 0.5f, 1.0f), new Vector4(0.0f, 0.8f, 1.0f, 1.0f),
            });
            SampleStream.Position = 0;

            SampleVertices = new SlimDX.Direct3D11.Buffer(Device, SampleStream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 3 * 32,
                Usage = ResourceUsage.Default
            });
        }

        public void Dispose()
        {
            Helper.Dispose(SampleStream, SampleLayout, SampleVertices, SampleEffect, VS);
        }
    }
}
