using System;
using System.Numerics;
using System.Collections.Generic;
using Tortuga.Utils.SDL2;
using System.Runtime.CompilerServices;
using System.Text;

namespace Tortuga.Input
{
    /// <summary>
    /// Input system for tortuga engine
    /// </summary>
    public static class InputSystem
    {
        /// <summary>
        /// This get's called every time a key is pressed down on keyboard
        /// </summary>
        public static Action<KeyCode, ModifierKeys> OnKeyDown;
        /// <summary>
        /// This get's called every time a key is released on keyboard
        /// </summary>
        public static Action<KeyCode, ModifierKeys> OnKeyUp;
        /// <summary>
        /// Triggers when a character is pressed on the keyboard
        /// </summary>
        public static Action<Char> OnTextInput;

        /// <summary>
        /// Checks if a specific key is being held right now
        /// </summary>
        /// <param name="key">The key code</param>
        /// <returns>Is the key being pressed</returns>
        public static bool IsKeyDown(KeyCode key)
            => _isKeyPressed[key];

        /// <summary>
        /// Get's called every time a mouse button is pressed down
        /// </summary>
        public static Action<MouseButton> OnMouseButtonDown;
        /// <summary>
        /// Get's called every time a mouse button is released
        /// </summary>
        public static Action<MouseButton> OnMouseButtonUp;
        /// <summary>
        /// Get's called every time the mouse position changes
        /// </summary>
        public static Action<Vector2> OnMousePositionChanged;
        /// <summary>
        /// Checks if a specific mouse button is being held down
        /// </summary>
        /// <param name="button">the mouse button</param>
        /// <returns>Is the mouse button being pressed</returns>
        public static bool IsMouseButtonDown(MouseButton button)
            => _isMouseButtonPressed[button];

        /// <summary>
        /// Current mouse position
        /// </summary>
        public static Vector2 MousePosition => _mousePosition;

        /// <summary>
        /// If mouse wheel has been scrolled
        /// </summary>
        public static Action<Vector2> OnMouseWheelChange;

        private static Vector2 _mousePosition = Vector2.Zero;
        private static Dictionary<KeyCode, bool> _isKeyPressed = new Dictionary<KeyCode, bool>();
        private static Dictionary<MouseButton, bool> _isMouseButtonPressed = new Dictionary<MouseButton, bool>();
        internal static unsafe void ProcessEvents(SDL_Event ev)
        {
            switch(ev.type)
            {
                case SDL_EventType.Quit:
                case SDL_EventType.Terminating: 
                    Engine.Instance.MainWindow.Close();
                    break;
                case SDL_EventType.WindowEvent:
                    var windowEvent = Unsafe.Read<SDL_WindowEvent>(&ev);
                    ProcessWindowEvents(windowEvent);
                    break;
                case SDL_EventType.KeyDown:
                case SDL_EventType.KeyUp:
                    var keyboardEvent = Unsafe.Read<SDL_KeyboardEvent>(&ev);
                    ProcessKeyboardEvents(keyboardEvent);
                    break;
                case SDL_EventType.TextInput:
                    var textInputEvent = Unsafe.Read<SDL_TextInputEvent>(&ev);
                    ProcessTextInputEvent(textInputEvent);
                    break;
                case SDL_EventType.MouseMotion:
                    var mouseMotionEvent = Unsafe.Read<SDL_MouseMotionEvent>(&ev);
                    ProcessMouseMotionEvent(mouseMotionEvent);
                    break;
                case SDL_EventType.MouseButtonDown:
                case SDL_EventType.MouseButtonUp:
                    var mouseButtonEvent = Unsafe.Read<SDL_MouseButtonEvent>(&ev);
                    ProcessMouseButtonEvent(mouseButtonEvent);
                    break;
                case SDL_EventType.MouseWheel:
                    var mouseWheelEvent = Unsafe.Read<SDL_MouseWheelEvent>(&ev);
                    ProcessMouseWheelEvent(mouseWheelEvent);
                    break;
                case SDL_EventType.JoyAxisMotion:
                    var joyAxiesEvent = Unsafe.Read<SDL_JoyAxisEvent>(&ev);
                    break;
                case SDL_EventType.JoyBallMotion:
                    var JoyBallMotion = Unsafe.Read<SDL_JoyBallEvent>(&ev);
                    break;
                case SDL_EventType.JoyHatMotion:
                    var joyHatMotion = Unsafe.Read<SDL_JoyHatEvent>(&ev);
                    break;
                case SDL_EventType.JoyButtonDown:
                case SDL_EventType.JoyButtonUp:
                    var joyButtonEvent = Unsafe.Read<SDL_JoyButtonEvent>(&ev);
                    break;
                case SDL_EventType.JoyDeviceAdded:
                case SDL_EventType.JoyDeviceRemoved:
                    var joyEvent = Unsafe.Read<SDL_JoyDeviceEvent>(&ev);
                    break;
                case SDL_EventType.ControllerAxisMotion:
                    var controllerAxiesEvent = Unsafe.Read<SDL_ControllerAxisEvent>(&ev);
                    break;
                case SDL_EventType.ControllerButtonDown:
                case SDL_EventType.ControllerButtonUp:
                    var controllerButtonEvent = Unsafe.Read<SDL_ControllerButtonEvent>(&ev);
                    break;
                case SDL_EventType.ControllerDeviceAdded:
                case SDL_EventType.ControllerDeviceRemoved:
                case SDL_EventType.ControllerDeviceRemapped:
                    var controllerDeviceEvent = Unsafe.Read<SDL_ControllerDeviceEvent>(&ev);
                    break;
                case SDL_EventType.FingerDown:
                case SDL_EventType.FingerUp:
                case SDL_EventType.FingerMotion:
                    var fingerMotionEvent = Unsafe.Read<SDL_TouchFingerEvent>(&ev);
                    break;
                case SDL_EventType.DollarGesture:
                    var dollarRecordEvent = Unsafe.Read<SDL_DollarGestureEvent>(&ev);
                    break;
                case SDL_EventType.DollarRecord:
                    break;
                case SDL_EventType.MultiGesture:
                    var multiGestureEvent = Unsafe.Read<SDL_MultiGestureEvent>(&ev);
                    break;
                case SDL_EventType.ClipboardUpdate:
                    break;
                case SDL_EventType.DropFile:
                case SDL_EventType.DropTest:
                case SDL_EventType.DropBegin:
                case SDL_EventType.DropComplete:
                    var dropEvent = Unsafe.Read<SDL_DropEvent>(&ev);
                    break;
                case SDL_EventType.AudioDeviceAdded:
                case SDL_EventType.AudioDeviceRemoved:
                    var audioDeviceEvent = Unsafe.Read<SDL_AudioDeviceEvent>(&ev);
                    break;
            }
        }
        internal static void Initialize()
        {
            var keys = System.Enum.GetValues(typeof(KeyCode)) as KeyCode[];
            foreach (var key in keys)
                _isKeyPressed[key] = false;
            var mouseButton = System.Enum.GetValues(typeof(MouseButton)) as MouseButton[];
            foreach (var button in mouseButton)
                _isMouseButtonPressed[button] = false;
        }

        private static void ProcessMouseWheelEvent(SDL_MouseWheelEvent ev)
        {
            OnMouseWheelChange?.Invoke(new Vector2(ev.x, ev.y));
        }

        private static void ProcessMouseButtonEvent(SDL_MouseButtonEvent ev)
        {
            var button = KeyMapper.MapMouseButton(ev.button);
            var isDown = ev.state == 1;
            if (isDown)
            {
                if (_isMouseButtonPressed[button] == false)
                {
                    OnMouseButtonDown?.Invoke(button);
                    _isMouseButtonPressed[button] = true;
                }
            }
            else
            {
                if (_isMouseButtonPressed[button])
                {
                    OnMouseButtonUp?.Invoke(button);
                    _isMouseButtonPressed[button] = false;
                }
            }
        }

        private static void ProcessMouseMotionEvent(SDL_MouseMotionEvent ev)
        {
            _mousePosition = new Vector2(ev.x, ev.y);
            OnMousePositionChanged?.Invoke(_mousePosition);
        }

        private static unsafe void ProcessTextInputEvent(SDL_TextInputEvent ev)
        {
            uint byteCount = 0;
            // Loop until the null terminator is found or the max size is reached.
            while (byteCount < SDL_TextInputEvent.MaxTextSize && ev.text[byteCount++] != 0) { }

            if (byteCount > 1)
            {
                // We don't want the null terminator.
                byteCount -= 1;
                int charCount = Encoding.UTF8.GetCharCount(ev.text, (int)byteCount);
                char* charsPtr = stackalloc char[charCount];
                Encoding.UTF8.GetChars(ev.text, (int)byteCount, charsPtr, charCount);
                for (int i = 0; i < charCount; i++)
                    OnTextInput?.Invoke(charsPtr[i]);
            }
        }

        private static void ProcessKeyboardEvents(SDL_KeyboardEvent ev)
        {
            var key = KeyMapper.Map(ev.keysym);
            var isDown = ev.state == 1;
            var modifiers = KeyMapper.MapModifiers(ev.keysym.mod);
            if (isDown)
            {
                if (_isKeyPressed[key] == false)
                {
                    OnKeyDown?.Invoke(key, modifiers);
                    _isKeyPressed[key] = true;
                }
            }
            else
            {
                if (_isKeyPressed[key])
                {
                    OnKeyUp?.Invoke(key, modifiers);
                    _isKeyPressed[key] = false;
                }
            }
        }

        private static void ProcessWindowEvents(SDL_WindowEvent ev)
        {
            switch (ev.@event)
            {
                case SDL_WindowEventID.Resized:
                    break;
                case SDL_WindowEventID.SizeChanged:
                    break;
                case SDL_WindowEventID.Minimized:
                    break;
                case SDL_WindowEventID.Maximized:
                    break;
                case SDL_WindowEventID.Restored:
                    break;
                case SDL_WindowEventID.FocusGained:
                    break;
                case SDL_WindowEventID.FocusLost:
                    break;
                case SDL_WindowEventID.Close:
                    Engine.Instance.MainWindow.Close();
                    break;
                default:
                    break;
            }
        }
    }
}