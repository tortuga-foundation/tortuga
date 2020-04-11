using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tortuga.SDL2
{
    /// <summary>
    /// SDL2 Native
    /// </summary>
    public static unsafe partial class SDL2Native
    {
        private static readonly NativeLibraryLoader.NativeLibrary s_sdl2Lib = LoadSdl2();
        private static NativeLibraryLoader.NativeLibrary LoadSdl2()
        {
            string name;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                name = "SDL2.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                name = "libSDL2-2.0.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                name = "libsdl2.dylib";
            }
            else
            {
                Debug.WriteLine("Unknown SDL platform. Attempting to load \"SDL2\"");
                name = "SDL2";
            }

            var lib = new NativeLibraryLoader.NativeLibrary(name);
            return lib;
        }

        private static T LoadFunction<T>(string name)
        {
            return s_sdl2Lib.LoadFunction<T>(name);
        }

        private delegate string SDL_GetError_t();
        private static SDL_GetError_t s_sdl_getError = LoadFunction<SDL_GetError_t>("SDL_GetError");
        /// <summary>
        /// returns the SDL error if an sdl error occurred
        /// </summary>
        /// <returns></returns>
        public static string SDL_GetError() => s_sdl_getError();
    }
}