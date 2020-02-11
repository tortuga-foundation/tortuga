using System;
using System.Collections.Generic;

namespace Tortuga.Core
{
    public class Scene
    {
        public List<Entity> Entities => _entities;
        public Dictionary<Type, List<BaseComponent>> EntitiesWithComponent => _components;
        public Dictionary<Type, BaseSystem> Systems => _systems;

        private List<Entity> _entities;
        private Dictionary<Type, List<BaseComponent>> _components;

        private Dictionary<Type, BaseSystem> _systems;

        public Scene()
        {
            _entities = new List<Entity>();
            _components = new Dictionary<Type, List<BaseComponent>>();
            _systems = new Dictionary<Type, BaseSystem>();
        }

        private void OnComponentAdded<T>(Entity entity, T component)
        {
            if (_components.ContainsKey(typeof(T)) == false)
                _components[typeof(T)] = new List<BaseComponent>();
            _components[typeof(T)].Add(entity.Components[typeof(T)]);
        }
        private void OnComponentRemoved<T>(Entity entity, T component)
        {
            if (_components.ContainsKey(typeof(T)) == false)
                return;
            _components[typeof(T)].Remove(entity.Components[typeof(T)]);
        }

        public void AddEntity(Entity e)
        {
            _entities.Add(e);
            foreach (var comp in e.Components)
            {
                if (_components.ContainsKey(comp.Key) == false)
                    _components[comp.Key] = new List<BaseComponent>();

                _components[comp.Key].Add(e.Components[comp.Key]);
            }
            e.OnComponentAdded = OnComponentAdded;
            e.OnComponentRemoved = OnComponentRemoved;
        }
        public void RemoveEntity(Entity e)
        {
            foreach (var comp in e.Components)
            {
                if (_components.ContainsKey(comp.Key) == false)
                    continue;
                _components[comp.Key].Remove(e.Components[comp.Key]);
            }
            e.OnComponentAdded = OnComponentAdded;
            e.OnComponentRemoved = OnComponentRemoved;
            _entities.Remove(e);
        }
        public void AddSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)))
                return;
            _systems.Add(typeof(T), BaseSystem.Create<T>(this));
        }
        public void RemoveSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)) == false)
                return;
            _systems.Remove(typeof(T));
        }

        public T[] GetComponents<T>() where T : BaseComponent, new()
        {
            var compArray = _components[typeof(T)].ToArray();
            var rtn = new List<T>();
            foreach (var t in compArray)
            {
                var c = t as T;
                if (c != null)
                    rtn.Add(c);
            }
            return rtn.ToArray();
        }
    }
}