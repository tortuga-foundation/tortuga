using System;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Tortuga.Utils.SDL2;

namespace Tortuga.Input
{
    /// <summary>
    /// Input system for tortuga engine
    /// </summary>
    public static class InputSystem
    {
        /// <summary>
        /// Get's triggered when the entire application get's asked to be closed.
        /// This includes all windows
        /// </summary>
        public static Action OnApplicationClose;

        /// <summary>
        /// Lock's the cursor 
        /// </summary>
        public static bool IsCursorLocked
        {
            get => _isCursorLocked;
            set
            {
                _isCursorLocked = value;
                Tortuga.Utils.SDL2.SDL2Native.SDL_SetRelativeMouseMode(value);
            }
        }
        private static bool _isCursorLocked = false;
        internal static unsafe void ProcessEvents(SDL_Event ev)
        {
            switch(ev.type)
            {
                case SDL_EventType.Quit:
                case SDL_EventType.Terminating:
                    OnApplicationClose?.Invoke();
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
                case SDL_EventType.JoyDeviceAdded:
                    var joyAddEvent = Unsafe.Read<SDL_JoyDeviceEvent>(&ev);
                    ProcessJoyEvent(joyAddEvent, true);
                    break;
                case SDL_EventType.JoyDeviceRemoved:
                    var joyRemoveEvent = Unsafe.Read<SDL_JoyDeviceEvent>(&ev);
                    ProcessJoyEvent(joyRemoveEvent, false);
                    break;
                case SDL_EventType.JoyAxisMotion:
                    var joyAxisEvent = Unsafe.Read<SDL_JoyAxisEvent>(&ev);
                    ProcessJoyAxisEvent(joyAxisEvent);
                    break;
                case SDL_EventType.JoyBallMotion:
                    var JoyBallMotion = Unsafe.Read<SDL_JoyBallEvent>(&ev);
                    ProcessJoyBallEvent(JoyBallMotion);
                    break;
                case SDL_EventType.JoyHatMotion:
                    var joyHatMotion = Unsafe.Read<SDL_JoyHatEvent>(&ev);
                    ProcessJoyHatEvent(joyHatMotion);
                    break;
                case SDL_EventType.JoyButtonDown:
                case SDL_EventType.JoyButtonUp:
                    var joyButtonEvent = Unsafe.Read<SDL_JoyButtonEvent>(&ev);
                    ProcessJoyButtonEvent(joyButtonEvent);
                    break;
                case SDL_EventType.ControllerAxisMotion:
                    var controllerAxisEvent = Unsafe.Read<SDL_ControllerAxisEvent>(&ev);
                    ProcessControllerAxisEvent(controllerAxisEvent);
                    break;
                case SDL_EventType.ControllerButtonDown:
                case SDL_EventType.ControllerButtonUp:
                    var controllerButtonEvent = Unsafe.Read<SDL_ControllerButtonEvent>(&ev);
                    ProcessControllerButtonEvent(controllerButtonEvent);
                    break;
                case SDL_EventType.ControllerDeviceAdded:
                    var controllerDeviceAddEvent = Unsafe.Read<SDL_ControllerDeviceEvent>(&ev);
                    ProcessControllerEvent(controllerDeviceAddEvent, true);
                    break;
                case SDL_EventType.ControllerDeviceRemoved:
                    var controllerDeviceRemoveEvent = Unsafe.Read<SDL_ControllerDeviceEvent>(&ev);
                    ProcessControllerEvent(controllerDeviceRemoveEvent, false);
                    break;
                case SDL_EventType.FingerDown:
                case SDL_EventType.FingerUp:
                case SDL_EventType.FingerMotion:
                    var fingerMotionEvent = Unsafe.Read<SDL_TouchFingerEvent>(&ev);
                    ProcessFingerMotionEvent(fingerMotionEvent);
                    break;
                case SDL_EventType.DropFile:
                case SDL_EventType.DropTest:
                case SDL_EventType.DropBegin:
                case SDL_EventType.DropComplete:
                    var dropEvent = Unsafe.Read<SDL_DropEvent>(&ev);
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

            _joysticks = new List<SDL_JoystickID>();
            _touches = new List<SDL_TouchID>();
            _fingers = new List<SDL_FingerID>();
        }

        #region Drop

        /// <summary>
        /// get's called when a user drops file in the window
        /// </summary>
        public static Action<string> OnDrop;

        private static void ProcessDropEvent(SDL_DropEvent ev)
        {
            OnDrop?.Invoke(ev.file);
        }

        #endregion

        #region Finger Motion Event

        /// <summary>
        /// Touch event data
        /// </summary>
        public struct TouchEvent
        {
            /// <summary>
            /// The identifier for this touch event
            /// </summary>
            public int TouchId;
            /// <summary>
            /// The finger identifier for this touch event
            /// </summary>
            public int FingerId;
            /// <summary>
            /// Position of the finger
            /// </summary>
            public Vector2 Position;
            /// <summary>
            /// The touch movement normalized
            /// </summary>
            public Vector2 Velocity;
            /// <summary>
            /// The amount of pressure on the touch/pad
            /// </summary>
            public float Pressure;
        }

        /// <summary>
        /// Get's called when a touch/finger motion event is trigger
        /// </summary>
        public static Action<TouchEvent> OnFingerMotion;
        /// <summary>
        /// Get's called when a finger get's pressed on touch screen/pad
        /// </summary>
        public static Action<TouchEvent> OnFingerDown;
        /// <summary>
        /// Get's called when finger is released on touch screen/pad
        /// </summary>
        public static Action<TouchEvent> OnFingerUp;

        private static List<SDL_TouchID> _touches;
        private static List<SDL_FingerID> _fingers;
        
        private static void ProcessFingerMotionEvent(SDL_TouchFingerEvent ev)
        {
            if (ev.type == SDL_EventType.FingerDown)
            {
                if (_touches.Exists((SDL_TouchID t) => t == ev.touchId) == false)
                    _touches.Add(ev.touchId);
                if (_fingers.Exists((SDL_FingerID f) => f == ev.fingerId) == false)
                    _fingers.Add(ev.fingerId);

                OnFingerDown?.Invoke(new TouchEvent()
                {
                    TouchId = _touches.FindIndex((SDL_TouchID t) => t == ev.touchId),
                    FingerId = _fingers.FindIndex((SDL_FingerID f) => f == ev.fingerId),
                    Position = new Vector2(ev.x, ev.y),
                    Velocity = new Vector2(ev.dx, ev.dy),
                    Pressure = ev.pressure
                });
            }
            else if (ev.type == SDL_EventType.FingerUp)
            {
                OnFingerUp?.Invoke(new TouchEvent(){
                    TouchId = _touches.FindIndex((SDL_TouchID t) => t == ev.touchId),
                    FingerId = _fingers.FindIndex((SDL_FingerID f) => f == ev.fingerId),
                    Position = new Vector2(ev.x, ev.y),
                    Velocity = new Vector2(ev.dx, ev.dy),
                    Pressure = ev.pressure
                });

                if (_touches.Exists((SDL_TouchID t) => t == ev.touchId))
                    _touches.Remove(ev.touchId);
                if (_fingers.Exists((SDL_FingerID f) => f == ev.fingerId))
                    _fingers.Remove(ev.fingerId);
            }
            else if (ev.type == SDL_EventType.FingerMotion)
            {
                OnFingerMotion?.Invoke(new TouchEvent(){
                    TouchId = _touches.FindIndex((SDL_TouchID t) => t == ev.touchId),
                    FingerId = _fingers.FindIndex((SDL_FingerID f) => f == ev.fingerId),
                    Position = new Vector2(ev.x, ev.y),
                    Velocity = new Vector2(ev.dx, ev.dy),
                    Pressure = ev.pressure
                });
            }
        }

        #endregion

        #region Controller Axis

        /// <summary>
        /// Controller axies event object
        /// </summary>
        public struct ControllerAxiesEvent
        {
            /// <summary>
            /// The unique identifier for the controller
            /// </summary>
            public int ControllerId;
            /// <summary>
            /// The unique identifier for the axies of the controller
            /// </summary>
            public int AxiesId;
            /// <summary>
            /// The value of the axies
            /// </summary>
            public int Value;
        }

        /// <summary>
        /// Get's called when a controller axies event is triggered
        /// </summary>
        public static Action<ControllerAxiesEvent> OnControllerAxis;

        private static void ProcessControllerAxisEvent(SDL_ControllerAxisEvent ev)
        {
            OnControllerAxis?.Invoke(new ControllerAxiesEvent()
            {
                ControllerId = ev.which,
                AxiesId = ev.axis,
                Value = ev.value
            });
        }

        #endregion

        #region Controller Button

        /// <summary>
        /// Used for controller button events
        /// </summary>
        public struct ControllerButtonEvent
        {
            /// <summary>
            /// Controller unique identifier
            /// </summary>
            public int ControllerId;
            /// <summary>
            /// Button unique identifier
            /// </summary>
            public int ButtonId;
        }

        /// <summary>
        /// Get's called when a controller button is pressed
        /// </summary>
        public static Action<ControllerButtonEvent> OnControllerButtonDown;
        /// <summary>
        /// Get's called when a controller button is released
        /// </summary>
        public static Action<ControllerButtonEvent> OnControllerButtonUp;

        private static void ProcessControllerButtonEvent(SDL_ControllerButtonEvent ev)
        {
            if (ev.state == 1)
                OnControllerButtonDown?.Invoke(new ControllerButtonEvent()
                {
                    ControllerId = ev.which,
                    ButtonId = ev.button
                });
            else
                OnControllerButtonUp?.Invoke(new ControllerButtonEvent()
                {
                    ControllerId = ev.which,
                    ButtonId = ev.button
                });
        }


        #endregion

        #region Controller Added / Removed

        /// <summary>
        /// Controller event object, used for when a controller is added or removed
        /// </summary>
        public struct ControllerEvent
        {
            /// <summary>
            /// A unique identifier for the controller
            /// </summary>
            public int ControllerId;
        }

        /// <summary>
        /// Get's called when a controller is added to the system
        /// </summary>
        public static Action<ControllerEvent> OnControllerAdded;
        /// <summary>
        /// Get's called when a controller is removed from the system
        /// </summary>
        public static Action<ControllerEvent> OnControllerRemoved;

        private static void ProcessControllerEvent(SDL_ControllerDeviceEvent ev, bool added)
        {
            if (added)
                OnControllerAdded?.Invoke(new ControllerEvent
                {
                    ControllerId = ev.which
                });
            else
                OnControllerRemoved?.Invoke(new ControllerEvent
                {
                    ControllerId = ev.which
                });
        }

        #endregion

        #region Joy Button

        /// <summary>
        /// Used for joy button events
        /// </summary>
        public struct JoyButtonEvent
        {
            /// <summary>
            /// A unique identifier for joy
            /// </summary>
            public int JoyId;
            /// <summary>
            /// A unique identifier for the button pressed
            /// </summary>
            public int ButtonId;
        }

        /// <summary>
        /// Get's called when a joy button is pressed
        /// </summary>
        public static Action<JoyButtonEvent> OnJoyButtonDown;
        /// <summary>
        /// Get's called when a joy button is released
        /// </summary>
        public static Action<JoyButtonEvent> OnJoyButtonUp;

        private static void ProcessJoyButtonEvent(SDL_JoyButtonEvent ev)
        {
            if (ev.state == 1)
                OnJoyButtonDown?.Invoke(new JoyButtonEvent()
                {
                    JoyId = _joysticks.FindIndex((SDL_JoystickID j) => j == ev.which),
                    ButtonId = ev.button
                });
            else
                OnJoyButtonUp?.Invoke(new JoyButtonEvent()
                {
                    JoyId = _joysticks.FindIndex((SDL_JoystickID j) => j == ev.which),
                    ButtonId = ev.button
                });
        }

        #endregion

        #region Joy Hat

        /// <summary>
        /// Types of hat value for joy
        /// </summary>
        public enum JoyHatType
        {
            /// <summary>
            /// Center
            /// </summary>
            Center = 0,
            /// <summary>
            /// Up
            /// </summary>
            Up = 1,
            /// <summary>
            /// Right
            /// </summary>
            Right = 2,
            /// <summary>
            /// Down
            /// </summary>
            Down = 4,
            /// <summary>
            /// Left
            /// </summary>
            Left = 8,
            /// <summary>
            /// RightUp
            /// </summary>
            RightUp = (JoyHatType.Right | JoyHatType.Up),
            /// <summary>
            /// LeftUp
            /// </summary>
            LeftUp = (JoyHatType.Left | JoyHatType.Up),
            /// <summary>
            /// LeftDown
            /// </summary>
            LeftDown = (JoyHatType.Left | JoyHatType.Down),
            /// <summary>
            /// RightDown
            /// </summary>
            RightDown = (JoyHatType.Right | JoyHatType.Down)
        }

        /// <summary>
        /// Used for joy hat events
        /// </summary>
        public struct JoyHatEvent
        {
            /// <summary>
            /// Unique identifier for joy
            /// </summary>
            public int JoyId;
            /// <summary>
            /// Unique hat identifier
            /// </summary>
            public int HatId;
            /// <summary>
            /// current value
            /// </summary>
            public JoyHatType Value;
        }

        /// <summary>
        /// Get's called when a joy hat event is triggered
        /// </summary>
        public static Action<JoyHatEvent> OnJoyHat;

        private static void ProcessJoyHatEvent(SDL_JoyHatEvent ev)
        {
            OnJoyHat?.Invoke(new JoyHatEvent()
            {
                JoyId = _joysticks.FindIndex((SDL_JoystickID j) => j == ev.which),
                HatId = ev.hat,
                Value = (JoyHatType)ev.value
            });
        }

        #endregion

        #region Joy Ball

        /// <summary>
        /// Used in joy ball event
        /// </summary>
        public struct JoyBallEvent
        {
            /// <summary>
            /// Unique identifier for joy
            /// </summary>
            public int JoyId;
            /// <summary>
            /// Unique identifier for joy ball
            /// </summary>
            public int BallId;
            /// <summary>
            /// The motion made with the joy ball
            /// </summary>
            public Vector2 Motion;
        }

        /// <summary>
        /// Get's called when joy ball event is triggered
        /// </summary>
        public static Action<JoyBallEvent> OnJoyBall;

        private static void ProcessJoyBallEvent(SDL_JoyBallEvent ev)
        {
            OnJoyBall?.Invoke(new JoyBallEvent()
            {
                JoyId = _joysticks.FindIndex((SDL_JoystickID j) => j == ev.which),
                BallId = ev.ball,
                Motion = new Vector2(ev.xrel, ev.yrel)
            });
        }

        #endregion

        #region Joy Added / Removed

        /// <summary>
        /// Used for joy events when joy is added or removed
        /// </summary>
        public struct JoyEvent
        {
            /// <summary>
            /// Unique identifier for joy
            /// </summary>
            public int JoyId;
        }

        /// <summary>
        /// Get's called when joytick is added to the system
        /// </summary>
        public static Action<JoyEvent> OnJoyAdded;
        /// <summary>
        /// Get's called when joystick get's removed from the system
        /// </summary>
        public static Action<JoyEvent> OnJoyRemove;

        private static List<SDL_JoystickID> _joysticks;

        private static void ProcessJoyEvent(SDL_JoyDeviceEvent ev, bool added)
        {
            if (added)
            {
                _joysticks.Add(ev.which);
                OnJoyAdded?.Invoke(new JoyEvent()
                {
                    JoyId = _joysticks.FindIndex((SDL_JoystickID j) => ev.which == j)
                });
            }
            else
            {
                OnJoyRemove?.Invoke(new JoyEvent()
                {
                    JoyId = _joysticks.FindIndex((SDL_JoystickID j) => ev.which == j)
                });
                _joysticks.Remove(ev.which);
            }
        }

        #endregion

        #region Joy Axis

        /// <summary>
        /// Used whena joy axies event is called
        /// </summary>
        public struct JoyAxiesEvent
        {
            /// <summary>
            /// Unique joy identifier
            /// </summary>
            public int JoyId;
            /// <summary>
            /// Unique axies identifier for joy
            /// </summary>
            public int AxisId;
            /// <summary>
            /// Current value of the axies
            /// </summary>
            public int Value;
        }

        /// <summary>
        /// Get's called when an Axis get's changed on a joystick
        /// </summary>
        public static Action<JoyAxiesEvent> OnJoyAxis;

        private static void ProcessJoyAxisEvent(SDL_JoyAxisEvent ev)
        {
            OnJoyAxis?.Invoke(new JoyAxiesEvent()
            {
                JoyId = _joysticks.FindIndex((SDL_JoystickID j) => ev.which == j),
                AxisId = ev.axis,
                Value = ev.value
            });
        }

        #endregion

        #region Mouse Wheel

        /// <summary>
        /// If mouse wheel has been scrolled
        /// </summary>
        public static Action<Vector2> OnMouseWheelChange;

        private static void ProcessMouseWheelEvent(SDL_MouseWheelEvent ev)
        {
            OnMouseWheelChange?.Invoke(new Vector2(ev.x, ev.y));
        }

        #endregion

        #region Mouse Buttons

        /// <summary>
        /// Get's called every time a mouse button is pressed down
        /// </summary>
        public static Action<MouseButton> OnMouseButtonDown;
        /// <summary>
        /// Get's called every time a mouse button is released
        /// </summary>
        public static Action<MouseButton> OnMouseButtonUp;

        /// <summary>
        /// Checks if a specific mouse button is being held down
        /// </summary>
        /// <param name="button">the mouse button</param>
        /// <returns>Is the mouse button being pressed</returns>
        public static bool IsMouseButtonDown(MouseButton button)
            => _isMouseButtonPressed[button];

        private static Dictionary<MouseButton, bool> _isMouseButtonPressed = new Dictionary<MouseButton, bool>();

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

        #endregion

        #region Mouse Position

        /// <summary>
        /// Get's called every time the mouse position changes
        /// </summary>
        public static Action<Vector2> OnMousePositionChanged;
        /// <summary>
        /// Current mouse position
        /// </summary>
        public static Vector2 MousePosition => _mousePosition;
        private static Vector2 _mousePosition = Vector2.Zero;

        private static void ProcessMouseMotionEvent(SDL_MouseMotionEvent ev)
        {
            _mousePosition = new Vector2(ev.x, ev.y);
            OnMousePositionChanged?.Invoke(new Vector2(ev.xrel, ev.yrel));
        }

        #endregion

        #region Text Input
        
        /// <summary>
        /// Triggers when a character is pressed on the keyboard
        /// </summary>
        public static Action<Char> OnTextInput;

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

        #endregion

        #region Keyboard

        /// <summary>
        /// This get's called every time a key is pressed down on keyboard
        /// </summary>
        public static Action<KeyCode, ModifierKeys> OnKeyDown;
        /// <summary>
        /// This get's called every time a key is released on keyboard
        /// </summary>
        public static Action<KeyCode, ModifierKeys> OnKeyUp;
        
        /// <summary>
        /// Checks if a specific key is being held right now
        /// </summary>
        /// <param name="key">The key code</param>
        /// <returns>Is the key being pressed</returns>
        public static bool IsKeyDown(KeyCode key)
            => _isKeyPressed[key];

        private static Dictionary<KeyCode, bool> _isKeyPressed = new Dictionary<KeyCode, bool>();

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
        
        #endregion

        #region Window

        /// <summary>
        /// Get's called when the window is resized by the user
        /// </summary>
        public static Action<uint, Vector2> OnWindowResized;

        /// <summary>
        /// Get's called when the window size changes 
        /// </summary>
        public static Action<uint, Vector2> OnWindowSizeChanged;

        /// <summary>
        /// Get's called when window is minimized
        /// </summary>
        public static Action<uint> OnWindowMinimized;

        /// <summary>
        /// Get's called when window is maximized
        /// </summary>
        public static Action<uint> OnWindowMaximized;

        /// <summary>
        /// Get's called when a window is restored
        /// </summary>
        public static Action<uint> OnWindowRestored;

        /// <summary>
        /// Get's called when a window gains or loses focus
        /// </summary>
        public static Action<uint, bool> OnWindowFocus;

        /// <summary>
        /// Get's called when a window is closed
        /// </summary>
        public static Action<uint> OnWindowClose;

        private static void ProcessWindowEvents(SDL_WindowEvent ev)
        {
            switch (ev.@event)
            {
                case SDL_WindowEventID.Resized:
                    OnWindowResized?.Invoke(ev.windowID, new Vector2(ev.data1, ev.data2));
                    break;
                case SDL_WindowEventID.SizeChanged:
                    OnWindowSizeChanged?.Invoke(ev.windowID, new Vector2(ev.data1, ev.data2));
                    break;
                case SDL_WindowEventID.Minimized:
                    OnWindowMinimized?.Invoke(ev.windowID);
                    break;
                case SDL_WindowEventID.Maximized:
                    OnWindowMaximized?.Invoke(ev.windowID);
                    break;
                case SDL_WindowEventID.Restored:
                    OnWindowRestored?.Invoke(ev.windowID);
                    break;
                case SDL_WindowEventID.FocusGained:
                    OnWindowFocus?.Invoke(ev.windowID, true);
                    break;
                case SDL_WindowEventID.FocusLost:
                    OnWindowFocus?.Invoke(ev.windowID, false);
                    break;
                case SDL_WindowEventID.Close:
                    OnWindowClose?.Invoke(ev.windowID);
                    break;
                default:
                    break;
            }
        }
    
        #endregion
    }
}