namespace Tortuga.Utils.OpenAL
{
    internal unsafe static partial class OpenALNative
    {
        private delegate ALCdevice alcOpenDevice_T(string deviceName);
        private static alcOpenDevice_T _alcOpenDevice = LoadFunction<alcOpenDevice_T>("alcOpenDevice");
        public static ALCdevice alcOpenDevice(string deviceName) => _alcOpenDevice(deviceName);
    
        private delegate ALCcontext alcCreateContext_T(ALCdevice device, int* attrlist);
        private static alcCreateContext_T _alcCreateContext = LoadFunction<alcCreateContext_T>("alcCreateContext");
        public static ALCcontext alcCreateContext(ALCdevice device) => _alcCreateContext(device, null);
    
        private delegate bool alcMakeContextCurrent_T(ALCcontext context);
        private static alcMakeContextCurrent_T _alcMakeContextCurrent = LoadFunction<alcMakeContextCurrent_T>("alcMakeContextCurrent");
        public static bool alcMakeContextCurrent(ALCcontext context) => _alcMakeContextCurrent(context);

        private delegate ALCcontext alcGetCurrentContext_T();
        private static alcGetCurrentContext_T _alcGetCurrentContext = LoadFunction<alcGetCurrentContext_T>("alcGetCurrentContext");
        public static ALCcontext alcGetCurrentContext() => _alcGetCurrentContext();

        private delegate void alcDestroyContext_T(ALCcontext context);
        private static alcDestroyContext_T _alcDestroyContext = LoadFunction<alcDestroyContext_T>("alcDestroyContext");
        public static void alcDestroyContext(ALCcontext context) => _alcDestroyContext(context);

        private delegate bool alcCloseDevice_T(ALCdevice device);
        private static alcCloseDevice_T _alcCloseDevice = LoadFunction<alcCloseDevice_T>("alcCloseDevice");
        public static bool alcCloseDevice(ALCdevice device) => _alcCloseDevice(device);
    }
}