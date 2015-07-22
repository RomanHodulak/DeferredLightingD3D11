using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    /// <summary>
    /// Represents a bounding frustum in three dimensional space.
    /// </summary>
    [Serializable]
    public class BoundingFrustum
    {
        Plane near;
        Plane far;
        Plane top;
        Plane bottom;
        Plane left;
        Plane right;

        Matrix matrix;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlimMath.BoundingFrustum"/> class.
        /// </summary>
        /// <param name="value">The <see cref="SlimMath.Matrix"/> to extract the planes from.</param>
        public BoundingFrustum(Matrix value)
        {
            SetMatrix(ref value);
        }

        /// <summary>
        /// Sets the matrix that represents this instance of <see cref="SlimMath.BoundingFrustum"/>.
        /// </summary>
        /// <param name="value">The <see cref="SlimMath.Matrix"/> to extract the planes from.</param>
        public void SetMatrix(ref Matrix value)
        {
            this.matrix = value;

            //Near
            near.Normal.X = value.M13;
            near.Normal.Y = value.M23;
            near.Normal.Z = value.M33;
            near.D = value.M43;

            //Far
            far.Normal.X = value.M14 - value.M13;
            far.Normal.Y = value.M24 - value.M23;
            far.Normal.Z = value.M34 - value.M33;
            far.D = value.M44 - value.M43;

            //Top
            top.Normal.X = value.M14 - value.M12;
            top.Normal.Y = value.M24 - value.M22;
            top.Normal.Z = value.M34 - value.M32;
            top.D = value.M44 - value.M42;

            //Bottom
            bottom.Normal.X = value.M14 + value.M12;
            bottom.Normal.Y = value.M24 + value.M22;
            bottom.Normal.Z = value.M34 + value.M32;
            bottom.D = value.M44 + value.M42;

            //Left
            left.Normal.X = value.M14 + value.M11;
            left.Normal.Y = value.M24 + value.M21;
            left.Normal.Z = value.M34 + value.M31;
            left.D = value.M44 + value.M41;

            //Right
            right.Normal.X = value.M14 - value.M11;
            right.Normal.Y = value.M24 - value.M21;
            right.Normal.Z = value.M34 - value.M31;
            right.D = value.M44 - value.M41;
        }

        public bool Intersects(BoundingBox box)
        {
	        bool result = true;
            int outP = 0;
            int inS = 0;
            Vector3[] Corners = box.GetCorners();
            Plane[] Planes = new Plane[]
            {
                near,
                far, 
                top,
                bottom,
                left,
                right
            };

	        // for each plane do ...
	        for(int i = 0; i < 6; i++) 
            {
		        // reset counters for corners in and out
		        outP = 0; 
                inS = 0;
		        // for each corner of the box do ...
		        // get out of the cycle as soon as a box as corners
		        // both inside and out of the frustum
		        for (int k = 0; k < 8 && (inS == 0 || outP == 0); k++) 
                {
			        // is the corner outside or inside
			        if (Plane.DotCoordinate(Planes[i], Corners[k]) < 0)
				        outP++;
			        else
				        inS++;
		        }
		        //if all corners are out
		        if (inS == 0)
			        return (false);
		        // if some corners are out and others are in	
		        else if (outP > 0)
			        result = true;
	        }
	        return(result);
        }

        public Vector3[] GetCorners()
        {
            Vector4[] ret = new Vector4[8];
            Matrix matrix = Matrix.Invert(this.matrix);
            ret[0] = (Vector4.Transform(new Vector4(-1, -1, -1, 1.0f), matrix));
            ret[1] = (Vector4.Transform(new Vector4(1, -1, -1, 1.0f), matrix));
            ret[2] = (Vector4.Transform(new Vector4(1, 1, -1, 1.0f), matrix));
            ret[3] = (Vector4.Transform(new Vector4(-1, 1, -1, 1.0f), matrix));

            ret[4] = (Vector4.Transform(new Vector4(-1, -1, 1, 1.0f), matrix));
            ret[5] = (Vector4.Transform(new Vector4(1, -1, 1, 1.0f), matrix));
            ret[6] = (Vector4.Transform(new Vector4(1, 1, 1, 1.0f), matrix));
            ret[7] = (Vector4.Transform(new Vector4(-1, 1, 1, 1.0f), matrix)); 
            Vector3[] rt = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                ret[i].X /= ret[i].W;
                ret[i].Y /= ret[i].W;
                ret[i].Z /= ret[i].W;
                ret[i].W = 1.0f;
                rt[i] = MathHelper.ToVector3(ret[i]);
            }
            return rt;
        }
    }
}
