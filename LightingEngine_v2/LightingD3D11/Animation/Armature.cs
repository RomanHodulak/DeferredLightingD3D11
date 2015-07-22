using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    public class Armature
    {
        public const byte MaxBones = 255;
        public List<AnimationClip> AnimationClips { get; set; }
        public List<Bone> Bones { get; set; }
    }
}
