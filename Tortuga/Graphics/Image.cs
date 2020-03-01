using System;
using System.Drawing;

namespace Tortuga.Graphics
{
    public class Image
    {
        [Flags]
        public enum Channel
        {
            R,
            G,
            B,
            A
        }

        public uint Width => _width;
        public uint Height => _height;
        public Color[] Pixels => _pixels;

        private Color[] _pixels;
        private uint _width;
        private uint _height;

        public Image(string path)
        {
            var img = new Bitmap(path);
            _width = Convert.ToUInt32(img.Width);
            _height = Convert.ToUInt32(img.Height);
            _pixels = new Color[img.Width * img.Height];
            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                    _pixels[(i * img.Height) + j] = img.GetPixel(i, j);
        }
        public Image(uint width, uint height)
        {
            _width = width;
            _height = height;
            _pixels = new Color[width * height];
        }

        public void CopyChannel(Image source, Channel channels)
        {
            if (source._pixels.Length != _pixels.Length)
                throw new NotSupportedException("source and destination images must be the same size");

            for (int i = 0; i < source._pixels.Length; i++)
            {
                if ((channels & Channel.R) != 0)
                {
                    _pixels[i] = Color.FromArgb(
                        _pixels[i].A,
                        source._pixels[i].R,
                        _pixels[i].G,
                        _pixels[i].B
                    );
                }
                if ((channels & Channel.G) != 0)
                {
                    _pixels[i] = Color.FromArgb(
                        _pixels[i].A,
                        _pixels[i].R,
                        source._pixels[i].G,
                        _pixels[i].B
                    );
                }
                if ((channels & Channel.B) != 0)
                {
                    _pixels[i] = Color.FromArgb(
                        _pixels[i].A,
                        _pixels[i].R,
                        _pixels[i].G,
                        source._pixels[i].B
                    );
                }
                if ((channels & Channel.A) != 0)
                {
                    _pixels[i] = Color.FromArgb(
                        source._pixels[i].A,
                        _pixels[i].R,
                        _pixels[i].G,
                        _pixels[i].B
                    );
                }
            }
        }

        public Color GetPixel(uint x, uint y)
        {
            return _pixels[(x * _height) + y];
        }

        public static Image SingleColor(Color color)
        {
            var image = new Image(1, 1);
            image._pixels[0] = color;
            return image;
        }
    }
}