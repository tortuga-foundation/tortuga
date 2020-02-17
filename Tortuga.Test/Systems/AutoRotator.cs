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
                var transforms = MyScene.GetComponents<Components.Transform>();
                foreach (var transform in transforms)
                    transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), _rotation);
            });
            _rotation += 0.01f;
        }
    }
}