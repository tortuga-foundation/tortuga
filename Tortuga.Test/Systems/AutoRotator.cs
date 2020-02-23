using System.Threading.Tasks;
using Tortuga.Core;
using System.Numerics;

namespace Tortuga.Test
{
    public class AutoRotator : BaseSystem
    {
        private float _rotation = 0.0f;

        public override async Task Update()
        {
            await Task.Run(() =>
            {
                var mesh = MyScene.GetComponents<Components.Mesh>();
                foreach (var m in mesh)
                {
                    var transform = m.MyEntity.GetComponent<Components.Transform>();
                    if (transform != null)
                        transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), _rotation);
                }
            });
            _rotation += 0.001f;
        }
    }
}