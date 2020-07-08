using System.Drawing;
using System.Drawing.Text;
using Tortuga.Graphics;
using System.Numerics;

namespace Tortuga.UI
{
    /// <summary>
    /// font used for user interface
    /// </summary>
    public class UiFont
    {
        /// <summary>
        /// an image contains drawn text
        /// </summary>
        public class TextImage
        {
            /// <summary>
            /// pixels of the image
            /// </summary>
            public ShaderPixel[] Pixels;
            /// <summary>
            /// Width of the image
            /// </summary>
            public int Width;
            /// <summary>
            /// Height of the image
            /// </summary>
            public int Height;
        }

        private PrivateFontCollection _fontCollection;
        private Font _font;
        private Bitmap _image;
        private System.Drawing.Graphics _drawing;
        private SolidBrush _textBrush;

        /// <summary>
        /// Constructor for ui font
        /// </summary> 
        public UiFont(string filename)
        {
            _fontCollection = new PrivateFontCollection();
            _fontCollection.AddFontFile(filename);
        }
        /// <summary>
        /// De-constructor for ui font
        /// </summary> 
        ~UiFont()
        {
            Dispose();
        }

        /// <summary>
        /// draws text into an image
        /// </summary>
        /// <param name="text">the text to draw</param>
        /// <param name="offsetX">text offset X</param>
        /// <param name="offsetY">text offset Y</param>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <param name="fontSize">font size</param>
        /// <returns></returns>
        public TextImage DrawText(
            string text,
            int offsetX,
            int offsetY,
            int width,
            int height,
            int fontSize
        )
        {
            Dispose();
            _font = new Font(_fontCollection.Families[0], fontSize);
            _image = new Bitmap(width, height);
            _drawing = System.Drawing.Graphics.FromImage(_image);
            _drawing.Clear(Color.FromArgb(0, 0, 0, 0));
            _textBrush = new SolidBrush(Color.White);
            _drawing.DrawString(text, _font, _textBrush, offsetX, offsetY);

            var textImage = new TextImage()
            {
                Width = width,
                Height = height,
                Pixels = new ShaderPixel[height * width]
            };

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var p = _image.GetPixel(i, j);
                    textImage.Pixels[(j * width) + (width - i - 1)] = new ShaderPixel(
                        p.R, p.G, p.B, p.A
                    );
                }
            }
            return textImage;
        }

        private void Dispose()
        {
            _font?.Dispose();
            _image?.Dispose();
            _drawing?.Dispose();
            _textBrush?.Dispose();
        }
    }
}