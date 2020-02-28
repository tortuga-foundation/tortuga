using System;
using System.Collections.Generic;

namespace Tortuga.Core
{
    public class Scene
    {
        public List<Entity> Entities => _entities;
        public Dictionary<Type, BaseComponent[]> EntitiesWithComponent => _components;
        public Dictionary<Type, BaseSystem> Systems => _systems;

        private List<Entity> _entities;
        private Dictionary<Type, BaseComponent[]> _components;
        private Dictionary<Type, List<BaseComponent>> _componentsList;

        private Dictionary<Type, BaseSystem> _systems;

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
        public T AddSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)))
                return null;
            var newSystem = BaseSystem.Create<T>(this);
            _systems.Add(typeof(T), newSystem);
            return newSystem;
        }
        public void RemoveSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)) == false)
                return;
            _systems.Remove(typeof(T));
        }
        public T GetSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)) == false)
                return null;

            return _systems[typeof(T)] as T;
        }

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