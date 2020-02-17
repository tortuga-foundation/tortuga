using System.Threading.Tasks;

namespace Tortuga.Core
{
    public abstract class BaseSystem
    {
        public Scene MyScene => _scene;
        private Scene _scene;

        public BaseSystem() { }
        public abstract Task Update();

        public static T Create<T>(Scene scene) where T : BaseSystem, new()
        {
            var t = new T();
            t._scene = scene;
            return t;
        }
    }
}