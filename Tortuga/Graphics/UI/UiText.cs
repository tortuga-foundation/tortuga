using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Drawing;
using Vulkan;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// text user interface element
    /// </summary>
    public class UiText : UiRenderable
    {
        private struct Vertex
        {
            public Vector2 Position;
            public Vector2 TextureCoordinates;
        }

        private class LineStructure
        {
            public int PixelSize = 0;
            public List<WordStructure> Words = new List<WordStructure>();
        }
        private class WordStructure
        {
            public int PixelSize = 0;
            public List<UiFont.Symbol> Symbols = new List<UiFont.Symbol>();
        }

        /// <summary>
        /// The text to display
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _isDirty = true;
            }
        }
        private string _text;

        /// <summary>
        /// Determains the size of the text
        /// </summary>
        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                _fontSizeMultipler = 0.02f * value;
                _isDirty = true;
            }
        }
        private float _fontSize;
        private float _fontSizeMultipler;

        /// <summary>
        /// The space between each line
        /// </summary>
        public float LineSpacing
        {
            get => _lineSpacing;
            set
            {
                _lineSpacing = value;
                _isDirty = true;
            }
        }
        private float _lineSpacing;

        /// <summary>
        /// If turned on then words won't be split into multiple lines
        /// </summary>
        public bool WordWrap
        {
            get => _wordWrap;
            set
            {
                _wordWrap = value;
                _isDirty = true;
            }
        }
        private bool _wordWrap;

        /// <summary>
        /// font used for rendering text
        /// </summary>
        public UiFont Font
        {
            get => _font;
            set
            {
                _font = value;
                var task = _material.UpdateSampledImage("Font", 0, Font.Atlas);
                task.Wait();
            }
        }
        private UiFont _font;

        /// <summary>
        /// The font color
        /// </summary>
        public Color TextColor
        {
            get => Background;
            set => Background = value;
        }

        /// <summary>
        /// The text horizontal alignment
        /// </summary>
        public UiHorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                _horizontalAlignment = value;
                _isDirty = true;
            }
        }
        private UiHorizontalAlignment _horizontalAlignment;

        /// <summary>
        /// The text vertical alignment
        /// </summary>
        public UiVerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                _verticalAlignment = value;
                _isDirty = true;
            }
        }
        private UiVerticalAlignment _verticalAlignment;

        private API.Buffer _vertexBuffer;
        private API.Buffer _indexBuffer;
        private uint _indexCount;
        private bool _isDirty;

        /// <summary>
        /// Constructor for Ui Text
        /// </summary>
        public UiText()
        {
            _font = UiResources.Font.Roboto;
            _material = UiResources.Materials.Text;
            _material.CreateSampledImage("Font", new uint[] { 1 });
            var task1 = _material.UpdateSampledImage("Font", 0, Font.Atlas);
            task1.Wait();
            Text = "Hello World";
            TextColor = Color.Black;
            FontSize = 24.0f;
            LineSpacing = 100.0f;
            WordWrap = true;
            _isDirty = true;
        }

        private Vertex[] BuildVertices(Vector2 cursor, UiFont.Symbol symbol, float fontSize, Graphics.Image atlas)
        {
            return new Vertex[]
            {
                new Vertex
                {
                    Position = new Vector2(
                        (cursor.X + symbol.OffsetX) * fontSize,
                        (cursor.Y + symbol.OffsetY) * fontSize
                    ),
                    TextureCoordinates = new Vector2(
                        (float)symbol.X / (float)atlas.Width,
                        (float)symbol.Y / (float)atlas.Height
                    )
                },
                new Vertex
                {
                    Position = new Vector2(
                        (cursor.X + symbol.OffsetX + symbol.Width) * fontSize,
                        (cursor.Y + symbol.OffsetY) * fontSize
                    ),
                    TextureCoordinates = new Vector2(
                        (float)(symbol.X + symbol.Width) / (float)atlas.Width,
                        (float)symbol.Y / (float)atlas.Height
                    )
                },
                new Vertex
                {
                    Position = new Vector2(
                        (cursor.X + symbol.OffsetX + symbol.Width) * fontSize,
                        (cursor.Y + symbol.OffsetY + symbol.Height) * fontSize
                    ),
                    TextureCoordinates = new Vector2(
                        (float)(symbol.X + symbol.Width) / (float)atlas.Width,
                        (float)(symbol.Y + symbol.Height) / (float)atlas.Height
                    )
                },
                new Vertex
                {
                    Position = new Vector2(
                        (cursor.X + symbol.OffsetX) * fontSize,
                        (cursor.Y + symbol.OffsetY + symbol.Height) * fontSize
                    ),
                    TextureCoordinates = new Vector2(
                        (float)symbol.X / (float)atlas.Width,
                        (float)(symbol.Y + symbol.Height) / (float)atlas.Height
                    )
                },
            };
        }

        private LineStructure[] BuildLines(string text)
        {
            var lines = new List<LineStructure>();
            var currentLine = new LineStructure();
            var currentWord = new WordStructure();
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    lines.Add(currentLine);
                    currentLine = new LineStructure();
                    continue;
                }

                var symbol = Array.Find(_font.Symbols, (UiFont.Symbol s) => s.Identifier == c);
                if (symbol == null)
                    continue;

                if ((currentLine.PixelSize + currentWord.PixelSize + symbol.AdvanceX) * _fontSizeMultipler > Scale.X)
                {
                    if (this.WordWrap)
                    {
                        lines.Add(currentLine);
                        currentLine = new LineStructure();
                    }
                    else
                    {
                        currentLine.Words.Add(currentWord);
                        currentLine.PixelSize += currentWord.PixelSize;
                        lines.Add(currentLine);
                        currentLine = new LineStructure();
                        currentWord = new WordStructure();
                    }
                }
                currentWord.PixelSize += symbol.AdvanceX;
                currentWord.Symbols.Add(symbol);

                if (c == ' ')
                {
                    currentLine.Words.Add(currentWord);
                    currentLine.PixelSize += currentWord.PixelSize;
                    currentWord = new WordStructure();
                }
            }
            if (currentWord.PixelSize > 0)
            {
                currentLine.Words.Add(currentWord);
                currentLine.PixelSize += currentWord.PixelSize;
            }
            if (currentLine.PixelSize > 0)
                lines.Add(currentLine);

            return lines.ToArray();
        }

        internal override API.BufferTransferObject[] UpdateBuffer()
        {
            var baseTransferObject = base.UpdateBuffer();

            if (Text == string.Empty || _isDirty == false)
                return baseTransferObject;

            var vertices = new List<Vertex>();
            var indices = new List<ushort>();
            uint verticesCount = 0;

            var lines = BuildLines(_text);
            var cursor = Vector2.Zero;
            if (VerticalAlignment == UiVerticalAlignment.Top)
                cursor.Y = 0;
            else if (VerticalAlignment == UiVerticalAlignment.Center)
                cursor.Y = ((Scale.Y / _fontSizeMultipler) - (lines.Length * LineSpacing) - FontSize - 20) / 2.0f;
            else if (VerticalAlignment == UiVerticalAlignment.Bottom)
                cursor.Y = (Scale.Y / _fontSizeMultipler) - (lines.Length * LineSpacing) - FontSize - 20;
            foreach (var line in lines)
            {
                if (HorizontalAlignment == UiHorizontalAlignment.Left)
                    cursor.X = 0;
                else if (HorizontalAlignment == UiHorizontalAlignment.Center)
                    cursor.X = ((Scale.X / _fontSizeMultipler) - line.PixelSize) / 2.0f;
                else if (HorizontalAlignment == UiHorizontalAlignment.Right)
                    cursor.X = (Scale.X / _fontSizeMultipler) - line.PixelSize;

                foreach (var word in line.Words)
                {
                    foreach (var symbol in word.Symbols)
                    {
                        foreach (var vertex in BuildVertices(cursor, symbol, _fontSizeMultipler, _font.Atlas))
                            vertices.Add(vertex);

                        indices.Add((ushort)(verticesCount + 0));
                        indices.Add((ushort)(verticesCount + 2));
                        indices.Add((ushort)(verticesCount + 1));

                        indices.Add((ushort)(verticesCount + 0));
                        indices.Add((ushort)(verticesCount + 3));
                        indices.Add((ushort)(verticesCount + 2));
                        cursor.X += symbol.AdvanceX;
                        verticesCount += 4;
                    }
                }
                cursor.Y += LineSpacing;
            }

            if (vertices.Count == 0 || indices.Count == 0)
                return baseTransferObject;
            _vertexBuffer = API.Buffer.CreateDevice(
                Convert.ToUInt32(Unsafe.SizeOf<Vertex>() * vertices.Count),
                VkBufferUsageFlags.VertexBuffer
            );
            _indexBuffer = API.Buffer.CreateDevice(
                Convert.ToUInt32(sizeof(ushort) * indices.Count),
                VkBufferUsageFlags.IndexBuffer
            );
            _indexCount = Convert.ToUInt32(indices.Count);
            var vertexT = _vertexBuffer.SetDataGetTransferObject(vertices.ToArray());
            var indexT = _indexBuffer.SetDataGetTransferObject(indices.ToArray());

            Array.Resize(ref baseTransferObject, baseTransferObject.Length + 2);
            baseTransferObject[baseTransferObject.Length - 2] = vertexT;
            baseTransferObject[baseTransferObject.Length - 1] = indexT;
            _isDirty = false;
            return baseTransferObject;
        }
        internal override Task<API.CommandPool.Command> RecordRenderCommand(Components.Camera camera)
        {
            var descriptorSets = new List<API.DescriptorSetPool.DescriptorSet>();
            descriptorSets.Add(camera.UiDescriptorSet);
            descriptorSets.Add(this.DescriptorSet);
            foreach (var set in _material.DescriptorSets)
                descriptorSets.Add(set);

            _material.ReCompilePipeline();

            this.RenderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer);
            if (Text != string.Empty && _vertexBuffer != null && _indexBuffer != null)
            {
                this.RenderCommand.BindPipeline(_material.Pipeline);
                this.RenderCommand.BindDescriptorSets(_material.Pipeline, descriptorSets.ToArray());
                this.RenderCommand.SetViewport(
                    System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.X)),
                    System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Height * camera.Viewport.Y)),
                    System.Convert.ToUInt32(System.Math.Round(camera.Resolution.X * camera.Viewport.Z)),
                    System.Convert.ToUInt32(System.Math.Round(camera.Resolution.Y * camera.Viewport.W))
                );
                this.RenderCommand.BindVertexBuffer(_vertexBuffer);
                this.RenderCommand.BindIndexBuffer(_indexBuffer);
                this.RenderCommand.DrawIndexed(_indexCount);
            }
            this.RenderCommand.End();
            return Task.FromResult(this.RenderCommand);
        }
    }
}