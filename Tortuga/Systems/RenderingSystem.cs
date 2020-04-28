using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tortuga.Graphics.UI.Base;

namespace Tortuga.Systems
{
    /// <summary>
    /// This is the rendering system, responsible for rendering every mesh
    /// </summary>
    public class RenderingSystem : Core.BaseSystem
    {
        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private Fence _renderWaitFence;
        private Semaphore _syncSemaphore;
        private float _cameraResolutionScale => Settings.Graphics.RenderResolutionScale;
        private Dictionary<Graphics.Material, Dictionary<Graphics.Mesh, List<Components.RenderMesh>>> _previousMaterialInstance;

        /// <summary>
        /// Constructor to initialize the rendering system
        /// </summary>
        public RenderingSystem()
        {
            _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands()[0];
            _renderWaitFence = new Fence(true);
            _syncSemaphore = new Semaphore();
        }

        /// <summary>
        /// Setup all meshes and static meshes for rendering
        /// </summary>
        public override void OnEnable()
        {
            var fence = new Fence();
            var transferCommands = new List<CommandPool.Command>();

            var tasks = new List<Task>();
            var meshes = MyScene.GetComponents<Components.RenderMesh>();
            foreach (var mesh in meshes)
            {
                tasks.Add(
                    mesh.Material.UpdateUniformData(
                        "MODEL",
                        0,
                        mesh.ModelMatrix
                    )
                );
            }
            var cameras = MyScene.GetComponents<Components.Camera>();
            foreach (var camera in cameras)
                tasks.Add(camera.UpdateCameraBuffers());

            //instancing transfer buffers
            var materialInstancing = new Dictionary<Graphics.Material, Dictionary<Graphics.Mesh, List<Components.RenderMesh>>>();
            foreach (var mesh in meshes)
            {
                if (materialInstancing.ContainsKey(mesh.Material) == false)
                    materialInstancing[mesh.Material] = new Dictionary<Graphics.Mesh, List<Components.RenderMesh>>();
                if (materialInstancing[mesh.Material].ContainsKey(mesh.Mesh) == false)
                    materialInstancing[mesh.Material][mesh.Mesh] = new List<Components.RenderMesh>();

                materialInstancing[mesh.Material][mesh.Mesh].Add(mesh);
            }
            _previousMaterialInstance = materialInstancing;
            foreach (var material in materialInstancing)
            {
                if (material.Key.IsInstanced == false)
                    continue;

                foreach (var mesh in material.Value)
                {
                    var commands = material.Key.BuildInstanceBuffers(mesh.Value.ToArray());
                    foreach (var t in commands)
                        transferCommands.Add(t.TransferCommand);
                }
            }
            if (transferCommands.Count > 0)
            {
                Engine.Instance.MainDevice.WaitForQueue(
                    Engine.Instance.MainDevice.TransferQueueFamily.Queues[0]
                );
                CommandPool.Command.Submit(
                    Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                    transferCommands.ToArray(),
                    null,
                    null,
                    fence
                );
            }

            //wait for all tasks and commands to finish
            Task.WaitAll(tasks.ToArray());
            if (transferCommands.Count > 0)
                fence.Wait();
        }

        /// <summary>
        /// nothing is performed here
        /// </summary>
        public override void OnDisable() { }

        private List<UiRenderable> UserInterfaceDeepSearch(UiElement[] elements)
        {
            var list = new List<UiRenderable>();
            foreach (var element in elements)
            {
                if (element.IsEnabled == false)
                    continue;

                element.UpdatePositionsWithConstraints();
                var renderable = element as UiRenderable;
                if (renderable != null)
                    list.Add(renderable);

                foreach (var child in UserInterfaceDeepSearch(element.Children))
                    list.Add(child);
            }
            return list;
        }


        /// <summary>
        /// wait for previous render to finish and render every mesh
        /// </summary>
        /// <returns>The task should be awaited on every frame as it creates draw commands for every mesh</returns>
        public override async Task Update()
        {
            await Task.Run(() =>
            {
                try
                {
                    var transferCommands = new List<CommandPool.Command>();

                    var cameras = MyScene.GetComponents<Components.Camera>();
                    var lights = MyScene.GetComponents<Components.Light>();
                    var meshes = MyScene.GetComponents<Components.RenderMesh>();
                    var meshBuffers = Task.Run(() =>
                    {
                        foreach (var mesh in meshes)
                        {
                            if (mesh.IsActive == false)
                                continue;
                            try
                            {
                                var meshLights = mesh.RenderingLights(lights);
                                var command = mesh.Material.UpdateUniformDataSemaphore("LIGHT", 0, meshLights);
                                transferCommands.Add(command.TransferCommand);
                            }
                            catch (System.Exception) { }
                            if (mesh.IsStatic == false)
                            {
                                try
                                {
                                    var command = mesh.UpdateUniformBuffer();
                                    if (command.TransferCommand != null)
                                        transferCommands.Add(command.TransferCommand);
                                }
                                catch (System.Exception) { }
                            }
                        }
                    });
                    var userInterfaceBuffers = Task.Run(() =>
                    {
                        var uiElements = UserInterfaceDeepSearch(MyScene.UserInterface);
                        var uiBufferTask = new List<BufferTransferObject[]>();
                        foreach (var ui in uiElements)
                        {
                            try
                            {
                                foreach (var cmd in ui.UpdateBuffer())
                                    if (cmd.TransferCommand != null)
                                        transferCommands.Add(cmd.TransferCommand);
                            }
                            catch (System.Exception) { }
                        }
                        return Task.FromResult(uiElements);
                    });

                    var materialInstancing = new Dictionary<Graphics.Material, Dictionary<Graphics.Mesh, List<Components.RenderMesh>>>();
                    var meshInstancingTask = Task.Run(() =>
                    {
                        foreach (var mesh in meshes)
                        {
                            if (mesh.IsActive == false)
                                continue;

                            if (materialInstancing.ContainsKey(mesh.Material) == false)
                                materialInstancing[mesh.Material] = new Dictionary<Graphics.Mesh, List<Components.RenderMesh>>();
                            if (materialInstancing[mesh.Material].ContainsKey(mesh.Mesh) == false)
                                materialInstancing[mesh.Material][mesh.Mesh] = new List<Components.RenderMesh>();

                            materialInstancing[mesh.Material][mesh.Mesh].Add(mesh);
                        }
                        foreach (var material in materialInstancing)
                        {
                            if (material.Key.IsInstanced == false)
                                continue;

                            foreach (var mesh in material.Value)
                            {
                                if (_previousMaterialInstance[material.Key][mesh.Key].Count == mesh.Value.Count)
                                {
                                    bool isDynamic = false;
                                    foreach (var meshRender in mesh.Value)
                                    {
                                        if (meshRender.IsStatic == false)
                                            isDynamic = true;
                                    }
                                    if (!isDynamic)
                                        continue;
                                }

                                var commands = material.Key.BuildInstanceBuffers(mesh.Value.ToArray());
                                foreach (var t in commands)
                                {
                                    if (t.TransferCommand != null && t.TransferCommand.Handle != null)
                                        transferCommands.Add(t.TransferCommand);
                                }
                            }
                        }
                        _previousMaterialInstance = materialInstancing;
                    });

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

                    foreach (var camera in cameras)
                    {
                        var cameraRes = Engine.Instance.MainWindow.Size * _cameraResolutionScale;
                        if (camera.Resolution != cameraRes)
                            camera.Resolution = cameraRes;
                        if (camera.IsStatic == false)
                        {
                            foreach (var t in camera.UpdateCameraBuffersSemaphore())
                            {
                                if (t.TransferCommand != null && t.TransferCommand.Handle != null)
                                    transferCommands.Add(t.TransferCommand);
                            }
                        }

                        //begin render pass for this camera
                        _renderCommand.BeginRenderPass(Engine.Instance.MainRenderPass, camera.Framebuffer);
                        var secondaryCmds = new List<CommandPool.Command>();

                        //build render command for each mesh
                        var secondaryCommandTask = new List<Task<CommandPool.Command>>();
                        {
                            if (meshInstancingTask.IsCompleted == false)
                                meshInstancingTask.Wait();
                            foreach (var material in materialInstancing)
                            {
                                foreach (var mesh in material.Value)
                                {
                                    if (material.Key.IsInstanced && mesh.Value.Count > 0)
                                    {
                                        secondaryCommandTask.Add(
                                            mesh.Value[0].RecordRenderCommand(
                                                camera,
                                                material.Key.InstanceBuffers,
                                                mesh.Value.Count
                                            )
                                        );
                                    }
                                    else
                                    {
                                        foreach (var meshRender in mesh.Value)
                                        {
                                            secondaryCommandTask.Add(
                                                meshRender.RecordRenderCommand(
                                                    camera
                                                )
                                            );
                                        }
                                    }
                                }
                            }
                        }
                        if (userInterfaceBuffers.IsCompleted == false)
                            userInterfaceBuffers.Wait();
                        foreach (var ui in userInterfaceBuffers.Result)
                        {
                            var command = ui.RecordRenderCommand(camera);
                            secondaryCommandTask.Add(command);
                        }
                        Task.WaitAll(secondaryCommandTask.ToArray());

                        //execute all meshes command buffer
                        if (secondaryCommandTask.Count > 0)
                        {
                            foreach (var task in secondaryCommandTask)
                                secondaryCmds.Add(task.Result);
                        }
                        if (secondaryCmds.Count > 0)
                            _renderCommand.ExecuteCommands(secondaryCmds.ToArray());
                        _renderCommand.EndRenderPass();

                        //copy rendered image to swapchian for displaying in the window
                        _renderCommand.TransferImageLayout(camera.Framebuffer.ColorImage, VkImageLayout.ColorAttachmentOptimal, VkImageLayout.TransferSrcOptimal);
                        var swapchain = Engine.Instance.MainWindow.Swapchain;
                        _renderCommand.BlitImage(
                            camera.Framebuffer.ColorImage.ImageHandle,
                            System.Convert.ToInt32(System.Math.Round(camera.Resolution.X * camera.Viewport.X)),
                            System.Convert.ToInt32(System.Math.Round(camera.Resolution.Y * camera.Viewport.Y)),
                            System.Convert.ToInt32(System.Math.Round(camera.Resolution.X * camera.Viewport.Z)),
                            System.Convert.ToInt32(System.Math.Round(camera.Resolution.Y * camera.Viewport.W)),
                            0,
                            swapchain.Images[Engine.Instance.MainWindow.SwapchainAcquiredImage],
                            System.Convert.ToInt32(System.Math.Round(swapchain.Extent.width * camera.Viewport.X)),
                            System.Convert.ToInt32(System.Math.Round(swapchain.Extent.height * camera.Viewport.Y)),
                            System.Convert.ToInt32(System.Math.Round(swapchain.Extent.width * camera.Viewport.Z)),
                            System.Convert.ToInt32(System.Math.Round(swapchain.Extent.height * camera.Viewport.W)),
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
                    if (meshBuffers.IsCompleted == false)
                        meshBuffers.Wait();
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
                }
                catch(System.Exception ex)
                {
                    System.Console.WriteLine(string.Format("Render Failed: {0}", ex));
                    _renderWaitFence = new Fence(true);
                }
            });
        }
    }
}