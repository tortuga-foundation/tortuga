using System;

namespace Tortuga.Utils.OpenAL
{
    /// <summary>
    /// A transparent wrapper over a pointer representing an ALC device object.
    /// </summary>
    internal struct ALCdevice
    {
        public readonly IntPtr NativePointer;
        public ALCdevice(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(ALCdevice device) => device.NativePointer;
        public static implicit operator ALCdevice(IntPtr pointer) => new ALCdevice(pointer);
    }
}