using BulletSharp;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightingEngine_v2.LightingD3D11
{
    public class BoxPrimitive : GeometricPrimitive
    {
        public Vector3 Dimensions { get; set; }

        /// <summary>
        /// Constructs a new box primitive, with the specified dimensions.
        /// </summary>
        public BoxPrimitive(Renderer Renderer, Vector3 dimensions)
        {
            this.Renderer = Renderer;
            this.Dimensions = dimensions;
            CollisionShape = new BoxShape(new Vector3(dimensions.X / 2, dimensions.Y / 2, dimensions.Z / 2));
            GeometryData = GenerateGeometry();
        }

        public override GeometryData GenerateGeometry()
        {
            GeometryData GeometryData = new GeometryData();

            Vector3[] normals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
            };

            Vector2[] texcoords =
            {
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),

                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),

                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),

                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),

                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
            };

            int i = 0;
            foreach (Vector3 normal in normals)
            {
                // Get two vectors perpendicular to the face normal and to each other.
                Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
                Vector3 side2 = Vector3.Cross(normal, side1);

                // Six indices (two triangles) per face.
                GeometryData.AddIndex(GeometryData.CurrentVertex + 0);
                GeometryData.AddIndex(GeometryData.CurrentVertex + 1);
                GeometryData.AddIndex(GeometryData.CurrentVertex + 2);

                GeometryData.AddIndex(GeometryData.CurrentVertex + 0);
                GeometryData.AddIndex(GeometryData.CurrentVertex + 2);
                GeometryData.AddIndex(GeometryData.CurrentVertex + 3);

                // Four vertices per face.
                float size = Dimensions.X;
                if (normal.Z != 0)
                    size = Dimensions.Z;
                else if (normal.Y != 0)
                    size = Dimensions.Y;

                GeometryData.AddVertex(new VertexPosNorTexTanBin((normal - side1 - side2) * size / 2, normal, texcoords[i], side2, side1));
                GeometryData.AddVertex(new VertexPosNorTexTanBin((normal - side1 + side2) * size / 2, normal, texcoords[i + 1], side2, side1));
                GeometryData.AddVertex(new VertexPosNorTexTanBin((normal + side1 + side2) * size / 2, normal, texcoords[i + 2], side2, side1));
                GeometryData.AddVertex(new VertexPosNorTexTanBin((normal + side1 - side2) * size / 2, normal, texcoords[i + 3], side2, side1));
                i += 4;
            }
            return GeometryData;
        }
    }
}
