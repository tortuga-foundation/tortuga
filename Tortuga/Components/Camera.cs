using Vulkan;
using Tortuga.Graphics;
using Tortuga.Graphics.API;

namespace Tortuga.Components
{
    public class Camera : Core.BaseComponent
    {
        private Framebuffer _framebuffer;
        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private CommandPool _transferCommandPool;
        private CommandPool.Command _transferCommand;
        private Semaphore _syncSemaphore;
        private Fence _renderWaiter;

        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            _framebuffer = new Framebuffer(
                Engine.Instance.MainRenderPass,
                1920, 1080
            );
            _renderCommandPool = new CommandPool(
                Engine.Instance.MainDevice.GraphicsQueueFamily
            );
            _renderCommand = _renderCommandPool.AllocateCommands()[0];

            //setup transfer image to swapchain command
            _transferCommandPool = new CommandPool(
                Engine.Instance.MainDevice.TransferQueueFamily
            );
            _transferCommand = _transferCommandPool.AllocateCommands()[0];
            _syncSemaphore = new Semaphore();
            _renderWaiter = new Fence(true);
        }

        public override void Update()
        {
            //wait for last frame to finish
            _renderWaiter.Wait();
            _renderWaiter.Reset();
            //render image
            _renderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue);
            _renderCommand.BeginRenderPass(Engine.Instance.MainRenderPass, _framebuffer);
            _renderCommand.EndRenderPass();
            _renderCommand.End();
            CommandPool.Command.Submit(
                Engine.Instance.MainDevice.GraphicsQueueFamily.Queues[0],
                new CommandPool.Command[]{
                    _renderCommand
                },
                new Semaphore[]{
                    _syncSemaphore
                }
            );

            //copy image to swapchain
            _transferCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            _transferCommand.TransferImageLayout(_framebuffer.ColorImage, VkImageLayout.ColorAttachmentOptimal, VkImageLayout.TransferSrcOptimal);
            _transferCommand.TransferImageLayout(Engine.Instance.MainWindow.Swapchain.SwapchainImage, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
            _transferCommand.BlitImage(_framebuffer.ColorImage, Engine.Instance.MainWindow.Swapchain.SwapchainImage);
            _transferCommand.TransferImageLayout(_framebuffer.ColorImage, VkImageLayout.TransferSrcOptimal, VkImageLayout.ColorAttachmentOptimal);
            _transferCommand.TransferImageLayout(Engine.Instance.MainWindow.Swapchain.SwapchainImage, VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR);
            _transferCommand.End();
            CommandPool.Command.Submit(
                Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                new CommandPool.Command[]{
                    _transferCommand
                }, new Semaphore[0],
                new Semaphore[]{
                    _syncSemaphore
                },
                _renderWaiter
            );
        }
    }
}