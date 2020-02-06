using System;
using System.Collections.Generic;

namespace Tortuga.Core
{
    public class Entity
    {
        internal Action<Entity, BaseComponent> OnComponentAdded;
        internal Action<Entity, BaseComponent> OnComponentRemoved;

        public Dictionary<Type, BaseComponent> Components => _components;
        private Dictionary<Type, BaseComponent> _components;

        public Entity()
        {
            _components = new Dictionary<Type, BaseComponent>();
        }

        public void AddComponent<T>() where T : BaseComponent, new()
        {
            if (_components.ContainsKey(typeof(T)))
                return;
            var newComp = BaseComponent.Create<T>(this);
            _components.Add(typeof(T), newComp);
            OnComponentAdded?.Invoke(this, newComp);
            newComp.OnEnable();
        }

        public void RemoveComponent<T>() where T : BaseComponent, new()
        {
            if (_components.ContainsKey(typeof(T)))
            {
                _components[typeof(T)].OnDisable();
                OnComponentRemoved?.Invoke(this, _components[typeof(T)]);
                _components.Remove(typeof(T));
            }
        }

        public T GetComponent<T>() where T : BaseComponent, new()
        {
            if (_components.ContainsKey(typeof(T)) == false)
                return default(T);

            _components.TryGetValue(typeof(T), out BaseComponent val);
            return val as T;
        }
    }
}