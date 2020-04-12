#pragma warning disable 1591
using System;

namespace Tortuga.Utils.SDL2
{
    public static unsafe partial class SDL2Native
    {
        private delegate int SDL_GetWindowWMInfo_t(SDL_Window window, SDL_SysWMinfo* info);
        private static readonly SDL_GetWindowWMInfo_t s_getWindowWMInfo = LoadFunction<SDL_GetWindowWMInfo_t>("SDL_GetWindowWMInfo");
        public static int SDL_GetWMWindowInfo(SDL_Window window, SDL_SysWMinfo* info) => s_getWindowWMInfo(window, info);
    }
    public struct SDL_SysWMinfo
    {
        public SDL_version version;
        public SysWMType subsystem;
        public WindowInfo info;
    }

    public unsafe struct WindowInfo
    {
        public const int WindowInfoSizeInBytes = 100;
        private fixed byte bytes[WindowInfoSizeInBytes];
    }

    public struct Win32WindowInfo
    {
        public IntPtr window;
        public IntPtr hdc;
        public IntPtr hinstance;
    }

    public struct X11WindowInfo
    {
        public IntPtr display;
        public IntPtr window;
    }

    public struct WaylandWindowInfo
    {

        public IntPtr display;
        public IntPtr window;
        public IntPtr surface;
    }

    public enum SysWMType
    {
        Unknown,
        Windows,
        X11,
        DirectFB,
        Cocoa,
        UIKit,
        Wayland,
        Mir,
        WinRT,
        Android,
        Vivante
    }
}
