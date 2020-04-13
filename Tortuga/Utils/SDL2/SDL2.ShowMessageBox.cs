#pragma warning disable 1591
using System;

namespace Tortuga.Utils.SDL2
{
    public static unsafe partial class SDL2Native
    {
        private delegate int SDL_ShowSimpleMessageBox_t(uint flags, string title, string message, SDL_Window* parentWindow);
        private static SDL_ShowSimpleMessageBox_t s_sdl_showSimpleMessageBox = LoadFunction<SDL_ShowSimpleMessageBox_t>("SDL_ShowSimpleMessageBox");
        public static int SDL_ShowSimpleMessageBox(uint flags, string title, string message, SDL_Window* parentWindow = null) => s_sdl_showSimpleMessageBox(flags, title, message, parentWindow);
    }    
}