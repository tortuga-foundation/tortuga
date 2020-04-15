using System;
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
        public static int alGetError() => _alGetError();

        private delegate void alGenBuffers_T1(int size, uint[] buffers);
        private static alGenBuffers_T1 _1alGenBuffers = LoadFunction<alGenBuffers_T1>("alGenBuffers");
        public static void alGenBuffers(int size, uint[] buffers) => _1alGenBuffers(size, buffers);

        private delegate void alGenBuffers_T2(int size, out uint buffers);
        private static alGenBuffers_T2 _2alGenBuffers = LoadFunction<alGenBuffers_T2>("alGenBuffers");
        public static void alGenBuffers(int size, out uint buffers) => _2alGenBuffers(size, out buffers);

        private delegate void alBufferData_T(uint buffer, ALFormat format, IntPtr data, int size, int frequence);
        private static alBufferData_T _alBufferData = LoadFunction<alBufferData_T>("alBufferData");
        public static void alBufferData(uint buffer, ALFormat format, IntPtr data, int size, int frequence) => _alBufferData(buffer, format, data, size, frequence);
    
        private delegate void alGenSources_T1(int size, uint[] sources);
        private static alGenSources_T1 _1alGenSources = LoadFunction<alGenSources_T1>("alGenSources");
        public static void alGenSources(int size, uint[] sources) => _1alGenSources(size, sources);

        private delegate void alGenSources_T2(int size, out uint sources);
        private static alGenSources_T2 _2alGenSources = LoadFunction<alGenSources_T2>("alGenSources");
        public static void alGenSources(int size, out uint sources) => _2alGenSources(size, out sources);

        private delegate void alSourcei_T(uint source, ALParams param, int value);
        private static alSourcei_T _alSourcei = LoadFunction<alSourcei_T>("alSourcei");
        public static void alSourcei(uint source, ALParams param, int value) => _alSourcei(source, param, value);

        private delegate void alSourcePlay_T(uint source);
        private static alSourcePlay_T _alSourcePlay = LoadFunction<alSourcePlay_T>("alSourcePlay");
        public static void alSourcePlay(uint source) => _alSourcePlay(source);
    }
}