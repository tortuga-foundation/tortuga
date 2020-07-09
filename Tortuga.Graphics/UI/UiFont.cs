using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Numerics;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

namespace Tortuga.UI
{
    /// <summary>
    /// font used for user interface
    /// </summary>
    public class UiFont
    {
        /// <summary>
        /// describes the layout of the atlas image
        /// </summary>
        [System.Serializable]
        public struct FontAtlasDescription
        {
            /// <summary>
            /// the character at a specific position
            /// </summary>
            public char Character { get; set; }
            /// <summary>
            /// position X of this character
            /// </summary>
            public float OffsetX { get; set; }
            /// <summary>
            /// position Y of this character
            /// </summary>
            public float OffsetY { get; set; }
            /// <summary>
            /// Size X of this character
            /// </summary>
            public float SizeX { get; set; }
            /// <summary>
            /// Size Y of this character
            /// </summary>
            public float SizeY { get; set; }
        }

        /// <summary>
        /// atlas image texture
        /// </summary>
        public Graphics.Texture Atlas => _atlas;
        /// <summary>
        /// description specifing where each character is in the atlas
        /// </summary>
        public List<FontAtlasDescription> Descriptions => _descriptions;
        private Graphics.Texture _atlas;
        private List<FontAtlasDescription> _descriptions;

        /// <summary>
        /// Constructor for ui font
        /// </summary> 
        private UiFont()
        {
        }

        /// <summary>
        /// Builds an atlas from ttf file and stores
        /// </summary>
        /// <param name="ttfFile">path to the ttf font file</param>
        /// <param name="path">path to store the atlas in</param>
        public static Task BuildAtlas(string ttfFile, string path)
        {
            return Task.Run(() =>
            {
                string content = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 !@#$%^&*()-=_+[];',./{}:\"?><~`";
                int fontSize = 50;
                int atlasSize = 512;

                //load ttf file
                var fontCollection = new PrivateFontCollection();
                fontCollection.AddFontFile(ttfFile);

                //setup resources for creating atlas
                var font = new Font(fontCollection.Families[0], fontSize);
                var atlas = new Bitmap(atlasSize, atlasSize);
                var description = new List<FontAtlasDescription>();

                //setup draw resources
                var textBrush = new SolidBrush(Color.White);
                var drawing = System.Drawing.Graphics.FromImage(atlas);
                drawing.Clear(Color.FromArgb(0, 0, 0, 0));

                //draw atlas on image
                var offset = Vector2.Zero;
                foreach (char character in content)
                {
                    var charSize = drawing.MeasureString(character.ToString(), font);
                    if (offset.X + charSize.Width > atlasSize)
                    {
                        offset.X = 0;
                        offset.Y += font.GetHeight();
                        if (offset.Y > atlasSize)
                            break;
                    }
                    description.Add(new FontAtlasDescription()
                    {
                        Character = character,
                        OffsetX = offset.X,
                        OffsetY = offset.Y,
                        SizeX = charSize.Width,
                        SizeY = charSize.Height
                    });
                    drawing.DrawString(
                        character.ToString(),
                        font,
                        textBrush,
                        new RectangleF()
                        {
                            X = offset.X,
                            Y = offset.Y,
                            Width = charSize.Width,
                            Height = charSize.Height
                        },
                        new StringFormat()
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Near
                        }
                    );
                    offset.X += charSize.Width;
                }
                drawing.Save();

                //save atlas to a temporary path
                atlas.Save(string.Format("{0}.png", path), ImageFormat.Png);
                var jsonDescription = JsonSerializer.Serialize(
                    description.ToArray(),
                    new JsonSerializerOptions()
                    {
                        WriteIndented = true,
                    }
                );
                File.WriteAllText(string.Format("{0}.json", path), jsonDescription);
            });
        }

        /// <summary>
        /// Builds an atlas from ttf and loads it into memory
        /// </summary>
        /// <param name="ttfFile">path to the ttf font file</param>
        public static async Task<UiFont> LoadFromTTF(string ttfFile)
        {
            var path = string.Format("{0}{1}", Path.GetTempPath(), "temp_font_atlas");
            await BuildAtlas(ttfFile, path);
            return await UiFont.LoadFromAtlas(string.Format("{0}.png", path), string.Format("{0}.json", path));
        }

        /// <summary>
        /// Load font from a pre built atlas files
        /// </summary>
        /// <param name="atlasImage">atlas image file</param>
        /// <param name="atlasDescription">atlas json description file</param>
        /// <returns></returns>
        public static async Task<UiFont> LoadFromAtlas(string atlasImage, string atlasDescription)
        {
            var atlasImageTask = Graphics.Texture.Load(atlasImage);
            var jsonContent = File.ReadAllText(atlasDescription);
            var descriptions = JsonSerializer.Deserialize<List<FontAtlasDescription>>(jsonContent);

            return new UiFont()
            {
                _atlas = await atlasImageTask,
                _descriptions = descriptions
            };
        }
    }
}