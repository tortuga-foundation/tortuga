using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Tortuga.Systems
{
    public class RenderingSystem : Core.BaseSystem
    {
#pragma warning disable 0649
        internal struct LightInfo
        {
            public Vector4 Position;
            public Vector4 Forward;
            public Vector4 Color;
            public int Type;
            public float Intensity;
            public int Reserved1;
            public int Reserved2;
        }
        internal struct LightShaderInfo
        {
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
            public int Count;
        }
#pragma warning restore 0649

        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private Fence _renderWaitFence;
        private Semaphore _syncSemaphore;
        private float _cameraResolutionScale => Settings.Graphics.RenderResolutionScale;

        public RenderingSystem()
        {
            _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands()[0];
            _renderWaitFence = new Fence(true);
            _syncSemaphore = new Semaphore();
        }

        public override async Task Update()
        {
            await Task.Run(() =>
            {
                //if previous frame has not finished rendering wait for it to finish before rendering next frame
                _renderWaitFence.Wait();
                _renderWaitFence.Reset();

                var transferCommands = new List<CommandPool.Command>();

                var cameras = MyScene.GetComponents<Components.Camera>();
                var lights = MyScene.GetComponents<Components.Light>();
                var meshes = MyScene.GetComponents<Components.Mesh>();
                var uis = MyScene.GetComponents<Components.UserInterface>();
                foreach (var mesh in meshes)
                {
                    if (mesh.ActiveMaterial.UsingLighting)
                    {
                        var meshLights = GetClosestLights(mesh, lights);
                        var command = mesh.ActiveMaterial.UpdateUniformDataSemaphore("LIGHT", meshLights);
                        transferCommands.Add(command.TransferCommand);
                    }
                    if (mesh.IsStatic == false || mesh.HasRenderedOnce == false)
                    {
                        var command = mesh.ActiveMaterial.UpdateUniformDataSemaphore("MODEL", mesh.ModelMatrix);
                        transferCommands.Add(command.TransferCommand);
                        mesh.HasRenderedOnce = true;
                    }
                }
                foreach (var mesh in uis)
                {
                    if (mesh.IsStatic == false || mesh.HasRenderedOnce == false)
                    {
                        var command = mesh.ActiveMaterial.UpdateUniformDataSemaphore("MODEL", mesh.ModelMatrix);
                        transferCommands.Add(command.TransferCommand);
                        mesh.HasRenderedOnce = true;
                    }
                }

                //begin rendering frame
                _renderCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);

                //prepare swapchain for image copy
                _renderCommand.TransferImageLayout(
                    Engine.Instance.MainWindow.Swapchain.Images[Engine.Instance.MainWindow.SwapchainAcquiredImage],
                    Engine.Instance.MainWindow.Swapchain.ImagesFormat,
                    VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal
                );

                foreach (var camera in cameras)
                {
                    var cameraRes = new IntVector2D
                    {
                        x = System.Convert.ToInt32(System.MathF.Round(Engine.Instance.MainWindow.Width * _cameraResolutionScale)),
                        y = System.Convert.ToInt32(System.MathF.Round(Engine.Instance.MainWindow.Height * _cameraResolutionScale)),
                    };
                    if (camera.Resolution != cameraRes)
                        camera.Resolution = cameraRes;
                    if (camera.IsStatic == false)
                        transferCommands.Add(camera.UpdateCameraBuffers().TransferCommand);

                    //begin render pass for this camera
                    _renderCommand.BeginRenderPass(Engine.Instance.MainRenderPass, camera.Framebuffer);

                    //build render command for each mesh

                    var secondaryCommands = new List<CommandPool.Command>();
                    foreach (var mesh in meshes)
                    {
                        var meshCommand = ProcessMeshCommands(mesh, camera, lights);
                        secondaryCommands.Add(meshCommand);
                    }
                    foreach (var mesh in uis)
                    {
                        var meshCommand = ProcessMeshCommands(mesh, camera, lights);
                        secondaryCommands.Add(meshCommand);
                    }

                    //execute all meshes command buffer
                    if (secondaryCommands.Count > 0)
                        _renderCommand.ExecuteCommands(secondaryCommands.ToArray());
                    _renderCommand.EndRenderPass();

                    //copy rendered image to swapchian for displaying in the window
                    _renderCommand.TransferImageLayout(camera.Framebuffer.ColorImage, VkImageLayout.ColorAttachmentOptimal, VkImageLayout.TransferSrcOptimal);
                    var swapchain = Engine.Instance.MainWindow.Swapchain;
                    _renderCommand.BlitImage(
                        camera.Framebuffer.ColorImage.ImageHandle,
                        System.Convert.ToInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.X)),
                        System.Convert.ToInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.Y)),
                        System.Convert.ToInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.Width)),
                        System.Convert.ToInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.Height)),
                        0,
                        swapchain.Images[Engine.Instance.MainWindow.SwapchainAcquiredImage],
                        System.Convert.ToInt32(System.Math.Round(swapchain.Extent.width * camera.Viewport.X)),
                        System.Convert.ToInt32(System.Math.Round(swapchain.Extent.height * camera.Viewport.Y)),
                        System.Convert.ToInt32(System.Math.Round(swapchain.Extent.width * camera.Viewport.Width)),
                        System.Convert.ToInt32(System.Math.Round(swapchain.Extent.height * camera.Viewport.Height)),
                        0
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
                var syncSemaphores = new Semaphore[0];
                if (transferCommands.Count > 0)
                {
                    CommandPool.Command.Submit(
                        Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                        transferCommands.ToArray(),
                        new Semaphore[] { _syncSemaphore },
                        null
                    );
                    syncSemaphores = new Semaphore[] { _syncSemaphore };
                }
                _renderCommand.Submit(
                    Engine.Instance.MainDevice.GraphicsQueueFamily.Queues[0],
                    null,
                    syncSemaphores,
                    _renderWaitFence
                );
            });
        }

        private CommandPool.Command ProcessMeshCommands(Components.Mesh mesh, Components.Camera camera, Components.Light[] allLights)
        {
            mesh.RenderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer, 0);
            mesh.RenderCommand.SetViewport(
                System.Convert.ToInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.X)),
                System.Convert.ToInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.Y)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.Width)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.Width))
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
            mesh.RenderCommand.BindVertexBuffer(mesh.VertexBuffer);
            mesh.RenderCommand.BindIndexBuffer(mesh.IndexBuffer);
            mesh.RenderCommand.DrawIndexed(mesh.IndicesCount);
            mesh.RenderCommand.End();
            return mesh.RenderCommand;
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
                    Type = (int)l.Type
                });
            for (int i = infoList.Count; i < 10; i++)
                infoList.Add(new LightInfo());
            return new LightShaderInfo
            {
                Count = lights.Length,
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