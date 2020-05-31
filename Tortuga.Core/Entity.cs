using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tortuga.Core
{
    /// <summary>
    /// Entity class used to create objects in the tortuga engine
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// A name identifier for the entity
        /// </summary>
        public string Name;
        internal Action<Entity, BaseComponent> OnComponentAdded;
        internal Action<Entity, BaseComponent> OnComponentRemoved;

        /// <summary>
        /// Get all components attached to this entity
        /// </summary>
        public Dictionary<Type, BaseComponent> Components => _components;
        private Dictionary<Type, BaseComponent> _components;
        private Dictionary<Type, BaseComponent> _markedForRemoval;

        /// <summary>
        /// Entity constructor
        /// </summary>
        public Entity()
        {
            Name = "My Entity";
            _components = new Dictionary<Type, BaseComponent>();
            _markedForRemoval = new Dictionary<Type, BaseComponent>();
            this.AddComponent<Core.Transform>().Wait();
        }

        /// <summary>
        /// Add a component to this entity
        /// </summary>
        /// <typeparam name="T">type of component</typeparam>
        /// <returns>async task with the component as a result</returns>
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

        /// <summary>
        /// Mark a component for removal in this entity
        /// </summary>
        /// <typeparam name="T">type of component</typeparam>
        public Task RemoveComponent<T>() where T : BaseComponent, new()
        {
            return Task.Run(() =>
            {
                if (_components.ContainsKey(typeof(T)))
                    _markedForRemoval.Add(typeof(T), _components[typeof(T)]);
            });
        }

        /// <summary>
        /// Remove a component from this entity imediately
        /// </summary>
        /// <typeparam name="T">type of component</typeparam>
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

        /// <summary>
        /// Remove all components marked for removal
        /// </summary>
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

        /// <summary>
        /// get an component attached to this entity
        /// </summary>
        /// <typeparam name="T">type of component</typeparam>
        /// <returns>component instance</returns>
        public T GetComponent<T>() where T : BaseComponent, new()
        {
            if (_components.ContainsKey(typeof(T)) == false)
                return null;

            _components.TryGetValue(typeof(T), out BaseComponent val);
            return val as T;
        }
    }
}