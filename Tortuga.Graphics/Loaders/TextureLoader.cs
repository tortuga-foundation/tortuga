using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    /// <summary>
    /// responsible loading files into memory
    /// </summary>
    public static partial class AssetLoader
    {
        /// <summary>
        /// loads an image file into texture object
        /// </summary>
        /// <param name="file">path of the file</param>
        public static Task<Texture> LoadTexture(string file) => Task.Run(() =>
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException();

            var bitmap = new Bitmap(file);
            var pixels = new Color[bitmap.Width * bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var p = bitmap.GetPixel(i, j);
                    pixels[(j * bitmap.Width) + i] = Color.FromArgb(
                        p.A, p.R, p.G, p.B
                    );
                }
            }
            var texture = new Texture(bitmap.Width, bitmap.Height);
            texture.SetPixels(pixels);
            return texture;
        });
    }
}