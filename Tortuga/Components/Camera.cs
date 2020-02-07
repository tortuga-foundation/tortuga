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
        private Pipeline _tempPipeline;
        private CommandPool.Command _secondaryCommand;

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
            _secondaryCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            //setup transfer image to swapchain command
            _transferCommandPool = new CommandPool(
                Engine.Instance.MainDevice.TransferQueueFamily
            );
            _transferCommand = _transferCommandPool.AllocateCommands()[0];
            _syncSemaphore = new Semaphore();
            _renderWaiter = new Fence(true);
            _tempPipeline = new Pipeline(new DescriptorSetLayout[0], new Shader("Assets/Shaders/Simple.vert.spv"), new Shader("Assets/Shaders/Simple.frag.spv"));
        }

        public override void Update()
        {
            //wait for last frame to finish
            _renderWaiter.Wait();
            _renderWaiter.Reset();
            //render image
            _renderCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            _renderCommand.BeginRenderPass(Engine.Instance.MainRenderPass, _framebuffer);
            _secondaryCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, _framebuffer, 0);
            _secondaryCommand.BindPipeline(_tempPipeline);
            _secondaryCommand.SetViewport(0, 0, 1920, 1080);
            _secondaryCommand.Draw();
            _secondaryCommand.End();
            _renderCommand.ExecuteCommands(new CommandPool.Command[] { _secondaryCommand });
            _renderCommand.EndRenderPass();
            _renderCommand.End();
            _renderCommand.Submit(
                Engine.Instance.MainDevice.GraphicsQueueFamily.Queues[0],
                new Semaphore[]{
                    _syncSemaphore
                }
            );

            //copy image to swapchain
            _transferCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            _transferCommand.TransferImageLayout(_framebuffer.ColorImage, VkImageLayout.ColorAttachmentOptimal, VkImageLayout.TransferSrcOptimal);
            _transferCommand.TransferImageLayout(Engine.Instance.MainWindow.SwapchainImage, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
            _transferCommand.BlitImage(_framebuffer.ColorImage, Engine.Instance.MainWindow.SwapchainImage);
            _transferCommand.TransferImageLayout(_framebuffer.ColorImage, VkImageLayout.TransferSrcOptimal, VkImageLayout.ColorAttachmentOptimal);
            _transferCommand.TransferImageLayout(Engine.Instance.MainWindow.SwapchainImage, VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR);
            _transferCommand.End();
            _transferCommand.Submit(
                Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                new Semaphore[0],
                new Semaphore[]{
                    _syncSemaphore
                },
                _renderWaiter
            );
        }
    }
}