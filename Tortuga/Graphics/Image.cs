using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Used to store / load images used by the tortuga engine
    /// </summary>
    public class Image
    {
        /// <summary>
        /// Different channels for the image
        /// </summary>
        [Flags]
        public enum Channel
        {
            /// <summary>
            /// Red Channel
            /// </summary>
            R,
            /// <summary>
            /// Green Channel
            /// </summary>
            G,
            /// <summary>
            /// Blue Channel
            /// </summary>
            B,
            /// <summary>
            /// Alpha Channel
            /// </summary>
            A
        }

        /// <summary>
        /// Width of the image
        /// </summary>
        public uint Width;
        /// <summary>
        /// Height of the image
        /// </summary>
        public uint Height;
        /// <summary>
        /// Image pixels
        /// </summary>
        public Color[] Pixels;
        
        /// <summary>
        /// Create a new image with a set of width and height
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        public Image(uint width, uint height)
        {
            Width = width;
            Height = height;
            Pixels = new Color[width * height];
        }

        /// <summary>
        /// Copy another image's channel into this image
        /// </summary>
        /// <param name="source">Source image to copy</param>
        /// <param name="channels">The channel to copy</param>
        public void CopyChannel(Image source, Channel channels)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    float xP = (float)x / (float)Width;
                    float yP = (float)y / (float)Height;
                    var sourcePixel = source.GetPixel(
                        Convert.ToUInt32(MathF.Round(xP * source.Width)),
                        Convert.ToUInt32(MathF.Round(yP * source.Height))
                    );

                    int index = (x * Convert.ToInt32(Height)) + y;
                    if ((channels & Channel.R) != 0)
                    {
                        Pixels[index] = Color.FromArgb(
                            Pixels[index].A,
                            sourcePixel.R,
                            Pixels[index].G,
                            Pixels[index].B
                        );
                    }
                    if ((channels & Channel.G) != 0)
                    {
                        Pixels[index] = Color.FromArgb(
                            Pixels[index].A,
                            Pixels[index].R,
                            sourcePixel.G,
                            Pixels[index].B
                        );
                    }
                    if ((channels & Channel.B) != 0)
                    {
                        Pixels[index] = Color.FromArgb(
                            Pixels[index].A,
                            Pixels[index].R,
                            Pixels[index].G,
                            sourcePixel.B
                        );
                    }
                    if ((channels & Channel.A) != 0)
                    {
                        Pixels[index] = Color.FromArgb(
                            sourcePixel.A,
                            Pixels[index].R,
                            Pixels[index].G,
                            Pixels[index].B
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Get a pixel in the image at a specific location
        /// </summary>
        /// <param name="x">X location of this pixel</param>
        /// <param name="y">Y locaiton of this pixel</param>
        /// <returns>Pixel as a color object</returns>
        public Color GetPixel(uint x, uint y)
        {
            if (y >= Height)
                y = Height - 1;
            else if (y < 0)
                y = 0;
            if (x >= Width)
                x = Width - 1;
            else if (x < 0)
                x = 0;

            return Pixels[(x * Height) + y];
        }

        /// <summary>
        /// Create a new image that is 1x1 with a color
        /// </summary>
        /// <param name="color">Color of the image</param>
        /// <returns>Image object that can be used by the tortuga engine</returns>
        public static Image SingleColor(Color color)
        {
            var image = new Image(1, 1);
            image.Pixels[0] = color;
            return image;
        }

        /// <summary>
        /// Load an image from a file
        /// </summary>
        /// <param name="path">path to the image file</param>
        /// <returns>Image object that can be used by the tortuga engine</returns>
        public static async Task<Image> Load(string path)
        {
            var loader = Graphics.Image.SingleColor(System.Drawing.Color.Black);
            await Task.Run(() =>
            {
                try
                {
                    if (System.IO.File.Exists(path) == false)
                        throw new System.IO.FileNotFoundException();

                    var img = new Bitmap(path);
                    loader.Width = Convert.ToUInt32(img.Width);
                    loader.Height = Convert.ToUInt32(img.Height);
                    loader.Pixels = new Color[img.Width * img.Height];
                    for (int i = 0; i < img.Width; i++)
                        for (int j = 0; j < img.Height; j++)
                            loader.Pixels[(i * img.Height) + j] = img.GetPixel(i, j);
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