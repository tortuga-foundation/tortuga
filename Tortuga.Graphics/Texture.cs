using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tortuga.Graphics
{
    /// <summary>
    /// texture class, used for storing images that can be used to render
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// width of the image
        /// </summary>
        public int Width => _width;
        /// <summary>
        /// height of the image
        /// </summary>
        public int Height => _height;
        /// <summary>
        /// pixels of the image
        /// </summary>
        public GraphicsColor[] Pixels => _pixels;

        private int _width;
        private int _height;
        private GraphicsColor[] _pixels;


        /// <summary>
        /// creates a 1x1 black image
        /// </summary>
        public Texture() : this(Color.Black) { }

        /// <summary>
        /// constructor for texture
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        public Texture(int width, int height)
        {
            _width = width;
            _height = height;
        }

        /// <summary>
        /// constructor for texture
        /// </summary>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <param name="pixels">pixels for image</param>
        public Texture(int width, int height, Color[] pixels) : this(width, height)
        {
            if (pixels == null)
                throw new InvalidOperationException("pixels are set to null");
            if (pixels.Length != width * height)
                throw new InvalidOperationException("pixels are not the correct size");

            _pixels = pixels.Select(p => p.ToGraphicsColor()).ToArray();
        }

        /// <summary>
        /// Creats a 1x1 texture image with a specific color
        /// </summary>
        /// <param name="color">Color of the image</param>
        public Texture(Color color) : this(1, 1)
        {
            _pixels = new GraphicsColor[] { color.ToGraphicsColor() };
        }

        /// <summary>
        /// resizes the image to a specific size
        /// </summary>
        /// <param name="width">new width of the image</param>
        /// <param name="height">new height of the image</param>
        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;
            _pixels = new GraphicsColor[width * height];
        }

        /// <summary>
        /// set pixels of the image
        /// </summary>
        /// <param name="pixels">pixel data for the iamge</param>
        public void SetPixels(Color[] pixels)
        {
            if (pixels.Length != _pixels.Length)
                throw new InvalidOperationException("pixels length does not match image size");

            _pixels = pixels.Select(p => p.ToGraphicsColor()).ToArray();
        }

        /// <summary>
        /// Get's the pixel data from the image
        /// </summary>
        public Color[] GetPixels()
        => _pixels.Select(p => Color.FromArgb(p.A, p.R, p.G, p.B)).ToArray();

        /// <summary>
        /// copies a specific channel(s) from an image to this image
        /// </summary>
        /// <param name="source">source image to copy from</param>
        /// <param name="channel">channel(s) to copy</param>
        public void CopyChannel(Texture source, TextureChannelFlags channel)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    float percentX = Convert.ToSingle(x) / Convert.ToSingle(_width);
                    float percentY = Convert.ToSingle(y) / Convert.ToSingle(_height);
                    int sourceX = Convert.ToInt32(MathF.Floor(percentX * source.Width));
                    int sourceY = Convert.ToInt32(MathF.Floor(percentY * source.Height));

                    if ((channel & TextureChannelFlags.R) != 0)
                        this._pixels[(y * _width) + x].R = source.Pixels[(sourceY * source.Width) + sourceX].R;
                    if ((channel & TextureChannelFlags.G) != 0)
                        this._pixels[(y * _width) + x].G = source.Pixels[(sourceY * source.Width) + sourceX].G;
                    if ((channel & TextureChannelFlags.B) != 0)
                        this._pixels[(y * _width) + x].B = source.Pixels[(sourceY * source.Width) + sourceX].B;
                    if ((channel & TextureChannelFlags.A) != 0)
                        this._pixels[(y * _width) + x].A = source.Pixels[(sourceY * source.Width) + sourceX].A;
                }
            }
        }
    }
}