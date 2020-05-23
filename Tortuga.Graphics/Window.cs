using System;
using Vulkan;
using System.Numerics;
using Tortuga.Utils.SDL2;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Tortuga.Utils;
using static Vulkan.VulkanNative;
using static Tortuga.Utils.SDL2.SDL2Native;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Different types of windows
    /// </summary>
    public enum WindowType
    {
        /// <summary>
        /// Default type of window
        /// </summary>
        Window,
        /// <summary>
        /// borderless window
        /// </summary>
        Borderless,
        /// <summary>
        /// Full screen window
        /// </summary>
        Fullscreen,
        /// <summary>
        /// Resizable window
        /// </summary>
        ResizeableWindow
    }

    /// <summary>
    /// Type of message box
    /// </summary>
    public enum MessageBoxFlags
    {
        /// <summary>
        /// Error type of message box
        /// </summary>
        Error = 16,
        /// <summary>
        /// Warning type of message box
        /// </summary>
        Warning = 32,
        /// <summary>
        /// Information type of message box
        /// </summary>
        Information = 64
    }

    /// <summary>
    /// This can be used to create a GUI window
    /// </summary>
    public class Window
    {
        /// <summary>
        /// returns acquired swapchain
        /// </summary>
        public uint SwapchainAcquiredImage => _swapchainImageIndex;
        internal VkSurfaceKHR Surface => _surface;
        internal API.Swapchain Swapchain => _swapchain;

        private IntPtr _windowHandle;
        private VkSurfaceKHR _surface;
        private API.Swapchain _swapchain;
        private uint _swapchainImageIndex;
        private API.Fence _swapchianFence;

        /// <summary>
        /// constructor to create a window
        /// </summary>
        /// <param name="title">the title of the window</param>
        /// <param name="x">x position of the window</param>
        /// <param name="y">y position of the window</param>
        /// <param name="width">the width of the window</param>
        /// <param name="height">the height of the window</param>
        /// <param name="type">the type of window to create</param>
        public unsafe Window(
            string title,
            int x, int y,
            int width, int height,
            WindowType type)
        {
            var flags = SDL_WindowFlags.AllowHighDpi;
            if (type == WindowType.Borderless)
                flags |= SDL_WindowFlags.Borderless;
            else if (type == WindowType.Fullscreen)
                flags |= SDL_WindowFlags.Fullscreen;
            else if (type == WindowType.ResizeableWindow)
                flags |= SDL_WindowFlags.Resizable;
                
            this._windowHandle = SDL_CreateWindow(
                title,
                x, y,
                width, height,
                flags
            );
            _exists = true;

            //create surface
            var sdlVersion = SDLVersion;
            SDL_SysWMinfo sysWindowInfo;
            sysWindowInfo.version = sdlVersion;
            if (SDL_GetWMWindowInfo(_windowHandle, &sysWindowInfo) == 0)
                throw new InvalidOperationException("couldn't retrive sdl window info");
            VkResult err;
            VkSurfaceKHR surface;
            if (sysWindowInfo.subsystem == SysWMType.Windows)
            {
                var win32Info = Unsafe.Read<Win32WindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = VkWin32SurfaceCreateInfoKHR.New();
                var processHandle = Process.GetCurrentProcess().SafeHandle.DangerousGetHandle();
                surfaceInfo.hinstance = processHandle;
                surfaceInfo.hwnd = win32Info.window;
                err = vkCreateWin32SurfaceKHR(API.Handler.Vulkan.Handle, &surfaceInfo, null, &surface);
            }
            else if (sysWindowInfo.subsystem == SysWMType.X11)
            {
                var x11Info = Unsafe.Read<X11WindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = VkXlibSurfaceCreateInfoKHR.New();
                surfaceInfo.dpy = (Vulkan.Xlib.Display*)x11Info.display;
                surfaceInfo.window = new Vulkan.Xlib.Window
                {
                    Value = x11Info.window
                };
                err = vkCreateXlibSurfaceKHR(API.Handler.Vulkan.Handle, &surfaceInfo, null, &surface);
            }
            else if (sysWindowInfo.subsystem == SysWMType.Wayland)
            {
                var waylandInfo = Unsafe.Read<WaylandWindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = VkWaylandSurfaceCreateInfoKHR.New();
                surfaceInfo.display = (Vulkan.Wayland.wl_display*)waylandInfo.display;
                surfaceInfo.surface = (Vulkan.Wayland.wl_surface*)waylandInfo.surface;
                err = vkCreateWaylandSurfaceKHR(API.Handler.Vulkan.Handle, &surfaceInfo, null, &surface);
            }
            else if (sysWindowInfo.subsystem == SysWMType.Android)
            {
                var androidInfo = Unsafe.Read<AndroidWindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = VkAndroidSurfaceCreateInfoKHR.New();
                surfaceInfo.window = (Vulkan.Android.ANativeWindow*)androidInfo.window;
                err = vkCreateAndroidSurfaceKHR(API.Handler.Vulkan.Handle, &surfaceInfo, null, &surface);
            }
            else if (sysWindowInfo.subsystem == SysWMType.Mir)
            {
                var mirInfo = Unsafe.Read<MirWindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = VkMirSurfaceCreateInfoKHR.New();
                surfaceInfo.connection = (Vulkan.Mir.MirConnection*)mirInfo.connection;
                surfaceInfo.mirSurface = (Vulkan.Mir.MirSurface*)mirInfo.mirSurface;
                err = vkCreateMirSurfaceKHR(API.Handler.Vulkan.Handle, &surfaceInfo, null, &surface);
            }
            else
                throw new PlatformNotSupportedException("This platform (window manager) is currently not supported");
            
            if (err != VkResult.Success)
                throw new Exception("failed to create window surface");

            this._surface = surface;
            this._swapchain = new API.Swapchain(API.Handler.MainDevice, this);
            this._swapchianFence = new API.Fence(API.Handler.MainDevice);
        }

        /// <summary>
        /// de-constructor for the window
        /// </summary>
        unsafe ~Window()
        {
            _exists = false;
            SDL_DestroyWindow(_windowHandle);
            vkDestroySurfaceKHR(API.Handler.Vulkan.Handle, this._surface, null);
        }

        internal static unsafe SDL_version SDLVersion
        {
            get
            {
                SDL_version version;
                SDL_GetVersion(&version);
                return version;
            }
        }

        /// <summary>
        /// Does this window exist or it has been destroyed
        /// </summary>
        public bool Exists => _exists;
        private bool _exists;

        /// <summary>
        /// sets or gets if the window is resizeable
        /// </summary>
        public bool Resizeable
        {
            get => (SDL_GetWindowFlags(_windowHandle) & SDL_WindowFlags.Resizable) != 0;
        }
        /// <summary>
        /// sets or gets the window's title
        /// </summary>
        public string Title
        {
            get => SDL_GetWindowTitle(_windowHandle);
            set => SDL_SetWindowTitle(_windowHandle, value);
        }
        /// <summary>
        /// sets or gets if the window is visible
        /// </summary>
        public bool Visible
        {
            get => (SDL_GetWindowFlags(_windowHandle) & SDL_WindowFlags.Shown) != 0;
            set
            {
                if (value)
                    SDL_ShowWindow(_windowHandle);
                else
                    SDL_HideWindow(_windowHandle);
            }
        }
        /// <summary>
        /// Window width and height
        /// </summary>
        public unsafe Vector2 Size
        {
            get 
            {
                int w, h;
                SDL_GetWindowSize(_windowHandle, &w, &h);
                return new Vector2(w, h);
            }
            set
            {
                int w = Convert.ToInt32(MathF.Round(value.X));
                int h = Convert.ToInt32(MathF.Round(value.Y));
                SDL_SetWindowSize(_windowHandle, w, h);
            }
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        public unsafe void Close()
        {
            _exists = false;
        }

        /// <summary>
        /// Aquire swapchain image and store the referance in 'SwapchainAcquiredImage'
        /// </summary>
        public unsafe void AcquireSwapchainImage()
        {
            _swapchianFence.Reset();
            uint imageIndex;
            var acquireResponse = vkAcquireNextImageKHR(
                API.Handler.MainDevice.LogicalDevice.Handle,
                _swapchain.Handle,
                ulong.MaxValue,
                VkSemaphore.Null,
                _swapchianFence.Handle,
                &imageIndex
            );
            if (acquireResponse == VkResult.ErrorOutOfDateKHR)
            {
                _swapchain.Resize();
                AcquireSwapchainImage();
                return;
            }
            else if (acquireResponse != VkResult.Success)
                throw new Exception("failed to get next swapchain image");
            else
                _swapchianFence.Wait();
            _swapchainImageIndex = imageIndex;
        }

        /// <summary>
        /// present acquired swapchain
        /// </summary>
        public unsafe void Present()
        {
            var swapchains = new NativeList<VkSwapchainKHR>();
            var imageIndices = new NativeList<uint>();
            swapchains.Add(_swapchain.Handle);
            imageIndices.Add(_swapchainImageIndex);

            var waitSemaphores = new NativeList<VkSemaphore>();

            var presentInfo = VkPresentInfoKHR.New();
            presentInfo.swapchainCount = swapchains.Count;
            presentInfo.pSwapchains = (VkSwapchainKHR*)swapchains.Data.ToPointer();
            presentInfo.pImageIndices = (uint*)imageIndices.Data.ToPointer();
            presentInfo.waitSemaphoreCount = waitSemaphores.Count;
            presentInfo.pWaitSemaphores = (VkSemaphore*)waitSemaphores.Data.ToPointer();

            var presentResponse = vkQueuePresentKHR(_swapchain.DevicePresentQueueFamily.Queues[0], &presentInfo);
            if (presentResponse != VkResult.Success && presentResponse != VkResult.ErrorOutOfDateKHR)
                throw new Exception("failed to present swapchain image");
        }

        /// <summary>
        /// Display's a message box
        /// </summary>
        /// <param name="flags">Type of message box</param>
        /// <param name="title">Title</param>
        /// <param name="message">Message to display</param>
        /// <param name="shouldPause">If true it will stop the program until message box is delt with</param>
        public unsafe void ShowMessageBox(MessageBoxFlags flags, string title, string message, bool shouldPause = false)
        {
            var t1 = System.Threading.Tasks.Task.Run(() => 
            {
                SDL_ShowSimpleMessageBox((uint)flags, title, message);
            });
            if (shouldPause)
                t1.Wait();
        }

        /// <summary>
        /// Set cursor position inside the window
        /// </summary>
        /// <param name="x">X position of the cursor</param>
        /// <param name="y">Y position of the cursor</param>
        public unsafe void SetCursor(int x, int y)
            => SDL_WarpMouseInWindow(_windowHandle, x, y);
    }
}