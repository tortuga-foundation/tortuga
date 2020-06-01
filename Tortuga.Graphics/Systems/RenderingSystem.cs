#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    public class RenderingSystem : Core.BaseSystem
    {
        private API.CommandPool _renderCommandPool;
        private API.CommandPool.Command _renderCommand;
        private API.Semaphore _renderSemaphore;
        private API.Fence _renderFence;
        private GraphicsModule _module;

        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            _module = Engine.Instance.GetModule<GraphicsModule>();
            //render command
            _renderCommandPool = new API.CommandPool(
                API.Handler.MainDevice, 
                API.Handler.MainDevice.GraphicsQueueFamily
            );
            _renderCommand = _renderCommandPool.AllocateCommands()[0];

            //sync
            _renderFence = new API.Fence(API.Handler.MainDevice);
            _renderSemaphore = new API.Semaphore(API.Handler.MainDevice);
        }

        public override Task EarlyUpdate()
        {
            return Task.Run(() =>
            {
                var cameras = MyScene.GetComponents<Camera>();
                foreach (var camera in cameras)
                {
                    if (camera.RenderToWindow != null)
                        camera.RenderToWindow.AcquireSwapchainImage();
                }
            });
        }

        public override Task LateUpdate()
        {
            return Task.Run(() =>
            {
                var cameras = MyScene.GetComponents<Camera>();
                foreach (var camera in cameras)
                {
                    if (camera.RenderToWindow != null)
                        camera.RenderToWindow.Present();
                }
            });
        }

        public override async Task Update()
        {
            if (_module == null)
                return;

            //begin render command
            var transferCommands = new List<API.CommandPool.Command>();
            _renderCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            var cameras = MyScene.GetComponents<Camera>();
            foreach (var camera in cameras)
            {
                //todo: apply frustrum culling

                //begin camera render pass
                _renderCommand.BeginRenderPass(_module.RenderPass, camera.Framebuffer);

                foreach (var transfer in camera.UpdateView())
                    transferCommands.Add(transfer.TransferCommand);

                var renderers = MyScene.GetComponents<Renderer>();

                //build render command for each mesh
                var secondaryTasks = new List<Task<API.CommandPool.Command>>();
                foreach (var renderer in renderers)
                {
                    secondaryTasks.Add(Task.Run(() => renderer.BuildDrawCommand(camera)));
                    //update mdoel matrix (position, rotation, scale)
                    foreach (var transfer in renderer.UpdateModel())
                        transferCommands.Add(transfer.TransferCommand);
                }
                
                //wait until task is completed
                if (secondaryTasks.Count > 0)
                    Task.WaitAll(secondaryTasks.ToArray());
                
                //extract each mesh render command
                var secondaryCommands = new List<API.CommandPool.Command>();
                foreach (var t in secondaryTasks)
                    secondaryCommands.Add(t.Result);

                //execute all render commands
                _renderCommand.ExecuteCommands(secondaryCommands.ToArray());
                _renderCommand.EndRenderPass();

                //make sure window exists before rendering
                if (camera.RenderToWindow != null && camera.RenderToWindow.Exists)
                {
                    var swapchian = camera.RenderToWindow.Swapchain;
                    var windowResolution = camera.RenderToWindow.Size;
                    var imageAttachment = camera.Framebuffer.AttachmentImages[(int)camera.RenderTarget];
                    _renderCommand.TransferImageLayout(
                        imageAttachment, 
                        VkImageLayout.ColorAttachmentOptimal, 
                        VkImageLayout.TransferSrcOptimal
                    );
                    _renderCommand.TransferImageLayout(
                        camera.RenderToWindow.CurrentImage, 
                        swapchian.ImagesFormat, 
                        VkImageLayout.Undefined, 
                        VkImageLayout.TransferDstOptimal
                    );
                    _renderCommand.BlitImage(
                        imageAttachment.ImageHandle,
                        0, 0,
                        Convert.ToInt32(camera.Resolution.X),
                        Convert.ToInt32(camera.Resolution.Y),
                        0,
                        camera.RenderToWindow.CurrentImage,
                        Convert.ToInt32(camera.Viewport.X * windowResolution.X), 
                        Convert.ToInt32(camera.Viewport.Y * windowResolution.Y),
                        Convert.ToInt32(camera.Viewport.Z * windowResolution.X),
                        Convert.ToInt32(camera.Viewport.W * windowResolution.Y),
                        0
                    );
                    _renderCommand.TransferImageLayout(
                        camera.RenderToWindow.CurrentImage, 
                        swapchian.ImagesFormat, 
                        VkImageLayout.TransferDstOptimal, 
                        VkImageLayout.PresentSrcKHR
                    );
                }
            }
            _renderCommand.End();
            var semaphores = new List<API.Semaphore>();
            if (transferCommands.Count > 0)
            {
                semaphores.Add(_renderSemaphore);
                API.CommandPool.Command.Submit(
                    API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                    transferCommands.ToArray(),
                    semaphores.ToArray()
                );
            }
            _renderCommand.Submit(
                API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                null,
                semaphores.ToArray(),
                _renderFence
            );
            // wait for render process to finish
            await _renderFence.WaitAsync();
        }
    }
}