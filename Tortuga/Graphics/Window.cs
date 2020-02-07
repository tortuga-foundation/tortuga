using System;
using Vulkan;
using Veldrid.Sdl2;
using System.Runtime.CompilerServices;
using static Vulkan.VulkanNative;
using static Veldrid.Sdl2.Sdl2Native;
using System.Diagnostics;

namespace Tortuga.Graphics
{
    public class Window
    {
        internal Sdl2Window SdlHandle => _windowHandle;
        internal VkSurfaceKHR Surface => _surface;
        internal API.Swapchain Swapchain => _swapchain;
        internal API.Image SwapchainImage => _swapchain.Images[Convert.ToInt32(_swapchainImageIndex)];

        private Sdl2Window _windowHandle;
        private VkSurfaceKHR _surface;
        private API.Swapchain _swapchain;
        private uint _swapchainImageIndex;
        private API.Fence _swapchianFence;

        public unsafe Window(
            string title,
            int x, int y,
            int width, int height,
            SDL_WindowFlags flags,
            bool threadProcessing)
        {
            this._windowHandle = new Veldrid.Sdl2.Sdl2Window(
                title,
                x, y,
                width, height,
                flags,
                threadProcessing
            );

            //create surface
            var sdlVersion = SDLVersion;
            SDL_SysWMinfo sysWindowInfo;
            sysWindowInfo.version = sdlVersion;
            if (SDL_GetWMWindowInfo(_windowHandle.SdlWindowHandle, &sysWindowInfo) == 0)
                throw new InvalidOperationException("couldn't retrive sdl window info");
            VkResult err;
            VkSurfaceKHR surface;
            if (sysWindowInfo.subsystem == SysWMType.Windows)
            {
                var win32Info = Unsafe.Read<Win32WindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = VkWin32SurfaceCreateInfoKHR.New();
                var processHandle = Process.GetCurrentProcess().SafeHandle.DangerousGetHandle();
                surfaceInfo.hinstance = processHandle;
                surfaceInfo.hwnd = win32Info.Sdl2Window;
                err = vkCreateWin32SurfaceKHR(Engine.Instance.Vulkan.Handle, &surfaceInfo, null, &surface);
            }
            else if (sysWindowInfo.subsystem == SysWMType.X11)
            {
                var x11Info = Unsafe.Read<X11WindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = VkXlibSurfaceCreateInfoKHR.New();
                surfaceInfo.dpy = (Vulkan.Xlib.Display*)x11Info.display;
                surfaceInfo.window = new Vulkan.Xlib.Window
                {
                    Value = x11Info.Sdl2Window
                };
                err = vkCreateXlibSurfaceKHR(Engine.Instance.Vulkan.Handle, &surfaceInfo, null, &surface);
            }
            else if (sysWindowInfo.subsystem == SysWMType.Wayland)
            {
                var waylandInfo = Unsafe.Read<WaylandWindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = VkWaylandSurfaceCreateInfoKHR.New();
                surfaceInfo.display = (Vulkan.Wayland.wl_display*)waylandInfo.display;
                surfaceInfo.surface = (Vulkan.Wayland.wl_surface*)waylandInfo.surface;
                err = vkCreateWaylandSurfaceKHR(Engine.Instance.Vulkan.Handle, &surfaceInfo, null, &surface);
            }
            else
                throw new PlatformNotSupportedException("this platform is not currently supported");
            if (err != VkResult.Success)
                throw new Exception("failed to create window surface");

            this._surface = surface;
            this._swapchain = new API.Swapchain(this);
            this._swapchianFence = new API.Fence();
        }

        unsafe ~Window()
        {
            vkDestroySurfaceKHR(Engine.Instance.Vulkan.Handle, this._surface, null);
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

        public bool Exists => _windowHandle.Exists;
        public bool Resizeable
        {
            get => _windowHandle.Resizable;
            set => _windowHandle.Resizable = value;
        }
        public string Title
        {
            get => _windowHandle.Title;
            set => _windowHandle.Title = value;
        }
        public bool Visible
        {
            get => _windowHandle.Visible;
            set => _windowHandle.Visible = value;
        }
        public int Width
        {
            get => _windowHandle.Width;
            set => _windowHandle.Width = value;
        }
        public int height
        {
            get => _windowHandle.Height;
            set => _windowHandle.Height = value;
        }

        public void RecreateSwapchain()
            => _swapchain = new API.Swapchain(this);

        public unsafe Veldrid.InputSnapshot PumpEvents()
            => _windowHandle.PumpEvents();

        public unsafe void AcquireSwapchainImage()
        {
            _swapchianFence.Reset();
            uint imageIndex;
            var acquireResponse = vkAcquireNextImageKHR(
                Engine.Instance.MainDevice.LogicalDevice.Handle,
                _swapchain.Handle,
                ulong.MaxValue,
                VkSemaphore.Null,
                _swapchianFence.Handle,
                &imageIndex
            );
            if (acquireResponse == VkResult.ErrorOutOfDateKHR)
            {
                RecreateSwapchain();
                AcquireSwapchainImage();
            }
            else if (acquireResponse != VkResult.Success)
                throw new Exception("failed to get next swapchain image");
            else
                _swapchianFence.Wait();
            _swapchainImageIndex = imageIndex;
        }

        public unsafe void Present()
        {
            var swapchains = new API.NativeList<VkSwapchainKHR>();
            var imageIndices = new API.NativeList<uint>();
            swapchains.Add(_swapchain.Handle);
            imageIndices.Add(_swapchainImageIndex);

            var waitSemaphores = new API.NativeList<VkSemaphore>();

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
    }
}