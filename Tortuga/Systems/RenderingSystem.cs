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

        public RenderingSystem()
        {
            _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands()[0];
            _renderWaitFence = new Fence(true);
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
            foreach (var camera in cameras)
            {
                await camera.UpdateCameraBuffers();
                //begin render pass for this camera
                _renderCommand.BeginRenderPass(Engine.Instance.MainRenderPass, camera.Framebuffer);

                //build render command for each mesh
                var secondaryCommandsWaiters = new List<Task<CommandPool.Command>>();
                var meshes = MyScene.GetComponents<Components.Mesh>();
                foreach (var mesh in meshes)
                    secondaryCommandsWaiters.Add(ProcessMeshCommands(mesh, camera));

                //wait for all meshes to finish building render command
                Task.WaitAll(secondaryCommandsWaiters.ToArray());
                var secondaryCommands = new List<CommandPool.Command>();
                foreach (var t in secondaryCommandsWaiters)
                    secondaryCommands.Add(t.Result);

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
            _renderCommand.Submit(
                Engine.Instance.MainDevice.GraphicsQueueFamily.Queues[0],
                new Tortuga.Graphics.API.Semaphore[0],
                new Tortuga.Graphics.API.Semaphore[0],
                _renderWaitFence
            );
        }

        private async Task<CommandPool.Command> ProcessMeshCommands(Components.Mesh mesh, Components.Camera camera)
        {
            mesh.RenderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer, 0);
            mesh.RenderCommand.SetViewport(
                System.Convert.ToInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.x)),
                System.Convert.ToInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.y)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.width)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.width)));

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
                await mesh.ActiveMaterial.UpdateModel(mesh.ModelMatrix);
            mesh.RenderCommand.BindVertexBuffer(mesh.VertexBuffer);
            mesh.RenderCommand.BindIndexBuffer(mesh.IndexBuffer);
            mesh.RenderCommand.DrawIndexed(mesh.IndicesCount);
            mesh.RenderCommand.End();
            return await Task.FromResult(mesh.RenderCommand);
        }
    }
}