using System;
using Vulkan;
using Veldrid.Sdl2;
using System.Runtime.CompilerServices;
using static Vulkan.VulkanNative;
using static Veldrid.Sdl2.Sdl2Native;
using System.Diagnostics;

namespace Tortuga.Graphics.API
{
    internal class Window
    {
        private Sdl2Window _windowHandle;
        private VkSurfaceKHR _surface;

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
        }

        unsafe ~Window()
        {
            vkDestroySurfaceKHR(Engine.Instance.Vulkan.Handle, this._surface, null);
        }

        private unsafe SDL_version SDLVersion
        {
            get
            {
                SDL_version version;
                SDL_GetVersion(&version);
                return version;
            }
        }
    }
}