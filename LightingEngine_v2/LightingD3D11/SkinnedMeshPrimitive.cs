#region File Description
//-----------------------------------------------------------------------------
// StaticMeshPrimitive.cs
//
// Copyright (C) Roman Hodulák. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using System.Collections.Specialized;
using SlimDX;
using SlimDX.Direct3D11;
using BulletSharp;
#endregion

namespace LightingEngine_v2.LightingD3D11
{
    /// <summary>
    /// A geometric primitive class that represents skinned mesh.
    /// </summary>
    public class SkinnedMeshPrimitive : GeometricPrimitive
    {
        public List<AnimationClip> AnimationClips { get; set; }
        public Dictionary<string, Bone> BonesSID { get; set; }
        public List<Bone> Bones { get; set; }
        public Buffer BoneBuffer { get; set; }
        public Matrix[] BoneMatrices { get; set; }
        public const byte MaxBones = 255;
        public Ragdoll Ragdoll = null;

        /// <summary>
        /// Constructs a new skinned mesh primitive, with given vertices and indices
        /// </summary>
        /// <param name="device">Device to create primitive with.</param>
        /// <param name="VerticesArray">An array of vertices</param>
        /// <param name="IndicesArray">An array of indices</param>
        public SkinnedMeshPrimitive(Renderer Renderer, Vertex[] VerticesArray, int[] IndicesArray, Bone[] Bones)
        {
            this.Renderer = Renderer;
            this.CollisionShape = new EmptyShape();
            this.AnimationClips = new List<AnimationClip>();
            this.Bones = new List<Bone>();
            this.BonesSID = new Dictionary<string, Bone>();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triIndex = new List<int>();
            foreach (Vertex v in VerticesArray)
            {
                GeometryData.AddVertex(v);
            }
            for (int i = 0; i < IndicesArray.Length; i++)
            {
                GeometryData.AddIndex(IndicesArray[i]);
            }
            foreach (Bone b in Bones)
            {
                this.Bones.Add(b);
                BonesSID.Add(b.Name, b);
            }

            BufferDescription BBDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = MaxBones * (sizeof(float) * 4 * 4),
                StructureByteStride = 0,
            };
            BoneBuffer = new Buffer(Renderer.Device, BBDesc);

            InitializePrimitive();
        }

        public void SetBoneMatrices(Renderer renderer)
        {
            BoneMatrices = new Matrix[Bones.Count];

            //Gets and saves transforms
            for (int i = 0; i < Bones.Count; i++)
            {
                if (Ragdoll != null)
                    BoneMatrices[i] = Ragdoll.GetTransforms(Bones[i]);
                else
                    BoneMatrices[i] = Bones[i].Transforms;
            }

            //Passes skinning data to skinning buffer
            renderer.Context.MapSubresource(BoneBuffer, MapMode.WriteDiscard, MapFlags.None).Data.WriteRange(BoneMatrices);
            renderer.Context.UnmapSubresource(BoneBuffer, 0);
        }

        public float CurAnimationTime = 0;
        public int CurAnimationIndex = 0;
        public bool IsLooping = true;
        public bool IsPlaying = true;

        public void PlayAnimation(int index, bool loop)
        {
            CurAnimationIndex = index;
            IsLooping = loop;
            IsPlaying = true;
        }

        public void PauseAnimation()
        {
            IsPlaying = !IsPlaying;
        }

        public void StopAnimation()
        {
            IsPlaying = false;
            CurAnimationTime = 0;
        }

        public void StepAnimation(float timestep)
        {
            if (!IsPlaying)
                return;
            if (AnimationClips[CurAnimationIndex].Length <= CurAnimationTime)
            {
                if (IsLooping)
                    CurAnimationTime = 0;
                else
                    IsPlaying = false;
            }
            else
                CurAnimationTime += timestep;
            foreach (Bone b in Bones)
            {
                Matrix m = AnimationClips[CurAnimationIndex].GetTransformsByTime(b, CurAnimationTime);
                if (m != Matrix.Identity)
                    b.BindPose = m;
            }
        }

        public void SetAnimationTime(float time)
        {
            CurAnimationTime = time;
            StepAnimation(0);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.BoneBuffer.Dispose();
        }
    }
}