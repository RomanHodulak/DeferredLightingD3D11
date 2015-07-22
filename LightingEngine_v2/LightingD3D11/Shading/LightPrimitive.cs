using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    public abstract class LightPrimitive
    {
        public Renderer Renderer { get; set; }

        public LightPrimitive()
        {
        }

        public abstract RenderableLightPrimitive GetRenderablePrimitive();

        public abstract void SetBuffers(RenderableLightPrimitive primitive, Light light);

        public abstract void Draw(RenderableLightPrimitive primitive);
    }

    public class RenderableLightPrimitive : IDisposable
    {
        public LightShader Shader { get; set; }
        public Buffer VertexBuffer { get; set; }
        public int VertexCount { get; set; }
        public Buffer IndexBuffer { get; set; }
        public int IndexCount { get; set; }

        public RenderableLightPrimitive(LightShader shader, Buffer vertexbuffer, int vertexCount, Buffer indexbuffer, int indexCount)
        {
            this.Shader = shader;
            this.VertexBuffer = vertexbuffer;
            this.IndexBuffer = indexbuffer;
            this.VertexCount = vertexCount;
            this.IndexCount = indexCount;
        }

        public void Dispose()
        {
            Shader.Dispose();
            VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();
        }
    }
}
