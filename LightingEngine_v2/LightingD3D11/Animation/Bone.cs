using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    /// <summary>
    /// Class for storing informations related to working with bones. An animation skeleton is a collection of these bones.
    /// </summary>
    public class Bone : Actor
    {
        /// <summary>
        /// String ID of this bone. Setting meaningful names (like "LeftArm" or "RightThumb") to bones is useful, so you can recognize it easily.
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// This is parent bone of the bone. That means the bone inherits its matrices. Every parent bone can be also child of its own parent bone.
        /// </summary>
        public Bone ParentBone;

        /// <summary>
        /// The reference transforms of the bone. This matrix changes when playing animation clip.
        /// </summary>
        public Matrix BindPose;

        /// <summary>
        /// The inversed default transforms of the bone. This matrix brings the bone to local bone space. It's like "undo the reference transforms".
        /// </summary>
        public Matrix InverseBindPose;

        /// <summary>
        /// This matrix deformes the bone. Here you can set rotation, position, scale and so on.
        /// </summary>
        public Matrix Transformation = Matrix.Identity;

        public override Vector3 Position { get; set; }

        public Bone(string name, Matrix InverseBindPose)
        {
            this.Name = name;
            this.InverseBindPose = InverseBindPose;
        }

        public Matrix TotalBindPose
        {
            get
            {
                if (ParentBone != null)
                    return ParentBone.TotalBindPose;
                else
                    return BindPose;
            }
        }

        public Matrix TotalTransformation
        {
            get
            {
                if (ParentBone != null)
                    return Transformation * BindPose * ParentBone.TotalTransformation;
                else
                    return Transformation;
            }
        }

        /// <summary>
        /// This matrix gets you the final matrix transformation, ready for rendering.
        /// </summary>
        public Matrix Transforms
        {
            get
            {
                if (ParentBone != null)
                    return InverseBindPose * Transformation * BindPose * ParentBone.TotalTransformation * ParentBone.TotalBindPose;
                else
                    return InverseBindPose * Transformation * BindPose;
            }
        }
    }
}
