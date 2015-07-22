using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.IO;

namespace LightingEngine_v2.LightingD3D11
{
    public static class ContentHelper
    {
        public static SlimDX.Direct3D11.ShaderResourceView GetShaderResourceView(SlimDX.Direct3D11.Device device, SlimDX.Direct3D11.Texture2D texture)
        {
            SlimDX.Direct3D11.ShaderResourceViewDescription desc = new SlimDX.Direct3D11.ShaderResourceViewDescription();
            desc.Format = texture.Description.Format;
            desc.Dimension = SlimDX.Direct3D11.ShaderResourceViewDimension.Texture2D;
            desc.MostDetailedMip = 0;
            desc.MipLevels = 1;

            return new SlimDX.Direct3D11.ShaderResourceView(device, texture, desc);
        }

        public static SlimDX.Direct3D11.Texture2D TextureFromColor(SlimDX.Direct3D11.Device device, Color4 color)
        {
            SlimDX.Direct3D11.ShaderResourceView srv;
            SlimDX.Direct3D11.Texture2D texture = TextureFromColor(device, color, out srv);
            srv.Dispose();
            return texture;
        }

        public static SlimDX.Direct3D11.Texture2D TextureFromColor(SlimDX.Direct3D11.Device device, Color4 color, out SlimDX.Direct3D11.ShaderResourceView srv)
        {
            /*SlimDX.Direct3D11.Texture2DDescription desc2 = new SlimDX.Direct3D11.Texture2DDescription();
            desc2.SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0);
            desc2.Width = 1;
            desc2.Height = 1;
            desc2.MipLevels = 1;
            desc2.ArraySize = 1;
            desc2.Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm;
            desc2.Usage = SlimDX.Direct3D11.ResourceUsage.Dynamic;
            desc2.BindFlags = SlimDX.Direct3D11.BindFlags.ShaderResource;
            desc2.CpuAccessFlags = SlimDX.Direct3D11.CpuAccessFlags.Write;
            SlimDX.Direct3D11.Texture2D texture = new SlimDX.Direct3D11.Texture2D(device, desc2);


            // fill the texture with rgba values
            DataRectangle rect = texture.AsSurface().Map(SlimDX.DXGI.MapFlags.Write | SlimDX.DXGI.MapFlags.Discard);
            if (rect.Data.CanWrite)
            {
                for (int row = 0; row < texture.Description.Height; row++)
                {
                    int rowStart = row * rect.Pitch;
                    rect.Data.Seek(rowStart, System.IO.SeekOrigin.Begin);
                    for (int col = 0; col < texture.Description.Width; col++)
                    {
                        rect.Data.WriteByte((byte)(color.Red * 255));
                        rect.Data.WriteByte((byte)(color.Green * 255));
                        rect.Data.WriteByte((byte)(color.Blue * 255));
                        rect.Data.WriteByte((byte)(color.Alpha * 255));
                    }
                }
            }
            texture.AsSurface().Unmap();
            texture.AsSurface().Dispose();

            srv = GetShaderResourceView(device, texture);*/

            SlimDX.Direct3D11.Texture2DDescription desc2 = new SlimDX.Direct3D11.Texture2DDescription();
            desc2.SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0);
            desc2.Width = 1;
            desc2.Height = 1;
            desc2.MipLevels = 1;
            desc2.ArraySize = 1;
            desc2.Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm;
            desc2.Usage = SlimDX.Direct3D11.ResourceUsage.Default;
            desc2.BindFlags = SlimDX.Direct3D11.BindFlags.ShaderResource;
            desc2.CpuAccessFlags = SlimDX.Direct3D11.CpuAccessFlags.None;

            // fill the texture with rgba values
            DataStream stream = new DataStream(sizeof(byte) * 4, false, true);
            stream.WriteByte((byte)(color.Red * 255));
            stream.WriteByte((byte)(color.Green * 255));
            stream.WriteByte((byte)(color.Blue * 255));
            stream.WriteByte((byte)(color.Alpha * 255));
            stream.Position = 0;

            SlimDX.Direct3D11.Texture2D texture = new SlimDX.Direct3D11.Texture2D(device, desc2, new DataRectangle(1, stream));
            srv = GetShaderResourceView(device, texture);
            return texture;
        }

        public static SlimDX.Direct3D11.Resource ResourceFromBitmap(System.Drawing.Image bmp)
        {
            return TextureFromBitmap(bmp);
        }

        public static SlimDX.Direct3D11.Texture2D TextureFromBitmap(System.Drawing.Image bmp)
        {
            SlimDX.Direct3D11.Texture2D tx = null;
            using (MemoryStream s = new MemoryStream())
            {
                bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                s.Seek(0, SeekOrigin.Begin); //must do this, or error is thrown in next line
                tx = SlimDX.Direct3D11.Texture2D.FromStream(Renderer.Device, s, (int)s.Length);
            }
            return tx;
        }

        public static SlimDX.Direct3D11.Texture2D TextureFromBitmap(SlimDX.Direct3D11.Device device, System.Drawing.Bitmap bmp, SlimDX.Direct3D11.ImageLoadInformation ImageLoadInformation)
        {
            SlimDX.Direct3D11.Texture2D tx = null;
            using (MemoryStream s = new MemoryStream())
            {
                bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                s.Seek(0, SeekOrigin.Begin); //must do this, or error is thrown in next line
                tx = SlimDX.Direct3D11.Texture2D.FromStream(device, s, (int)s.Length, ImageLoadInformation);
            }
            return tx;
        }

        public static SlimDX.Direct3D11.Texture2D TextureFromBitmap(SlimDX.Direct3D11.Device device, System.Drawing.Bitmap bmp, out SlimDX.Direct3D11.ShaderResourceView srv)
        {
            SlimDX.Direct3D11.Texture2D tx = null;
            using (MemoryStream s = new MemoryStream())
            {
                bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                s.Seek(0, SeekOrigin.Begin); //must do this, or error is thrown in next line
                tx = SlimDX.Direct3D11.Texture2D.FromStream(device, s, (int)s.Length);
            }
            srv = GetShaderResourceView(device, tx);
            return tx;
        }

        public static System.Drawing.Image ImageFromTexture(SlimDX.Direct3D11.Texture2D texture, SlimDX.Direct3D11.DeviceContext Context)
        {
            System.Drawing.Image img;
            if (texture.Description.CpuAccessFlags == SlimDX.Direct3D11.CpuAccessFlags.Read)
            {
                DataBox dataRec = Context.MapSubresource(texture, 0, SlimDX.Direct3D11.MapMode.Read, SlimDX.Direct3D11.MapFlags.None);
                byte[] buffer = new byte[dataRec.Data.Length];
                MemoryStream s = new MemoryStream(buffer, true);
                dataRec.Data.CopyTo(s);
                s.Seek(0, SeekOrigin.Begin); //must do this, or error is thrown in next line
                img = System.Drawing.Image.FromStream(s);
                Context.UnmapSubresource(texture, 0);
            }
            else
            {
                img = null;
            }
            return img;
        }
    }
}
