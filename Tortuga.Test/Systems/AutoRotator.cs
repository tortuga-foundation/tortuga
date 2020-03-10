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
        private float _multiplier = 0.05f;

        public AutoRotator()
        {
        }
        ~AutoRotator()
        {

        }

        public override void OnEnable() { }
        public override void OnDisable() { }

        public override async Task Update()
        {
            if (InputSystem.IsKeyDown(KeyCode.D))
                _left += Time.DeltaTime * _multiplier;
            else if (InputSystem.IsKeyDown(KeyCode.A))
                _left -= Time.DeltaTime * _multiplier;

            if (InputSystem.IsKeyDown(KeyCode.W))
                _top += Time.DeltaTime * _multiplier;
            else if (InputSystem.IsKeyDown(KeyCode.S))
                _top -= Time.DeltaTime * _multiplier;

            if (InputSystem.IsKeyDown(KeyCode.Up))
                _forward += Time.DeltaTime * _multiplier;
            else if (InputSystem.IsKeyDown(KeyCode.Down))
                _forward -= Time.DeltaTime * _multiplier;

            await Task.Run(() =>
            {
                var mesh = MyScene.GetComponents<Components.RenderMesh>();
                foreach (var m in mesh)
                {
                    var transform = m.MyEntity.GetComponent<Components.Transform>();
                    if (transform == null)
                        continue;

                    transform.Position = new Vector3(_left, _top, _forward);
                    transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), _rotation);
                }
            });
            _rotation += Time.DeltaTime * _multiplier * 0.2f;
            if (_rotation >= 360)
                _rotation = 0;
        }
    }
}