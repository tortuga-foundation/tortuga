#pragma warning disable 1591
using System;

namespace Tortuga.SDL2
{
    /// <summary>
    /// A transparent wrapper over a pointer representing an SDL Window object.
    /// </summary>
    public struct SDL_Window
    {
        public readonly IntPtr NativePointer;
        public SDL_Window(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_Window window) => window.NativePointer;
        public static implicit operator SDL_Window(IntPtr pointer) => new SDL_Window(pointer);
    }
}
