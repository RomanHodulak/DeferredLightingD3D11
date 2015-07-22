using System.IO;
using SlimDX.D3DCompiler;

namespace LightingEngine_v2.LightingD3D11.Shading
{
    public class IncludeFX : Include
    {
        static string includeDirectory = @"Shaders\";

        public IncludeFX(string path)
        {
            includeDirectory = path;
        }

        public void Close(Stream stream)
        {
            stream.Close();
        }

        public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
        {
            stream = new FileStream(includeDirectory + fileName, FileMode.Open);
        }
    }
}
