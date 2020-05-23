using System;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.Core;
using Tortuga.Input;

namespace Tortuga.Test
{
    public class CameraMovement : BaseSystem
    {
        private Vector2 _input = Vector2.Zero;
        private float _yaw;
        private float _pitch;
        private Vector3 _targetPosition;

        public override void OnDisable()
        {
            InputSystem.OnKeyDown -= OnKeyDown;
            InputSystem.OnKeyUp -= OnKeyUp;
            InputSystem.OnMousePositionChanged -= OnMousePositionChanged;
        }

        public override void OnEnable()
        {
            InputSystem.OnKeyDown += OnKeyDown;
            InputSystem.OnKeyUp += OnKeyUp;
            InputSystem.OnMousePositionChanged += OnMousePositionChanged;
        }

        private void OnMousePositionChanged(Vector2 mouseDelta)
        {
            if (InputSystem.IsMouseButtonDown(MouseButton.Right) == false)
                return;
            var mousePosDelta = mouseDelta * Time.DeltaTime * 0.25f;
            _yaw -= mousePosDelta.X;
            _pitch += mousePosDelta.Y;
        }

        private void OnKeyUp(KeyCode key, ModifierKeys mod)
        {
            if (InputSystem.IsMouseButtonDown(MouseButton.Right) == false)
            {
                _input = Vector2.Zero;
                return;
            }
            if (key == KeyCode.W)
                _input.Y--;
            if (key == KeyCode.S)
                _input.Y++;
            if (key == KeyCode.D)
                _input.X++;
            if (key == KeyCode.A)
                _input.X--;
        }

        private void OnKeyDown(KeyCode key, ModifierKeys mod)
        {
            if (InputSystem.IsMouseButtonDown(MouseButton.Right) == false)
            {
                _input = Vector2.Zero;
                return;
            }
            if (key == KeyCode.W)
                _input.Y++;
            if (key == KeyCode.S)
                _input.Y--;
            if (key == KeyCode.D)
                _input.X--;
            if (key == KeyCode.A)
                _input.X++;
            if (key == KeyCode.Escape)
                InputSystem.IsCursorLocked = !InputSystem.IsCursorLocked;
        }

        public override Task Update()
        {
            return Task.Run(() => 
            {
                var cameras = MyScene.GetComponents<Tortuga.Graphics.Camera>();
                foreach (var camera in cameras)
                {
                    var transform = camera.MyEntity.GetComponent<Tortuga.Core.Transform>();
                    if (transform == null)
                        continue;
                    
                    var _movement = (transform.Forward * _input.Y + transform.Right * _input.X) * Time.DeltaTime * 30.0f;
                    _targetPosition -= _movement;
                    transform.Position = Vector3.Lerp(transform.Position, _targetPosition, Time.DeltaTime * 10.0f);
                    
                    var targetRotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0.0f);
                    transform.Rotation = Quaternion.Slerp(transform.Rotation, targetRotation, Time.DeltaTime * 10.0f);
                }
            });
        }
    }
}