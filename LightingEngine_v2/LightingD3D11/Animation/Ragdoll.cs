using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    public class Ragdoll
    {
        //public List<Constraint> Constrains = new List<Constraint>();
        //public List<Joint> Joints = new List<Joint>();
        //public Dictionary<Bone, IBroadphaseEntity> Parts = new Dictionary<Bone, IBroadphaseEntity>();

        //public void AddToWorld(World world)
        //{
        //    foreach (IBroadphaseEntity body in Parts.Values)
        //    {
        //        if (body.GetType() == typeof(RigidBody))
        //        {
        //            world.AddBody(body as RigidBody);
        //        }
        //        else if (body.GetType() == typeof(SoftBody))
        //        {
        //            world.AddBody(body as SoftBody);
        //        }
        //    }
        //    foreach (Constraint c in Constrains)
        //    {
        //        world.AddConstraint(c);
        //    }
        //}

        public Matrix GetTransforms(Bone b)
        {
            //if (Parts.ContainsKey(b))
                //return b.InverseBindPose * JConvert.ToDXMatrix((Parts[b] as RigidBody).Orientation) * Matrix.Translation(JConvert.ToDXVector((Parts[b] as RigidBody).Position));
            //else
                return Matrix.Identity;
        }

        ///*public void ResetBodies()
        //{
        //    foreach (RigidBody body in Parts.Values)
        //    {
        //        body.AngularVelocity = JVector.Zero;
        //        body.LinearVelocity = JVector.Zero;
        //        body.
        //    }
        //}*/

        //public void ApplyImpulse(JVector impulse)
        //{
        //    foreach (IBroadphaseEntity body in Parts.Values)
        //    {
        //        if (body.GetType() == typeof(RigidBody))
        //        {
        //            RigidBody b = body as RigidBody;
        //            b.ApplyImpulse(impulse);
        //        }
        //        else if (body.GetType() == typeof(SoftBody))
        //        {
        //            SoftBody b = body as SoftBody;
        //        }
        //    }
        //}
    }
}
