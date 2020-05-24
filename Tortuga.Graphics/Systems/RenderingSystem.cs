#pragma warning disable 1591
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    public class RenderingSystem : Core.BaseSystem
    {
        API.CommandPool _commandPool;
        API.CommandPool.Command _command;
        API.Buffer _tempBuffer;
        API.Fence _renderFence;

        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            var temp = new ShaderPixel[1920 * 1080];
            for (int i = 0; i < temp.Length; i++)
                temp[i] = new ShaderPixel(255, 0, 0, 255);

            _tempBuffer = API.Buffer.CreateDevice(
                API.Handler.MainDevice,
                Convert.ToUInt32(1920 * 1080 * Unsafe.SizeOf<ShaderPixel>()),
                VkBufferUsageFlags.TransferDst | VkBufferUsageFlags.TransferSrc
            );
            _tempBuffer.SetDataWithStaging(temp).Wait();

            _commandPool = new API.CommandPool(API.Handler.MainDevice, API.Handler.MainDevice.GraphicsQueueFamily);
            _command = _commandPool.AllocateCommands()[0];
            _renderFence = new API.Fence(API.Handler.MainDevice);
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

        public override Task Update()
        {
            return Task.Run(() =>
            {
                _command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
                var cameras = MyScene.GetComponents<Camera>();
                foreach (var camera in cameras)
                {
                    _command.TransferImageLayout(camera.RenderedImage, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
                    _command.BufferToImage(_tempBuffer, camera.RenderedImage);
                    if (camera.RenderToWindow != null)
                    {
                        var swapchian = camera.RenderToWindow.Swapchain;
                        var windowResolution = camera.RenderToWindow.Size;
                        _command.TransferImageLayout(camera.RenderedImage, VkImageLayout.TransferDstOptimal, VkImageLayout.TransferSrcOptimal);
                        _command.TransferImageLayout(camera.RenderToWindow.CurrentImage, swapchian.ImagesFormat, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
                        _command.BlitImage(
                            camera.RenderedImage.ImageHandle,
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
                        _command.TransferImageLayout(camera.RenderToWindow.CurrentImage, swapchian.ImagesFormat, VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR);
                    }
                }
                _command.End();
                _command.Submit(
                    API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                    null,
                    null,
                    _renderFence
                );
                _renderFence.Wait();
            });
        }
    }
}