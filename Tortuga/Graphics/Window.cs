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

        private Sdl2Window _windowHandle;
        private VkSurfaceKHR _surface;
        private API.Swapchain _swapchain;
        private API.Semaphore _presentSync;

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
            this._presentSync = new API.Semaphore();
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
        public unsafe Veldrid.InputSnapshot PumpEvents()
        {
            uint nextImageIndex;
            if (vkAcquireNextImageKHR(
                Engine.Instance.MainDevice.LogicalDevice,
                this._swapchain.Handle,
                ulong.MaxValue,
                _presentSync.Handle,
                VkFence.Null,
                &nextImageIndex
            ) != VkResult.Success)
                throw new Exception("failed to acquire next swapchain image");

            var swapchains = new API.NativeList<VkSwapchainKHR>();
            swapchains.Add(_swapchain.Handle);

            var imageIndices = new API.NativeList<uint>();
            imageIndices.Add(nextImageIndex);

            var waitSemaphores = new API.NativeList<VkSemaphore>();
            waitSemaphores.Add(_presentSync.Handle);

            var presentInfo = VkPresentInfoKHR.New();
            presentInfo.swapchainCount = swapchains.Count;
            presentInfo.pSwapchains = (VkSwapchainKHR*)swapchains.Data.ToPointer();
            presentInfo.pImageIndices = (uint*)imageIndices.Data.ToPointer();
            presentInfo.waitSemaphoreCount = waitSemaphores.Count;
            presentInfo.pWaitSemaphores = (VkSemaphore*)waitSemaphores.Data.ToPointer();

            if (vkQueuePresentKHR(_swapchain.DevicePresentQueueFamily.Queues[0], &presentInfo) != VkResult.Success)
                throw new Exception("failed to present swapchain image");

            return _windowHandle.PumpEvents();
        }
    }
}