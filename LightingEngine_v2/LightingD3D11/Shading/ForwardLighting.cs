using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using Buffer = SlimDX.Direct3D11.Buffer;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    public class ForwardLighting : LightingSystem
    {
        int maxLights = 1;
        PointLightStruct[] plStruct;
        public StructuredBuffer<PointLightStruct> pLightBuffer;
        PixelShader PS_PointLights;

        DepthStencilState dss;

        public ForwardLighting(Renderer Renderer) : base(Renderer)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            plStruct = new PointLightStruct[maxLights];
            pLightBuffer = new StructuredBuffer<PointLightStruct>(maxLights, false, true);
            string path = @"R:\Users\Rox Cox\Documents\Visual Studio 2013\Projects\LightingEngine_v2\LightingEngine_v2\LightingD3D11\HLSL\";
            ShaderFlags flags = ShaderFlags.Debug | ShaderFlags.EnableStrictness | ShaderFlags.PackMatrixRowMajor;
            using (ShaderBytecode bytecode = ShaderBytecode.CompileFromFile(path + "forward.hlsl", "ForwardPS", "ps_5_0", flags, EffectFlags.None, null, new IncludeFX(path)))
            {
                PS_PointLights = new PixelShader(Renderer.Device, bytecode);
            }
            DepthStencilStateDescription dssdesc = States.DepthDefault();
            dssdesc.DepthComparison = Comparison.GreaterEqual;
            dss = DepthStencilState.FromDescription(Renderer.Device, dssdesc);
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
                if (Lights.Count()-1 < i)
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
            Context.GeometryShader.Set(null);
            Context.PixelShader.Set(PS_PointLights);
            Context.PixelShader.SetShaderResource(pLightBuffer.ShaderResource, 5);
            Context.OutputMerger.DepthStencilState = dss;
        }

        public override void Dispose()
        {
            base.Dispose();
            Helper.Dispose(pLightBuffer, PS_PointLights, dss);
        }
    }

    public struct PointLightStruct
    {
        public Vector3 positionView;
        public float attenuationBegin;
        public Vector3 color;
        public float attenuationEnd;
    }

    public struct SpotLightStruct
    {
        public Vector3 positionView;
        public float attenuationBegin;
        public Vector3 color;
        public float attenuationEnd;
        public Vector3 direction;
        public float coneAngle;
    }

    public struct DirectionalLightStruct
    {
        public Vector3 positionView;
        public Vector3 color;
        public Vector3 direction;
    }
}
