#region Using Statements
using System;
using SlimDX.Direct3D11;
using SlimDX;
using BulletSharp;
#endregion

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    /// <summary>
    /// A geometric primitive class for spheres.
    /// </summary>
    public class SpherePrimitive : GeometricPrimitive
    {
        public float Radius { get; set; }
        public float Tessellation { get; set; }

        /// <summary>
        /// Constructs a new sphere primitive, using default settings.
        /// </summary>
        public SpherePrimitive(Renderer Renderer)
            : this(Renderer, 1, 16)
        {
        }


        /// <summary>
        /// Constructs a new sphere primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public SpherePrimitive(Renderer Renderer, float radius, int tessellation)
        {
            this.Renderer = Renderer;
            this.CollisionShape = new SphereShape(radius);
            this.Tessellation = tessellation - (tessellation % 2);
            if (Tessellation < 3)
                this.Tessellation = 3;
            this.Radius = radius;
            this.GeometryData = GenerateGeometry();
        }

        public override GeometryData GenerateGeometry()
        {
            GeometryData GeometryData = new GeometryData();
            int verticalSegments = (int)Tessellation;
            int horizontalSegments = (int)Tessellation * 2;

            // Start with a single vertex at the bottom of the sphere.
            GeometryData.AddVertex(new VertexPosNorTexTanBin(MathHelper.Down * Radius, MathHelper.Down, new Vector2(0.5f, 1.0f), new Vector3(0, 0, -1), new Vector3(-1, 0, 0)));

            float maxlat = ((verticalSegments) * MathHelper.Pi /
                                        verticalSegments) - MathHelper.PiOver2;
            float maxlon = horizontalSegments * MathHelper.TwoPi / horizontalSegments;

            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i < verticalSegments - 1; i++)
            {
                float latitude = ((i + 1) * MathHelper.Pi / verticalSegments) - MathHelper.PiOver2;

                float dy = (float)Math.Sin(latitude);
                float dxz = (float)Math.Cos(latitude);
                float dyb = (float)Math.Atan2((float)Math.Sin(-latitude), (float)Math.Cos(-latitude));

                // Create a single ring of vertices at this latitude.
                for (int j = -(horizontalSegments / 2) + 1; j < (horizontalSegments / 2) + 1; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude) * dxz;
                    float dxt = (float)Math.Cos(longitude + MathHelper.PiOver2);
                    float dz = (float)Math.Sin(longitude) * dxz;
                    float dzt = (float)Math.Sin(longitude + MathHelper.PiOver2);
                    Vector3 normal = new Vector3(dx, dy, dz);
                    Vector3 position = normal * Radius;
                    Vector2 texcoord = GeometryData.GenerateUV(ProjectionType.Spherical, position, normal, new Vector2(1, 1));
                    GeometryData.AddVertex(new VertexPosNorTexTanBin(position, normal, texcoord,
                        new Vector3(dxt, 0, dzt), Vector3.Normalize(Vector3.Cross(normal, new Vector3(dxt, 0, dzt)))));
                }
            }

            // Finish with a single vertex at the top of the sphere.
            GeometryData.AddVertex(new VertexPosNorTexTanBin(MathHelper.Up * Radius, MathHelper.Up, new Vector2(0.5f, 1.0f), new Vector3(0, 0, 1), new Vector3(1, 0, 0)));

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                GeometryData.AddIndex(0);
                GeometryData.AddIndex(1 + (i + 1) % horizontalSegments);
                GeometryData.AddIndex(1 + i);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (int i = 0; i < verticalSegments - 2; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    GeometryData.AddIndex(1 + i * horizontalSegments + j);
                    GeometryData.AddIndex(1 + i * horizontalSegments + nextJ);
                    GeometryData.AddIndex(1 + nextI * horizontalSegments + j);

                    GeometryData.AddIndex(1 + i * horizontalSegments + nextJ);
                    GeometryData.AddIndex(1 + nextI * horizontalSegments + nextJ);
                    GeometryData.AddIndex(1 + nextI * horizontalSegments + j);
                }
            }

            // Create a fan connecting the top vertex to the top latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                GeometryData.AddIndex(GeometryData.CurrentVertex - 1);
                GeometryData.AddIndex(GeometryData.CurrentVertex - 2 - (i + 1) % horizontalSegments);
                GeometryData.AddIndex(GeometryData.CurrentVertex - 2 - i);
            }

            return GeometryData;
        }
    }
}
