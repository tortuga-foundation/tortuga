using System.Threading.Tasks;
using Tortuga.Core;
using System.Numerics;
using Tortuga.Input;

namespace Tortuga.Test
{
    public class LightMovement : BaseSystem
    {
        private float _left = 0;


        public override void OnEnable() { }
        public override void OnDisable() { }
        public override async Task Update()
        {
            if (InputSystem.IsKeyDown(KeyCode.Right))
                _left += 0.001f;
            else if (InputSystem.IsKeyDown(KeyCode.Left))
                _left -= 0.001f;

            await Task.Run(() =>
            {
                var light = MyScene.GetComponents<Components.Light>();
                foreach (var l in light)
                {
                    var transform = l.MyEntity.GetComponent<Components.Transform>();
                    if (transform == null)
                        continue;

                    transform.Position = new Vector3(_left, 0, 0);
                }
            });
        }
    }
}