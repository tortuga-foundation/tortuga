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
            Outset = 0,
            Inset = 1
        }

        public class BoxShadow
        {
            public ShadowType Type;
            public Vector2 Offset;
            public float Blur;
            public float Spread;
            public Color Color;
        }

        public Vector2 PositionPixel;

        public Vector2 ScalePixel;

        public bool IsStatic;
        public int IndexZ = 0;
        public float BorderRadius = 0;
        public Graphics.Image Background;
        public BoxShadow Shadow;

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
                    false,
                    true
                );
                ActiveMaterial.CreateUniformData<ShaderUIStruct>("Data");
                ActiveMaterial.CreateSampledImage("Albedo", new uint[] { 1 });
                await ActiveMaterial.UpdateSampledImage("Albedo", 0, Graphics.Image.SingleColor(Color.White));
            }

            _renderCommandPool = new CommandPool(
                Engine.Instance.MainDevice.GraphicsQueueFamily
            );
            _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            if (Shadow == null)
                Shadow = new BoxShadow();
        }

        public Task UpdateImage(Graphics.Image image)
        {
            return ActiveMaterial.UpdateSampledImage("Albedo", 0, image);
        }

        internal struct ShaderUIStruct
        {
            public Vector2 Position;
            public Vector2 Scale;
            public int IsStatic;
            public int IndexZ;
            public float BorderRadius;
            public int ShadowType;
            public Vector2 ShadowOffset;
            public float ShadowBlur;
            public float ShadowSpread;
            public Vector4 ShadowColor;
        }
        internal ShaderUIStruct BuildShaderStruct
            => new ShaderUIStruct
            {
                Position = PositionPixel,
                Scale = ScalePixel,
                IsStatic = IsStatic ? 1 : 0,
                IndexZ = IndexZ,
                BorderRadius = BorderRadius,
                ShadowType = (int)Shadow.Type,
                ShadowOffset = Shadow.Offset,
                ShadowBlur = Shadow.Blur,
                ShadowSpread = Shadow.Spread,
                ShadowColor = new Vector4(Shadow.Color.R, Shadow.Color.G, Shadow.Color.B, Shadow.Color.A)
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