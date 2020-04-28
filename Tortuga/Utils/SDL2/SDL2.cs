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
    }
}