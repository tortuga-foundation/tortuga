using System;
using System.Runtime.InteropServices;

namespace Tortuga.Utils.BulletPhysics
{
    internal unsafe static partial class BulletPhysicsLoader
    {
        public static string[] GetLibName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new string[]{
                    "bullet-static.dll"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new string[]{
                    "bullet-static.so"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new string[]{
                    "bullet-static.dylib",
                };
            }
            else
            {
                return new string[]{
                    "bullet-static"
                };
            }
        }
    
        private static readonly NativeLibraryLoader.NativeLibrary _lib = _loadBulletPhysics();
        private static NativeLibraryLoader.NativeLibrary _loadBulletPhysics()
        {
            var lib = new NativeLibraryLoader.NativeLibrary(GetLibName());
            System.Console.WriteLine("Loaded Bullet Physics");
            return lib;
        }

        public static T LoadFunction<T>(string name)
        {
            return _lib.LoadFunction<T>(name);
        }
    }
}