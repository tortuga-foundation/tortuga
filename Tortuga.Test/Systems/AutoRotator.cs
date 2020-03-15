using System.Threading.Tasks;
using Tortuga.Core;
using System.Numerics;
using Tortuga.Input;

namespace Tortuga.Test
{
    public class AutoRotator : BaseSystem
    {
        private float _rotation = 0.0f;
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
            await Task.Run(() =>
            {
                var mesh = MyScene.GetComponents<Components.RenderMesh>();
                foreach (var m in mesh)
                {
                    var transform = m.MyEntity.GetComponent<Components.Transform>();
                    if (transform == null)
                        continue;

                    var left = transform.Position.X;
                    var top = transform.Position.Y;
                    var forward = transform.Position.Z;

                    if (InputSystem.IsKeyDown(KeyCode.D))
                        left += Time.DeltaTime * _multiplier;
                    else if (InputSystem.IsKeyDown(KeyCode.A))
                        left -= Time.DeltaTime * _multiplier;

                    if (InputSystem.IsKeyDown(KeyCode.W))
                        top += Time.DeltaTime * _multiplier;
                    else if (InputSystem.IsKeyDown(KeyCode.S))
                        top -= Time.DeltaTime * _multiplier;

                    if (InputSystem.IsKeyDown(KeyCode.Up))
                        forward += Time.DeltaTime * _multiplier;
                    else if (InputSystem.IsKeyDown(KeyCode.Down))
                        forward -= Time.DeltaTime * _multiplier;

                    transform.Position = new Vector3(left, top, forward);
                    transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), _rotation);
                }
            });
            _rotation += Time.DeltaTime * _multiplier * 0.2f;
            if (_rotation >= 360)
                _rotation = 0;
        }
    }
}