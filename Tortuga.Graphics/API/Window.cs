#pragma warning disable CS1591
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Tortuga.Utils.SDL2;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Window
    {
        public GraphicsService GraphicsService => _graphicsService;
        public SDL_Window Handle => _handle;
        public VkSurfaceKHR Surface => _surface;
        public SDL_version Version => _version;
        public int Width => _width;
        public int Height => _height;

        private GraphicsService _graphicsService;
        private SDL_Window _handle;
        private SDL_version _version;
        private VkSurfaceKHR _surface;
        private int _width;
        private int _height;

        public unsafe Window(
            GraphicsService graphicsService,
            string title,
            int x, int y,
            int width, int height,
            SDL_WindowFlags flags
        )
        {
            _graphicsService = graphicsService;
            _handle = SDL2Native.SDL_CreateWindow(
                title,
                x, y,
                width, height,
                flags
            );

            //get sdl version
            var sysWindowInfo = new SDL_SysWMinfo();
            SDL2Native.SDL_GetVersion(&sysWindowInfo.version);
            _version = sysWindowInfo.version;
            if (SDL2Native.SDL_GetWMWindowInfo(
                _handle,
                &sysWindowInfo
            ) == 0)
                throw new Exception("couldn't retrive sdl window information");

            VkResult error;
            VkSurfaceKHR surface;
            if (sysWindowInfo.subsystem == SysWMType.Windows)
            {
                var processHandle = (
                    Process
                    .GetCurrentProcess()
                    .SafeHandle
                    .DangerousGetHandle()
                );
                var win32Info = Unsafe.Read<Win32WindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = new VkWin32SurfaceCreateInfoKHR
                {
                    sType = VkStructureType.Win32SurfaceCreateInfoKHR,
                    hinstance = processHandle,
                    hwnd = win32Info.window
                };
                error = VulkanNative.vkCreateWin32SurfaceKHR(
                    graphicsService.Handle,
                    &surfaceInfo,
                    null,
                    &surface
                );
            }
            else if (sysWindowInfo.subsystem == SysWMType.X11)
            {
                var x11Info = Unsafe.Read<X11WindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = new VkXlibSurfaceCreateInfoKHR
                {
                    dpy = (Vulkan.Xlib.Display*)x11Info.display,
                    window = new Vulkan.Xlib.Window
                    {
                        Value = x11Info.window
                    }
                };
                error = VulkanNative.vkCreateXlibSurfaceKHR(
                    graphicsService.Handle,
                    &surfaceInfo,
                    null,
                    &surface
                );
            }
            else if (sysWindowInfo.subsystem == SysWMType.Wayland)
            {
                var waylandINfo = Unsafe.Read<WaylandWindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = new VkWaylandSurfaceCreateInfoKHR
                {
                    display = (Vulkan.Wayland.wl_display*)waylandINfo.display,
                    surface = (Vulkan.Wayland.wl_surface*)waylandINfo.surface
                };
                error = VulkanNative.vkCreateWaylandSurfaceKHR(
                    graphicsService.Handle,
                    &surfaceInfo,
                    null,
                    &surface
                );
            }
            else if (sysWindowInfo.subsystem == SysWMType.Android)
            {
                var androidInfo = Unsafe.Read<AndroidWindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = new VkAndroidSurfaceCreateInfoKHR
                {
                    window = (Vulkan.Android.ANativeWindow*)androidInfo.window
                };
                error = VulkanNative.vkCreateAndroidSurfaceKHR(
                    graphicsService.Handle,
                    &surfaceInfo,
                    null,
                    &surface
                );
            }
            else if (sysWindowInfo.subsystem == SysWMType.Mir)
            {
                var mirInfo = Unsafe.Read<MirWindowInfo>(&sysWindowInfo.info);
                var surfaceInfo = new VkMirSurfaceCreateInfoKHR
                {
                    connection = (Vulkan.Mir.MirConnection*)mirInfo.connection,
                    mirSurface = (Vulkan.Mir.MirSurface*)mirInfo.mirSurface
                };
                error = VulkanNative.vkCreateMirSurfaceKHR(
                    graphicsService.Handle,
                    &surfaceInfo,
                    null,
                    &surface
                );
            }
            else
                throw new PlatformNotSupportedException("this platform is currently not supported");

            if (error != VkResult.Success)
                throw new Exception("failed to create window surface");

            _surface = surface;
        }

        unsafe ~Window()
        {
            if (_handle != IntPtr.Zero)
            {
                SDL2Native.SDL_DestroyWindow(_handle);
                _handle = IntPtr.Zero;
            }
            if (_surface != VkSurfaceKHR.Null)
            {
                VulkanNative.vkDestroySurfaceKHR(
                    _graphicsService.Handle,
                    _surface,
                    null
                );
                _surface = VkSurfaceKHR.Null;
            }
        }
    }
}