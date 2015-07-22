using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    public class AnimationClip
    {
        public List<Keyframe> Keyframes { get; set; }
        public Dictionary<Bone, List<Keyframe>> KeyframesT = new Dictionary<Bone, List<Keyframe>>();
        public float Length { get; set; }

        public AnimationClip(List<Keyframe> keyframes)
            : this(keyframes.ToArray())
        {
        }

        public AnimationClip(params Keyframe[] keyframes)
        {
            Keyframes = new List<Keyframe>();
            List<Bone> bones = new List<Bone>();
            foreach (Keyframe k in keyframes)
            {
                Keyframes.Add(k);
                if (k.Time > Length)
                    Length = k.Time;
                if (k.Bone != null)
                {
                    if (!bones.Contains(k.Bone))
                        bones.Add(k.Bone);
                }
            }
            foreach (Bone b in bones)
            {
                List<Keyframe> kfs = new List<Keyframe>();
                foreach (Keyframe k in keyframes)
                {
                    if (k.Bone == b)
                        kfs.Add(k);
                }
                KeyframesT.Add(b, kfs);
            }
        }

        public Matrix GetTransformsByTime(Actor actor, float time)
        {
            Quaternion q;
            Vector3 v;
            return GetTransformsByTime(time, out v, out q);
        }

        public Matrix GetTransformsByTime(float time)
        {
            Quaternion q;
            Vector3 v;
            return GetTransformsByTime(time, out v, out q);
        }

        public Matrix GetTransformsByTime(float time, out Vector3 Translation, out Quaternion Rotation)
        {
            Rotation = Quaternion.Identity;
            Translation = Vector3.Zero;
            float closestPrev = 0;
            float closestNext = float.MaxValue;
            Keyframe kPrev = null;
            Keyframe kNext = null;
            foreach (Keyframe k in Keyframes)
            {
                if (k.Time < time)
                {
                    if (k.Time >= closestPrev)
                    {
                        closestPrev = k.Time;
                        kPrev = k;
                    }
                }
                if (k.Time > time)
                {
                    if (k.Time < closestNext)
                    {
                        closestNext = k.Time;
                        kNext = k;
                    }
                }
            }
            float weight = (float)Interpolation.Linear((double)closestPrev, (double)closestNext, (double)time);
            if (kPrev == null)
            {
                return Matrix.Identity;
            }
            else if (kNext == null)
            {
                return Matrix.Identity;
            }
            else
            {
                Rotation = Quaternion.Lerp(kPrev.Rotation, kNext.Rotation, weight);
                Translation = Vector3.Lerp(kPrev.Translation, kNext.Translation, weight);
                return Matrix.RotationQuaternion(Rotation) * Matrix.Translation(Translation);
            }
        }

        public Matrix GetTransformsByTime(Bone bone, float time)
        {
            if (!KeyframesT.ContainsKey(bone))
                return Matrix.Identity;
            float closestPrev = 0;
            float closestNext = float.MaxValue;
            Keyframe kPrev = null;
            Keyframe kNext = null;
            foreach (Keyframe k in KeyframesT[bone])
            {
                if (k.Time < time)
                {
                    if (k.Time >= closestPrev)
                    {
                        closestPrev = k.Time;
                        kPrev = k;
                    }
                }
                if (k.Time > time)
                {
                    if (k.Time < closestNext)
                    {
                        closestNext = k.Time;
                        kNext = k;
                    }
                }
            }
            float weight = (float)Interpolation.Linear((double)closestPrev, (double)closestNext, (double)time);
            if (kPrev == null)
            {
                return Matrix.Identity;
            }
            else if (kNext == null)
            {
                return Matrix.Identity;
            }
            else
            {
                Quaternion Rotation = Quaternion.Lerp(kPrev.Rotation, kNext.Rotation, weight);
                Vector3 Translation = Vector3.Lerp(kPrev.Translation, kNext.Translation, weight);
                bone.Position = Translation;
                return Matrix.RotationQuaternion(Rotation) * Matrix.Translation(Translation);
            }
        }
    }
}
