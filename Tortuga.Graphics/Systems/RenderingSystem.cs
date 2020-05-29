#pragma warning disable 1591
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    public class RenderingSystem : Core.BaseSystem
    {
        private API.CommandPool _renderCommandPool;
        private API.CommandPool.Command _renderCommand;
        private API.Fence _renderFence;
        private API.Shader _shader;
        private API.Pipeline _pipeline;
        private DescriptorSetHelper _descriptorHelper;
        private const string MESH_DATA_KEY = "MESH";

        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            _renderCommandPool = new API.CommandPool(API.Handler.MainDevice, API.Handler.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands()[0];
            _renderFence = new API.Fence(API.Handler.MainDevice);

            _shader = new API.Shader(API.Handler.MainDevice, "Assets/Shaders/ray.comp");
            _pipeline = new API.Pipeline(
                _shader,
                Engine.Instance.GetModule<GraphicsModule>().RenderDescriptorLayouts
            );
            _descriptorHelper = new DescriptorSetHelper();
            _descriptorHelper.InsertKey(MESH_DATA_KEY, Engine.Instance.GetModule<GraphicsModule>().RenderDescriptorLayouts[1]);
            _descriptorHelper.BindBuffer(MESH_DATA_KEY, 0, null, 1).Wait();
            _descriptorHelper.BindBuffer(MESH_DATA_KEY, 0, null, 1).Wait();
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
            //get renderers list
            var renderers = MyScene.GetComponents<Renderer>();
            if (renderers.Length > 0)
            {
                uint fullVerticesSize = 0;
                uint fullIndicesSize = 0;
                foreach (var r in renderers)
                {
                    fullVerticesSize += r.MeshData.VertexBuffer.Size;
                    fullIndicesSize += r.MeshData.IndexBuffer.Size;
                }
                //make sure the full mesh data is correct size
                await _descriptorHelper.BindBuffer(MESH_DATA_KEY, 0, null, Convert.ToInt32(fullVerticesSize));
                await _descriptorHelper.BindBuffer(MESH_DATA_KEY, 1, null, Convert.ToInt32(fullIndicesSize));
            }
            //begin render command
            _renderCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            //copy mesh data for the pipeline to be processed
            ulong verticesOffset = 0;
            ulong indicesOffset = 0;
            foreach (var render in renderers)
            {
                _renderCommand.CopyBuffer(
                    render.MeshData.VertexBuffer, 
                    _descriptorHelper.DescriptorObjectMapper[MESH_DATA_KEY].Buffers[0]
                );
                _renderCommand.CopyBuffer(
                    render.MeshData.IndexBuffer, 
                    _descriptorHelper.DescriptorObjectMapper[MESH_DATA_KEY].Buffers[1]
                );
                verticesOffset += render.MeshData.VertexBuffer.Size;
                indicesOffset += render.MeshData.IndexBuffer.Size;
            }
            //bind ray tracing pipeline
            _renderCommand.BindPipeline(_pipeline, VkPipelineBindPoint.Compute);
            var cameras = MyScene.GetComponents<Camera>();
            foreach (var camera in cameras)
            {
                _renderCommand.BindDescriptorSets(
                    _pipeline,
                    new API.DescriptorSetPool.DescriptorSet[]
                    { 
                        camera.RenderImageDescriptorMap.Set, 
                        _descriptorHelper.DescriptorObjectMapper[MESH_DATA_KEY].Set
                    },
                    VkPipelineBindPoint.Compute
                );
                _renderCommand.Dispatch(
                    Convert.ToUInt32(camera.RenderImageDescriptorMap.Images[0].Width),
                    Convert.ToUInt32(camera.RenderImageDescriptorMap.Images[0].Height),
                    1
                );

                //make sure window exists before rendering
                if (camera.RenderToWindow != null && camera.RenderToWindow.Exists )
                {
                    var swapchian = camera.RenderToWindow.Swapchain;
                    var windowResolution = camera.RenderToWindow.Size;
                    _renderCommand.TransferImageLayout(camera.RenderImageDescriptorMap.Images[0], VkImageLayout.TransferDstOptimal, VkImageLayout.TransferSrcOptimal);
                    _renderCommand.TransferImageLayout(camera.RenderToWindow.CurrentImage, swapchian.ImagesFormat, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
                    _renderCommand.BlitImage(
                        camera.RenderImageDescriptorMap.Images[0].ImageHandle,
                        0, 0,
                        Convert.ToInt32(camera.Resolution.X),
                        Convert.ToInt32(camera.Resolution.Y),
                        0,
                        camera.RenderToWindow.CurrentImage,
                        0, 0,
                        Convert.ToInt32(windowResolution.X),
                        Convert.ToInt32(windowResolution.Y),
                        0
                    );
                    _renderCommand.TransferImageLayout(camera.RenderToWindow.CurrentImage, swapchian.ImagesFormat, VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR);
                }
            }
            _renderCommand.End();
            _renderCommand.Submit(
                API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                null,
                null,
                _renderFence
            );
            // wait for render process to finish
            _renderFence.Wait();
        }
    }
}