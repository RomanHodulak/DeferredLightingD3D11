using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D11;

namespace LightingEngine_v2.LightingD3D11
{
    public static class States
    {
        public static DepthStencilStateDescription DepthDefault()
        {
            return new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                IsStencilEnabled = false,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
                BackFace = new DepthStencilOperationDescription()
                {
                    Comparison = Comparison.Always,
                    PassOperation = StencilOperation.Keep,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                },
                FrontFace = new DepthStencilOperationDescription()
                {
                    Comparison = Comparison.Always,
                    PassOperation = StencilOperation.Keep,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                },
            };
        }

        public static RasterizerStateDescription RasterizerDefault()
        {
            return new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                IsFrontCounterclockwise = false,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                IsDepthClipEnabled = true,
                IsScissorEnabled = false,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false,
            };
        }

        public static BlendStateDescription BlendDefault()
        {
            BlendStateDescription bsDesc = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            bsDesc.RenderTargets[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = BlendOption.One,
                DestinationBlendAlpha = BlendOption.Zero,
                BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
            };
            return bsDesc;
        }
    }
}
