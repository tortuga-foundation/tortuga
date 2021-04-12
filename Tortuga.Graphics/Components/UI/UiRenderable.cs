
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// a renderable user interface element
    /// </summary>
    public class UiRenderable : UiElement
    {
        private const string DATA_KEY = "_UI";
        private const string TEXTURE_KEY = "_UI_TEXTURE";

        /// <summary>
        /// Used for rendering UiRenderable, this data is sent to the GPU
        /// </summary>
        public struct UiVertex
        {
            /// <summary>
            /// position of the vertex
            /// </summary>
            public Vector2 Position;
            /// <summary>
            /// Vertex UV coordinates
            /// </summary>
            public Vector2 UV;
        }

        /// <summary>
        /// Render data sent to the gpu when rendering UiRenderable
        /// </summary>
        public struct RenderData
        {
            /// <summary>
            /// position of the element
            /// </summary>
            public Vector2 Position;
            /// <summary>
            /// scale of the element
            /// </summary>
            public Vector2 Scale;
            /// <summary>
            /// border radius, used for rounding corners
            /// </summary>
            public Vector4 BorderRadius;
            /// <summary>
            /// color of the element
            /// </summary>
            public Vector4 Color;
        }


        /// <summary>
        /// Uses render target as the image for the ui element
        /// </summary>
        public RenderTarget RenderFrom
        {
            get => _renderFrom;
            set
            {
                SetTexture(value.RenderedImage, value.RenderedImageView);
                _renderFrom = value;
            }
        }
        private RenderTarget _renderFrom;

        private GraphicsModule _module;
        private DescriptorService _descriptor;
        private Graphics.API.CommandBuffer _command;

        private Graphics.API.CommandBuffer _transferCommand;
        private Graphics.API.Buffer _vertexBuffer;
        private Graphics.API.Buffer _vertexBufferStaging;
        private Graphics.API.Buffer _indexBuffer;
        private Graphics.API.Buffer _indexBufferStaging;

        /// <summary>
        /// Checks if the mouse is inside the ui element
        /// </summary>
        /// <value>Returns true if mouse is inside the element</value>
        public bool IsMouseInside
        {
            get
            {
                var pos = AbsolutePosition;
                var scale = Scale;
                var mousePosition = Input.InputModule.MousePosition;
                return (
                    mousePosition.X >= pos.X &&
                    mousePosition.Y >= pos.Y &&
                    mousePosition.X <= scale.X + pos.X &&
                    mousePosition.Y <= scale.Y + pos.Y
                );
            }
        }

        /// <summary>
        /// verticies for rendering this user interface element
        /// </summary>
        protected virtual UiVertex[] Vertices
        {
            get
            {
                var position = AbsolutePosition;
                var scale = Scale + position;

                return new UiVertex[]
                {
                    new UiVertex
                    {
                        Position = position,
                        UV = new Vector2(1, 0)
                    },
                    new UiVertex
                    {
                        Position = new Vector2(scale.X, position.Y),
                        UV = new Vector2(0, 0)
                    },
                    new UiVertex
                    {
                        Position = new Vector2(position.X, scale.Y),
                        UV = new Vector2(1, 1)
                    },
                    new UiVertex
                    {
                        Position = scale,
                        UV = new Vector2(0, 1)
                    }
                };
            }
        }

        /// <summary>
        /// indices for rendering this user interface element
        /// </summary>
        protected virtual ushort[] Indices
        {
            get
            {
                return new ushort[]
                {
                    0, 1, 2,
                    3, 2, 1
                };
            }
        }

        /// <summary>
        /// constructor for ui renderable
        /// </summary>
        public UiRenderable()
        {
            _module = Engine.Instance.GetModule<GraphicsModule>();

            // setup descriptor sets
            _descriptor = new DescriptorService();
            _descriptor.InsertKey(DATA_KEY, _module.DescriptorLayouts[DATA_KEY]);
            _descriptor.InsertKey(TEXTURE_KEY, _module.DescriptorLayouts[TEXTURE_KEY]);
            _descriptor.BindBuffer(DATA_KEY, 0, new RenderData[]
            {
                new RenderData
                {
                    Position = AbsolutePosition,
                    Scale = this.Scale,
                    BorderRadius = Vector4.Zero,
                    Color = new Vector4(255, 255, 255, 255)
                }
            });
            _descriptor.BindImage(TEXTURE_KEY, 0, new Texture(System.Drawing.Color.White));

            // setup commands
            _command = _module.CommandBufferService.GetNewCommand(
                Graphics.API.QueueFamilyType.Graphics,
                CommandType.Secondary
            );
            _transferCommand = _module.CommandBufferService.GetNewCommand(
                Graphics.API.QueueFamilyType.Transfer,
                CommandType.Primary
            );

            // setup vertex and index buffers
            var vertices = this.Vertices;
            var indices = this.Indices;
            _vertexBufferStaging = new Graphics.API.Buffer(
                _module.GraphicsService.PrimaryDevice,
                Convert.ToUInt32(Unsafe.SizeOf<UiVertex>() * vertices.Length),
                Vulkan.VkBufferUsageFlags.VertexBuffer,
                Vulkan.VkMemoryPropertyFlags.HostVisible |
                Vulkan.VkMemoryPropertyFlags.HostCoherent
            );
            _indexBufferStaging = new Graphics.API.Buffer(
                _module.GraphicsService.PrimaryDevice,
                Convert.ToUInt32(sizeof(short) * indices.Length),
                Vulkan.VkBufferUsageFlags.IndexBuffer,
                Vulkan.VkMemoryPropertyFlags.HostVisible |
                Vulkan.VkMemoryPropertyFlags.HostCoherent
            );
            _vertexBuffer = new Graphics.API.Buffer(
                _module.GraphicsService.PrimaryDevice,
                _vertexBufferStaging.Size,
                Vulkan.VkBufferUsageFlags.VertexBuffer,
                Vulkan.VkMemoryPropertyFlags.DeviceLocal
            );
            _indexBuffer = new Graphics.API.Buffer(
                _module.GraphicsService.PrimaryDevice,
                _indexBufferStaging.Size,
                Vulkan.VkBufferUsageFlags.IndexBuffer,
                Vulkan.VkMemoryPropertyFlags.DeviceLocal
            );
            _transferCommand.Begin(Vulkan.VkCommandBufferUsageFlags.SimultaneousUse);
            _transferCommand.CopyBuffer(_vertexBufferStaging, _vertexBuffer);
            _transferCommand.CopyBuffer(_indexBufferStaging, _indexBuffer);
            _transferCommand.End();
            _module.CommandBufferService.Submit(_transferCommand);
        }

        /// <summary>
        /// set texture to use for this render ui element
        /// </summary>
        /// <param name="texture">texture</param>
        public void SetTexture(Texture texture)
        => _descriptor.BindImage(TEXTURE_KEY, 0, texture);

        /// <summary>
        /// set a vulkan image as the image to render the ui element
        /// </summary>
        /// <param name="image">vulkan image</param>
        /// <param name="view">vulkan image view</param>
        public void SetTexture(Graphics.API.Image image, Graphics.API.ImageView view)
        => _descriptor.BindImage(TEXTURE_KEY, 0, image, view);

        /// <summary>
        /// Updates 'RenderData' Buffer once the element is marked as dirty
        /// </summary>
        public virtual void CreateOrUpdateBuffers()
        {
            if (_isDirty == false) return;

            _descriptor.BindBuffer(DATA_KEY, 0, new RenderData[]
            {
                new RenderData
                {
                    Position = AbsolutePosition,
                    Scale = this.Scale,
                    BorderRadius = new Vector4(
                        BorderRadiusTopLeft,
                        BorderRadiusTopRight,
                        BorderRadiusBottomLeft,
                        BorderRadiusBottomRight
                    ),
                    Color = new Vector4(
                        Background.R / 255.0f,
                        Background.G / 255.0f,
                        Background.B / 255.0f,
                        Background.A / 255.0f
                    )
                }
            });
            _isDirty = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="framebuffer"></param>
        /// <param name="ProjectionDescriptorSet"></param>
        /// <returns></returns>
        public virtual Graphics.API.CommandBuffer Draw(
            Graphics.API.Framebuffer framebuffer,
            Graphics.API.DescriptorSet ProjectionDescriptorSet
        )
        {
            var scissorPosition = AbsolutePosition;
            var scissorScale = Scale;
            if (Mask == null)
            {
                var maskPosition = Mask.AbsolutePosition;
                var maskScale = Mask.Scale;

                // update scissor position using mask
                scissorPosition = new Vector2(
                    MathF.Min(scissorPosition.X, maskPosition.X),
                    MathF.Min(scissorPosition.Y, maskPosition.Y)
                );

                // update scissor scale using mask
                scissorScale = new Vector2(
                    MathF.Max(scissorScale.X, maskScale.X),
                    MathF.Max(scissorScale.Y, maskScale.Y)
                );
            }

            _command.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);

            if (scissorScale.X >= 1 && scissorScale.Y >= 1)
            {
                // _command.BindPipeline(
                //     pipeline
                // );
                // _command.BindDescriptorSets(
                //     pipeline,
                //     new List<Graphics.API.DescriptorSet>
                //     {
                //         ProjectionDescriptorSet,
                //         _descriptor.Handle[DATA_KEY].Set,
                //         _descriptor.Handle[TEXTURE_KEY].Set
                //     }
                // );
                _command.BindVertexBuffers(new List<Graphics.API.Buffer> { _vertexBuffer });
                _command.BindIndexBuffer(_indexBuffer);
                _command.SetScissor(
                    Convert.ToInt32(scissorPosition.X),
                    Convert.ToInt32(scissorPosition.Y),
                    Convert.ToUInt32(scissorScale.X),
                    Convert.ToUInt32(scissorScale.Y)
                );
                _command.SetViewport(
                    0, 0,
                    framebuffer.Width,
                    framebuffer.Height
                );
                _command.DrawIndexed(Convert.ToUInt32(Indices.Length), 1);
            }

            _command.End();
            return _command;
        }
    }
}