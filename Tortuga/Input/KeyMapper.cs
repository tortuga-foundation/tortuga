using Tortuga.Utils.SDL2;

namespace Tortuga.Input
{
    /// <summary>
    /// Used for mapping keys
    /// </summary>
    public static class KeyMapper
    {
        /// <summary>
        /// Map SDL keysym to Keycode key
        /// </summary>
        /// <param name="keysym">SDL Keysym</param>
        /// <returns>Keycode</returns>
        public static KeyCode Map(SDL_Keysym keysym)
        {
            switch (keysym.scancode)
            {
                case SDL_Scancode.SDL_SCANCODE_A:
                    return KeyCode.A;
                case SDL_Scancode.SDL_SCANCODE_B:
                    return KeyCode.B;
                case SDL_Scancode.SDL_SCANCODE_C:
                    return KeyCode.C;
                case SDL_Scancode.SDL_SCANCODE_D:
                    return KeyCode.D;
                case SDL_Scancode.SDL_SCANCODE_E:
                    return KeyCode.E;
                case SDL_Scancode.SDL_SCANCODE_F:
                    return KeyCode.F;
                case SDL_Scancode.SDL_SCANCODE_G:
                    return KeyCode.G;
                case SDL_Scancode.SDL_SCANCODE_H:
                    return KeyCode.H;
                case SDL_Scancode.SDL_SCANCODE_I:
                    return KeyCode.I;
                case SDL_Scancode.SDL_SCANCODE_J:
                    return KeyCode.J;
                case SDL_Scancode.SDL_SCANCODE_K:
                    return KeyCode.K;
                case SDL_Scancode.SDL_SCANCODE_L:
                    return KeyCode.L;
                case SDL_Scancode.SDL_SCANCODE_M:
                    return KeyCode.M;
                case SDL_Scancode.SDL_SCANCODE_N:
                    return KeyCode.N;
                case SDL_Scancode.SDL_SCANCODE_O:
                    return KeyCode.O;
                case SDL_Scancode.SDL_SCANCODE_P:
                    return KeyCode.P;
                case SDL_Scancode.SDL_SCANCODE_Q:
                    return KeyCode.Q;
                case SDL_Scancode.SDL_SCANCODE_R:
                    return KeyCode.R;
                case SDL_Scancode.SDL_SCANCODE_S:
                    return KeyCode.S;
                case SDL_Scancode.SDL_SCANCODE_T:
                    return KeyCode.T;
                case SDL_Scancode.SDL_SCANCODE_U:
                    return KeyCode.U;
                case SDL_Scancode.SDL_SCANCODE_V:
                    return KeyCode.V;
                case SDL_Scancode.SDL_SCANCODE_W:
                    return KeyCode.W;
                case SDL_Scancode.SDL_SCANCODE_X:
                    return KeyCode.X;
                case SDL_Scancode.SDL_SCANCODE_Y:
                    return KeyCode.Y;
                case SDL_Scancode.SDL_SCANCODE_Z:
                    return KeyCode.Z;
                case SDL_Scancode.SDL_SCANCODE_1:
                    return KeyCode.Number1;
                case SDL_Scancode.SDL_SCANCODE_2:
                    return KeyCode.Number2;
                case SDL_Scancode.SDL_SCANCODE_3:
                    return KeyCode.Number3;
                case SDL_Scancode.SDL_SCANCODE_4:
                    return KeyCode.Number4;
                case SDL_Scancode.SDL_SCANCODE_5:
                    return KeyCode.Number5;
                case SDL_Scancode.SDL_SCANCODE_6:
                    return KeyCode.Number6;
                case SDL_Scancode.SDL_SCANCODE_7:
                    return KeyCode.Number7;
                case SDL_Scancode.SDL_SCANCODE_8:
                    return KeyCode.Number8;
                case SDL_Scancode.SDL_SCANCODE_9:
                    return KeyCode.Number9;
                case SDL_Scancode.SDL_SCANCODE_0:
                    return KeyCode.Number0;
                case SDL_Scancode.SDL_SCANCODE_RETURN:
                    return KeyCode.Enter;
                case SDL_Scancode.SDL_SCANCODE_ESCAPE:
                    return KeyCode.Escape;
                case SDL_Scancode.SDL_SCANCODE_BACKSPACE:
                    return KeyCode.BackSpace;
                case SDL_Scancode.SDL_SCANCODE_TAB:
                    return KeyCode.Tab;
                case SDL_Scancode.SDL_SCANCODE_SPACE:
                    return KeyCode.Space;
                case SDL_Scancode.SDL_SCANCODE_MINUS:
                    return KeyCode.Minus;
                case SDL_Scancode.SDL_SCANCODE_EQUALS:
                    return KeyCode.Plus;
                case SDL_Scancode.SDL_SCANCODE_LEFTBRACKET:
                    return KeyCode.BracketLeft;
                case SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET:
                    return KeyCode.BracketRight;
                case SDL_Scancode.SDL_SCANCODE_BACKSLASH:
                    return KeyCode.BackSlash;
                case SDL_Scancode.SDL_SCANCODE_SEMICOLON:
                    return KeyCode.Semicolon;
                case SDL_Scancode.SDL_SCANCODE_APOSTROPHE:
                    return KeyCode.Quote;
                case SDL_Scancode.SDL_SCANCODE_GRAVE:
                    return KeyCode.Grave;
                case SDL_Scancode.SDL_SCANCODE_COMMA:
                    return KeyCode.Comma;
                case SDL_Scancode.SDL_SCANCODE_PERIOD:
                    return KeyCode.Period;
                case SDL_Scancode.SDL_SCANCODE_SLASH:
                    return KeyCode.Slash;
                case SDL_Scancode.SDL_SCANCODE_CAPSLOCK:
                    return KeyCode.CapsLock;
                case SDL_Scancode.SDL_SCANCODE_F1:
                    return KeyCode.F1;
                case SDL_Scancode.SDL_SCANCODE_F2:
                    return KeyCode.F2;
                case SDL_Scancode.SDL_SCANCODE_F3:
                    return KeyCode.F3;
                case SDL_Scancode.SDL_SCANCODE_F4:
                    return KeyCode.F4;
                case SDL_Scancode.SDL_SCANCODE_F5:
                    return KeyCode.F5;
                case SDL_Scancode.SDL_SCANCODE_F6:
                    return KeyCode.F6;
                case SDL_Scancode.SDL_SCANCODE_F7:
                    return KeyCode.F7;
                case SDL_Scancode.SDL_SCANCODE_F8:
                    return KeyCode.F8;
                case SDL_Scancode.SDL_SCANCODE_F9:
                    return KeyCode.F9;
                case SDL_Scancode.SDL_SCANCODE_F10:
                    return KeyCode.F10;
                case SDL_Scancode.SDL_SCANCODE_F11:
                    return KeyCode.F11;
                case SDL_Scancode.SDL_SCANCODE_F12:
                    return KeyCode.F12;
                case SDL_Scancode.SDL_SCANCODE_PRINTSCREEN:
                    return KeyCode.PrintScreen;
                case SDL_Scancode.SDL_SCANCODE_SCROLLLOCK:
                    return KeyCode.ScrollLock;
                case SDL_Scancode.SDL_SCANCODE_PAUSE:
                    return KeyCode.Pause;
                case SDL_Scancode.SDL_SCANCODE_INSERT:
                    return KeyCode.Insert;
                case SDL_Scancode.SDL_SCANCODE_HOME:
                    return KeyCode.Home;
                case SDL_Scancode.SDL_SCANCODE_PAGEUP:
                    return KeyCode.PageUp;
                case SDL_Scancode.SDL_SCANCODE_DELETE:
                    return KeyCode.Delete;
                case SDL_Scancode.SDL_SCANCODE_END:
                    return KeyCode.End;
                case SDL_Scancode.SDL_SCANCODE_PAGEDOWN:
                    return KeyCode.PageDown;
                case SDL_Scancode.SDL_SCANCODE_RIGHT:
                    return KeyCode.Right;
                case SDL_Scancode.SDL_SCANCODE_LEFT:
                    return KeyCode.Left;
                case SDL_Scancode.SDL_SCANCODE_DOWN:
                    return KeyCode.Down;
                case SDL_Scancode.SDL_SCANCODE_UP:
                    return KeyCode.Up;
                case SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR:
                    return KeyCode.NumLock;
                case SDL_Scancode.SDL_SCANCODE_KP_DIVIDE:
                    return KeyCode.KeypadDivide;
                case SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY:
                    return KeyCode.KeypadMultiply;
                case SDL_Scancode.SDL_SCANCODE_KP_MINUS:
                    return KeyCode.KeypadMinus;
                case SDL_Scancode.SDL_SCANCODE_KP_PLUS:
                    return KeyCode.KeypadPlus;
                case SDL_Scancode.SDL_SCANCODE_KP_ENTER:
                    return KeyCode.KeypadEnter;
                case SDL_Scancode.SDL_SCANCODE_KP_1:
                    return KeyCode.Keypad1;
                case SDL_Scancode.SDL_SCANCODE_KP_2:
                    return KeyCode.Keypad2;
                case SDL_Scancode.SDL_SCANCODE_KP_3:
                    return KeyCode.Keypad3;
                case SDL_Scancode.SDL_SCANCODE_KP_4:
                    return KeyCode.Keypad4;
                case SDL_Scancode.SDL_SCANCODE_KP_5:
                    return KeyCode.Keypad5;
                case SDL_Scancode.SDL_SCANCODE_KP_6:
                    return KeyCode.Keypad6;
                case SDL_Scancode.SDL_SCANCODE_KP_7:
                    return KeyCode.Keypad7;
                case SDL_Scancode.SDL_SCANCODE_KP_8:
                    return KeyCode.Keypad8;
                case SDL_Scancode.SDL_SCANCODE_KP_9:
                    return KeyCode.Keypad9;
                case SDL_Scancode.SDL_SCANCODE_KP_0:
                    return KeyCode.Keypad0;
                case SDL_Scancode.SDL_SCANCODE_KP_PERIOD:
                    return KeyCode.KeypadPeriod;
                case SDL_Scancode.SDL_SCANCODE_NONUSBACKSLASH:
                    return KeyCode.NonUSBackSlash;
                case SDL_Scancode.SDL_SCANCODE_KP_EQUALS:
                    return KeyCode.KeypadPlus;
                case SDL_Scancode.SDL_SCANCODE_F13:
                    return KeyCode.F13;
                case SDL_Scancode.SDL_SCANCODE_F14:
                    return KeyCode.F14;
                case SDL_Scancode.SDL_SCANCODE_F15:
                    return KeyCode.F15;
                case SDL_Scancode.SDL_SCANCODE_F16:
                    return KeyCode.F16;
                case SDL_Scancode.SDL_SCANCODE_F17:
                    return KeyCode.F17;
                case SDL_Scancode.SDL_SCANCODE_F18:
                    return KeyCode.F18;
                case SDL_Scancode.SDL_SCANCODE_F19:
                    return KeyCode.F19;
                case SDL_Scancode.SDL_SCANCODE_F20:
                    return KeyCode.F20;
                case SDL_Scancode.SDL_SCANCODE_F21:
                    return KeyCode.F21;
                case SDL_Scancode.SDL_SCANCODE_F22:
                    return KeyCode.F22;
                case SDL_Scancode.SDL_SCANCODE_F23:
                    return KeyCode.F23;
                case SDL_Scancode.SDL_SCANCODE_F24:
                    return KeyCode.F24;
                case SDL_Scancode.SDL_SCANCODE_MENU:
                    return KeyCode.Menu;
                case SDL_Scancode.SDL_SCANCODE_LCTRL:
                    return KeyCode.ControlLeft;
                case SDL_Scancode.SDL_SCANCODE_LSHIFT:
                    return KeyCode.ShiftLeft;
                case SDL_Scancode.SDL_SCANCODE_LALT:
                    return KeyCode.AltLeft;
                case SDL_Scancode.SDL_SCANCODE_RCTRL:
                    return KeyCode.ControlRight;
                case SDL_Scancode.SDL_SCANCODE_RSHIFT:
                    return KeyCode.ShiftRight;
                case SDL_Scancode.SDL_SCANCODE_RALT:
                    return KeyCode.AltRight;
                default:
                    return KeyCode.Unknown;
            }
        }
    
        /// <summary>
        /// Used for mapping modifiers
        /// </summary>
        /// <param name="mod">SDL modifier</param>
        /// <returns>Tortuga modifier</returns>
        public static ModifierKeys MapModifiers(SDL_Keymod mod)
        {
            var mods = ModifierKeys.None;
            if ((mod & (SDL_Keymod.LeftShift | SDL_Keymod.RightShift)) != 0)
                mods |= ModifierKeys.Shift;
            if ((mod & (SDL_Keymod.LeftAlt | SDL_Keymod.RightAlt)) != 0)
                mods |= ModifierKeys.Alt;
            if ((mod & (SDL_Keymod.LeftControl | SDL_Keymod.RightControl)) != 0)
                mods |= ModifierKeys.Control;
            return mods;
        }
    
        /// <summary>
        /// Maps mouse buttons to tortuga mouse buttons
        /// </summary>
        /// <param name="button">SDL mouse button</param>
        /// <returns>tortuga mouse button</returns>
        public static MouseButton MapMouseButton(SDL_MouseButton button)
        {
            switch (button)
            {
                case SDL_MouseButton.Left:
                    return MouseButton.Left;
                case SDL_MouseButton.Middle:
                    return MouseButton.Middle;
                case SDL_MouseButton.Right:
                    return MouseButton.Right;
                case SDL_MouseButton.X1:
                    return MouseButton.Button1;
                case SDL_MouseButton.X2:
                    return MouseButton.Button2;
                default:
                    return MouseButton.Left;
            }
        }
    }
}