using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    public abstract class Actor
    {
        public virtual string Name { get; set; }
        public virtual Vector3 Position { get; set; }
    }
}
