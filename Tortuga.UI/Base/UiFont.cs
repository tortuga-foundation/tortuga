using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Tortuga.UI.Base
{
    /// <summary>
    /// Represents a font, can be used to render ui text
    /// </summary>
    public class UiFont
    {
        /// <summary>
        /// This class represents how each character is represented in the font image atlas
        /// </summary>
        public class Symbol
        {
            /// <summary>
            /// Character code
            /// </summary>
            public int Identifier { set; get; }
            /// <summary>
            /// X position of the character in the atlas
            /// </summary>
            public int X { set; get; }
            /// <summary>
            /// Y position of the character in the atlas
            /// </summary>
            public int Y { set; get; }
            /// <summary>
            /// The height of the character in the atlas
            /// </summary>
            public int Height { set; get; }
            /// <summary>
            /// The width of the character in the atlas
            /// </summary>
            public int Width { set; get; }
            /// <summary>
            /// The X offset of the character in the atlas
            /// </summary>
            public int OffsetX { set; get; }
            /// <summary>
            /// The y offset of the character in the atlas
            /// </summary>
            public int OffsetY { set; get; }
            /// <summary>
            /// How many pixels to go forward after the character
            /// </summary>
            public int AdvanceX { set; get; }
        }

        /// <summary>
        /// Image texture containg the font data
        /// </summary>
        public Graphics.Image Atlas { private set; get; }
        /// <summary>
        /// 
        /// </summary>
        public int Base { private set; get; }
        /// <summary>
        /// How bold is the font
        /// </summary>
        public int Bold { private set; get; }
        /// <summary>
        /// Character height of the font
        /// </summary>
        public int CharHeight { private set; get; }
        /// <summary>
        /// Character spacing in the font
        /// </summary>
        public int CharSpacing { private set; get; }
        /// <summary>
        /// Name of the font
        /// </summary>
        public string Name { private set; get; }
        /// <summary>
        /// How italic is the font
        /// </summary>
        public int Italic { private set; get; }
        /// <summary>
        /// Line spacing of the font
        /// </summary>
        public int LineSpacing { private set; get; }
        /// <summary>
        /// Character size in the font
        /// </summary>
        public int Size { private set; get; }
        /// <summary>
        /// How smooth is the font
        /// </summary>
        public int Smooth { private set; get; }
        /// <summary>
        /// Where the texture atlas is stored
        /// </summary>
        public string TextureFile { private set; get; }
        /// <summary>
        /// All character symbols in the font
        /// </summary>
        public Symbol[] Symbols { private set; get; }

        #region Font JSON Loader

        private class ConfigJSON
        {
            public int Base { get; set; }
            public int Bold { get; set; }
            public int CharHeight { get; set; }
            public int CharSpacing { get; set; }
            public string Name { get; set; }
            public int Italic { get; set; }
            public int LineSpacing { get; set; }
            public int Size { get; set; }
            public int Smooth { get; set; }
            public string TextureFile { get; set; }
        }

        private class FontJSON
        {
            public string Type { get; set; }
            public ConfigJSON Config { get; set; }
            public IList<Symbol> Symbols { get; set; }
        }


        /// <summary>
        /// loads a font file 
        /// </summary>
        /// <param name="path">path to </param>
        /// <returns>returns user inter font</returns>
        public static async Task<UiFont> Load(string path)
        {
            var textContent = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<FontJSON>(textContent);

            //setup font
            var font = new UiFont();
            font.Base = loaded.Config.Base;
            font.Bold = loaded.Config.Bold;
            font.CharHeight = loaded.Config.CharHeight;
            font.CharSpacing = loaded.Config.CharSpacing;
            font.Name = loaded.Config.Name;
            font.Italic = loaded.Config.Italic;
            font.LineSpacing = loaded.Config.LineSpacing;
            font.Size = loaded.Config.Size;
            font.Smooth = loaded.Config.Smooth;
            font.TextureFile = loaded.Config.TextureFile;
            font.Symbols = loaded.Symbols.ToArray();
            font.Atlas = await Image.Load(loaded.Config.TextureFile);
            return font;
        }

        #endregion
    }
}