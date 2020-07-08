#pragma warning disable 1591
using System;
using Tortuga.Graphics;

namespace Tortuga.UI
{
    /// <summary>
    /// Renderable text ui element
    /// </summary>
    public class UiText : UiRenderable
    {
        public UiFont Font
        {
            get => _font;
            set
            {
                _isTextDirty = true;
                _font = value;
            }
        }
        private UiFont _font;
        public string Text
        {
            get => _text;
            set
            {
                _isTextDirty = true;
                _text = value;
            }
        }
        private string _text;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _isTextDirty = true;
                _fontSize = value;
            }
        }
        private int _fontSize = 10;
        private bool _isTextDirty = false;

        public UiText()
        {
            _font = UiResources.Instance.DefaultFont;
        }

        internal override Graphics.API.BufferTransferObject[] CreateOrUpdateBuffers()
        {
            if (_isTextDirty)
            {
                var textImage = Font.DrawText(
                    _text,
                    0, 0,
                    Convert.ToInt32(Scale.X), Convert.ToInt32(Scale.Y),
                    _fontSize
                );
                SetTexture(textImage.Pixels, textImage.Width, textImage.Height);
                _isTextDirty = false;
            }
            return base.CreateOrUpdateBuffers();
        }

        internal override Graphics.API.CommandPool.Command Draw(Camera camera)
        {
            return base.Draw(camera);
        }
    }
}