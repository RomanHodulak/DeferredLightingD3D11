using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SlimDX.Direct3D11.Buffer;
using SlimDX;
using System.Runtime.InteropServices;

namespace LightingEngine_v2.LightingD3D11
{
    public class StructuredBuffer<T> : IDisposable where T : struct
    {
        public Buffer Buffer { get { return buffer; } }
        public UnorderedAccessView UnorderedAccess { get { return uaView; } }
        public ShaderResourceView ShaderResource { get { return srView; } }
        public int ElementsCount { get { return elementsCount; } }
        public int StructureByteStride { get { return stride; } }

        Buffer buffer;
        UnorderedAccessView uaView;
        ShaderResourceView srView;
        int elementsCount;
        int stride;
        bool csAccess;
        bool dynamic;

        public StructuredBuffer(int elementsCount, bool computeShaderAccess, bool dynamic)
        {
            this.csAccess = computeShaderAccess;
            this.dynamic = dynamic;
            this.elementsCount = elementsCount;
            this.stride = Marshal.SizeOf(typeof(T));
            InitBuffers(csAccess, dynamic);
        }

        public void FillBuffer(DeviceContext Context, T[] data)
        {
            DataStream sbdata = Context.MapSubresource(buffer, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None).Data;
            sbdata.WriteRange(data);
            Context.UnmapSubresource(buffer, 0);
        }

        public void FillBuffer(DeviceContext Context, int startIndex, T[] data)
        {
            DataStream sbdata = Context.MapSubresource(buffer, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None).Data;
            sbdata.Position = startIndex * stride;
            sbdata.WriteRange(data);
            Context.UnmapSubresource(buffer, 0);
        }

        public void Resize(int elementsCount)
        {
            Helper.Dispose(buffer, uaView, srView);
            this.elementsCount = elementsCount;
            InitBuffers(csAccess, dynamic);

        }

        void InitBuffers(bool computeShaderAccess, bool dynamic)
        {
            buffer = new Buffer(Renderer.Device, new BufferDescription()
            {
                SizeInBytes = stride * elementsCount,
                Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                BindFlags = computeShaderAccess ? (BindFlags.UnorderedAccess | BindFlags.ShaderResource) : BindFlags.ShaderResource,
                CpuAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.StructuredBuffer,
                StructureByteStride = stride,
            });
            if (computeShaderAccess)
            {
                uaView = new UnorderedAccessView(Renderer.Device, buffer, new UnorderedAccessViewDescription()
                {
                    Format = SlimDX.DXGI.Format.Unknown,
                    Dimension = UnorderedAccessViewDimension.Buffer,
                    FirstElement = 0,
                    Flags = UnorderedAccessViewBufferFlags.None,
                    ElementCount = elementsCount,
                });
            }
            srView = new ShaderResourceView(Renderer.Device, buffer, new ShaderResourceViewDescription()
            {
                Format = SlimDX.DXGI.Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                ElementOffset = 0,
                ElementWidth = elementsCount,
            });
        }

        public void Dispose()
        {
            Helper.Dispose(buffer, uaView, srView);
        }
    }
}
