using System;
using System.Runtime.CompilerServices;
using Vulkan;
using Veldrid.Sdl2;
using static Vulkan.VulkanNative;
using static Veldrid.Sdl2.Sdl2Native;
using System.Diagnostics;

namespace Tortuga
{
    namespace Graphics
    {
        public class Window
        {
            public Sdl2Window NativeWindow => _window;
            public VkSurfaceKHR Surface => _surface;
            public VkInstance VulkanInstance => _instance;

            private Sdl2Window _window;
            private VkSurfaceKHR _surface;
            private VkInstance _instance;

            private unsafe static SDL_version SDLVersion
            {
                get
                {
                    SDL_version version;
                    SDL_GetVersion(&version);
                    return version;
                }
            }

            public unsafe Window(VulkanInstance instance, string name, int x, int y, int width, int height, SDL_WindowFlags flags, bool threadProcessing)
            {
                _instance = instance.Instance;
                _window = new Sdl2Window(name, x, y, width, height, flags, threadProcessing);

                SDL_SysWMinfo systemWindowInfo;
                systemWindowInfo.version = SDLVersion;
                int result = SDL_GetWMWindowInfo(_window.SdlWindowHandle, &systemWindowInfo);
                if (result == 0)
                    throw new InvalidOperationException("Could not retrive SDL window info");

                VkResult err;
                if (systemWindowInfo.subsystem == SysWMType.Windows)
                {
                    Win32WindowInfo win32Info = Unsafe.Read<Win32WindowInfo>(&systemWindowInfo.info);
                    // Create the os-specific Surface
                    VkWin32SurfaceCreateInfoKHR surfaceCreateInfo = VkWin32SurfaceCreateInfoKHR.New();
                    var processHandle = Process.GetCurrentProcess().SafeHandle.DangerousGetHandle();
                    surfaceCreateInfo.hinstance = processHandle;
                    surfaceCreateInfo.hwnd = win32Info.Sdl2Window;
                    VkSurfaceKHR surface;
                    err = vkCreateWin32SurfaceKHR(instance.Instance, &surfaceCreateInfo, null, &surface);
                    _surface = surface;
                }
                else if (systemWindowInfo.subsystem == SysWMType.X11)
                {
                    X11WindowInfo x11Info = Unsafe.Read<X11WindowInfo>(&systemWindowInfo);
                    VkXlibSurfaceCreateInfoKHR surfaceCreateInfo = VkXlibSurfaceCreateInfoKHR.New();
                    surfaceCreateInfo.dpy = (Vulkan.Xlib.Display*)x11Info.display;
                    surfaceCreateInfo.window = new Vulkan.Xlib.Window { Value = x11Info.Sdl2Window };
                    VkSurfaceKHR surface;
                    err = vkCreateXlibSurfaceKHR(instance.Instance, &surfaceCreateInfo, null, out surface);
                    _surface = surface;
                }
                else
                    throw new PlatformNotSupportedException("Only X11 and WIN32 are supported");
            }
            unsafe ~Window()
            {
                vkDestroySurfaceKHR(_instance, _surface, null);
            }
            public Veldrid.InputSnapshot PumpEvents()
                => NativeWindow.PumpEvents();
        }
    }
}