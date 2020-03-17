namespace Tortuga.Graphics.UI
{
    public struct Symbol
    {
        public int Identifier;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int OffsetX;
        public int OffsetY;
    }

    public class Font
    {
        public Graphics.Image Atlas;
        public Symbol[] Symbols;
        public int Base;
        public int Bold;
        public int CharHeight;
        public int CharSpacing;
        public string Face;
        public int Italic;
        public int LineSpacing;
        public int Size;
        public int Smooth;
        public int TextureWidth;
        public int TextureHeight;
    }
}