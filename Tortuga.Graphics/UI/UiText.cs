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
        public UiHorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                _isDirty = true;
                _horizontalAlignment = value;
            }
        }
        private UiHorizontalAlignment _horizontalAlignment;
        public UiVerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                _isDirty = true;
                _verticalAlignment = value;
            }
        }
        private UiVerticalAlignment _verticalAlignment;
        public UiFont Font
        {
            get => _font;
            set
            {
                _isDirty = true;
                _font = value;
            }
        }
        private UiFont _font;
        public string Text
        {
            get => _text;
            set
            {
                _isDirty = true;
                _text = value;
            }
        }
        private string _text;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _isDirty = true;
                _fontSize = value;
            }
        }
        private int _fontSize = 10;

        public UiText()
        {
            _font = UiResources.Instance.DefaultFont;
            _horizontalAlignment = UiHorizontalAlignment.Left;
            _verticalAlignment = UiVerticalAlignment.Top;
        }

        internal override Graphics.API.BufferTransferObject[] CreateOrUpdateBuffers()
        {
            if (_isDirty)
            {
                //var textImage = Font.DrawText(
                //    _text,
                //    0, 0,
                //    Convert.ToInt32(Scale.X), Convert.ToInt32(Scale.Y),
                //    _fontSize,
                //    _horizontalAlignment,
                //    _verticalAlignment
                //);
                //SetTexture(textImage.Pixels, textImage.Width, textImage.Height);
            }
            return base.CreateOrUpdateBuffers();
        }
    }
}