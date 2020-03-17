using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Tortuga.Utils
{
    public static class FontLoader
    {
        private class ConfigJSON
        {
            public int bold { get; set; }
            public int charHeight { get; set; }
            public int charSpacing { get; set; }
            public string face { get; set; }
            public int italic { get; set; }
            public int lineSpacing { get; set; }
            public int size { get; set; }
            public int smooth { get; set; }
            public string textureFile { get; set; }
            public int textureWidth { get; set; }
            public int textureHeight { get; set; }
        }
        private class SymbolJSON
        {
            public int id { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int xoffset { get; set; }
            public int yoffset { get; set; }
        }
        private class FontJSON
        {
            public ConfigJSON config { get; set; }
            public IList<SymbolJSON> symbols { get; set; }
        }

        public static async Task<Graphics.UI.Font> Load(string path)
        {
            try
            {
                var jsonContent = File.ReadAllText(path);
                var deserializedJSON = JsonSerializer.Deserialize<FontJSON>(jsonContent);

                var font = new Graphics.UI.Font();
                font.Base = 17;
                font.Bold = deserializedJSON.config.bold;
                font.CharHeight = deserializedJSON.config.charHeight;
                font.CharSpacing = deserializedJSON.config.charSpacing;
                font.Face = deserializedJSON.config.face;
                font.Italic = deserializedJSON.config.italic;
                font.LineSpacing = deserializedJSON.config.lineSpacing;
                font.Size = deserializedJSON.config.size;
                font.Smooth = deserializedJSON.config.smooth;
                font.Atlas = await ImageLoader.Load(deserializedJSON.config.textureFile);
                font.Symbols = new Graphics.UI.Symbol[deserializedJSON.symbols.Count];
                for (int i = 0; i < font.Symbols.Length; i++)
                {
                    var symbol = deserializedJSON.symbols[i];
                    font.Symbols[i].Identifier = symbol.x;
                    font.Symbols[i].X = symbol.x;
                    font.Symbols[i].Y = symbol.y;
                    font.Symbols[i].OffsetX = symbol.xoffset;
                    font.Symbols[i].OffsetY = symbol.yoffset;
                    font.Symbols[i].Width = symbol.width;
                    font.Symbols[i].Height = symbol.height;
                }
                return font;
            }
            catch (System.Exception)
            {
                throw new InvalidDataException();
            }
        }
    }
}