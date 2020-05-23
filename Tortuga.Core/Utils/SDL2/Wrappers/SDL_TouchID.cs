#pragma warning disable 1591
using System;

namespace Tortuga.Utils.SDL2
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
    
        public static bool operator ==(SDL_TouchID left, SDL_TouchID right)
        {
            return left.NativePointer == right.NativePointer;
        }

        public static bool operator !=(SDL_TouchID left, SDL_TouchID right)
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
    
        public static bool operator ==(SDL_FingerID left, SDL_FingerID right)
        {
            return left.NativePointer == right.NativePointer;
        }

        public static bool operator !=(SDL_FingerID left, SDL_FingerID right)
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
    
        public static bool operator ==(SDL_GestureID left, SDL_GestureID right)
        {
            return left.NativePointer == right.NativePointer;
        }

        public static bool operator !=(SDL_GestureID left, SDL_GestureID right)
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
