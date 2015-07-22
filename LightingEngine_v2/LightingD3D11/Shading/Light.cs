using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    public abstract class Light : Actor
    {
        public Vector3 Color { get; set; }
        public virtual LightPrimitive GetPrimitive(Renderer Renderer)
        {
            return null;
        }
    }
}
