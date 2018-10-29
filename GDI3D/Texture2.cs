using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GDI3D
{
    public class Texture2
    {
        private byte[] Data;
        public int Stride => 4 * Width;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture2(string filename)
        {
            using (Bitmap bmp = new Bitmap(filename))
            {
                Width = bmp.Width;
                Height = bmp.Height;
                Data = new byte[bmp.Width * bmp.Height * 4];
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Marshal.Copy(bmpData.Scan0, Data, 0, bmp.Width * bmp.Height * 4);
                bmp.UnlockBits(bmpData);
            }
        }

        public Vector4 Sample(Vector2 v) => Sample(v.X, v.Y);

        public Vector4 Sample(float u, float v)
        {
            if (float.IsNaN(u) || float.IsNaN(v))
                return Vector4.Zero;
            if (u > 1 || u < 0 || v > 1 || v < 0)
                return Vector4.Zero;
            int x = (int)(u * Width);
            int y = (int)((1 - v) * Height);
            if (x >= Width || y >= Height)
                return Vector4.Zero;
            int pos = (x + (y*Width))*4;
            return new Vector4(Data[pos + 2] / 255.0f, Data[pos + 1] / 255.0f, Data[pos] / 255.0f, Data[pos + 3]);
        }
    }
}
