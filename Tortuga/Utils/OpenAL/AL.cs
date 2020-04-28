using System;
using System.Runtime.InteropServices;

namespace Tortuga.Utils.OpenAL
{
    internal unsafe static partial class OpenALNative
    {
        #region library loader

        public static string[] GetLibName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new string[]{
                    "openal32.dll",
                    "soft_oal.dll"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new string[]{
                    "libopenal.so",
                    "libopenal.so.1"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new string[]{
                    "libopenal.dylib", 
                    "openal.dylib"
                };
            }
            else
            {
                return new string[]{
                    "openal"
                };
            }
        }
    
        private static readonly NativeLibraryLoader.NativeLibrary _lib = _loadOpenAL();
        private static NativeLibraryLoader.NativeLibrary _loadOpenAL()
        {
            var lib = new NativeLibraryLoader.NativeLibrary(GetLibName());
            System.Console.WriteLine("Loaded Open AL");
            return lib;
        }

        private static T LoadFunction<T>(string name)
        {
            return _lib.LoadFunction<T>(name);
        }

        #endregion
    
        #region error handling

        private delegate int alGetError_T();
        private static alGetError_T _alGetError = LoadFunction<alGetError_T>("alGetError");
        public static ALError alGetError() => (ALError)_alGetError();
        public static void alHandleError(string message)
        {
            var err = alGetError();
            if (err != ALError.None)
                throw new Exception(message + err.ToString());
        }

        private delegate int alGetEnumValue_T(string name);
        private static alGetEnumValue_T _alGetEnumValue = LoadFunction<alGetEnumValue_T>("alGetEnumValue");
        public static int alGetEnumValue(string name) => _alGetEnumValue(name);

        #endregion

        #region buffers

        private delegate void alGenBuffers_T(int size, out uint buffers);
        private static alGenBuffers_T _alGenBuffers = LoadFunction<alGenBuffers_T>("alGenBuffers");
        public static void alGenBuffers(out uint buffers) => _alGenBuffers(1, out buffers);

        private delegate void alDeleteBuffers_T(int size, uint[] buffers);
        private static alDeleteBuffers_T _alDeleteBuffers = LoadFunction<alDeleteBuffers_T>("alDeleteBuffers");
        public static void alDeleteBuffers(int size, uint[] buffers) => _alDeleteBuffers(size, buffers);

        private delegate void alBufferData_T(uint buffer, ALFormat format, IntPtr data, int size, int frequence);
        private static alBufferData_T _alBufferData = LoadFunction<alBufferData_T>("alBufferData");
        public static void alBufferData(uint buffer, ALFormat format, IntPtr data, int size, int frequence) => _alBufferData(buffer, format, data, size, frequence);

        #endregion

        #region source

        private delegate void alGenSources_T(int size, out uint sources);
        private static alGenSources_T _alGenSources = LoadFunction<alGenSources_T>("alGenSources");
        public static void alGenSources(out uint sources) => _alGenSources(1, out sources);

        private delegate void alDeleteSources_T(int size, uint[] sources);
        private static alDeleteSources_T _alDeleteSources = LoadFunction<alDeleteSources_T>("alDeleteSources");
        public static void alDeleteSources(int size, uint[] sources) => _alDeleteSources(size, sources);

        #endregion

        #region source playback

        private delegate void alSourcePlay_T(uint source);
        private static alSourcePlay_T _alSourcePlay = LoadFunction<alSourcePlay_T>("alSourcePlay");
        public static void alSourcePlay(uint source) => _alSourcePlay(source);

        private delegate void alSourceStop_T(uint source);
        private static alSourceStop_T _alSourceStop = LoadFunction<alSourceStop_T>("alSourceStop");
        public static void alSourceStop(uint source) => _alSourceStop(source);

        private delegate void alSourcePause_T(uint source);
        private static alSourcePause_T _alSourcePause = LoadFunction<alSourcePause_T>("alSourcePause");
        public static void alSourcePause(uint source) => _alSourcePause(source);

        #endregion

        #region source floats

        private delegate void alSourcefv_T(uint source, int param, float[] vals);
        private static alSourcefv_T _alSourcefv = LoadFunction<alSourcefv_T>("alSourcefv");
        public static void alSourcefv(uint source, ALSource param, float[] vals) => _alSourcefv(source, (int)param, vals);

        private delegate void alGetSourcefv_T(uint source, int param, float[] vals);
        private static alGetSourcefv_T _alGetSourcefv = LoadFunction<alGetSourcefv_T>("alSourcefv");
        public static void alGetSourcefv(uint source, ALSource param, float[] vals) => _alGetSourcefv(source, (int)param, vals);

        #endregion 

        #region source ints

        private delegate void alSourceiv_T(uint source, int param, int[] value);
        private static alSourceiv_T _alSourceiv = LoadFunction<alSourceiv_T>("alSourceiv");
        public static void alSourceiv(uint source, ALSource param, int[] value) => _alSourceiv(source, (int)param, value);

        private delegate void alGetSourceiv_T(uint source, int param, int[] value);
        private static alGetSourceiv_T _alGetSourceiv = LoadFunction<alGetSourceiv_T>("alGetSourcei");
        public static void alGetSourceiv(uint source, ALSource param, int[] value) => _alGetSourceiv(source, (int)param, value);

        #endregion

        #region listener floats

        private delegate void alListenerfv_T(ALListener param, float[] vals);
        private static alListenerfv_T _alListenerfv = LoadFunction<alListenerfv_T>("alListenerfv");
        public static void alListenerfv(ALListener param, float[] vals) => _alListenerfv(param, vals);

        private delegate void alGetListenerfv_T(ALListener param, float[] vals);
        private static alGetListenerfv_T _alGetListenerfv = LoadFunction<alGetListenerfv_T>("alGetListenerfv");
        public static void alGetListenerfv(ALListener param, float[] vals) => _alGetListenerfv(param, vals);
    
        #endregion

        #region global settings

        private delegate void alDistanceModel_T(ALDistanceModel model);
        private static alDistanceModel_T _alDistanceModel = LoadFunction<alDistanceModel_T>("alDistanceModel");
        public static void alDistanceModel(ALDistanceModel model) => _alDistanceModel(model);
    
        private delegate void alSpeedOfSound_T(float val);
        private static alSpeedOfSound_T _alSpeedOfSound = LoadFunction<alSpeedOfSound_T>("alSpeedOfSound");
        public static void alSpeedOfSound(float val) => _alSpeedOfSound(val);
    
        private delegate void alDopplerFactor_T(float val);
        private static alDopplerFactor_T _alDopplerFactor = LoadFunction<alDopplerFactor_T>("alDopplerFactor");
        public static void alDopplerFactor(float val) => _alDopplerFactor(val);
    
        #endregion
    }
}