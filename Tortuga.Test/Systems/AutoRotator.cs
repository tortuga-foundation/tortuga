using System.Threading.Tasks;
using Tortuga.Core;
using System.Numerics;
using Tortuga.Input;

namespace Tortuga.Test
{
    public class AutoRotator : BaseSystem
    {
        private float _rotation = 0.0f;
        private float _left = 0;
        private float _top = 0;
        private float _forward = -10.0f;

        public AutoRotator()
        {
        }
        ~AutoRotator()
        {

        }

        public override async Task Update()
        {
            if (InputSystem.IsKeyDown(KeyCode.D))
                _left += 0.1f;
            else if (InputSystem.IsKeyDown(KeyCode.A))
                _left -= 0.1f;

            if (InputSystem.IsKeyDown(KeyCode.W))
                _top += 0.1f;
            else if (InputSystem.IsKeyDown(KeyCode.S))
                _top -= 0.1f;
            
            if (InputSystem.IsKeyDown(KeyCode.Up))
                _forward += 0.1f;
            else if (InputSystem.IsKeyDown(KeyCode.Down))
                _forward -= 0.1f;

            await Task.Run(() =>
            {
                var mesh = MyScene.GetComponents<Components.Mesh>();
                foreach (var m in mesh)
                {
                    var transform = m.MyEntity.GetComponent<Components.Transform>();
                    if (transform == null)
                        continue;

                    transform.Position = new Vector3(_left, _top, _forward);
                    transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), _rotation);
                }
            });
            _rotation += 0.001f;
        }
    }
}