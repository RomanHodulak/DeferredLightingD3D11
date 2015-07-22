#region Using Statements
using SlimDX;
using SlimDX.Direct3D11;
using BulletSharp;
#endregion

namespace LightingEngine_v2.LightingD3D11
{
    /// <summary>
    /// A geometric primitive class that represents planes.
    /// </summary>
    public class PlanePrimitive : GeometricPrimitive
    {
        /// <summary>
        /// Constructs a new plane primitive, using default settings.
        /// </summary>
        public PlanePrimitive(Renderer Renderer)
            : this(Renderer, 1, 1, 1, 1)
        {
        }


        /// <summary>
        /// Constructs a new plane primitive, with the specified size.
        /// </summary>
        public PlanePrimitive(Renderer Renderer, float width, float length, int tessellationU, int tessellationV)
        {
            this.Renderer = Renderer;
            Vector3 normal = new Vector3(0, 1, 0);
            CollisionShape = new BoxShape(new Vector3(width / 2, 0.001f, length / 2));
            for (int v = 0; v < tessellationV+1; v++)
            {
                for (int u = 0; u < tessellationU+1; u++)
                {
                    GeometryData.AddVertex(new Vector3((((float)u / (float)tessellationU) * width) - (width / 2), 0, (((float)v / (float)tessellationV) * length) - (length / 2)), normal, new Vector2(((float)width / (float)tessellationU) * u, ((float)length / (float)tessellationV) * v));
                }
            }
            for (int v = 0; v < tessellationV - 0; v++)
            {
                for (int u = 0; u < tessellationU - 0; u++)
                {
                    GeometryData.AddIndex(u + 0 + ((tessellationU + 1) * v));
                    GeometryData.AddIndex(u + 1 + ((tessellationU + 1) * v));
                    GeometryData.AddIndex(u + 0 + tessellationU + 1 + ((tessellationU + 1) * v));

                    GeometryData.AddIndex(u + 0 + tessellationU + 1 + ((tessellationU + 1) * v));
                    GeometryData.AddIndex(u + 1 + ((tessellationU + 1) * v));
                    GeometryData.AddIndex(u + 1 + tessellationU + 1 + ((tessellationU + 1) * v));
                }
            }
            InitializePrimitive();
        }
    }
}