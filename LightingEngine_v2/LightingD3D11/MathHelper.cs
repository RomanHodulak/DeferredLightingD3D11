using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    public static class MathHelper
    {
        public const float PiOver4 = (float)Math.PI / 4;
        public const float PiOver2 = (float)Math.PI / 2;
        public const float TwoPi = (float)Math.PI * 2;
        public const float Pi = (float)Math.PI;

        public static float ToRadians(float degrees)
        {
            return ((float)Math.PI / 180.0f) * degrees;
        }

        public static float ToDegrees(float radians)
        {
            return (radians / Pi) * 360;
        }

        public static Matrix FlipMatrixHanding(Matrix m)
        {
            Matrix ret = new Matrix();
            ret.set_Rows(0, new Vector4(m.M11, m.M13, m.M12, 0));
            ret.set_Rows(1, new Vector4(m.M31, m.M33, m.M32, 0));
            ret.set_Rows(2, new Vector4(m.M21, m.M23, m.M22, 0));
            ret.set_Rows(3, new Vector4(m.M41, m.M43, m.M42, 1));
            return ret;
        }

        public static Vector3 Round(Vector3 vector)
        {
            return new Vector3((int)Math.Round(vector.X), (int)Math.Round(vector.Y), (int)Math.Round(vector.Z));
        }

        public static Vector3 ViewPosition(Camera cam)
        {
            return MathHelper.ViewNormal(cam) + cam.Position;
        }

        public static Vector3 ViewNormal(Camera Camera)
        {
            Vector3 nearSource = new Vector3(new Vector2(0, 0), 0f);
            Vector3 farSource = new Vector3(new Vector2(0, 0), 1f);

            Vector3 nearPoint = MathHelper.Unproject(nearSource, Camera.Viewport, Camera.MatrixView);
            Vector3 farPoint = MathHelper.Unproject(farSource, Camera.Viewport, Camera.MatrixView);

            Vector3 pos = farPoint - nearPoint;
            pos.Normalize();
            return -pos;
        }

        public static string RoundString(float value, int decimals)
        {
            float val = (float)Math.Round(value, decimals);
            string text = val.ToString();
            int deciCount = 0;
            if (text.IndexOf(",") >= 0)
            {
                deciCount = text.Substring(text.IndexOf(",") + 1).Length;
            }
            else
            {
                text += ",";
            }
            if (deciCount < decimals)
            {
                for (int i = 0; i < decimals - deciCount; i++)
                {
                    text += "0";
                }
            }
            return text;
        }

        public static Ray CursorRay(Vector2 cursorPoint, Camera Camera)
        {
            Vector3 nearSource = new Vector3(cursorPoint, 0f);
            Vector3 farSource = new Vector3(cursorPoint, 1f);

            Vector3 nearPoint = MathHelper.Unproject(nearSource, Camera.Viewport, Camera.MatrixView * Camera.MatrixProj * Matrix.Identity);
            Vector3 farPoint = MathHelper.Unproject(farSource, Camera.Viewport, Camera.MatrixView * Camera.MatrixProj * Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public static Vector3 CalculateTangent(Vector3 normal)
        {
            Vector3 tangent;
            Vector3 c1 = Vector3.Cross(normal, new Vector3(0, 0, 1));
            Vector3 c2 = Vector3.Cross(normal, new Vector3(0, 1, 0));
            if (c1.Length() > c2.Length())
                tangent = c1;
            else
                tangent = c2;

            tangent = Vector3.Normalize(tangent);
            return tangent;
        }

        public static Vector3 CalculateTangent(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            // Edges of the triangle : postion delta
            Vector3 deltaPos1 = v1 - v0;
            Vector3 deltaPos2 = v2 - v0;
 
            // UV delta
            Vector2 deltaUV1 = uv1 - uv0;
            Vector2 deltaUV2 = uv2 - uv0;

            float r = 1.0f / ((deltaUV1.X) * (deltaUV2.Y) - (deltaUV1.Y) * (deltaUV2.X));
            Vector3 tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
            return tangent;
        }

        public static Vector3 CalculateTangent(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2, out Vector3 Bitangent)
        {
            // Edges of the triangle : postion delta
            Vector3 deltaPos1 = v1 - v0;
            Vector3 deltaPos2 = v2 - v0;

            // UV delta
            Vector2 deltaUV1 = uv1 - uv0;
            Vector2 deltaUV2 = uv2 - uv0;

            float r = 1.0f / ((deltaUV1.X * deltaUV2.Y) - (deltaUV1.Y * deltaUV2.X));
            Vector3 tangent = ((deltaPos1 * deltaUV2.Y) - (deltaPos2 * deltaUV1.Y)) * r;
            Bitangent = ((deltaPos2 * deltaUV1.X) - (deltaPos1 * deltaUV2.X)) * r;
            return tangent;
        }

        public static Vector3 CalculateBitangent(Vector3 normal)
        {
            Vector3 tangent = CalculateTangent(normal);
            Vector3 binormal = Vector3.Cross(normal, tangent);
            binormal = Vector3.Normalize(binormal);
            return binormal;
        }

        public static Vector3 CalculateBitangent(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            // Edges of the triangle : postion delta
            Vector3 deltaPos1 = v1 - v0;
            Vector3 deltaPos2 = v2 - v0;

            // UV delta
            Vector2 deltaUV1 = uv1 - uv0;
            Vector2 deltaUV2 = uv2 - uv0;

            float r = 1.0f / ((deltaUV1.X) * (deltaUV2.Y) - (deltaUV1.Y) * (deltaUV2.X));
            Vector3 bitangent = (deltaPos2 * deltaUV1.X - deltaPos1 * deltaUV2.X) * r;
            return bitangent;
        }

        public static Vector3 CalculateBitangent(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2, out Vector3 Tangent)
        {
            // Edges of the triangle : postion delta
            Vector3 deltaPos1 = v1 - v0;
            Vector3 deltaPos2 = v2 - v0;

            // UV delta
            Vector2 deltaUV1 = uv1 - uv0;
            Vector2 deltaUV2 = uv2 - uv0;

            float r = 1.0f / ((deltaUV1.X) * (deltaUV2.Y) - (deltaUV1.Y) * (deltaUV2.X));
            Vector3 bitangent = (deltaPos2 * deltaUV1.X - deltaPos1 * deltaUV2.X) * r;
            Tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
            return bitangent;
        }

        public static Vector3[] TransformArray(List<Vector3> Vectors, Matrix m)
        {
            Vector3[] v = new Vector3[Vectors.Count];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = MathHelper.ToVector3(Vector3.Transform(Vectors[i], m));
            }
            return v;
        }

        public static Vector3[] TransformArray(Vector3[] Vectors, Matrix m)
        {
            Vector3[] v = new Vector3[Vectors.Length];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = MathHelper.ToVector3(Vector3.Transform(Vectors[i], m));
            }
            return v;
        }

        public static float? RayIntersectsModel(Ray ray, Model model)
        {
            GeometricPrimitive prim = null;
            return RayIntersectsModel(ray, model, out prim);
        }

        public static float? RayIntersectsModel(Ray ray, Model model, out GeometricPrimitive primitive)
        {
            Vector3 vertex1 = Vector3.Zero, vertex2 = Vector3.Zero, vertex3 = Vector3.Zero;
            primitive = null;
            float closest = float.MaxValue;
            foreach (GeometricPrimitive prim in model.Primitives)
            {
                float? f = RayIntersectsPrimitive(ray, prim, out vertex1, out vertex2, out vertex3);
                if (f.HasValue)
                {
                    if (f.Value < closest)
                    {
                        closest = f.Value;
                        primitive = prim;
                    }
                }
            }
            if (closest == float.MaxValue)
                return null;
            return closest;
        }

        public static float? RayIntersectsPrimitive(Ray ray, GeometricPrimitive primitive)
        {
            Vector3 vertex1 = Vector3.Zero, vertex2 = Vector3.Zero, vertex3 = Vector3.Zero;
            return RayIntersectsPrimitive(ray, primitive, out vertex1, out vertex2, out vertex3);
        }

        public static float? RayIntersectsPrimitive(Ray ray, GeometricPrimitive primitive, out Vector3 vertex1, out Vector3 vertex2, out Vector3 vertex3)
        {
            return RayIntersectsMesh(ray, primitive.GeometryData.Positions, primitive.GeometryData.Indices.ToArray(), primitive.MatrixWorld, out vertex1, out vertex2, out vertex3);
        }

        public static float? RayIntersectsMesh(Ray ray, Vector3[] vertices, ushort[] indices, Matrix world)
        {
            Vector3 vertex1 = Vector3.Zero, vertex2 = Vector3.Zero, vertex3 = Vector3.Zero;
            return RayIntersectsMesh(ray, vertices, indices, world, out vertex1, out vertex2, out vertex3);
        }

        public static float? RayIntersectsMesh(Ray ray, Vector3[] vertices, ushort[] indices, Matrix world, out Vector3 vertex1, out Vector3 vertex2, out Vector3 vertex3)
        {
            vertex1 = vertex2 = vertex3 = Vector3.Zero;

            Matrix inverseTransform = Matrix.Invert(world);

            ray.Position = MathHelper.ToVector3(Vector3.Transform(ray.Position, inverseTransform));
            ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransform);

            BoundingBox boundingSphere = BoundingBox.FromPoints(vertices);

            float dist = 0;

            if (BoundingBox.Intersects(boundingSphere, ray, out dist) == false)
            {
                return null;
            }
            else
            {
                float? closestIntersection = null;
                for (int i = 0; i < indices.Length - 2; i++)
                {
                    float? intersection;


                    RayIntersectsTriangle(ref ray,
                        ref vertices[indices[i]],
                        ref vertices[indices[i + 1]],
                        ref vertices[indices[i + 2]], out intersection);

                    if (intersection != null)
                    {
                        if ((closestIntersection == null) || (intersection < closestIntersection))
                        {
                            closestIntersection = intersection;
                            Vector4 outUnit;
                            Vector3.Transform(ref vertices[indices[i]],
                                              ref world, out outUnit);
                            vertex1 = MathHelper.ToVector3(outUnit);

                            Vector3.Transform(ref vertices[indices[i + 1]],
                                              ref world, out outUnit);
                            vertex2 = MathHelper.ToVector3(outUnit);

                            Vector3.Transform(ref vertices[indices[i + 2]],
                                              ref world, out outUnit);
                            vertex3 = MathHelper.ToVector3(outUnit);
                        }
                    }
                }

                return closestIntersection;
            }
        }

        static void RayIntersectsTriangle(ref Ray ray, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant = Vector3.Dot(edge1, directionCrossEdge2);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU = Vector3.Dot(distanceVector, directionCrossEdge2);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV = Vector3.Dot(ray.Direction, distanceCrossEdge1);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance = Vector3.Dot(edge2, distanceCrossEdge1);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }

        public static Vector4 Centroid(Vector4[] points)
        {
            Vector4 center = Vector4.Zero;
            foreach (Vector4 point in points) center += point;
            center.X /= points.Length;
            center.Y /= points.Length;
            center.Z /= points.Length;
            center.W /= points.Length;
            return center;
        }

        public static Vector3 Centroid(Vector3[] points)
        {
            Vector3 center = Vector3.Zero;
            foreach (Vector3 point in points) center += point;
            center.X /= points.Length;
            center.Y /= points.Length;
            center.Z /= points.Length;
            return center;
        }

        public static Vector2 Centroid(Vector2[] points)
        {
            Vector2 center = Vector2.Zero;
            foreach (Vector2 point in points) center += point;
            center.X /= points.Length;
            center.Y /= points.Length;
            return center;
        }

        public static void sf(Matrix proj, Vector3 cameraPosition, Vector3 cameraDirection, int NumSplits)
        {
            
            //// shadow view matrix
            //Matrix mShadowView = Matrix.LookAtRH(cameraPosition, cameraPosition + cameraDirection, MathHelper.Up);

            ///*// determine shadow projection based on model bounding sphere
            //{
            //    var center = Vector3.Transform(arena.BoundingSphere.Center, mShadowView);

            //    var min = center - new Vector3(arena.BoundingSphere.Radius);
            //    var max = center + new Vector3(arena.BoundingSphere.Radius);

            //    mShadowProjection = Matrix.CreateOrthographicOffCenter(min.X, max.X, min.Y, max.Y, -max.Z, -min.Z);
            //}

            //mShadowTransform = mShadowView * mShadowProjection;*/



            //// determine clip space split distances
            //var splitDistances = new[] { -1, -100.0f, -300.0f, -600 }
            //    .Select(d =>
            //    {
            //        var c = Vector3.Transform(new Vector3(0, 0, d), proj);
            //        return c.Z / c.W;
            //    })
            //    .ToArray();

            //// determine split projections
            //var splitData = Enumerable.Range(0, NumSplits).Select(i =>
            //{
            //    var n = splitDistances[i];
            //    var f = splitDistances[i + 1];

            //    // get frustum split corners and transform into shadow space
            //    //var frustumCorners = splitFrustum(n, f)
            //        //.Select(v => Vector3.Transform(v, mShadowView));
            //    var frustumCorners = new BoundingFrustum(proj * mShadowView).GetCorners().Select(v => Vector3.Transform(v, mShadowView));

            //    var min = frustumCorners.Aggregate((v1, v2) => Vector3.Min(v1, v2));
            //    var max = frustumCorners.Aggregate((v1, v2) => Vector3.Max(v1, v2));

            //    // determine the min/max z values based on arena bounding box
            //    var arenaBB = GeometryHelper.transformBoundingBox(arena.CollisionData.geometry.boundingBox, ShadowView);
            //    var minZ = -arenaBB.Max.Z;
            //    var maxZ = -arenaBB.Min.Z;

            //    // return orthographic projection
            //    return new
            //    {
            //        Distance = f,
            //        Projection = Matrix.OrthoOffCenterRH(min.X, max.X, min.Y, max.Y, minZ, maxZ)
            //    };
            //}).ToArray();

            //// compute final split transforms
            //ShadowSplitProjections = splitData.Select(s => mShadowView * s.Projection).ToArray();
            //ShadowSplitDistances = splitData.Select(s => s.Distance).ToArray();
        }

        //public static Vector3 Min(Vector3 v1, Vector3 v2)
        //{

        //}

        public static Matrix[] SplitFrustum(int NumSplits, Vector3 cameraDirection, Vector3 cameraPosition, float NearClip, float FarClip)
        {
            Matrix[] Frustums = new Matrix[NumSplits];
            float[] splitDepths = new float[NumSplits + 1];
            const float splitConstant = 0.95f;
            splitDepths[0] = NearClip;
            splitDepths[NumSplits] = FarClip;
            for (int i = 1; i < splitDepths.Length - 1; i++)
                splitDepths[i] = splitConstant * NearClip * (float)Math.Pow(FarClip / NearClip, i / NumSplits) + (1.0f - splitConstant) * ((NearClip + (i / NumSplits)) * (FarClip - NearClip));

            // Render our scene geometry to each split of the cascade
            for (int i = 0; i < NumSplits; i++)
            {
                float minZ = splitDepths[i];
                float maxZ = splitDepths[i + 1];

                //Frustums[i] = CalculateFrustum(cameraDirection, cameraPosition, FarClip, minZ, maxZ);

                //RenderShadowMap(modelList, i);
            }
            return Frustums;
        }

        public static Matrix CalculateFrustum(Vector3 cameraDirection, Vector3 cameraPosition, Matrix proj, Matrix view, float FarClip, float minZ, float maxZ, float shadowMapSize)
        {
            Vector4[] frustumCornersVS = new Vector4[8];
            Vector3[] frustumCorners = new BoundingFrustum(view * proj).GetCorners();
            for (int i = 0; i < 8; i++)
                frustumCornersVS[i] = new Vector4(frustumCorners[i], 1);
            Vector3[] frustumCornersWS = new Vector3[8];
            Vector4[] frustumCornersLS = new Vector4[8];
            Vector3[] farFrustumCornersVS = new Vector3[4];
            Vector3[] splitFrustumCornersVS = new Vector3[8];

            // Shorten the view frustum according to the shadow view distance
            Matrix cameraMatrix = Matrix.Translation(cameraPosition);

            for (int i = 0; i < 4; i++)
                splitFrustumCornersVS[i] = MathHelper.ToVector3(frustumCornersVS[i + 4]) * (minZ / FarClip);

            for (int i = 4; i < 8; i++)
                splitFrustumCornersVS[i] = MathHelper.ToVector3(frustumCornersVS[i]) * (maxZ / FarClip);

            for (int i = 0; i < 8; i++)
            {
                frustumCornersWS[i] = MathHelper.ToVector3(Vector3.Transform(splitFrustumCornersVS[i], cameraMatrix));
            }

            // Find the centroid
            Vector3 frustumCentroid = new Vector3(0, 0, 0);
            for (int i = 0; i < 8; i++)
                frustumCentroid += frustumCornersWS[i];
            frustumCentroid /= 8;

            // Position the shadow-caster camera so that it's looking at the centroid,
            // and backed up in the direction of the sunlight
            float distFromCentroid = Math.Max((maxZ - minZ), Vector3.Distance(splitFrustumCornersVS[4], splitFrustumCornersVS[5])) + 50.0f;
            Matrix viewMatrix = Matrix.LookAtRH(frustumCentroid - (cameraDirection * distFromCentroid), frustumCentroid, new Vector3(0, 1, 0));

            // Determine the position of the frustum corners in light space
            Vector3.Transform(frustumCornersWS, ref viewMatrix, frustumCornersLS);

            // Calculate an orthographic projection by sizing a bounding box 
            // to the frustum coordinates in light space
            Vector3 mins = MathHelper.ToVector3(frustumCornersLS[0]);
            Vector3 maxes = MathHelper.ToVector3(frustumCornersLS[0]);
            for (int i = 0; i < 8; i++)
            {
                if (frustumCornersLS[i].X > maxes.X)
                    maxes.X = frustumCornersLS[i].X;
                else if (frustumCornersLS[i].X < mins.X)
                    mins.X = frustumCornersLS[i].X;
                if (frustumCornersLS[i].Y > maxes.Y)
                    maxes.Y = frustumCornersLS[i].Y;
                else if (frustumCornersLS[i].Y < mins.Y)
                    mins.Y = frustumCornersLS[i].Y;
                if (frustumCornersLS[i].Z > maxes.Z)
                    maxes.Z = frustumCornersLS[i].Z;
                else if (frustumCornersLS[i].Z < mins.Z)
                    mins.Z = frustumCornersLS[i].Z;
            }

            // Create an orthographic camera for use as a shadow caster
            const float nearClipOffset = 100.0f;
            maxes.Z += nearClipOffset;

            /*Vector3 vDiagonal = MathHelper.ToVector3(frustumCornersLS[0] - frustumCornersLS[6]);
            float fCascadeBound = MathHelper.Length(vDiagonal);
            float fWorldUnitsPerTexel = fCascadeBound / (float)1024;

            mins /= fWorldUnitsPerTexel;
            mins = MathHelper.Floor(mins);
            mins *= fWorldUnitsPerTexel;
            maxes /= fWorldUnitsPerTexel;
            maxes = MathHelper.Floor(maxes);
            maxes *= fWorldUnitsPerTexel;*/


            /*float ShadowMapSize = 1024.0f; // Set this to the size of your shadow map
            Vector3 shadowOrigin = Vector3.Transform(Vector3.Zero, shadowMatrix);
            shadowOrigin *= (ShadowMapSize / 2.0f);
            Vector2 roundedOrigin = new Vector2((float)Math.Round(shadowOrigin.X), (float)Math.Round(shadowOrigin.Y));
            Vector2 rounding = roundedOrigin - new Vector2(shadowOrigin.X, shadowOrigin.Y);
            rounding /= (ShadowMapSize / 2.0f);

            Matrix roundMatrix = Matrix.Translation(rounding.X, rounding.Y, 0.0f);
            shadowMatrix *= roundMatrix;*/

            float scale = 1.2f;
            Matrix projMatrix = Matrix.OrthoOffCenterRH(mins.X * scale, maxes.X * scale, mins.Y * scale, maxes.Y * scale, -maxes.Z * scale, -mins.Z * scale);
            Matrix shadowMatrix = viewMatrix * projMatrix;

            float ShadowMapSize = shadowMapSize; // Set this to the size of your shadow map
            Vector3 shadowOrigin = MathHelper.ToVector3(Vector3.Transform(Vector3.Zero, shadowMatrix));
            shadowOrigin *= (ShadowMapSize / 2.0f);
            Vector2 roundedOrigin = new Vector2((float)Math.Round(shadowOrigin.X), (float)Math.Round(shadowOrigin.Y));
            Vector2 rounding = roundedOrigin - new Vector2(shadowOrigin.X, shadowOrigin.Y);
            rounding /= (ShadowMapSize / 2.0f);

            Matrix roundMatrix = Matrix.Translation(rounding.X, rounding.Y, 0.0f);
            shadowMatrix *= roundMatrix;

            return shadowMatrix;
        }

        public static Matrix inverseTranspose(Matrix matrix) 
        { 
        Matrix copy = matrix; 
        copy.set_Rows(3,new Vector4( 0.0f, 0.0f, 0.0f, 1.0f));
        //DirectX::XMVECTOR determinant = DirectX::XMMatrixDeterminant(matrix); 
                //matrix.Determinant()
        return Matrix.Transpose(Matrix.Invert(copy));
        //return DirectX::XMMatrixTranspose(DirectX::XMMatrixInverse(&determinant, matrix)); 
        }

        public static Matrix MatrixFromString(string str)
        {
            string s = str.Substring(str.IndexOf("M44:") + 4, str.IndexOf("]]") - str.IndexOf("M44:") - 4);
            Matrix m = new Matrix();
            m.M11 = float.Parse(str.Substring(str.IndexOf("M11:") + 4, str.IndexOf("M12:") - str.IndexOf("M11:") - 5));
            m.M12 = float.Parse(str.Substring(str.IndexOf("M12:") + 4, str.IndexOf("M13:") - str.IndexOf("M12:") - 5));
            m.M13 = float.Parse(str.Substring(str.IndexOf("M13:") + 4, str.IndexOf("M14:") - str.IndexOf("M13:") - 5));
            m.M14 = float.Parse(str.Substring(str.IndexOf("M14:") + 4, str.IndexOf("] [M21") - str.IndexOf("M14:") - 4));
            m.M21 = float.Parse(str.Substring(str.IndexOf("M21:") + 4, str.IndexOf("M22:") - str.IndexOf("M21:") - 5));
            m.M22 = float.Parse(str.Substring(str.IndexOf("M22:") + 4, str.IndexOf("M23:") - str.IndexOf("M22:") - 5));
            m.M23 = float.Parse(str.Substring(str.IndexOf("M23:") + 4, str.IndexOf("M24:") - str.IndexOf("M23:") - 5));
            m.M24 = float.Parse(str.Substring(str.IndexOf("M24:") + 4, str.IndexOf("] [M31") - str.IndexOf("M24:") - 4));
            m.M31 = float.Parse(str.Substring(str.IndexOf("M31:") + 4, str.IndexOf("M32:") - str.IndexOf("M31:") - 5));
            m.M32 = float.Parse(str.Substring(str.IndexOf("M32:") + 4, str.IndexOf("M33:") - str.IndexOf("M32:") - 5));
            m.M33 = float.Parse(str.Substring(str.IndexOf("M33:") + 4, str.IndexOf("M34:") - str.IndexOf("M33:") - 5));
            m.M34 = float.Parse(str.Substring(str.IndexOf("M34:") + 4, str.IndexOf("] [M41") - str.IndexOf("M34:") - 4));
            m.M41 = float.Parse(str.Substring(str.IndexOf("M41:") + 4, str.IndexOf("M42:") - str.IndexOf("M41:") - 5));
            m.M42 = float.Parse(str.Substring(str.IndexOf("M42:") + 4, str.IndexOf("M43:") - str.IndexOf("M42:") - 5));
            m.M43 = float.Parse(str.Substring(str.IndexOf("M43:") + 4, str.IndexOf("M44:") - str.IndexOf("M43:") - 5));
            m.M44 = float.Parse(str.Substring(str.IndexOf("M44:") + 4, str.IndexOf("]]") - str.IndexOf("M44:") - 4));
            return m;
        }

        public static Vector4 Vector4FromString(string str)
        {
            return new Vector4(float.Parse(str.Substring(str.IndexOf("X:") + 2, str.IndexOf("Y:") - str.IndexOf("X:") - 3)),
                               float.Parse(str.Substring(str.IndexOf("Y:") + 2, str.IndexOf("Z:") - str.IndexOf("Y:") - 3)),
                               float.Parse(str.Substring(str.IndexOf("Z:") + 2, str.IndexOf("W:") - str.IndexOf("Z:") - 3)),
                               float.Parse(str.Substring(str.IndexOf("W:") + 2)));
        }

        public static Vector3 Vector3FromString(string str)
        {
            return new Vector3(float.Parse(str.Substring(str.IndexOf("X:") + 2, str.IndexOf("Y:") - str.IndexOf("X:") - 3)),
                               float.Parse(str.Substring(str.IndexOf("Y:") + 2, str.IndexOf("Z:") - str.IndexOf("Y:") - 3)),
                               float.Parse(str.Substring(str.IndexOf("Z:") + 2)));
        }

        public static Vector2 Vector2FromString(string str)
        {
            return new Vector2(float.Parse(str.Substring(str.IndexOf("X:") + 2, str.IndexOf("Y:") - str.IndexOf("X:") - 3)),
                               float.Parse(str.Substring(str.IndexOf("Y:") + 2)));
        }

        public static System.Windows.Point PointFromString(string str)
        {
            return new System.Windows.Point(int.Parse(str.Substring(str.IndexOf("X=") + 2, str.IndexOf("Y=") - str.IndexOf("X=") - 3)),
                                            int.Parse(str.Substring(str.IndexOf("Y=") + 2, str.Length - str.IndexOf("Y=") - 3)));
        }

        public static Vector3 ToVector3(Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 ToVector3(Vector2 vector)
        {
            return new Vector3(vector.X, vector.Y, 0);
        }

        public static Vector2 ToVector2(Vector4 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector2 ToVector2(Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector3 Add(Vector3 left, Vector3 right)
        {
            Vector3 ret = Vector3.Zero;
            ret.X = left.X + right.X;
            ret.Y = left.Y + right.Y;
            ret.Z = left.Z + right.Z;
            return ret;
        }
        public static Vector3 Substract(Vector3 left, Vector3 right)
        {
            Vector3 ret = Vector3.Zero;
            ret.X = left.X - right.X;
            ret.Y = left.Y - right.Y;
            ret.Z = left.Z - right.Z;
            return ret;
        }
        public static Vector3 Multiply(Vector3 left, Vector3 right)
        {
            Vector3 ret = Vector3.Zero;
            ret.X = left.X * right.X;
            ret.Y = left.Y * right.Y;
            ret.Z = left.Z * right.Z;
            return ret;
        }
        public static Vector2 Multiply(Vector2 left, Vector2 right)
        {
            Vector2 ret = Vector2.Zero;
            ret.X = left.X * right.X;
            ret.Y = left.Y * right.Y;
            return ret;
        }
        public static Vector3 Divide(Vector3 left, Vector3 right)
        {
            Vector3 ret = Vector3.Zero;
            ret.X = left.X / right.X;
            ret.Y = left.Y / right.Y;
            ret.Z = left.Z / right.Z;
            return ret;
        }

        public static float Distance(Vector3 point1, Vector3 point2)
        {
            double distX = (point1.X + -point2.X) * (point1.X + -point2.X);
            double distY = (point1.Y + -point2.Y) * (point1.Y + -point2.Y);
            double distZ = (point1.Z + -point2.Z) * (point1.Z + -point2.Z);
            double dist = distX + distY + distZ;
            float value = (float)Math.Sqrt(dist) * 0.1f;
            return value;
        }

        public static float Length(Vector3 vector)
        {
            return (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
        }

        public static Vector3 Floor(Vector3 vector)
        {
            return new Vector3((float)Math.Floor(vector.X), (float)Math.Floor(vector.Y), (float)Math.Floor(vector.Z));
        }

        public static Vector3 Unproject(Vector3 vector, SlimDX.Direct3D11.Viewport viewport, Matrix WorldViewProj)
        {
            return Vector3.Unproject(vector, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinZ, viewport.MaxZ, WorldViewProj);
        }

        public static Vector4 Vector4FromColor(System.Windows.Media.Color color)
        {
            float r = (float)color.R / (float)255;
            float g = (float)color.G / (float)255;
            float b = (float)color.B / (float)255;
            float a = (float)color.A / (float)255;
            return new Vector4(r, g, b, a);
        }

        public static System.Windows.Media.Color ColorFromVector4(Vector4 color)
        {
            float r = (float)color.X * 255;
            float g = (float)color.Y * 255;
            float b = (float)color.Z * 255;
            float a = (float)color.W * 255;
            return System.Windows.Media.Color.FromArgb((byte)(a), (byte)r, (byte)g, (byte)b);
        }

        public static Vector3 Up = new Vector3(0, 1, 0);
        public static Vector3 Down = new Vector3(0, -1, 0);
        public static Vector3 One = new Vector3(1, 1, 1);
    }
}
