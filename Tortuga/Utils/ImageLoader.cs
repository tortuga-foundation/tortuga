using System;
using System.Drawing;

namespace Tortuga.Utils
{
    public class ImageLoader
    {
        public uint Width => _width;
        public uint Height => _height;
        public Color[] Pixels => _pixels;

        private Color[] _pixels;
        private uint _width;
        private uint _height;

        public ImageLoader(string path)
        {
            var img = new Bitmap(path);
            _width = Convert.ToUInt32(img.Width);
            _height = Convert.ToUInt32(img.Height);
            _pixels = new Color[img.Width * img.Height];
            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                    _pixels[(i * img.Height) + j] = img.GetPixel(i, j);
        }
    }
}