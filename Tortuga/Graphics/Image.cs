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
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    float xP = (float)x / (float)_width;
                    float yP = (float)y / (float)_height;
                    var sourcePixel = source.GetPixel(
                        Convert.ToUInt32(MathF.Round(xP * source.Width)),
                        Convert.ToUInt32(MathF.Round(yP * source.Height))
                    );

                    int index = (x * Convert.ToInt32(_height)) + y;
                    if ((channels & Channel.R) != 0)
                    {
                        _pixels[index] = Color.FromArgb(
                            _pixels[index].A,
                            sourcePixel.R,
                            _pixels[index].G,
                            _pixels[index].B
                        );
                    }
                    if ((channels & Channel.G) != 0)
                    {
                        _pixels[index] = Color.FromArgb(
                            _pixels[index].A,
                            _pixels[index].R,
                            sourcePixel.G,
                            _pixels[index].B
                        );
                    }
                    if ((channels & Channel.B) != 0)
                    {
                        _pixels[index] = Color.FromArgb(
                            _pixels[index].A,
                            _pixels[index].R,
                            _pixels[index].G,
                            sourcePixel.B
                        );
                    }
                    if ((channels & Channel.A) != 0)
                    {
                        _pixels[index] = Color.FromArgb(
                            sourcePixel.A,
                            _pixels[index].R,
                            _pixels[index].G,
                            _pixels[index].B
                        );
                    }
                }
            }
        }

        public Color GetPixel(uint x, uint y)
        {
            if (y >= _height)
                y = _height - 1;
            else if (y < 0)
                y = 0;
            if (x >= _width)
                x = _width - 1;
            else if (x < 0)
                x = 0;

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