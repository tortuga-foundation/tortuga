using System;
using Tortuga.Utils.SDL2;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Flags for window
    /// </summary>
    [Flags]
    public enum WindowFlags : uint
    {
        /// <summary>
        /// Set's window to full screen mode
        /// </summary>
        FullScreen = SDL_WindowFlags.Fullscreen,
        /// <summary>
        /// Window should be visible to the user
        /// </summary>
        Shown = SDL_WindowFlags.Shown,
        /// <summary>
        /// Window should be hidden to the user
        /// </summary>
        Hidden = SDL_WindowFlags.Hidden,
        /// <summary>
        /// Window should be borderless
        /// </summary>
        Borderless = SDL_WindowFlags.Borderless,
        /// <summary>
        /// Window should be resiable
        /// </summary>
        Resizeable = SDL_WindowFlags.Resizable,
        /// <summary>
        /// Window should be minimized
        /// </summary>
        Minimized = SDL_WindowFlags.Minimized,
        /// <summary>
        /// Window should be maximized
        /// </summary>
        Maximized = SDL_WindowFlags.Maximized,
        /// <summary>
        /// Window has grabbed input focus
        /// </summary>
        InputGrabbed = SDL_WindowFlags.InputGrabbed,
        /// <summary>
        /// Window has input focus
        /// </summary>
        InputFocused = SDL_WindowFlags.InputFocus,
        /// <summary>
        /// Window has mouse focus
        /// </summary>
        MouseFocus = SDL_WindowFlags.MouseFocus,
        /// <summary>
        /// Set's window to full screen desktop mode
        /// </summary>
        FullScreenDesktop = SDL_WindowFlags.FullScreenDesktop,
        /// <summary>
        /// Allow window to use high Dpi setting
        /// </summary>
        AllowHighDpi = SDL_WindowFlags.AllowHighDpi,
        /// <summary>
        /// Window has mouse capture (unrelated to input grabbed)
        /// </summary>
        MouseCapture = SDL_WindowFlags.MouseCapture,
        /// <summary>
        /// Window should always be on top
        /// </summary>
        AlwaysOnTop = SDL_WindowFlags.AlwaysOnTop,
        /// <summary>
        /// Hide window from task bar
        /// </summary>
        SkipTaskbar = SDL_WindowFlags.SkipTaskbar,
        /// <summary>
        /// Window should be treated as a tooltip
        /// </summary>
        Tooltip = SDL_WindowFlags.Tooltip,
        /// <summary>
        /// Window should be treated as a popup menu
        /// </summary>
        PopupMenu = SDL_WindowFlags.PopupMenu
    }
}