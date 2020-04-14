#pragma warning disable 1591

namespace Tortuga.Utils.SDL2
{
    internal static unsafe partial class SDL2Native
    {
        private delegate void SDL_GetVersion_t(SDL_version* version);
        private static SDL_GetVersion_t s_getVersion = LoadFunction<SDL_GetVersion_t>("SDL_GetVersion");
        public static void SDL_GetVersion(SDL_version* version) => s_getVersion(version);
    }
    internal struct SDL_version
    {
        public byte major;
        public byte minor;
        public byte patch;
    }
}
