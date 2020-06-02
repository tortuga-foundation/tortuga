#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    public class RenderingSystem : Core.BaseSystem
    {
        private API.CommandPool _graphicsCommandPool;
        private API.CommandPool.Command _renderCommand;
        private API.CommandPool.Command _lightCommand;
        private API.CommandPool.Command _deferredCommand;
        private API.Semaphore _transferCommandSemaphore;
        private API.Semaphore _lightCommandSemaphore;
        private API.Semaphore _renderCommandSemaphore;
        private API.Fence _waitFence;
        private GraphicsModule _module;

        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            _module = Engine.Instance.GetModule<GraphicsModule>();
            //render command
            _graphicsCommandPool = new API.CommandPool(
                API.Handler.MainDevice,
                API.Handler.MainDevice.GraphicsQueueFamily
            );
            _renderCommand = _graphicsCommandPool.AllocateCommands()[0];
            _lightCommand = _graphicsCommandPool.AllocateCommands()[0];
            _deferredCommand = _graphicsCommandPool.AllocateCommands()[0];

            //sync
            _waitFence = new API.Fence(API.Handler.MainDevice);
            _transferCommandSemaphore = new API.Semaphore(API.Handler.MainDevice);
            _lightCommandSemaphore = new API.Semaphore(API.Handler.MainDevice);
            _renderCommandSemaphore = new API.Semaphore(API.Handler.MainDevice);
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

            var transferCommands = new List<API.CommandPool.Command>();

            #region light commands

            _lightCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            var lights = MyScene.GetComponents<Light>();
            var lightInfos = new List<Light.LightInfo>();
            foreach (var light in lights)
            {
                lightInfos.Add(light.GetShaderInfo);
                _lightCommand.BeginRenderPass(_module.LightRenderPass, light.Framebuffer);
                _lightCommand.EndRenderPass();
            }
            _lightCommand.End();

            #endregion

            #region render command

            _renderCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            var cameras = MyScene.GetComponents<Camera>();
            foreach (var camera in cameras)
            {
                foreach (var t in camera.UpdateLightInfo(lightInfos.ToArray()))
                    transferCommands.Add(t.TransferCommand);

                //todo: apply frustrum culling

                //begin camera render pass
                _renderCommand.BeginRenderPass(_module.MeshRenderPassMRT, camera.Framebuffer);

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
            }
            _renderCommand.End();

            #endregion

            #region deffered command

            _deferredCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            foreach (var camera in cameras)
            {
                _deferredCommand.BeginRenderPass(_module.DefferedRenderPass, camera.DefferedFramebuffer);
                _deferredCommand.BindPipeline(camera.DefferedPipeline);
                _deferredCommand.BindDescriptorSets(
                    camera.DefferedPipeline,
                    new API.DescriptorSetPool.DescriptorSet[]
                    {
                        camera.MrtDescriptorSet,
                        camera.CameraPositionDescriptorSet,
                        camera.LightDescriptorSet
                    }
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
                _deferredCommand.Draw(6);
                _deferredCommand.EndRenderPass();

                //make sure window exists before rendering
                if (camera.RenderToWindow != null && camera.RenderToWindow.Exists)
                {
                    var swapchian = camera.RenderToWindow.Swapchain;
                    var windowResolution = camera.RenderToWindow.Size;
                    API.Image imageAttachment;
                    if (camera.RenderTarget == Camera.TypeOfRenderTarget.DeferredRendering)
                        imageAttachment = camera.DefferedFramebuffer.AttachmentImages[0];
                    else
                        imageAttachment = camera.Framebuffer.AttachmentImages[(int)camera.RenderTarget];

                    _deferredCommand.TransferImageLayout(
                        imageAttachment,
                        VkImageLayout.ColorAttachmentOptimal,
                        VkImageLayout.TransferSrcOptimal
                    );
                    _deferredCommand.TransferImageLayout(
                        camera.RenderToWindow.CurrentImage,
                        swapchian.ImagesFormat,
                        VkImageLayout.Undefined,
                        VkImageLayout.TransferDstOptimal
                    );
                    _deferredCommand.BlitImage(
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
                    _deferredCommand.TransferImageLayout(
                        camera.RenderToWindow.CurrentImage,
                        swapchian.ImagesFormat,
                        VkImageLayout.TransferDstOptimal,
                        VkImageLayout.PresentSrcKHR
                    );
                }
            }
            _deferredCommand.End();

            #endregion

            #region submit commands and sync

            var semaphores = new List<API.Semaphore>();
            if (transferCommands.Count > 0)
            {
                semaphores.Add(_transferCommandSemaphore);
                API.CommandPool.Command.Submit(
                    API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                    transferCommands.ToArray(),
                    semaphores.ToArray()
                );
            }
            _lightCommand.Submit(
                API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                new API.Semaphore[] { _lightCommandSemaphore },
                semaphores.ToArray()
            );
            _renderCommand.Submit(
                API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                new API.Semaphore[] { _renderCommandSemaphore },
                semaphores.ToArray()
            );
            _deferredCommand.Submit(
                API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                null,
                new API.Semaphore[] { _lightCommandSemaphore, _renderCommandSemaphore },
                _waitFence
            );
            // wait for render process to finish
            await _waitFence.WaitAsync();

            #endregion
        }
    }
}