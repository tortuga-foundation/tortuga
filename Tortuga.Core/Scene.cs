using System;
using System.Collections.Generic;

namespace Tortuga.Core
{
    /// <summary>
    /// Scene that is used to store all entities, components and systems currently active
    /// </summary>
    public partial class Scene
    {
        /// <summary>
        /// All the entities in the scene
        /// </summary>
        public List<Entity> Entities => _entities;
        /// <summary>
        /// All the components in the scene
        /// </summary>
        public Dictionary<Type, BaseComponent[]> EntitiesWithComponent => _components;
        /// <summary>
        /// All the systems in the scene
        /// </summary>
        public Dictionary<Type, BaseSystem> Systems => _systems;

        private List<Entity> _entities;
        private Dictionary<Type, BaseComponent[]> _components;
        private Dictionary<Type, List<BaseComponent>> _componentsList;

        private Dictionary<Type, BaseSystem> _systems;

        /// <summary>
        /// initialize a new scene object
        /// </summary>
        public Scene()
        {
            _entities = new List<Entity>();
            _components = new Dictionary<Type, BaseComponent[]>();
            _componentsList = new Dictionary<Type, List<BaseComponent>>();
            _systems = new Dictionary<Type, BaseSystem>();
        }

        private void OnComponentAdded<T>(Entity entity, T component)
        {
            if (_componentsList.ContainsKey(typeof(T)) == false)
                _componentsList[typeof(T)] = new List<BaseComponent>();

            _componentsList[typeof(T)].Add(entity.Components[typeof(T)]);
            UpdateComponentsArray();
        }
        private void OnComponentRemoved<T>(Entity entity, T component)
        {
            if (_componentsList.ContainsKey(typeof(T)) == false)
                return;

            _componentsList[typeof(T)].Remove(entity.Components[typeof(T)]);
            UpdateComponentsArray();
        }
        private void UpdateComponentsArray()
        {
            _components = new Dictionary<Type, BaseComponent[]>();
            foreach (var key in _componentsList.Keys)
                _components[key] = _componentsList[key].ToArray();
        }

        /// <summary>
        /// Add an entity to the scene
        /// </summary>
        /// <param name="e">entity object</param>
        public void AddEntity(Entity e)
        {
            _entities.Add(e);
            foreach (var comp in e.Components)
            {
                if (_componentsList.ContainsKey(comp.Key) == false)
                    _componentsList[comp.Key] = new List<BaseComponent>();

                _componentsList[comp.Key].Add(e.Components[comp.Key]);
                UpdateComponentsArray();
            }
            e.OnComponentAdded = OnComponentAdded;
            e.OnComponentRemoved = OnComponentRemoved;
        }
        /// <summary>
        /// Remove an entity from the scene
        /// </summary>
        /// <param name="e">entity object</param>
        public void RemoveEntity(Entity e)
        {
            foreach (var comp in e.Components)
            {
                if (_componentsList.ContainsKey(comp.Key) == false)
                    continue;
                _componentsList[comp.Key].Remove(e.Components[comp.Key]);
                UpdateComponentsArray();
            }
            e.OnComponentAdded = OnComponentAdded;
            e.OnComponentRemoved = OnComponentRemoved;
            _entities.Remove(e);
        }
        /// <summary>
        /// Add a type of system into the scene
        /// </summary>
        /// <typeparam name="T">type of system</typeparam>
        /// <returns>an instance of the system</returns>
        public T AddSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)))
                return null;
            var newSystem = BaseSystem.Create<T>(this);
            _systems.Add(typeof(T), newSystem);
            return newSystem;
        }
        /// <summary>
        /// remove a type of system from the scene
        /// </summary>
        /// <typeparam name="T">type of system</typeparam>
        public void RemoveSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)) == false)
                return;
            _systems.Remove(typeof(T));
        }
        /// <summary>
        /// Get a type of system that is in the scene
        /// </summary>
        /// <typeparam name="T">type of system</typeparam>
        /// <returns>instance of the system</returns>
        public T GetSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)) == false)
                return null;

            return _systems[typeof(T)] as T;
        }

        /// <summary>
        /// Get all specific type of components currently attached to all entities in the scene
        /// </summary>
        /// <typeparam name="T">type of component to get</typeparam>
        /// <returns>array of type T components</returns>
        public T[] GetComponents<T>() where T : BaseComponent, new()
        {
            if (_components.ContainsKey(typeof(T)) == false)
                return new T[] { };
            var compArray = _components[typeof(T)];
            var rtn = new T[compArray.Length];
            for (int i = 0; i < compArray.Length; i++)
                rtn[i] = compArray[i] as T;
            return rtn;
        }
    }
}