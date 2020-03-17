using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
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
        private Dictionary<Graphics.Material, Dictionary<Graphics.Mesh, List<Components.RenderMesh>>> _previousMaterialInstance;

        public RenderingSystem()
        {
            _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands()[0];
            _renderWaitFence = new Fence(true);
            _syncSemaphore = new Semaphore();
        }

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

        public override void OnDisable() { }

        public override async Task Update()
        {
            await Task.Run(() =>
            {
                var transferCommands = new List<CommandPool.Command>();

                var cameras = MyScene.GetComponents<Components.Camera>();
                var lights = MyScene.GetComponents<Components.Light>();
                var meshes = MyScene.GetComponents<Components.RenderMesh>();
                var uis = Graphics.UI.UiRender.UiRenderers;
                foreach (var mesh in meshes)
                {
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
                            transferCommands.Add(command.TransferCommand);
                        }
                        catch (System.Exception) { }
                    }
                }
                foreach (var ui in uis)
                {
                    foreach (var t in ui.UpdateBuffers())
                        transferCommands.Add(t.TransferCommand);
                }

                var materialInstancing = new Dictionary<Graphics.Material, Dictionary<Graphics.Mesh, List<Components.RenderMesh>>>();
                foreach (var mesh in meshes)
                {
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
                            transferCommands.Add(t.TransferCommand);
                    }
                }
                _previousMaterialInstance = materialInstancing;

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
                    var secondaryCmds = new List<CommandPool.Command>();

                    //build render command for each ui
                    foreach (var ui in uis)
                        secondaryCmds.Add(ui.BuildRenderCommand(camera));

                    //build render command for each mesh
                    var secondaryCommandTask = new List<Task<CommandPool.Command>>();
                    {
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
    }
}