using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    public class Keyframe
    {
        public Vector3 Translation, Scale;
        public Quaternion Rotation;
        public Matrix Transform = Matrix.Identity;
        public float Time = 0;
        public InterpolationType Interpolation;
        public Bone Bone;

        public Keyframe(Bone bone, float time, Matrix output, InterpolationType interpolation)
        {
            Bone = bone;
            Time = time;
            Transform = output;
            Interpolation = interpolation;
            Transform.Decompose(out Scale, out Rotation, out Translation);
        }
    }
}
