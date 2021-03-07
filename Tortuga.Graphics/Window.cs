using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tortuga.Graphics.API;
using Tortuga.Utils;
using Tortuga.Utils.SDL2;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// This class is used to create a window used by tortuga engine
    /// </summary>
    public class Window : RenderTarget
    {
        /// <summary>
        /// list of all windows created by the engine
        /// </summary>
        public List<Window> Windows = new List<Window>();
        /// <summary>
        /// Native SDL window class
        /// </summary>
        public NativeWindow NativeWindow => _nativeWindow;
        /// <summary>
        /// Swapchain used for vulkan renderer
        /// </summary>
        public Swapchain Swapchain => _swapchain;

        private GraphicsModule _graphicsModule;
        private NativeWindow _nativeWindow;
        private Swapchain _swapchain;

        /// <summary>
        /// Create a window
        /// </summary>
        /// <param name="title">title for the window</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="width">width of the window</param>
        /// <param name="height">height of the window</param>
        /// <param name="flags">flags for window</param>
        public Window(
            string title,
            int x, int y,
            int width, int height,
            WindowFlags flags = (
                WindowFlags.AllowHighDpi |
                WindowFlags.Shown
            )
        ) : base(
            Engine.Instance.GetModule<GraphicsModule>().GraphicsService.PrimaryDevice,
            Convert.ToUInt32(width),
            Convert.ToUInt32(height)
        )
        {
            Windows.Add(this);
            _graphicsModule = Engine.Instance.GetModule<GraphicsModule>();
            _nativeWindow = new NativeWindow(
                _graphicsModule.GraphicsService,
                title,
                x, y,
                width, height,
                (SDL_WindowFlags)flags
            );
            _swapchain = new Swapchain(
                _graphicsModule.GraphicsService.PrimaryDevice,
                _nativeWindow
            );
        }
        /// <summary>
        /// De-Constructor
        /// </summary> 
        ~Window()
        {
            Windows.Remove(this);
        }

        /// <summary>
        /// acquire's swapchain image index
        /// </summary>
        /// <returns>image index</returns>
        public unsafe Task<uint> AcquireSwapchainImage()
        => Task.Run(() =>
        {
            var fence = new Fence(_graphicsModule.GraphicsService.PrimaryDevice);
            uint imageIndex = 0;
            VulkanNative.vkAcquireNextImageKHR(
                _graphicsModule.GraphicsService.PrimaryDevice.Handle,
                _swapchain.Handle,
                ulong.MaxValue,
                VkSemaphore.Null,
                fence.Handle,
                &imageIndex
            );
            fence.Wait();
            return imageIndex;
        });

        /// <summary>
        /// shows a message box
        /// </summary>
        /// <param name="flags">type of message box</param>
        /// <param name="title">title for the message box</param>
        /// <param name="message">body of the message box</param>
        /// <param name="shouldPause">should the game engine wait for message box to be closed?</param>
        public static unsafe void ShowMessageBox(
            MessageBoxFlags flags,
            string title,
            string message,
            bool shouldPause = false
        )
        {
            var t1 = Task.Run(() =>
            {
                SDL2Native.SDL_ShowSimpleMessageBox(
                    (uint)flags,
                    title,
                    message
                );
            });
            if (shouldPause)
                t1.Wait();
        }

        /// <summary>
        /// shows a message box that is parented to this window
        /// </summary>
        /// <param name="flags">type of message box</param>
        /// <param name="title">title for the message box</param>
        /// <param name="message">body of the message box</param>
        /// <param name="shouldPause">should the game engine wait for message box to be closed?</param>
        public unsafe void ShowParentedMessageBox(
            MessageBoxFlags flags,
            string title,
            string message,
            bool shouldPause = false
        )
        {
            var t1 = Task.Run(() =>
            {
                SDL2Native.SDL_ShowSimpleMessageBox(
                    (uint)flags,
                    title,
                    message,
                    (SDL_Window*)_nativeWindow.Handle.NativePointer
                );
            });
            if (shouldPause)
                t1.Wait();
        }
    }
}