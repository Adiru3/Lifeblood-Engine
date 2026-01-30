using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Lifeblood.Engine;

namespace Lifeblood.Rendering
{
    public class Texture : IDisposable
    {
        public uint ID { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture(string path)
        {
            if (System.IO.File.Exists(path))
            {
                using (Bitmap bmp = new Bitmap(path))
                {
                    LoadFromBitmap(bmp);
                }
            }
            else
            {
                // Fallback: Checkerboard
                using (Bitmap bmp = CreateCheckerboard(64, 64))
                {
                    LoadFromBitmap(bmp);
                }
            }
        }

        public Texture(int width, int height, Color color)
        {
             using (Bitmap bmp = new Bitmap(width, height))
             {
                 using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(color); }
                 LoadFromBitmap(bmp);
             }
        }
        
        public static Bitmap CreateCheckerboard(int w, int h)
        {
             Bitmap bmp = new Bitmap(w, h);
             using (Graphics g = Graphics.FromImage(bmp))
             {
                 g.Clear(Color.Gray);
                 for(int y=0; y<h; y+=16)
                    for(int x=0; x<w; x+=16)
                        if ((x/16 + y/16) % 2 == 0) g.FillRectangle(Brushes.LightGray, x, y, 16, 16);
             }
             return bmp;
        }

        private void LoadFromBitmap(Bitmap bmp)
        {
            Width = bmp.Width;
            Height = bmp.Height;

            uint[] ids = new uint[1];
            GL.glGenTextures(1, ids);
            ID = ids[0];

            GL.glBindTexture(GL.GL_TEXTURE_2D, ID);

            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, (int)GL.GL_RGBA, 
                bmp.Width, bmp.Height, 0, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, data.Scan0);

            bmp.UnlockBits(data);

            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_NEAREST);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_NEAREST);
        }

        public void Bind()
        {
            GL.glBindTexture(GL.GL_TEXTURE_2D, ID);
        }

        public void Unbind()
        {
             GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
        }

        public void Dispose()
        {
            // GL.glDeleteTextures...
        }
    }
}
