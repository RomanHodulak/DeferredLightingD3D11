using System.Collections.Generic;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;
using System;
using SlimDX;

namespace LightingEngine_v2.LightingD3D11
{
    public class ConstantBufferWrapper : IDisposable
    {
        public Renderer Renderer { get; set; }
        public Buffer CBuffer { get; set; }
        public int SizeInBytes { get; set; }
        public ShaderType ShaderType { get; set; }
        public int ResourceSlot { get; set; }
        public List<Semantic> Semantics { get; set; }
        public ConstantBufferWrapperDescription Description
        {
            get
            {
                return new ConstantBufferWrapperDescription()
                    {
                        ResourceSlot = ResourceSlot,
                        Semantics = Semantics,
                        ShaderType = ShaderType,
                        SizeInBytes = SizeInBytes,
                    };
            }
        }

        public ConstantBufferWrapper(Renderer Renderer, int SizeInBytes, ShaderType ShaderType, int slot)
        {
            this.Renderer = Renderer;
            this.SizeInBytes = SizeInBytes;
            this.ShaderType = ShaderType;
            this.ResourceSlot = slot;
            CBuffer = new Buffer(Renderer.Device, new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = SizeInBytes,
                StructureByteStride = 0,
            });
            this.Semantics = new List<Semantic>();
        }

        /// <summary>
        /// Fills the buffer by semantics
        /// </summary>
        public void FillBuffer(DeviceContext Context)
        {
            FillBuffer(Context, Matrix.Identity);
        }

        /// <summary>
        /// Fills the buffer by semantics
        /// </summary>
        public void FillBuffer(DeviceContext Context, GeometricPrimitive primitive)
        {
            FillBuffer(Context, primitive.MatrixWorld);
        }

        /// <summary>
        /// Fills the buffer by semantics
        /// </summary>
        public void FillBuffer(DeviceContext Context, Matrix World)
        {
            DataStream cbdata = Context.MapSubresource(CBuffer, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None).Data;
            FillData(cbdata, World);
            Context.UnmapSubresource(CBuffer, ResourceSlot);
        }

        /// <summary>
        /// Fills the buffer by semantics
        /// </summary>
        public void FillBuffer(DeviceContext Context, Matrix World, params object[] CustomData)
        {
            DataStream cbdata = Context.MapSubresource(CBuffer, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None).Data;
            FillData(cbdata, World);
            foreach (object o in CustomData)
            {
                Type T = o.GetType();
                if (T == typeof(Vector3))
                {
                    cbdata.Write((Vector3)o);
                    cbdata.Position += sizeof(float);
                }
                else if (T == typeof(Vector2))
                {
                    long i = cbdata.Position;
                    cbdata.Write((Vector2)o);
                    cbdata.Position += 2 * sizeof(float);
                }
                else if (T == typeof(Vector4))
                {
                    cbdata.Write((Vector4)o);
                }
                else if (T == typeof(float))
                {
                    cbdata.Write((float)o);
                    cbdata.Position += 3 * sizeof(float);
                }
                else if (T == typeof(int))
                {
                    cbdata.Write((int)o);
                    cbdata.Position += 3 * sizeof(float);
                }
                else if (T == typeof(Matrix))
                {
                    cbdata.Write((Matrix)o);
                }
                else if (T == typeof(Matrix[]))
                {
                    cbdata.WriteRange((Matrix[])o);
                }
                else if (T == typeof(Vector2[]))
                {
                    Vector2[] a = (Vector2[])o;
                    foreach (Vector2 vec in a)
                    {
                        cbdata.Write(vec);
                        cbdata.Position += 2 * sizeof(float);
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            Context.UnmapSubresource(CBuffer, ResourceSlot);
        }

        void FillData(DataStream cbdata, Matrix World)
        {
            foreach (Semantic s in Semantics)
            {
                if (s == Semantic.World)
                {
                    cbdata.Write((World));
                }
                else if (s == Semantic.View)
                {
                    cbdata.Write((Renderer.Camera.MatrixView));
                }
                else if (s == Semantic.Projection)
                {
                    cbdata.Write((Renderer.Camera.MatrixProj));
                }
                else if (s == Semantic.ViewProj)
                {
                    cbdata.Write((Renderer.Camera.MatrixViewProj));
                }
                else if (s == Semantic.WorldViewProj)
                {
                    cbdata.Write(World * Renderer.Camera.MatrixViewProj);
                }
                else if (s == Semantic.WorldViewProjInverse)
                {
                    Matrix m = World * Renderer.Camera.MatrixViewProj;
                    m.Invert();
                    cbdata.Write(m);
                }
                else if (s == Semantic.ProjectionInverse)
                {
                    Matrix m = Renderer.Camera.MatrixProj;
                    m.Invert();
                    cbdata.Write(m);
                }
                else if (s == Semantic.WorldViewInverse)
                {
                    Matrix m = World * Renderer.Camera.MatrixView;
                    m.Invert();
                    cbdata.Write(m);
                }
                else if (s == Semantic.WorldViewInverseTranspose)
                {
                    Matrix m = World * Renderer.Camera.MatrixView;
                    m.Invert();
                    cbdata.Write((m));
                }
                else if (s == Semantic.WorldView)
                {
                    Matrix m = World * Renderer.Camera.MatrixView;
                    cbdata.Write((m));
                }
                else if (s == Semantic.ViewInverse)
                {
                    Matrix m = Renderer.Camera.MatrixView;
                    m.Invert();
                    cbdata.Write(m);
                }
                else if (s == Semantic.ViewProjInverse)
                {
                    cbdata.Write(Matrix.Invert(Renderer.Camera.MatrixViewProj));
                }
                else if (s == Semantic.CameraPosition)
                {
                    cbdata.Write(Renderer.Camera.Position);
                    cbdata.Position += sizeof(float);
                }
                else if (s == Semantic.CameraNearFar)
                {
                    cbdata.Write(Renderer.Camera.zNear);
                    cbdata.Write(Renderer.Camera.zFar);
                    cbdata.Write(0.0f);
                    cbdata.Write(0.0f);
                    //cbdata.Position += sizeof(float) * 2;
                }
            }
        }

        public static ConstantBufferWrapper FromDescription(Renderer Renderer, ConstantBufferWrapperDescription desc)
        {
            ConstantBufferWrapper w = new ConstantBufferWrapper(Renderer, desc.SizeInBytes, desc.ShaderType, desc.ResourceSlot);
            w.Semantics = desc.Semantics;
            return w;
        }

        public void Dispose()
        {
            if (!CBuffer.Disposed)
                CBuffer.Dispose();
        }
    }

    public class ConstantBufferWrapperDescription
    {
        public int SizeInBytes { get; set; }
        public ShaderType ShaderType { get; set; }
        public int ResourceSlot { get; set; }
        public List<Semantic> Semantics { get; set; }
    }

    public enum ShaderType
    {
        PixelShader,
        HullShader,
        DomainShader,
        ComputeShader,
        VertexShader,
        GeometryShader,
    }

    public enum Semantic
    {
        World,
        WorldView,
        WorldViewInverse,
        WorldViewInverseTranspose,
        View,
        ViewInverse,
        Projection,
        ProjectionInverse,
        ViewProj,
        ViewProjInverse,
        WorldViewProj,
        WorldViewProjInverse,
        CameraPosition,
        CameraNearFar,
    }
}
