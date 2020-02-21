using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Tortuga.Systems
{
    public class RenderingSystem : Core.BaseSystem
    {
        internal struct LightInfo
        {
            public Vector4 Position;
            public Vector4 Forward;
            public Vector4 Color;
            public int Type;
            public float Intensity;
            public float Range;
        }
        internal struct LightShaderInfo
        {
            public int Count;
            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public LightInfo Light0;
            public LightInfo Light1;
            public LightInfo Light2;
            public LightInfo Light3;
            public LightInfo Light4;
            public LightInfo Light5;
            public LightInfo Light6;
            public LightInfo Light7;
            public LightInfo Light8;
            public LightInfo Light9;
        }

        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private Fence _renderWaitFence;
        private Semaphore _syncSemaphore;

        public RenderingSystem()
        {
            _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands()[0];
            _renderWaitFence = new Fence(true);
            _syncSemaphore = new Semaphore();
        }

        public override async Task Update()
        {
            //if previous frame has not finished rendering wait for it to finish before rendering next frame
            _renderWaitFence.Wait();
            _renderWaitFence.Reset();

            //begin rendering frame
            _renderCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);

            //prepare swapchain for image copy
            _renderCommand.TransferImageLayout(
                Engine.Instance.MainWindow.Swapchain.Images[Engine.Instance.MainWindow.SwapchainAcquiredImage],
                Engine.Instance.MainWindow.Swapchain.ImagesFormat,
                VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal
            );

            var cameras = MyScene.GetComponents<Components.Camera>();
            var lights = MyScene.GetComponents<Components.Light>();
            var transferCommands = new List<CommandPool.Command>();
            foreach (var camera in cameras)
            {
                await camera.UpdateCameraBuffers();
                //begin render pass for this camera
                _renderCommand.BeginRenderPass(Engine.Instance.MainRenderPass, camera.Framebuffer);

                //build render command for each mesh
                var meshes = MyScene.GetComponents<Components.Mesh>();
                var secondaryCommands = new List<CommandPool.Command>();
                foreach (var mesh in meshes)
                {
                    var meshCommand = ProcessMeshCommands(mesh, camera, lights);
                    secondaryCommands.Add(meshCommand.RenderCommand);
                    foreach (var command in meshCommand.TransferCommands)
                        transferCommands.Add(command.TransferCommand);
                }

                //execute all meshes command buffer
                _renderCommand.ExecuteCommands(secondaryCommands.ToArray());
                _renderCommand.EndRenderPass();

                //copy rendered image to swapchian for displaying in the window
                _renderCommand.TransferImageLayout(camera.Framebuffer.ColorImage, VkImageLayout.ColorAttachmentOptimal, VkImageLayout.TransferSrcOptimal);
                _renderCommand.BlitImage(
                    camera.Framebuffer.ColorImage.ImageHandle,
                    System.Convert.ToInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.x)),
                    System.Convert.ToInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.y)),
                    System.Convert.ToInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.width)),
                    System.Convert.ToInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.height)),
                    Engine.Instance.MainWindow.Swapchain.Images[Engine.Instance.MainWindow.SwapchainAcquiredImage],
                    System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.x)),
                    System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.height * camera.Viewport.y)),
                    System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.width)),
                    System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.height * camera.Viewport.height))
                );
                _renderCommand.TransferImageLayout(camera.Framebuffer.ColorImage, VkImageLayout.TransferSrcOptimal, VkImageLayout.ColorAttachmentOptimal);
            }
            //prepare swapchain for presentation
            _renderCommand.TransferImageLayout(
                Engine.Instance.MainWindow.Swapchain.Images[Engine.Instance.MainWindow.SwapchainAcquiredImage],
                Engine.Instance.MainWindow.Swapchain.ImagesFormat,
                VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR
            );
            _renderCommand.End();
            CommandPool.Command.Submit(
                Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                transferCommands.ToArray(),
                new Semaphore[] { _syncSemaphore },
                null
            );
            _renderCommand.Submit(
                Engine.Instance.MainDevice.GraphicsQueueFamily.Queues[0],
                null,
                new Semaphore[] { _syncSemaphore },
                _renderWaitFence
            );
        }

        private struct MeshCommands
        {
            public CommandPool.Command RenderCommand;
            public BufferTransferObject[] TransferCommands;
        }

        private MeshCommands ProcessMeshCommands(Components.Mesh mesh, Components.Camera camera, Components.Light[] allLights)
        {
            var transferCommands = new List<BufferTransferObject>();
            if (mesh.ActiveMaterial.UsingLighting)
            {
                var lights = GetClosestLights(mesh, allLights);
                transferCommands.Add(mesh.ActiveMaterial.LightingTransferObject(lights));
            }
            mesh.RenderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer, 0);
            mesh.RenderCommand.SetViewport(
                System.Convert.ToInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.x)),
                System.Convert.ToInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.y)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.width)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.width))
            );

            var descriptorSets = new List<DescriptorSetPool.DescriptorSet>();
            descriptorSets.Add(camera.CameraDescriptorSet);
            foreach (var d in mesh.ActiveMaterial.DescriptorSets)
                descriptorSets.Add(d);
            mesh.ActiveMaterial.ReCompilePipeline();
            mesh.RenderCommand.BindPipeline(
                mesh.ActiveMaterial.ActivePipeline,
                VkPipelineBindPoint.Graphics,
                descriptorSets.ToArray()
            );
            if (mesh.IsStatic == false)
                transferCommands.Add(mesh.ActiveMaterial.ModelTransferObject(mesh.ModelMatrix));
            mesh.RenderCommand.BindVertexBuffer(mesh.VertexBuffer);
            mesh.RenderCommand.BindIndexBuffer(mesh.IndexBuffer);
            mesh.RenderCommand.DrawIndexed(mesh.IndicesCount);
            mesh.RenderCommand.End();
            return new MeshCommands
            {
                RenderCommand = mesh.RenderCommand,
                TransferCommands = transferCommands.ToArray()
            };
        }

        private LightShaderInfo GetClosestLights(Components.Mesh mesh, Components.Light[] lights)
        {
            System.Array.Sort(lights, (Components.Light left, Components.Light right) =>
            {
                var leftDist = Vector3.Distance(left.Position, mesh.Position);
                var rightDist = Vector3.Distance(right.Position, mesh.Position);
                return System.Convert.ToInt32(System.MathF.Round(leftDist - rightDist));
            });
            if (lights.Length > 10)
                System.Array.Resize(ref lights, 10);
            var infoList = new List<LightInfo>();
            foreach (var l in lights)
                infoList.Add(new LightInfo
                {
                    Color = new Vector4(l.Color.R, l.Color.G, l.Color.B, l.Color.A),
                    Forward = new Vector4(l.Forward, 1),
                    Position = new Vector4(l.Position, 1),
                    Intensity = l.Intensity,
                    Range = l.Range,
                    Type = (int)l.Type
                });
            for (int i = infoList.Count; i < 10; i++)
                infoList.Add(new LightInfo());
            return new LightShaderInfo
            {
                Count = lights.Length,
                Reserved1 = 0,
                Reserved2 = 0,
                Reserved3 = 0,
                Light0 = infoList[0],
                Light1 = infoList[1],
                Light2 = infoList[2],
                Light3 = infoList[3],
                Light4 = infoList[4],
                Light5 = infoList[5],
                Light6 = infoList[6],
                Light7 = infoList[7],
                Light8 = infoList[8],
                Light9 = infoList[9]
            };
        }
    }
}