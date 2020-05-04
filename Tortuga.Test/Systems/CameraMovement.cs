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
        private Vector2 _mousePosition = Vector2.Zero;
        private Vector2 _movement = Vector2.Zero;
        private float _yaw;
        private float _pitch;

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
            _mousePosition = InputSystem.MousePosition;
            Console.WriteLine(Matrix4x4.CreateTranslation(new Vector3(2, 3, 4)));
        }

        private void OnMousePositionChanged(Vector2 mouseDelta)
        {
            var mousePosDelta = mouseDelta * Time.DeltaTime * 0.001f;
            _yaw += mousePosDelta.X;
            _pitch -= mousePosDelta.Y;

            var cameras = MyScene.GetComponents<Tortuga.Components.Camera>();
                foreach (var camera in cameras)
                {
                    var transform = camera.MyEntity.GetComponent<Tortuga.Components.Transform>();
                    if (transform == null)
                        continue;
                    
                    transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), _yaw);
                }
        }

        private void OnKeyUp(KeyCode key, ModifierKeys mod)
        {
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
            InputSystem.IsCursorLocked = true;
            return Task.Run(() => 
            {
                _movement += _input * Time.DeltaTime * 0.1f;

                var cameras = MyScene.GetComponents<Tortuga.Components.Camera>();
                foreach (var camera in cameras)
                {
                    var transform = camera.MyEntity.GetComponent<Tortuga.Components.Transform>();
                    if (transform == null)
                        continue;
                    
                    transform.Position = (-transform.Forward *  _movement.Y) + (transform.Right * _movement.X);
                }
            });
        }
    }
}