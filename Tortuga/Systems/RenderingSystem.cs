using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Tortuga.Systems
{
    public class RenderingSystem : Core.BaseSystem
    {
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

        public override void OnEnable()
        {
            var tasks = new List<Task>();
            var meshes = MyScene.GetComponents<Components.Mesh>();
            foreach (var mesh in meshes)
            {
                tasks.Add(
                    mesh.ActiveMaterial.UpdateUniformData(
                        "MODEL", 
                        0, 
                        mesh.ModelMatrix
                    )
                );
            }
            var cameras = MyScene.GetComponents<Components.Camera>();
            foreach (var camera in cameras)
                tasks.Add(camera.UpdateCameraBuffers());
            Task.WaitAll(tasks.ToArray());
        }

        public override void OnDisable() { }

        public override async Task Update()
        {
            await Task.Run(() =>
            {
                //if previous frame has not finished rendering wait for it to finish before rendering next frame
                _renderWaitFence.Wait();
                _renderWaitFence.Reset();

                var transferCommands = new List<CommandPool.Command>();

                var uis = MyScene.GetComponents<Components.UserInterface>();
                var cameras = MyScene.GetComponents<Components.Camera>();
                var lights = MyScene.GetComponents<Components.Light>();
                var meshes = MyScene.GetComponents<Components.Mesh>();
                foreach (var mesh in meshes)
                {
                    if (mesh.ActiveMaterial.UsingLighting)
                    {
                        var meshLights = GetClosestLights(mesh, lights);
                        var command = mesh.ActiveMaterial.UpdateUniformDataSemaphore("LIGHT", 0, meshLights);
                        transferCommands.Add(command.TransferCommand);
                    }
                    if (mesh.IsStatic == false)
                    {
                        var command = mesh.ActiveMaterial.UpdateUniformDataSemaphore("MODEL", 0, mesh.ModelMatrix);
                        transferCommands.Add(command.TransferCommand);
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
                        transferCommands.Add(camera.UpdateCameraBuffersSemaphore().TransferCommand);

                    //begin render pass for this camera
                    _renderCommand.BeginRenderPass(Engine.Instance.MainRenderPass, camera.Framebuffer);

                    //build render command for each mesh

                    var secondaryCommands = new List<CommandPool.Command>();
                    foreach (var mesh in meshes)
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

        private Components.Light.FullShaderInfo GetClosestLights(Components.Mesh mesh, Components.Light[] lights)
        {
            System.Array.Sort(lights, (Components.Light left, Components.Light right) =>
            {
                var leftDist = Vector3.Distance(left.Position, mesh.Position);
                var rightDist = Vector3.Distance(right.Position, mesh.Position);
                return System.Convert.ToInt32(System.MathF.Round(leftDist - rightDist));
            });
            if (lights.Length > 10)
                System.Array.Resize(ref lights, 10);
            var infoList = new List<Components.Light.LightShaderInfo>();
            foreach (var l in lights)
                infoList.Add(l.BuildShaderInfo);
            for (int i = infoList.Count; i < 10; i++)
                infoList.Add(new Components.Light.LightShaderInfo());
            return new Components.Light.FullShaderInfo
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