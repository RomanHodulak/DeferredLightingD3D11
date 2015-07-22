using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using Vector3 = SlimDX.Vector3;
using Buffer = SlimDX.Direct3D11.Buffer;
using DataStream = SlimDX.DataStream;
using BulletSharp;

namespace LightingEngine_v2.LightingD3D11
{
    [Serializable]
    public abstract class GeometricPrimitive : System.IDisposable
    {
        public Renderer Renderer
        {
            get { return renderer; }
            set { renderer = value; }
        }
        public GeometryData GeometryData
        {
            get { return geometryData; }
            set { if (geometryData != null) geometryData.Dispose(); geometryData = value; InitializePrimitive(); }
        }
        public MaterialShader MaterialShader
        {
            get
            {
                return materialShader;
            }
            set
            {
                materialShader = value;
                GeometryData.Dispose();
                InitializePrimitive();
            }
        }
        public Matrix MatrixWorld
        {
            get { return matrixWorld; }
            set { matrixWorld = value; }
        }
        public CollisionShape CollisionShape
        {
            get { return collisionShape; }
            set { collisionShape = value; }
        }

        [NonSerialized]
        MaterialShader materialShader;

        [NonSerialized]
        Renderer renderer;

        [NonSerialized]
        GeometryData geometryData;

        [NonSerialized]
        Matrix matrixWorld;

        [NonSerialized]
        CollisionShape collisionShape;

        public GeometricPrimitive()
        {
            geometryData = new GeometryData();
            matrixWorld = Matrix.Identity;
        }

        public void GenerateTangentFrame(out Vector3[] Tangent, out Vector3[] Bitangent)
        {
            Tangent = new Vector3[GeometryData.Vertices.Count];
            Bitangent = new Vector3[GeometryData.Vertices.Count];
            for (int i = 0; i < GeometryData.Indices.Count; i += 3)
            {
                Vector3 bitangent = Vector3.Zero;
                Vector3 tangent = MathHelper.CalculateTangent(
                    GeometryData.Vertices[GeometryData.Indices[i]].GetVertex<Vector3>(VertexParameterType.Position).Value,
                    GeometryData.Vertices[GeometryData.Indices[i + 1]].GetVertex<Vector3>(VertexParameterType.Position).Value,
                    GeometryData.Vertices[GeometryData.Indices[i + 2]].GetVertex<Vector3>(VertexParameterType.Position).Value,
                    GeometryData.Vertices[GeometryData.Indices[i]].GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value,
                    GeometryData.Vertices[GeometryData.Indices[i + 1]].GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value,
                    GeometryData.Vertices[GeometryData.Indices[i + 2]].GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value, out bitangent);
                Tangent[GeometryData.Indices[i]] += tangent;
                Tangent[GeometryData.Indices[i + 1]] += tangent;
                Tangent[GeometryData.Indices[i + 2]] += tangent;
                Bitangent[GeometryData.Indices[i]] += bitangent;
                Bitangent[GeometryData.Indices[i + 1]] += bitangent;
                Bitangent[GeometryData.Indices[i + 2]] += bitangent;
            }
            for (int a = 0; a < GeometryData.Vertices.Count; a++)
            {
                Vector3 n = GeometryData.Vertices[a].GetVertex<Vector3>(VertexParameterType.Normal).Value;
                Vector3 t = Tangent[a];
        
                // Gram-Schmidt orthogonalize
                Tangent[a] = Vector3.Normalize(t - n * Vector3.Dot(n, t));

                //Bitangent[a] = Vector3.Cross(n, Tangent[a]);
        
                // Calculate handedness
                //tangent[a].w = (Dot(Cross(n, t), tan2[a]) < 0.0F) ? -1.0F : 1.0F;
            }
        }

        protected void InitializePrimitive()
        {
            if (materialShader == null)
                materialShader = MaterialShader.DefaultMaterial(Renderer);
            if (this.GetType() == typeof(SkinnedMeshPrimitive))
                GeometryData.VertexDefinition = MaterialShader.SkinnedMeshVertexDefinition;
            else
                GeometryData.VertexDefinition = MaterialShader.StaticMeshVertexDefinition;
            GeometryData.VertexStride = GeometryData.VertexDefinition.SizeInBytes;
            GeometryData.VertexCount = GeometryData.Vertices.Count;
            GeometryData.IndexCount = GeometryData.Indices.Count;

            DataStream VerticesStream = new DataStream(GeometryData.VertexCount * GeometryData.VertexStride, true, true);
            Vector3[] Tangent;
            Vector3[] Bitangent;
            GenerateTangentFrame(out Tangent, out Bitangent);
            for (int j = 0; j < GeometryData.Vertices.Count; j++)
            {
                Vertex v = GeometryData.Vertices[j];
                for (int i = 0; i < GeometryData.VertexDefinition.Parameters.Count; i++)
                {
                    VertexParameterType vp = GeometryData.VertexDefinition.Parameters[i];
                    if (vp == VertexParameterType.Position)
                        VerticesStream.Write(v.GetVertex<Vector3>(vp).Value);
                    else if (vp == VertexParameterType.Normal)
                        VerticesStream.Write(v.GetVertex<Vector3>(vp).Value);
                    else if (vp == VertexParameterType.TextureCoordinate)
                        VerticesStream.Write(v.GetVertex<Vector2>(vp).Value);
                    else if (vp == VertexParameterType.Tangent)
                    {
                        //VerticesStream.Write(Vector3.Normalize(Tangent[j]));
                        VerticesStream.Write(MathHelper.CalculateTangent(v.GetVertex<Vector3>(VertexParameterType.Normal).Value));
                        /*if ((j + (GeometryData.Vertices.Count % 3)) % 3 == 0 & GeometryData.Vertices.Count - (j + (GeometryData.Vertices.Count % 3)) > 2)
                        {
                            tan = MathHelper.CalculateTangent(
                            v.GetVertex<Vector3>(VertexParameterType.Position).Value,
                            GeometryData.Vertices[j + 1].GetVertex<Vector3>(VertexParameterType.Position).Value,
                            GeometryData.Vertices[j + 2].GetVertex<Vector3>(VertexParameterType.Position).Value,
                            v.GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value,
                            GeometryData.Vertices[j + 1].GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value,
                            GeometryData.Vertices[j + 2].GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value
                            );
                        }
                        Vector3 val = v.GetVertex<Vector3>(VertexParameterType.Normal).Value;
                        Vector3 vec = MathHelper.ToVector3(Vector3.Transform(val, Matrix.RotationX(MathHelper.PiOver2)));
                        if (v is VertexPosNorTexTanBin)
                        {
                            VerticesStream.Write(v.GetVertex<Vector3>(vp).Value);
                        }
                        else
                        {
                            VerticesStream.Write(MathHelper.CalculateTangent(v.GetVertex<Vector3>(VertexParameterType.Normal).Value));
                        }*/
                    }
                    else if (vp == VertexParameterType.Binormal)
                    {
                        //VerticesStream.Write(Vector3.Normalize(Bitangent[j]));
                        VerticesStream.Write(-MathHelper.CalculateTangent(v.GetVertex<Vector3>(VertexParameterType.Normal).Value));
                        /*if ((j + (GeometryData.Vertices.Count % 3)) % 3 == 0 & GeometryData.Vertices.Count - (j + (GeometryData.Vertices.Count % 3)) > 2)
                        {
                            bin = MathHelper.CalculateBitangent(
                                v.GetVertex<Vector3>(VertexParameterType.Position).Value,
                                GeometryData.Vertices[j + 1].GetVertex<Vector3>(VertexParameterType.Position).Value,
                                GeometryData.Vertices[j + 2].GetVertex<Vector3>(VertexParameterType.Position).Value,
                                v.GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value,
                                GeometryData.Vertices[j + 1].GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value,
                                GeometryData.Vertices[j + 2].GetVertex<Vector2>(VertexParameterType.TextureCoordinate).Value
                                );
                        }
                        Vector3 val = v.GetVertex<Vector3>(VertexParameterType.Normal).Value;
                        Vector3 vec = MathHelper.ToVector3(Vector3.Transform(val, Matrix.RotationZ(MathHelper.PiOver2)));
                        if (v is VertexPosNorTexTanBin)
                        {
                            VerticesStream.Write(v.GetVertex<Vector3>(vp).Value);
                        }
                        else
                        {
                            VerticesStream.Write(MathHelper.CalculateBitangent(v.GetVertex<Vector3>(VertexParameterType.Normal).Value));
                        }*/
                    }
                    else if (vp == VertexParameterType.Bones32Bit)
                    {
                        Vector4 vec = (v.GetVertex<Vector4>(vp).Value);
                        VerticesStream.Write((uint)vec.X);
                        VerticesStream.Write((uint)vec.Y);
                        VerticesStream.Write((uint)vec.Z);
                        VerticesStream.Write((uint)vec.W);
                    }
                    else if (vp == VertexParameterType.Weights)
                    {
                        Vector4 vec = (v.GetVertex<Vector4>(vp).Value);
                        VerticesStream.Write(v.GetVertex<Vector4>(vp).Value);
                    }
                    else if (vp == VertexParameterType.ColorRGB)
                        VerticesStream.Write(v.GetVertex<Vector3>(vp).Value);
                }
            }
            VerticesStream.Position = 0;
            GeometryData.VertexBuffer = new Buffer(Renderer.Device, VerticesStream, GeometryData.VertexStride * GeometryData.VertexCount,
                ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, GeometryData.VertexStride);

            DataStream IndicesStream = new DataStream(GeometryData.Indices.ToArray(), true, true);
            IndicesStream.Position = 0;

            GeometryData.IndexBuffer = new Buffer(Renderer.Device, IndicesStream, sizeof(ushort) * GeometryData.IndexCount, 
                ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, sizeof(ushort));

            IndicesStream.Close();
            VerticesStream.Close();
        }

        /// <summary>
        /// Correctly integrated generatable primitive returns new generated geometry. Otherwise returns null.
        /// </summary>
        public virtual GeometryData GenerateGeometry()
        {
            return null;
        }

        public void Draw()
        {
            Draw(Renderer.Context);
        }

        public void Draw(DeviceContext Context)
        {
            Context.InputAssembler.PrimitiveTopology = MaterialShader.Topology;
            if (this is SkinnedMeshPrimitive)
                Context.InputAssembler.InputLayout = MaterialShader.SkinnedMeshInputLayout;
            else
                Context.InputAssembler.InputLayout = MaterialShader.StaticMeshInputLayout;
            Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(GeometryData.VertexBuffer, GeometryData.VertexStride, 0));
            Context.InputAssembler.SetIndexBuffer(GeometryData.IndexBuffer, SlimDX.DXGI.Format.R16_UInt, 0);

            Context.DrawIndexed(GeometryData.IndexCount, 0, 0);
        }

        public virtual void Dispose()
        {
            Helper.Dispose(GeometryData, CollisionShape, MaterialShader);
        }
    }

    public class GeometryData : System.IDisposable
    {
        public Buffer VertexBuffer { get; set; }
        public List<Vertex> Vertices = new List<Vertex>();
        public Vector3[] Positions
        {
            get
            {
                Vector3[] pts = new Vector3[Vertices.Count];
                for(int i = 0; i < Vertices.Count; i++)
                    pts[i] = (Vertices[i].Vertices[0] as VertexParameter<Vector3>).Value;
                return pts;
            }
        }
        public VertexDefinition VertexDefinition { get; set; }
        public int VertexCount { get; set; }
        public int VertexStride { get; set; }
        public Buffer IndexBuffer { get; set; }
        public List<ushort> Indices = new List<ushort>();
        public int IndexCount { get; set; }
        public int CurrentVertex
        {
            get { return Vertices.Count; }
        }

        public void AddVertex(Vector3 position, Vector3 normal, Vector2 textCoordinate)
        {
            AddVertex(new VertexPosNorTex(position, normal, textCoordinate));
        }

        public void AddVertex(Vector3 position, Vector3 normal, ProjectionType type)
        {
            AddVertex(new VertexPosNorTex(position, normal, GenerateUV(type, position, normal, new Vector2(1, 1))));
        }

        public void AddVertex(Vector3 position, ProjectionType type)
        {
            Vector3 normal = new Vector3(1,1,1);
            if ((Vertices.Count + 1) % 3 == 0)
            {
                normal = Vector3.Cross(position, (Vertices[Vertices.Count - 1] as VertexPosNorTex).Position);
                (Vertices[Vertices.Count - 1] as VertexPosNorTex).Normal = normal;
                (Vertices[Vertices.Count - 2] as VertexPosNorTex).Normal = normal;
            }
            AddVertex(new VertexPosNorTex(position, normal, GenerateUV(type, position, normal, new Vector2(1, 1))));
        }

        public void AddVertex(Vertex vert)
        {
            Vertices.Add(vert);
            VertexCount = Vertices.Count;
        }

        public void AddIndex(int Index)
        {
            Indices.Add((ushort)Index);
        }

        public static Vector2 GenerateUV(ProjectionType type, Vector3 position, Vector3 normal, Vector2 scale)
        {
            if (type == ProjectionType.Spherical)
            {
                return MathHelper.Multiply(
                    new Vector2(
                        0.5f - ((float)Math.Atan2(position.Z, position.X)) / ((float)Math.PI * 2),
                        0.5f - 2 * ((float)Math.Asin(position.Y)) / ((float)Math.PI * 2)), scale * 6);
            }
            else if (type == ProjectionType.Box)
            {
                //return MathHelper.Multiply(new Vector2((position.X * normal.Z) + (position.Y * normal.X) + (position.Z * normal.Y), (position.X * normal.Y) + (position.Y * normal.Z) + (position.Z * normal.X)), (textureScale * 0.1f));
                return MathHelper.Multiply(
                    new Vector2(
                        (position.X * normal.Z) + (position.Y * normal.X) + (position.Z * normal.Y),
                        (position.X * normal.Y) + (position.Y * normal.Z) + (position.Z * normal.X)),
                        (scale * 0.1f)
                        );
            }
            else if (type == ProjectionType.Conical)
            {
                float phi = (float)Math.Atan2(normal.Z, normal.X);
                return MathHelper.Multiply(new Vector2(
                    position.Y,
                    phi
                    ), scale);
            }
            return Vector2.Zero;
        }

        public void Dispose()
        {
            if (VertexBuffer != null)
                if (!VertexBuffer.Disposed)
                    VertexBuffer.Dispose();
            if (IndexBuffer != null)
                if (!IndexBuffer.Disposed)
                    IndexBuffer.Dispose();
        }
    }

    public enum ProjectionType
    {
        Spherical,
        Conical,
        Box
    }
}
