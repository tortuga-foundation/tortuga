using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.Graphics;
using System.Runtime.CompilerServices;
using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;

namespace Tortuga.Components
{
    public class UserInterface : Core.BaseComponent
    {
        public Graphics.Material ActiveMaterial;

        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;

        public enum ShadowType
        {
            None = 0,
            Outset = 1,
            Inset = 2
        }

        public class BoxShadow
        {
            public ShadowType Type;
            public Vector2 Offset;
            public float Blur;
            public float Spread;
            public Color Color;

            public BoxShadow()
            {
                Type = ShadowType.None;
                Offset = new Vector2(0, 10);
                Blur = 10;
                Spread = 5;
                Color = Color.FromArgb(255, 255, 255, 255);
            }
        }

        public Vector2 Position;

        public Vector2 Scale;

        public bool IsStatic;
        public int IndexZ = 0;
        public float BorderRadiusTopLeft = 0;
        public float BorderRadiusTopRight = 0;
        public float BorderRadiusBottomLeft = 0;
        public float BorderRadiusBottomRight = 0;
        public float BorderRadius
        {
            set
            {
                BorderRadiusTopLeft = value;
                BorderRadiusTopRight = value;
                BorderRadiusBottomLeft = value;
                BorderRadiusBottomRight = value;
            }
        }
        public float Rotation;
        public Graphics.Image Background;
        public BoxShadow Shadow = new BoxShadow();

        public override async Task OnEnable()
        {
            if (this.ActiveMaterial == null)
            {
                ActiveMaterial = new Material(
                    new Graphics.Shader(
                        "Assets/Shaders/UserInterface/UserInterface.vert",
                        "Assets/Shaders/UserInterface/UserInterface.frag"
                    ),
                    false,
                    false
                );
                ActiveMaterial.CreateUniformData<ShaderUIStruct>("Data");
                ActiveMaterial.CreateSampledImage("Albedo", new uint[] { 1 });
                await ActiveMaterial.UpdateSampledImage("Albedo", 0, await Utils.ImageLoader.Load("Assets/Images/Sample.jpg"));
            }

            _renderCommandPool = new CommandPool(
                Engine.Instance.MainDevice.GraphicsQueueFamily
            );
            _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
        }

        public Task UpdateImage(Graphics.Image image)
        {
            return ActiveMaterial.UpdateSampledImage("Albedo", 0, image);
        }

        internal struct ShaderUIStruct
        {
            public Vector4 ShadowColor;
            public Vector2 Position;
            public Vector2 Scale;
            public Vector2 ShadowOffset;
            public float BorderRadiusTopLeft;
            public float BorderRadiusTopRight;
            public float BorderRadiusBottomLeft;
            public float BorderRadiusBottomRight;
            public int IndexZ;
            public int ShadowType;
            public float ShadowBlur;
            public float ShadowSpread;
            public float Rotation;
        }
        internal ShaderUIStruct BuildShaderStruct
            => new ShaderUIStruct
            {

                ShadowColor = new Vector4(
                    Shadow.Color.R / 255, 
                    Shadow.Color.G / 255, 
                    Shadow.Color.B / 255, 
                    Shadow.Color.A / 255
                ),
                Position = Position,
                Scale = Scale,
                ShadowOffset = Shadow.Offset,
                BorderRadiusTopLeft = BorderRadiusTopLeft,
                BorderRadiusTopRight = BorderRadiusTopRight,
                BorderRadiusBottomLeft = BorderRadiusBottomLeft,
                BorderRadiusBottomRight = BorderRadiusBottomRight,
                IndexZ = IndexZ,
                ShadowType = (int)Shadow.Type,
                ShadowBlur = Shadow.Blur,
                ShadowSpread = Shadow.Spread,
                Rotation = Rotation
            };

        internal CommandPool.Command BuildDrawCommand(Components.Camera camera)
        {
            _renderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer, 0);
            _renderCommand.SetViewport(
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.X)),
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Height * camera.Viewport.Y)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.Width)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.Width))
            );

            var descriptorSets = new List<DescriptorSetPool.DescriptorSet>();
            descriptorSets.Add(camera.CameraDescriptorSet);
            foreach (var d in ActiveMaterial.DescriptorSets)
                descriptorSets.Add(d);
            ActiveMaterial.ReCompilePipeline();
            _renderCommand.BindPipeline(
                ActiveMaterial.ActivePipeline,
                VkPipelineBindPoint.Graphics,
                descriptorSets.ToArray()
            );
            _renderCommand.Draw(6);
            _renderCommand.End();
            return _renderCommand;
        }
    }
}