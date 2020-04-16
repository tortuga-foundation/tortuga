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
        public static void alSource3f(uint source, ALParams param, Vector3 vec) => _alSource3f(source, param, vec.X, vec.Y, vec.Z);

        private delegate void alGetSourcei_T(uint source, ALParams param, out int value);
        private static alGetSourcei_T _alGetSourcei = LoadFunction<alGetSourcei_T>("alGetSourcei");
        public static void alGetSourcei(uint source, ALParams param, out int value) => _alGetSourcei(source, param, out value);

        private delegate void alListener3f_T(ALParams param, float x, float y, float z);
        private static alListener3f_T _alListener3f = LoadFunction<alListener3f_T>("alListener3f");
        public static void alListener3f(ALParams param, Vector3 vec) => _alListener3f(param, vec.X, vec.Y, vec.Z);

        private delegate void alGetListener3f_T(ALParams param, out float x, out float y, out float z);
        private static alGetListener3f_T _alGetListener3f = LoadFunction<alGetListener3f_T>("alGetListener3f");
        public static void alGetListener3f(ALParams param, out float x, out float y, out float z) => _alGetListener3f(param, out x, out y, out z);
    }
}