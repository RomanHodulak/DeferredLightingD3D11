using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using BulletSharp;
using BulletSharp.SoftBody;

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    public class Model : Actor
    {
        public CollisionObject Body
        {
            get { return body; }
            set { body = value; }
        }
        RigidBody RigidBody
        {
            get
            {
                if (body is RigidBody)
                    return body as RigidBody;
                return null;
            }
            set
            {
                if (body != null & !body.IsDisposed)
                    body.Dispose();
                body = value;
            }
        }
        SoftBody SoftBody
        {
            get
            {
                if (body is SoftBody)
                    return body as SoftBody;
                return null;
            }
            set
            {
                if (body != null & !body.IsDisposed)
                    body.Dispose();
                body = value;
            }
        }


        public override Vector3 Position
        {
            get
            {
                if (body is RigidBody)
                    return (RigidBody.CenterOfMassPosition);
                else
                {
                    Vector3 min, max;
                    SoftBody.GetAabb(out min, out max);
                    return new Vector3((min.X + max.X) / 2, (min.Y + max.Y) / 2, (min.Z + max.Z) / 2);
                }
            }
            set
            {
                if (body is RigidBody)
                    RigidBody.Translate(-(RigidBody.CenterOfMassPosition) + (value));
                else
                    SoftBody.Translate((value));
                body.Activate();
                foreach (GeometricPrimitive p in Primitives)
                    p.MatrixWorld = MatrixWorld;
            }
        }

        public Vector3 Rotation
        {
            get
            {
                if (body is RigidBody)
                {
                    Vector3 r = Vector3.Zero;
                    Quaternion q = (RigidBody.Orientation);
                    double sqw = q.W*q.W;
                    double sqx = q.X*q.X;
                    double sqy = q.Y*q.Y;
                    double sqz = q.Z*q.Z;
	                double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
	                double test = q.X*q.Y + q.Z*q.W;
	                if (test > 0.499*unit) { // singularity at north pole
                        r.Y = 2 * (float)Math.Atan2(q.X, q.W); 
                        r.Z = (float)Math.PI / 2; 
		                r.X = 0; 
		                return r;
	                }
	                if (test < -0.499*unit) { // singularity at south pole
		                r.Y = -2 * (float)Math.Atan2(q.X,q.W);
                        r.Z = -(float)Math.PI / 2;
                        r.X = 0;
		                return r;
	                }
                    r.Y = (float)Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, sqx - sqy - sqz + sqw); //head
                    r.Z = (float)Math.Asin(2 * test / unit); //attitude
                    r.X = (float)Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, -sqx + sqy - sqz + sqw); //bank
                    return r;
                }
                else
                {
                    return Vector3.Zero;
                }
            }
            set
            {
                Matrix mat;
                body.GetWorldTransform(out mat);
                mat = Matrix.RotationYawPitchRoll(value.Y, value.X, value.Z);
                Vector3 p = Position;
                body.WorldTransform = mat;
                Position = p;
            }
        }

        public bool CastShadow { get; set; }
        public Quaternion Orientation
        {
            get
            {
                if (body is RigidBody)
                    return (RigidBody.Orientation);
                else
                {
                    return Quaternion.Identity;
                }
            }
            set
            {
                if (body is RigidBody)
                {
                }
                else
                {
                }
            }
        }


        public bool IsStatic
        {
            get
            {
                return isStatic;
            }
            set
            {
                if (body is RigidBody)
                {
                    if (value)
                    {
                        mass = RigidBody.InvMass;
                        RigidBody.SetMassProps(0, RigidBody.InvInertiaDiagLocal);
                    }
                    else
                    {
                        RigidBody.SetMassProps(mass, RigidBody.InvInertiaDiagLocal);
                    }
                    isStatic = value;
                }
            }
        }

        public bool IsParticle
        {
            get
            {
                return isParticle;
            }
            set
            {
                if (body is RigidBody)
                {
                    if (value)
                    {
                        RigidBody.SetMassProps(RigidBody.InvMass, new Vector3(0, 0, 0));
                    }
                    else
                    {
                        RigidBody.SetMassProps(RigidBody.InvMass, new Vector3(0, 0, 0));
                    }
                    isParticle = value;
                }
            }
        }
        float mass = 0;

        public Matrix MatrixWorld
        {
            get
            {
                if (body is RigidBody)
                    return Matrix.RotationQuaternion(new Quaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * Matrix.Translation(Position);
                else
                    return Matrix.Translation(Position);
            }
        }
        public BoundingBox BoundingBox
        {
            get
            {
                Vector3[] c = bbox.GetCorners();
                for (int i = 0; i < c.Length; i++)
                    c[i] = MathHelper.ToVector3(Vector3.Transform(c[i], MatrixWorld));
                return BoundingBox.FromPoints(c);
            }
        }
        public List<GeometricPrimitive> Primitives { get; set; }

        public string Path { get; set; }
        bool isStatic = false;
        bool isParticle = false;
        BoundingBox bbox;
        [NonSerialized]
        CollisionObject body;

        public Model(params GeometricPrimitive[] Primitives)
        {
            this.Primitives = Primitives.ToList();
            CollisionShape compound = null;
            CastShadow = true;
            List<Vector3> points = new List<Vector3>();
            if (Primitives.Length > 1)
            {
                compound = new CompoundShape(true);
                foreach (GeometricPrimitive prim in Primitives)
                {
                    (compound as CompoundShape).AddChildShape(Matrix.Identity, prim.CollisionShape);
                    points.AddRange(prim.GeometryData.Positions);
                }
            }
            else
            {
                compound = Primitives[0].CollisionShape;
                points.AddRange(Primitives[0].GeometryData.Positions);
            }
            mass = 10;
            body = new RigidBody(new RigidBodyConstructionInfo(mass, new DefaultMotionState(), compound, compound.CalculateLocalInertia(mass)));
            bbox = BoundingBox.FromPoints(points.ToArray());
        }

        public Model()
        {
        }

        public void GenerateBody()
        {
            if (body != null)
                body.Dispose();
            CollisionShape compound = null;
            if (Primitives.Count > 1)
            {
                compound = new CompoundShape(true);
                foreach (GeometricPrimitive prim in Primitives)
                {
                    (compound as CompoundShape).AddChildShape((Matrix.Identity), prim.CollisionShape);
                }
            }
            else
            {
                compound = Primitives[0].CollisionShape;
            }
            mass = 10;
            body = new RigidBody(new RigidBodyConstructionInfo(mass, new DefaultMotionState(), compound, compound.CalculateLocalInertia(mass)));
        }

        public void ResetVelocity()
        {
            if (body is RigidBody)
            {
                RigidBody.AngularVelocity = Vector3.Zero;
                RigidBody.LinearVelocity = Vector3.Zero;
            }
            else if (body is SoftBody)
            {
                SoftBody.SetVelocity(Vector3.Zero);
            }
        }

        public void Dispose()
        {
            foreach (GeometricPrimitive prim in Primitives)
                prim.Dispose();
            body.Dispose();
        }
    }
}
