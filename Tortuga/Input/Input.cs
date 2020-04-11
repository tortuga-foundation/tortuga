using System;
using System.Numerics;
using System.Collections.Generic;

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
        public static Action<KeyCode> OnKeyDown;
        /// <summary>
        /// This get's called every time a key is released on keyboard
        /// </summary>
        public static Action<KeyCode> OnKeyUp;
        /// <summary>
        /// Triggers when a character is pressed on the keyboard
        /// </summary>
        public static Action<Char> OnCharacterPress;

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
        public static Action<float> OnWheelDeltaChange;

        private static Vector2 _mousePosition = Vector2.Zero;
        private static float _wheelDelta = 0.0f;
        private static Dictionary<KeyCode, bool> _isKeyPressed = new Dictionary<KeyCode, bool>();
        private static Dictionary<MouseButton, bool> _isMouseButtonPressed = new Dictionary<MouseButton, bool>();
        internal static void ProcessEvents(SDL2.SDL_Event ev)
        {
            /*
            foreach (var c in snapshot.KeyCharPresses)
                OnCharacterPress?.Invoke(c);
            foreach (var keyEvent in snapshot.KeyEvents)
            {
                var key = (KeyCode)keyEvent.Key;
                if (keyEvent.Down)
                {
                    if (_isKeyPressed[key] == false)
                    {
                        _isKeyPressed[key] = true;
                        OnKeyDown?.Invoke(key);
                    }
                }
                else
                {
                    _isKeyPressed[key] = false;
                    OnKeyUp?.Invoke(key);
                }
            }
            foreach (var mouseEvent in snapshot.MouseEvents)
            {
                var button = (MouseButton)mouseEvent.MouseButton;
                if (mouseEvent.Down)
                {
                    if (_isMouseButtonPressed[button] == false)
                    {
                        _isMouseButtonPressed[button] = true;
                        OnMouseButtonDown?.Invoke(button);
                    }
                }
                else
                {
                    _isMouseButtonPressed[button] = false;
                    OnMouseButtonUp?.Invoke(button);
                }
            }
            var mousePosition = new Vector2(
                snapshot.MousePosition.X,
                snapshot.MousePosition.Y
            );
            if (mousePosition != _mousePosition)
            {
                _mousePosition = mousePosition;
                OnMousePositionChanged?.Invoke(_mousePosition);
            }
            if (snapshot.WheelDelta != 0)
                OnWheelDeltaChange?.Invoke(snapshot.WheelDelta);
            _wheelDelta = snapshot.WheelDelta;
            */
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
    }
}