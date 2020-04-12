#pragma warning disable 1591
using System;

namespace Tortuga.SDL2
{
    /// <summary>
    /// A transparent wrapper over a pointer representing an SDL Touch ID object.
    /// </summary>
    public struct SDL_TouchID
    {
        public readonly IntPtr NativePointer;
        public SDL_TouchID(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_TouchID touchId) => touchId.NativePointer;
        public static implicit operator SDL_TouchID(IntPtr pointer) => new SDL_TouchID(pointer);
    }

    /// <summary>
    /// A transparent wrapper over a pointer representing an SDL Finger ID object.
    /// </summary>
    public struct SDL_FingerID
    {
        public readonly IntPtr NativePointer;
        public SDL_FingerID(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_FingerID fingerId) => fingerId.NativePointer;
        public static implicit operator SDL_FingerID(IntPtr pointer) => new SDL_FingerID(pointer);
    }

    /// <summary>
    /// A transparent wrapper over a pointer representing an SDL Gesture ID object.
    /// </summary>
    public struct SDL_GestureID
    {
        public readonly IntPtr NativePointer;
        public SDL_GestureID(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_GestureID gestureId) => gestureId.NativePointer;
        public static implicit operator SDL_GestureID(IntPtr pointer) => new SDL_GestureID(pointer);
    }
}
