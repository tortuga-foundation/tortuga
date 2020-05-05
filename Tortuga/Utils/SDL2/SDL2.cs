using System.Runtime.InteropServices;

namespace Tortuga.Utils.SDL2
{
    internal static unsafe partial class SDL2Native
    {
        private static string[] GetLibName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new string[]{
                    "SDL2.dll"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new string[]{
                    "libSDL2.so",
                    "libSDL2-2.0.so",
                    "libSDL2-2.0.so.0",
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new string[]{
                    "libsdl2.dylib",
                    "sdl2.dylib"
                };
            }
            else
            {
                return new string[]{
                    "SDL2"
                };
            }
        }

        private static readonly NativeLibraryLoader.NativeLibrary _lib = _loadSDL2();
        private static NativeLibraryLoader.NativeLibrary _loadSDL2()
        {
            var lib = new NativeLibraryLoader.NativeLibrary(GetLibName());
            System.Console.WriteLine("Loaded SDL2");
            return lib;
        }

        private static T LoadFunction<T>(string name)
        {
            return _lib.LoadFunction<T>(name);
        }

        private delegate string SDL_GetError_t();
        private static SDL_GetError_t s_sdl_getError = LoadFunction<SDL_GetError_t>("SDL_GetError");
        public static string SDL_GetError() => s_sdl_getError();
    
        private delegate void SDL_ShowCursor_T(int param);
        private static SDL_ShowCursor_T _SDL_ShowCursor = LoadFunction<SDL_ShowCursor_T>("SDL_ShowCursor");
        public static void SDL_ShowCursor(int param) => _SDL_ShowCursor(param);

        private delegate void SDL_SetRelativeMouseMode_T(bool isOn);
        private static SDL_SetRelativeMouseMode_T _SDL_SetRelativeMouseMode = LoadFunction<SDL_SetRelativeMouseMode_T>("SDL_SetRelativeMouseMode");
        public static void SDL_SetRelativeMouseMode(bool isOn) => _SDL_SetRelativeMouseMode(isOn);

        private delegate void SDL_WarpMouseInWindow_T(SDL_Window window, int x, int y);
        private static SDL_WarpMouseInWindow_T _SDL_WarpMouseInWindow = LoadFunction<SDL_WarpMouseInWindow_T>("SDL_WarpMouseInWindow");
        public static void SDL_WarpMouseInWindow(SDL_Window window, int x, int y) => _SDL_WarpMouseInWindow(window, x, y);
    }
}