using System;

namespace Tortuga.Utils.OpenAL
{
    /// <summary>
    /// A transparent wrapper over a pointer representing an ALCcontext object.
    /// </summary>
    internal struct ALCcontext
    {
        public readonly IntPtr NativePointer;
        public ALCcontext(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(ALCcontext context) => context.NativePointer;
        public static implicit operator ALCcontext(IntPtr pointer) => new ALCcontext(pointer);
    }
}