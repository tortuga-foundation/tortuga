using System;

namespace Tortuga.SDL2
{
    /// <summary>
    /// A transparent wrapper over a pointer representing an SDL Window object.
    /// </summary>
    public struct SDL_Window
    {
        /// <summary>
        /// The native SDL_Window pointer.
        /// </summary>
        public readonly IntPtr NativePointer;

        /// <summary>
        /// Constructor for SDL2 Window
        /// </summary>
        /// <param name="pointer"></param>
        public SDL_Window(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        /// <summary>
        /// Get pointer to SDL2 Window object
        /// </summary>
        /// <param name="window">Window object</param>
        public static implicit operator IntPtr(SDL_Window window) => window.NativePointer;
        /// <summary>
        /// Convert pointer to SDL2 Window object
        /// </summary>
        /// <param name="pointer">Pointer</param>
        public static implicit operator SDL_Window(IntPtr pointer) => new SDL_Window(pointer);
    }
}
