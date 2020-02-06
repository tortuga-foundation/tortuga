
namespace Tortuga.Core
{
    public abstract class BaseComponent
    {
        public Entity MyEntity => _myEntity;
        private Entity _myEntity;

        public abstract void Update();
        public abstract void OnEnable();
        public abstract void OnDisable();

        public static T Create<T>(Entity e) where T : BaseComponent, new()
        {
            var t = new T();
            t._myEntity = e;
            return t;
        }
    }
}