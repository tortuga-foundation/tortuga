using System;
using System.Drawing;
using System.Threading.Tasks;
using Tortuga.Graphics;

namespace Tortuga.Utils
{
    public static class ImageLoader
    {
        public static async Task<Graphics.Image> Load(string path)
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