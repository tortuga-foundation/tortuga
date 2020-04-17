using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Tortuga.Utils.OpenAL
{
    internal unsafe static partial class OpenALNative
    {
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
    
        private static readonly NativeLibraryLoader.NativeLibrary _lib = LoadOpenAL();
        private static NativeLibraryLoader.NativeLibrary LoadOpenAL()
        {
            var lib = new NativeLibraryLoader.NativeLibrary(GetLibName());
            System.Console.WriteLine("Loaded Open AL");
            return lib;
        }

        private static T LoadFunction<T>(string name)
        {
            return _lib.LoadFunction<T>(name);
        }
    
        private delegate int alGetError_T();
        private static alGetError_T _alGetError = LoadFunction<alGetError_T>("alGetError");
        public static ALError alGetError() => (ALError)_alGetError();
        public static void alHandleError(string message)
        {
            var err = alGetError();
            if (err != ALError.None)
                throw new Exception(message + err.ToString());
        }

        private delegate void alGenBuffers_T1(int size, uint[] buffers);
        private static alGenBuffers_T1 _1alGenBuffers = LoadFunction<alGenBuffers_T1>("alGenBuffers");
        public static void alGenBuffers(int size, uint[] buffers) => _1alGenBuffers(size, buffers);

        private delegate void alGenBuffers_T2(int size, out uint buffers);
        private static alGenBuffers_T2 _2alGenBuffers = LoadFunction<alGenBuffers_T2>("alGenBuffers");
        public static void alGenBuffers(out uint buffers) => _2alGenBuffers(1, out buffers);

        private delegate void alDeleteBuffers_T(int size, uint[] buffers);
        private static alDeleteBuffers_T _alDeleteBuffers = LoadFunction<alDeleteBuffers_T>("alDeleteBuffers");
        public static void alDeleteBuffers(int size, uint[] buffers) => _alDeleteBuffers(size, buffers);

        private delegate void alBufferData_T(uint buffer, ALFormat format, IntPtr data, int size, int frequence);
        private static alBufferData_T _alBufferData = LoadFunction<alBufferData_T>("alBufferData");
        public static void alBufferData(uint buffer, ALFormat format, IntPtr data, int size, int frequence) => _alBufferData(buffer, format, data, size, frequence);
    
        private delegate void alGenSources_T1(int size, uint[] sources);
        private static alGenSources_T1 _1alGenSources = LoadFunction<alGenSources_T1>("alGenSources");
        public static void alGenSources(int size, uint[] sources) => _1alGenSources(size, sources);

        private delegate void alGenSources_T2(int size, out uint sources);
        private static alGenSources_T2 _2alGenSources = LoadFunction<alGenSources_T2>("alGenSources");
        public static void alGenSources(out uint sources) => _2alGenSources(1, out sources);

        private delegate void alDeleteSources_T(int size, uint[] sources);
        private static alDeleteSources_T _alDeleteSources = LoadFunction<alDeleteSources_T>("alDeleteSources");
        public static void alDeleteSources(int size, uint[] sources) => _alDeleteSources(size, sources);

        private delegate void alSourcePlay_T(uint source);
        private static alSourcePlay_T _alSourcePlay = LoadFunction<alSourcePlay_T>("alSourcePlay");
        public static void alSourcePlay(uint source) => _alSourcePlay(source);

        private delegate void alSourcei_T(uint source, ALParams param, int value);
        private static alSourcei_T _alSourcei = LoadFunction<alSourcei_T>("alSourcei");
        public static void alSourcei(uint source, ALParams param, int value) => _alSourcei(source, param, value);

        private delegate void alSourcef_T(uint source, ALParams param, float value);
        private static alSourcef_T _alSourcef = LoadFunction<alSourcef_T>("alSourcef");
        public static void alSourcef(uint source, ALParams param, float value) => _alSourcef(source, param, value);

        private delegate void alGetSourcef_T(uint source, ALParams param, out float value);
        private static alGetSourcef_T _alGetSourcef = LoadFunction<alGetSourcef_T>("alGetSourcef");
        public static void alGetSourcef(uint source, ALParams param, out float value) => _alGetSourcef(source, param, out value);

        private delegate void alGetSource3f_T(uint source, ALParams param, out float x, out float y, out float z);
        private static alGetSource3f_T _alGetSource3f = LoadFunction<alGetSource3f_T>("alGetSourcef");
        public static void alGetSource3f(uint source, ALParams param, out float x, out float y, out float z) => _alGetSource3f(source, param, out x, out y, out z);

        private delegate void alSource3f_T(uint source, ALParams param, float x, float y, float z);
        private static alSource3f_T _alSource3f = LoadFunction<alSource3f_T>("alSource3f");
        public static void alSource3f(uint source, ALParams param, float x, float y, float z) => _alSource3f(source, param, x, y, z);

        private delegate void alSourcefv_T(uint source, ALParams param, float[] vals);
        private static alSourcefv_T _alSourcefv = LoadFunction<alSourcefv_T>("alSourcefv");
        public static void alSourcefv(uint source, ALParams param, float[] vals) => _alSourcefv(source, param, vals);

        private delegate void alGetSourcefv_T(uint source, ALParams param, out float[] vals);
        private static alGetSourcefv_T _alGetSourcefv = LoadFunction<alGetSourcefv_T>("alSourcefv");
        public static void alGetSourcefv(uint source, ALParams param, out float[] vals) => _alGetSourcefv(source, param, out vals);

        private delegate void alGetSourcei_T(uint source, ALParams param, out int value);
        private static alGetSourcei_T _alGetSourcei = LoadFunction<alGetSourcei_T>("alGetSourcei");
        public static void alGetSourcei(uint source, ALParams param, out int value) => _alGetSourcei(source, param, out value);

        private delegate void alListener3f_T(ALParams param, float x, float y, float z);
        private static alListener3f_T _alListener3f = LoadFunction<alListener3f_T>("alListener3f");
        public static void alListener3f(ALParams param, float x, float y, float z) => _alListener3f(param, x, y, z);

        private delegate void alGetListener3f_T(ALParams param, out float x, out float y, out float z);
        private static alGetListener3f_T _alGetListener3f = LoadFunction<alGetListener3f_T>("alGetListener3f");
        public static void alGetListener3f(ALParams param, out float x, out float y, out float z) => _alGetListener3f(param, out x, out y, out z);

        private delegate void alListenerfv_T(ALParams param, float[] vals);
        private static alListenerfv_T _alListenerfv = LoadFunction<alListenerfv_T>("alListenerfv");
        public static void alListenerfv(ALParams param, float[] vals) => _alListenerfv(param, vals);

        private delegate void alGetListenerfv_T(ALParams param, out float[] vals);
        private static alGetListenerfv_T _alGetListenerfv = LoadFunction<alGetListenerfv_T>("alGetListenerfv");
        public static void alGetListenerfv(ALParams param, out float[] vals) => _alGetListenerfv(param, out vals);
    }
}