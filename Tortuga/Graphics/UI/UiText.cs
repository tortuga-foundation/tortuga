using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
                _isDirty = true;
            }
        }
        private float _fontSize;

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
            var task = _material.UpdateSampledImage("Font", 0, Font.Atlas);
            task.Wait();
            _text = "Hello World Hello World";
            _fontSize = 24.0f;
            _lineSpacing = 5.0f;
            _wordWrap = true;
            _isDirty = true;
        }

        private Vertex[] BuildVertices(Vector2 offset, UiFont.Symbol symbol, float multiplier, Graphics.Image atlas)
        {
            return new Vertex[]
            {
                new Vertex
                {
                    Position = new Vector2(
                        (offset.X + symbol.OffsetX) * multiplier,
                        (offset.Y + symbol.OffsetY) * multiplier
                    ),
                    TextureCoordinates = new Vector2(
                        (float)symbol.X / (float)atlas.Width,
                        (float)symbol.Y / (float)atlas.Height
                    )
                },
                new Vertex
                {
                    Position = new Vector2(
                        (offset.X + symbol.OffsetX + symbol.Width) * multiplier,
                        (offset.Y + symbol.OffsetY) * multiplier
                    ),
                    TextureCoordinates = new Vector2(
                        (float)(symbol.X + symbol.Width) / (float)atlas.Width,
                        (float)symbol.Y / (float)atlas.Height
                    )
                },
                new Vertex
                {
                    Position = new Vector2(
                        (offset.X + symbol.OffsetX + symbol.Width) * multiplier,
                        (offset.Y + symbol.OffsetY + symbol.Height) * multiplier
                    ),
                    TextureCoordinates = new Vector2(
                        (float)(symbol.X + symbol.Width) / (float)atlas.Width,
                        (float)(symbol.Y + symbol.Height) / (float)atlas.Height
                    )
                },
                new Vertex
                {
                    Position = new Vector2(
                        (offset.X + symbol.OffsetX) * multiplier,
                        (offset.Y + symbol.OffsetY + symbol.Height) * multiplier
                    ),
                    TextureCoordinates = new Vector2(
                        (float)symbol.X / (float)atlas.Width,
                        (float)(symbol.Y + symbol.Height) / (float)atlas.Height
                    )
                },
            };
        }

        internal override API.BufferTransferObject[] UpdateBuffer()
        {
            var baseTransferObject = base.UpdateBuffer();

            if (Text == string.Empty || _isDirty == false)
                return baseTransferObject;

            var vertices = new List<Vertex>();
            var indices = new List<ushort>();
            uint verticesCount = 0;
            Vector2 offset = Vector2.Zero;
            var atlas = _font.Atlas;
            float multiplier = 0.01f * _fontSize;
            var verticesInWord = new List<int>();
            float lastWordXPos = 0;
            float wordAdvance = 0;
            foreach (char c in Text)
            {
                var symbol = Array.Find(_font.Symbols, (UiFont.Symbol s) => s.Identifier == c);
                if (symbol == null)
                    continue;

                if ((offset.X + symbol.OffsetX + symbol.Width) * multiplier > this.Scale.X)
                {
                    offset.Y += _fontSize * _lineSpacing;
                    offset.X = wordAdvance;
                    foreach (var vertexIndex in verticesInWord)
                    {
                        var vertex = vertices[vertexIndex];
                        vertex.Position.X -= lastWordXPos;
                        vertex.Position.Y += offset.Y * multiplier;
                        vertices[vertexIndex] = vertex;
                    }
                }
                if (offset.Y * multiplier > this.Scale.Y)
                    continue;

                foreach (var vertex in BuildVertices(offset, symbol, multiplier, atlas))
                    vertices.Add(vertex);

                if (symbol.Identifier == ' ')
                {
                    verticesInWord.Clear();
                    lastWordXPos = (offset.X + symbol.AdvanceX) * multiplier;
                    wordAdvance = 0;
                }
                else
                {
                    verticesInWord.Add(vertices.Count - 4);
                    verticesInWord.Add(vertices.Count - 3);
                    verticesInWord.Add(vertices.Count - 2);
                    verticesInWord.Add(vertices.Count - 1);
                    wordAdvance += symbol.AdvanceX;
                }

                indices.Add((ushort)(verticesCount + 0));
                indices.Add((ushort)(verticesCount + 2));
                indices.Add((ushort)(verticesCount + 1));

                indices.Add((ushort)(verticesCount + 0));
                indices.Add((ushort)(verticesCount + 3));
                indices.Add((ushort)(verticesCount + 2));

                offset.X += symbol.AdvanceX;
                verticesCount += 4;
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