using System.Threading.Tasks;

namespace Tortuga.Core
{
    public class BaseComponent
    {
        public Entity MyEntity => _myEntity;
        private Entity _myEntity;

        public virtual void Update() { }
        public virtual async Task OnEnable() { await Task.Run(() => { }); }
        public virtual async Task OnDisable() { await Task.Run(() => { }); }

        public static T Create<T>(Entity e) where T : BaseComponent, new()
        {
            var t = new T();
            t._myEntity = e;
            return t;
        }
    }
}