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
        private Semaphore _transferCommandSemaphore;
        private Semaphore _renderCommandSemaphore;
        private Semaphore _deferredCommandSemaphore;
        private Semaphore _presentCommandSemaphore;
        private Dictionary<Window, Task<uint>> _swapchainIndexes;

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

            //sync helpers
            _transferCommandSemaphore = new Semaphore(_device);
            _renderCommandSemaphore = new Semaphore(_device);
            _deferredCommandSemaphore = new Semaphore(_device);
            _presentCommandSemaphore = new Semaphore(_device);
        }

        /// <summary>
        /// 
        /// </summary>
        public override Task EarlyUpdate()
        => Task.Run(() =>
        {
            _swapchainIndexes = new Dictionary<Window, Task<uint>>();
            var cameras = MyScene.GetComponents<Camera>();
            foreach (var camera in cameras)
            {
                var window = camera.RenderTarget as Window;
                if (window == null) continue;
                _swapchainIndexes.Add(
                    window,
                    window.AcquireSwapchainImage()
                );
            }
        });

        /// <summary>
        /// 
        /// </summary>
        public override Task Update()
        => Task.Run(() =>
        {
            var cameras = MyScene.GetComponents<Camera>();
            var lights = MyScene.GetComponents<Light>();
            var meshRenderers = MyScene.GetComponents<MeshRenderer>();
            var transferCommands = new List<CommandBuffer>();

            #region record command buffers

            _renderCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);
            _deferredCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);
            _presentCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);
            foreach (var camera in cameras)
            {
                // render command
                {
                    // update camera descriptor sets
                    camera.UpdateDescriptorSets();
                    // update lighting information
                    camera.UpdateLights(lights);

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
                    _renderCommand.BeginRenderPass(
                        camera.MrtFramebuffer,
                        Vulkan.VkSubpassContents.SecondaryCommandBuffers
                    );


                    var secondaryDrawCommands = new List<API.CommandBuffer>();
                    foreach (var meshRenderer in meshRenderers)
                    {
                        // transfer material textures to the correct image layout
                        transferCommands.Add(meshRenderer.Material.TransferImages());
                        // update mesh render descriptor sets
                        meshRenderer.UpdateDescriptorSet();

                        // setup mesh draw command
                        secondaryDrawCommands.Add(meshRenderer.DrawCommand(
                            camera.MrtFramebuffer,
                            0,
                            camera.ProjectionDescriptorSet,
                            camera.ViewDescriptorSet,
                            camera.Viewport,
                            camera.Resolution
                        ));
                    }
                    // execute all draw comands for meshes
                    if (secondaryDrawCommands.Count > 0)
                        _renderCommand.ExecuteCommands(secondaryDrawCommands);

                    _renderCommand.EndRenderPass();
                }

                // deffered command
                {
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
                }

                // present command
                {
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
                            camera.DefferedFramebuffer.Images[0],
                            0, 0,
                            (int)camera.DefferedFramebuffer.Width,
                            (int)camera.DefferedFramebuffer.Height,
                            0,
                            camera.RenderTarget.RenderedImage,
                            0, 0,
                            (int)camera.RenderTarget.RenderedImage.Width,
                            (int)camera.RenderTarget.RenderedImage.Height,
                            0
                        );
                    }
                }
            }
            _renderCommand.End();
            _deferredCommand.End();

            #endregion

            #region submit commands

            var transformCommandSemaphores = new List<Semaphore>();
            transferCommands = transferCommands.Where(t => t != null).ToList();
            if (transferCommands.Count > 0)
            {
                _module.CommandBufferService.Submit(
                    transferCommands,
                    new List<Semaphore> { _transferCommandSemaphore }
                );
                transformCommandSemaphores.Add(_transferCommandSemaphore);
            }
            _module.CommandBufferService.Submit(
                _renderCommand,
                new List<Semaphore> { _renderCommandSemaphore },
                transformCommandSemaphores
            );
            _module.CommandBufferService.Submit(
                _deferredCommand,
                new List<Semaphore> { _deferredCommandSemaphore },
                new List<Semaphore> { _renderCommandSemaphore }
            );

            #endregion

            #region update render targets

            foreach (var swapchain in _swapchainIndexes)
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
                    window.RenderedImage,
                    0, 0,
                    (int)window.RenderedImage.Width,
                    (int)window.RenderedImage.Height,
                    0,
                    window.Swapchain.Images[swapchainIndex],
                    0, 0,
                    (int)window.Swapchain.SurfaceExtent.width,
                    (int)window.Swapchain.SurfaceExtent.height,
                    0
                );
            }

            _presentCommand.End();

            #endregion

            #region present

            _module.CommandBufferService.Submit(
                _presentCommand,
                new List<Semaphore> { _presentCommandSemaphore },
                new List<Semaphore> { _deferredCommandSemaphore }
            );
            foreach (var swapchain in _swapchainIndexes)
            {
                var window = swapchain.Key;
                _module.CommandBufferService.Present(
                    window.Swapchain,
                    swapchain.Value.Result,
                    new List<Semaphore> { _presentCommandSemaphore }
                );
            }

            #endregion
        });
    }
}