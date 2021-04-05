#pragma warning disable 1591
using System;

namespace Tortuga.Utils.SDL2
{
    /// <summary>
    /// A transparent wrapper over a pointer representing an SDL Joystick ID object.
    /// </summary>
    public struct SDL_JoystickID
    {
        public readonly IntPtr NativePointer;
        public SDL_JoystickID(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_JoystickID joystickId) => joystickId.NativePointer;
        public static implicit operator SDL_JoystickID(IntPtr pointer) => new SDL_JoystickID(pointer);

        public static bool operator ==(SDL_JoystickID left, SDL_JoystickID right)
        {
            return left.NativePointer == right.NativePointer;
        }

        public static bool operator !=(SDL_JoystickID left, SDL_JoystickID right)
        {
            return left.NativePointer != right.NativePointer;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
