using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    /// <summary>
    /// image texture
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// image width
        /// </summary>
        public int Width => _width;
        /// <summary>
        /// image height
        /// </summary>
        public int Height => _height;
        private int _width;
        private int _height;
        /// <summary>
        /// raw pixels
        /// </summary>
        public ShaderPixel[] Pixels => _pixels;
        private ShaderPixel[] _pixels;

        /// <summary>
        /// constructor for texture
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        public Texture(int width, int height)
        {
            if (width == 0 || height == 0)
                throw new InvalidOperationException("image texture's height and width cannot be zero");

            _width = width;
            _height = height;
            _pixels = new ShaderPixel[width * height];
        }

        /// <summary>
        /// constructor for texture
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="pixels">image texture raw pixels</param>
        public Texture(int width, int height, ShaderPixel[] pixels) : this(width, height)
        {
            if (pixels == null)
                throw new InvalidOperationException("pixels are set to null");
            if (pixels.Length != width * height)
                throw new InvalidOperationException("pixels size must be (width * height)");
            _pixels = pixels;
        }
        /// <summary>
        /// constructor for texture
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="pixels">image texture pixels</param>
        public Texture(int width, int height, Color[] pixels) : this(width, height)
        {
            if (pixels == null)
                throw new InvalidOperationException("pixels are set to null");
            if (pixels.Length != width * height)
                throw new InvalidOperationException("pixels size must be (width * height)");

            _pixels = new ShaderPixel[width * height];
            for (int i = 0; i < pixels.Length; i++)
                _pixels[i] = new ShaderPixel(pixels[i].R, pixels[i].G, pixels[i].B, pixels[i].A);
        }

        /// <summary>
        /// creates a 1 pixel image texture
        /// </summary>
        /// <param name="color">the color of the image texture</param>
        /// <returns>image texture</returns>
        public static Texture SingleColor(Color color)
        {
            return new Texture(1, 1)
            {
                _pixels = new ShaderPixel[]
                {
                    new ShaderPixel(color.R, color.B, color.G, color.A)
                }
            };
        }

        /// <summary>
        /// Loads an image texture from a file
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns>image texture</returns>
        public static async Task<Texture> Load(string path)
        {
            var loader = Texture.SingleColor(System.Drawing.Color.Black);
            await Task.Run(() =>
            {
                try
                {
                    if (System.IO.File.Exists(path) == false)
                        throw new System.IO.FileNotFoundException();

                    var img = new Bitmap(path);
                    var width = img.Width;
                    var height = img.Height;
                    var pixels = new Color[img.Width * img.Height];
                    loader = new Texture(width, height);
                    for (int i = 0; i < img.Width; i++)
                    {
                        for (int j = 0; j < img.Height; j++)
                        {
                            var p = img.GetPixel(i, j);
                            loader._pixels[(j * img.Width) + i] = new ShaderPixel(
                                p.R, p.G, p.B, p.A
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return loader;
        }
    }
}