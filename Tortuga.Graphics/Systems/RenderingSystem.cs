using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tortuga.Graphics.API;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Responsible for rendering objects into the scene
    /// </summary>
    public class RenderingSystem : Core.BaseSystem
    {
        private GraphicsModule _module;
        private API.Device _device;
        private CommandBuffer _renderCommand;
        private CommandBuffer _deferredCommand;
        private CommandBuffer _presentCommand;
        private Task<Dictionary<KeyValuePair<Material, Mesh>, List<MeshRenderer>>> _instancedMeshes;

        /// <summary>
        /// 
        /// </summary>
        public override void OnEnable()
        {
            _module = Engine.Instance.GetModule<GraphicsModule>();
            _device = _module.GraphicsService.PrimaryDevice;

            _renderCommand = _module.CommandBufferService.GetNewCommand(
                QueueFamilyType.Graphics,
                CommandType.Primary
            );
            _deferredCommand = _module.CommandBufferService.GetNewCommand(
                QueueFamilyType.Graphics,
                CommandType.Primary
            );
            _presentCommand = _module.CommandBufferService.GetNewCommand(
                QueueFamilyType.Graphics,
                CommandType.Primary
            );
        }

        /// <summary>
        /// 
        /// </summary>
        public override Task EarlyUpdate()
        => Task.Run(() =>
        {
            // get all instaned meshes
            _instancedMeshes = Task.Run(() =>
            {
                var instanced = new Dictionary<KeyValuePair<Material, Mesh>, List<MeshRenderer>>();
                foreach (var meshRenderer in MyScene.GetComponents<MeshRenderer>())
                {
                    if (meshRenderer.Material.Instanced == false)
                        continue;

                    var key = new KeyValuePair<Material, Mesh>(
                        meshRenderer.Material,
                        meshRenderer.Mesh
                    );
                    if (instanced.ContainsKey(key) == false)
                        instanced[key] = new List<MeshRenderer>();

                    instanced[key].Add(meshRenderer);
                }
                return instanced;
            });
        });

        /// <summary>
        /// 
        /// </summary>
        public override Task Update()
        => Task.Run(() =>
        {
            var swapchainIndexes = new Dictionary<Window, Task<uint>>();
            var cameras = MyScene.GetComponents<Camera>();
            var lights = MyScene.GetComponents<Light>();
            var transferCommands = new List<CommandBuffer>();

            // fetch instanced and non-instanced meshes
            var meshRenderers = (
                MyScene.GetComponents<MeshRenderer>()
                .Where(m => m.Material.Instanced == false)
                .ToArray()
            );
            _instancedMeshes.Wait();

            #region record command buffers

            _renderCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);
            _deferredCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);
            _presentCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);
            var cameraTasks = new List<Task>();
            foreach (var camera in cameras)
            {

                var window = camera.RenderTarget as Window;
                if (window != null)
                {
                    swapchainIndexes.Add(
                        window,
                        window.AcquireSwapchainImage()
                    );
                }

                // update camera descriptor sets
                camera.UpdateDescriptorSets();
                // update lighting information
                camera.UpdateLights(lights);

                #region transfer images layout

                int attachmentCount = 0;
                foreach (var attachment in camera.MrtFramebuffer.RenderPass.Attachments)
                {
                    if (attachment.Format != API.RenderPassAttachment.Default.Format) continue;

                    // transfer image layout to color attachment optimial
                    // used in render command
                    _renderCommand.TransferImageLayout(
                        camera.MrtFramebuffer.Images[attachmentCount],
                        Vulkan.VkImageLayout.ColorAttachmentOptimal
                    );
                    // transfer image layout to shader read only optimal
                    // used in deffered command
                    _deferredCommand.TransferImageLayout(
                        camera.MrtFramebuffer.Images[attachmentCount],
                        Vulkan.VkImageLayout.ShaderReadOnlyOptimal
                    );
                    attachmentCount++;
                }

                #endregion

                #region mesh draw commands

                _renderCommand.BeginRenderPass(
                    camera.MrtFramebuffer,
                    Vulkan.VkSubpassContents.SecondaryCommandBuffers
                );

                var secondaryDrawCommands = new List<Task<API.CommandBuffer>>();
                // non-instanced
                foreach (var meshRenderer in meshRenderers)
                {
                    // setup mesh draw command
                    secondaryDrawCommands.Add(Task.Run(() =>
                    {
                        // transfer material textures to the correct image layout
                        transferCommands.Add(meshRenderer.Material.TransferImages());
                        // update mesh render descriptor sets
                        meshRenderer.UpdateDescriptorSet();

                        return meshRenderer.DrawCommand(
                            camera.MrtFramebuffer,
                            0,
                            camera.ProjectionDescriptorSet,
                            camera.ViewDescriptorSet,
                            camera.Viewport,
                            camera.Resolution
                        );
                    }));
                }
                foreach (var mesh in _instancedMeshes.Result)
                {
                    secondaryDrawCommands.Add(Task.Run(() =>
                    {
                        // transfer material textures to the correct image layout
                        transferCommands.Add(mesh.Key.Key.TransferImages());

                        // update instanced buffers
                        transferCommands.Add(mesh.Key.Key.UpdateInstanceBuffers(
                            mesh.Key.Value,
                            mesh.Value
                        ));

                        return mesh.Key.Key.DrawInstanced(
                            mesh.Key.Value,
                            mesh.Value,
                            camera.MrtFramebuffer,
                            0,
                            camera.ProjectionDescriptorSet,
                            camera.ViewDescriptorSet,
                            camera.Viewport,
                            camera.Resolution
                        );
                    }));
                }
                Task.WaitAll(secondaryDrawCommands.ToArray());
                // execute all draw comands for meshes
                if (secondaryDrawCommands.Count > 0)
                    _renderCommand.ExecuteCommands(secondaryDrawCommands.Select(s => s.Result).ToList());

                _renderCommand.EndRenderPass();

                #endregion

                #region deffered commands

                _deferredCommand.BeginRenderPass(
                    camera.DefferedFramebuffer,
                    Vulkan.VkSubpassContents.SecondaryCommandBuffers
                );
                _deferredCommand.BindPipeline(Camera.DefferedPipeline);
                _deferredCommand.BindDescriptorSets(
                    Camera.DefferedPipeline,
                    camera.MrtDescriptorSets
                );
                _deferredCommand.SetScissor(
                    0, 0,
                    camera.DefferedFramebuffer.Width,
                    camera.DefferedFramebuffer.Height
                );
                _deferredCommand.SetViewport(
                    0, 0,
                    camera.DefferedFramebuffer.Width,
                    camera.DefferedFramebuffer.Height
                );
                _deferredCommand.Draw(6, 1);
                _deferredCommand.EndRenderPass();

                #endregion

                #region update render targets

                if (camera.RenderTarget != null)
                {
                    _presentCommand.TransferImageLayout(
                        camera.DefferedFramebuffer.Images[0],
                        Vulkan.VkImageLayout.TransferSrcOptimal
                    );
                    _presentCommand.TransferImageLayout(
                        camera.RenderTarget.RenderedImage,
                        Vulkan.VkImageLayout.TransferDstOptimal
                    );
                    // copy image from camera framebuffer to render target
                    _presentCommand.BlitImage(
                        camera.DefferedFramebuffer.Images[0].Handle,
                        camera.DefferedFramebuffer.Images[0].Format,
                        camera.DefferedFramebuffer.Images[0].Layout.First(),
                        0, 0,
                        (int)camera.DefferedFramebuffer.Width,
                        (int)camera.DefferedFramebuffer.Height,
                        0,
                        camera.RenderTarget.RenderedImage.Handle,
                        camera.RenderTarget.RenderedImage.Format,
                        camera.RenderTarget.RenderedImage.Layout.First(),
                        0, 0,
                        (int)camera.RenderTarget.RenderedImage.Width,
                        (int)camera.RenderTarget.RenderedImage.Height,
                        0
                    );
                }

                #endregion
            }
            _renderCommand.End();
            _deferredCommand.End();

            #endregion

            #region submit commands

            var transformCommandSemaphores = new List<Semaphore>();
            transferCommands = transferCommands.Where(t => t != null).ToList();
            if (transferCommands.Count > 0)
            {
                _module.CommandBufferService.Submit(transferCommands);
                transformCommandSemaphores.Add(transferCommands[0].SignalSemaphore);
            }
            _module.CommandBufferService.Submit(_renderCommand, transformCommandSemaphores);
            _module.CommandBufferService.Submit(
                _deferredCommand,
                new List<Semaphore> { _renderCommand.SignalSemaphore }
            );

            #endregion

            #region update render targets

            foreach (var swapchain in swapchainIndexes)
            {
                swapchain.Value.Wait();
                var swapchainIndex = (int)swapchain.Value.Result;
                var window = swapchain.Key;
                _presentCommand.TransferImageLayout(
                    window.RenderedImage,
                    Vulkan.VkImageLayout.TransferSrcOptimal
                );
                _presentCommand.TransferImageLayout(
                    window.Swapchain.Images[swapchainIndex],
                    Vulkan.VkImageLayout.TransferDstOptimal
                );
                _presentCommand.BlitImage(
                    window.RenderedImage.Handle,
                    window.RenderedImage.Format,
                    window.RenderedImage.Layout.First(),
                    0, 0,
                    (int)window.RenderedImage.Width,
                    (int)window.RenderedImage.Height,
                    0,
                    window.Swapchain.Images[swapchainIndex].Handle,
                    window.Swapchain.Images[swapchainIndex].Format,
                    window.Swapchain.Images[swapchainIndex].Layout.First(),
                    0, 0,
                    (int)window.Swapchain.SurfaceExtent.width,
                    (int)window.Swapchain.SurfaceExtent.height,
                    0
                );
            }

            _presentCommand.End();

            _module.CommandBufferService.Submit(
                _presentCommand,
                new List<Semaphore> { _deferredCommand.SignalSemaphore }
            );

            #endregion

            #region update swapchain

            foreach (var swapchain in swapchainIndexes)
            {
                var window = swapchain.Key;
                _module.CommandBufferService.Present(
                    window.Swapchain,
                    swapchain.Value.Result,
                    new List<Semaphore> { _presentCommand.SignalSemaphore }
                );
            }

            #endregion
        });

        /// <summary>
        /// 
        /// </summary>
        public override async Task LateUpdate()
        {
            await _presentCommand.Fence.WaitAsync();
        }
    }
}