namespace Tortuga.Utils.OpenAL
{
    internal unsafe static partial class OpenALNative
    {
        #region device

        private delegate ALCdevice alcOpenDevice_T(char* deviceName);
        private static alcOpenDevice_T _alcOpenDevice = LoadFunction<alcOpenDevice_T>("alcOpenDevice");
        public static ALCdevice alcOpenDevice(string deviceName) => _alcOpenDevice(deviceName.ToCharPointer());

        private delegate bool alcCloseDevice_T(ALCdevice device);
        private static alcCloseDevice_T _alcCloseDevice = LoadFunction<alcCloseDevice_T>("alcCloseDevice");
        public static bool alcCloseDevice(ALCdevice device) => _alcCloseDevice(device);

        #endregion

        #region context

        private delegate ALCcontext alcCreateContext_T(ALCdevice device, int[] attrlist);
        private static alcCreateContext_T _alcCreateContext = LoadFunction<alcCreateContext_T>("alcCreateContext");
        public static ALCcontext alcCreateContext(ALCdevice device, int[] attributes) => _alcCreateContext(device, attributes);

        private delegate bool alcMakeContextCurrent_T(ALCcontext context);
        private static alcMakeContextCurrent_T _alcMakeContextCurrent = LoadFunction<alcMakeContextCurrent_T>("alcMakeContextCurrent");
        public static bool alcMakeContextCurrent(ALCcontext context) => _alcMakeContextCurrent(context);

        private delegate ALCcontext alcGetCurrentContext_T();
        private static alcGetCurrentContext_T _alcGetCurrentContext = LoadFunction<alcGetCurrentContext_T>("alcGetCurrentContext");
        public static ALCcontext alcGetCurrentContext() => _alcGetCurrentContext();

        private delegate void alcDestroyContext_T(ALCcontext context);
        private static alcDestroyContext_T _alcDestroyContext = LoadFunction<alcDestroyContext_T>("alcDestroyContext");
        public static void alcDestroyContext(ALCcontext context) => _alcDestroyContext(context);

        #endregion

        #region extensions

        private delegate bool alcIsExtensionPresent_T(ALCdevice device, string extenstion);
        private static alcIsExtensionPresent_T _alcIsExtensionPresent = LoadFunction<alcIsExtensionPresent_T>("alcIsExtensionPresent");
        public static bool alcIsExtensionPresent(ALCdevice device, string extension) => _alcIsExtensionPresent(device, extension);

        #endregion

        #region query functions

        private delegate void alcGetIntegerv_T(ALCdevice device, int param, int size, int[] values);
        private static alcGetIntegerv_T _alcGetIntegerv = LoadFunction<alcGetIntegerv_T>("alcGetIntegerv");
        public static void alcGetIntegerv(ALCdevice device, int param, int[] values) => _alcGetIntegerv(device, param, values.Length, values);

        private delegate char* alcGetString_T(ALCdevice device, int param);
        private static alcGetString_T _alcGetString = LoadFunction<alcGetString_T>("alcGetString");
        public static string alcGetString(ALCdevice device, int param) => new string(_alcGetString(device, param));

        #endregion
    }
}