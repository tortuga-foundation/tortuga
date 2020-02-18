using System;
using System.Numerics;
using System.Collections.Generic;

namespace Tortuga.Input
{
    public static class Input
    {
        public static Action<KeyCode> OnKeyDown;
        public static Action<KeyCode> OnKeyUp;
        public static bool IsKeyDown(KeyCode key)
            => _isKeyPressed[key];
        public static Action<MouseButton> OnMouseButtonDown;
        public static Action<MouseButton> OnMouseButtonUp;
        public static bool IsMouseButtonDown(MouseButton button)
            => _isMouseButtonPressed[button];
        public static Vector2 MousePosition => _mousePosition;
        public static Action<float> OnWheelDeltaChange;
        public static float WheelDelta => _wheelDelta;


        private static Vector2 _mousePosition = Vector2.Zero;
        private static float _wheelDelta = 0.0f;
        private static Dictionary<KeyCode, bool> _isKeyPressed = new Dictionary<KeyCode, bool>();
        private static Dictionary<MouseButton, bool> _isMouseButtonPressed = new Dictionary<MouseButton, bool>();
        internal static void ProcessEvents(Veldrid.InputSnapshot snapshot)
        {
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
            _mousePosition = new Vector2(
                snapshot.MousePosition.X,
                snapshot.MousePosition.Y
            );
            if (snapshot.WheelDelta != 0)
                OnWheelDeltaChange?.Invoke(snapshot.WheelDelta);
            _wheelDelta = snapshot.WheelDelta;
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