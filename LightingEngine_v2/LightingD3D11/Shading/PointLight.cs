using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    [Serializable]
    public class PointLight : Light
    {
        public float Radius { get; set; }

        public PointLight()
        {
        }

        public override LightPrimitive GetPrimitive(Renderer Renderer)
        {
            LightPrimitive prim = new PointLightPrimitive(Renderer);
            return prim;
        }
    }
}
