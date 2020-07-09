#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using Vulkan;

namespace Tortuga.Graphics
{
    public class RenderingSystem : Core.BaseSystem
    {
        private const string UI_PROJECTION_KEY = "UI_PROJECTION";

        private API.CommandPool _graphicsCommandPool;
        private API.CommandPool.Command _renderCommand;
        private API.CommandPool.Command _lightCommand;
        private API.CommandPool.Command _deferredCommand;
        private API.CommandPool.Command _presentCommand;
        private API.Semaphore _transferCommandSemaphore;
        private API.Semaphore _lightCommandSemaphore;
        private API.Semaphore _renderCommandSemaphore;
        private API.Semaphore _presentSemaphore;
        private API.Fence _waitFence;
        private GraphicsModule _module;
        private API.Framebuffer _uiFramebuffer;
        private DescriptorSetHelper _descriptorHelper;

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
            _presentCommand = _graphicsCommandPool.AllocateCommands()[0];

            //sync
            _waitFence = new API.Fence(API.Handler.MainDevice);
            _transferCommandSemaphore = new API.Semaphore(API.Handler.MainDevice);
            _lightCommandSemaphore = new API.Semaphore(API.Handler.MainDevice);
            _renderCommandSemaphore = new API.Semaphore(API.Handler.MainDevice);
            _presentSemaphore = new API.Semaphore(API.Handler.MainDevice);

            //setup a framebuffer for user interface
            var windowSize = Window.Instance.Size;
            _uiFramebuffer = new API.Framebuffer(
                UI.UiResources.Instance.RenderPass,
                Convert.ToUInt32(windowSize.X),
                Convert.ToUInt32(windowSize.Y)
            );
            _descriptorHelper = new DescriptorSetHelper();
            _descriptorHelper.InsertKey(UI_PROJECTION_KEY, UI.UiResources.Instance.DescriptorSetLayouts[0]);
            _descriptorHelper.BindBuffer(UI_PROJECTION_KEY, 0, new Matrix4x4[]
            {
                Matrix4x4.CreateOrthographicOffCenter(
                    0, Window.Instance.Size.X,
                    0, Window.Instance.Size.Y,
                    0, 1
                )
            }).Wait();
        }

        public override Task EarlyUpdate()
        {
            return Task.Run(() =>
            {
                Window.Instance.AcquireSwapchainImage();
            });
        }

        public override async Task LateUpdate()
        {
            // wait for render process to finish
            await _waitFence.WaitAsync();

            //present
            Window.Instance.Present();
        }

        private UI.UiRenderable[] FetchRenderableUi(UI.UiElement[] elements)
        {
            var renderable = new List<UI.UiRenderable>();
            foreach (var e in elements)
            {
                var renderableElement = e as UI.UiRenderable;
                if (renderableElement != null)
                    renderable.Add(renderableElement);
                var children = FetchRenderableUi(e.Children);
                foreach (var child in children)
                {
                    var renderableChild = child as UI.UiRenderable;
                    if (renderableChild != null)
                        renderable.Add(renderableChild);
                }
            }
            return renderable.ToArray();
        }

        public override Task Update()
        {
            #region pre render task

            var uiTasks = new List<Task>();
            if (_uiFramebuffer.Width != Window.Instance.Size.X || _uiFramebuffer.Height != Window.Instance.Size.Y)
            {
                var windowSize = Window.Instance.Size;
                _uiFramebuffer = new API.Framebuffer(
                    UI.UiResources.Instance.RenderPass,
                    Convert.ToUInt32(windowSize.X),
                    Convert.ToUInt32(windowSize.Y)
                );
                _descriptorHelper = new DescriptorSetHelper();
                _descriptorHelper.InsertKey(UI_PROJECTION_KEY, UI.UiResources.Instance.DescriptorSetLayouts[0]);
                uiTasks.Add(
                    _descriptorHelper.BindBuffer(UI_PROJECTION_KEY, 0, new Matrix4x4[]
                    {
                        Matrix4x4.CreateOrthographicOffCenter(
                            0, Window.Instance.Size.X,
                            0, Window.Instance.Size.Y,
                            0, 1
                        )
                    })
                );
            }
            //fetch all renderable ui in the background
            var renderableUis = Task.Run(() => FetchRenderableUi(MyScene.UserInterface));

            #endregion

            return Task.Run(() =>
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
                    //render
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
                }
                _deferredCommand.End();

                #endregion

                #region Draw UI & Update Window Swapchain Image

                //wait for ui tasks 
                Task.WaitAll(uiTasks.ToArray());

                _presentCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
                _presentCommand.BeginRenderPass(
                    UI.UiResources.Instance.RenderPass,
                    _uiFramebuffer
                );
                var uiRenderCommand = new List<API.CommandPool.Command>();
                foreach (var ui in renderableUis.Result)
                {
                    foreach (var t in ui.CreateOrUpdateBuffers())
                        transferCommands.Add(t.TransferCommand);

                    uiRenderCommand.Add(
                        ui.Draw(
                            _uiFramebuffer,
                            _descriptorHelper.DescriptorObjectMapper[UI_PROJECTION_KEY].Set
                        )
                    );
                }
                _presentCommand.ExecuteCommands(uiRenderCommand.ToArray());
                _presentCommand.EndRenderPass();
                var windowSize = Window.Instance.Size;
                var windowWidth = Convert.ToInt32(windowSize.X);
                var windowHeight = Convert.ToInt32(windowSize.Y);
                foreach (var camera in cameras)
                {
                    _presentCommand.BlitImage(
                        _uiFramebuffer.AttachmentImages[0].ImageHandle,
                        0,
                        0,
                        Convert.ToInt32(camera.DefferedFramebuffer.Width),
                        Convert.ToInt32(camera.DefferedFramebuffer.Height),
                        0,
                        Window.Instance.CurrentImage.Handle,
                        Convert.ToInt32(camera.Viewport.X * windowWidth),
                        Convert.ToInt32(camera.Viewport.Y * windowHeight),
                        Convert.ToInt32(camera.Viewport.Z * windowWidth),
                        Convert.ToInt32(camera.Viewport.W * windowHeight),
                        0
                    );
                }
                _presentCommand.End();

                #endregion

                #region submit commands and sync

                var semaphores = new List<API.Semaphore>();
                //process transfer command
                if (transferCommands.Count > 0)
                {
                    semaphores.Add(_transferCommandSemaphore);
                    API.CommandPool.Command.Submit(
                        API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                        transferCommands.ToArray(),
                        semaphores.ToArray()
                    );
                }
                //process light commands (Shadow Mapping)
                _lightCommand.Submit(
                    API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                    new API.Semaphore[] { _lightCommandSemaphore },
                    semaphores.ToArray()
                );
                //process render commands (MRT)
                _renderCommand.Submit(
                    API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                    new API.Semaphore[] { _renderCommandSemaphore },
                    semaphores.ToArray()
                );
                //process deffered commands
                _deferredCommand.Submit(
                    API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                    new API.Semaphore[] { _presentSemaphore },
                    new API.Semaphore[] { _lightCommandSemaphore, _renderCommandSemaphore }
                );
                //copy rendered image to window swapchain
                _presentCommand.Submit(
                    API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                    null,
                    new API.Semaphore[] { _presentSemaphore },
                    _waitFence
                );

                #endregion
            });
        }
    }
}