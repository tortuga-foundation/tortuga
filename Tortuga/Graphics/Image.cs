using System;
using System.Drawing;
using System.Threading.Tasks;

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

        public uint Width;
        public uint Height;
        public Color[] Pixels;
        
        public Image(uint width, uint height)
        {
            Width = width;
            Height = height;
            Pixels = new Color[width * height];
        }

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

        public static Image SingleColor(Color color)
        {
            var image = new Image(1, 1);
            image.Pixels[0] = color;
            return image;
        }
    }
}