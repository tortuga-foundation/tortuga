using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tortuga.Core
{
    public class Entity
    {
        internal Action<Entity, BaseComponent> OnComponentAdded;
        internal Action<Entity, BaseComponent> OnComponentRemoved;

        public Dictionary<Type, BaseComponent> Components => _components;
        private Dictionary<Type, BaseComponent> _components;
        private Dictionary<Type, BaseComponent> _markedForRemoval;

        public Entity()
        {
            _components = new Dictionary<Type, BaseComponent>();
        }

        public async Task<T> AddComponent<T>() where T : BaseComponent, new()
        {
            if (_components.ContainsKey(typeof(T)))
                return null;
            var newComp = BaseComponent.Create<T>(this);
            _components.Add(typeof(T), newComp);
            OnComponentAdded?.Invoke(this, newComp);
            await newComp.OnEnable();
            return newComp;
        }

        public Task RemoveComponent<T>() where T : BaseComponent, new()
        {
            return Task.Run(() =>
            {
                if (_components.ContainsKey(typeof(T)))
                    _markedForRemoval.Add(typeof(T), _components[typeof(T)]);
            });
        }

        public async Task RemoveComponentImediate<T>() where T : BaseComponent, new()
        {
            if (_components.ContainsKey(typeof(T)))
            {
                await _components[typeof(T)].OnDisable();
                OnComponentRemoved?.Invoke(this, _components[typeof(T)]);
                _components.Remove(typeof(T));
            }
            if (_markedForRemoval.ContainsKey(typeof(T)))
                _markedForRemoval.Remove(typeof(T));
        }

        public async Task RemoveAllMarkedForRemoval()
        {
            foreach (var marked in _markedForRemoval)
            {
                if (_components.ContainsKey(marked.Key))
                {
                    await _components[marked.Key].OnDisable();
                    OnComponentRemoved?.Invoke(this, _components[marked.Key]);
                    _components.Remove(marked.Key);
                }
            }
            _markedForRemoval.Clear();
        }

        public T GetComponent<T>() where T : BaseComponent, new()
        {
            if (_components.ContainsKey(typeof(T)) == false)
                return null;

            _components.TryGetValue(typeof(T), out BaseComponent val);
            return val as T;
        }
    }
}