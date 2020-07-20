#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Numerics;

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
                SetTexture(value.Atlas.Pixels, value.Atlas.Width, value.Atlas.Height);
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

        public override UiVertex[] BuildVertices
        {
            get
            {
                var vertices = new List<UiVertex>();
                var position = AbsolutePosition;

                var cursor = Vector2.Zero;
                if (_text != null)
                {
                    for (int i = 0; i < _text.Length; i++)
                    {
                        var description = _font.Descriptions.Find((UiFont.FontAtlasDescription d) => d.Character == _text[i]);
                        if (description == null)
                            continue;

                        if (cursor.X + description.SizeX > Scale.X)
                        {
                            cursor.X = 0;
                            cursor.Y += (_font.LineHeight / _font.FontSize) * _fontSize;
                        }

                        var characterRect = new Vector4(
                            description.OffsetX,
                            description.OffsetY,
                            description.SizeX,
                            description.SizeY
                        );
                        var pixelSize = (
                                new Vector2(
                                    characterRect.Z,
                                    characterRect.W
                                ) / _font.FontSize
                            ) * _fontSize;

                        vertices.Add(new UiVertex()
                        {
                            Position = position + cursor,
                            UV = new Vector2(
                                characterRect.X / _font.Atlas.Width,
                                characterRect.W / _font.Atlas.Height
                            ),
                        });
                        vertices.Add(new UiVertex()
                        {
                            Position = position + cursor + new Vector2(pixelSize.X, 0),
                            UV = new Vector2(
                                characterRect.X / _font.Atlas.Width,
                                characterRect.Y / _font.Atlas.Height
                            ),
                        });
                        vertices.Add(new UiVertex()
                        {
                            Position = position + cursor + new Vector2(0, pixelSize.Y),
                            UV = new Vector2(
                                characterRect.Z / _font.Atlas.Width,
                                characterRect.W / _font.Atlas.Height
                            ),
                        });
                        vertices.Add(new UiVertex()
                        {
                            Position = position + cursor + new Vector2(pixelSize.X, pixelSize.Y),
                            UV = new Vector2(
                                characterRect.X / _font.Atlas.Width,
                                characterRect.W / _font.Atlas.Height
                            ),
                        });
                        cursor.X += pixelSize.X;
                    }
                }
                return vertices.ToArray();
            }
        }

        public override ushort[] BuildIndices
        {
            get
            {
                if (_text == null)
                {
                    _indicesLength = 0;
                    return new ushort[] { };
                }

                var indices = new ushort[_text.Length * 6];
                _indicesLength = Convert.ToUInt32(indices.Length);
                for (int i = 0; i < indices.Length; i += 6)
                {
                    var temp = MathF.Round(((float)i / 6.0f) * 4.0f);
                    indices[i + 0] = Convert.ToUInt16(temp + 0);
                    indices[i + 1] = Convert.ToUInt16(temp + 1);
                    indices[i + 2] = Convert.ToUInt16(temp + 2);
                    indices[i + 3] = Convert.ToUInt16(temp + 3);
                    indices[i + 4] = Convert.ToUInt16(temp + 2);
                    indices[i + 5] = Convert.ToUInt16(temp + 1);
                }
                return indices;
            }
        }
    }
}