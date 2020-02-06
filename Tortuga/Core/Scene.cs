using System;
using System.Collections.Generic;

namespace Tortuga.Core
{
    public class Scene
    {
        public List<Entity> Entities => _entities;
        public Dictionary<Type, List<Entity>> EntitiesWithComponent => _components;
        public Dictionary<Type, BaseSystem> Systems => _systems;

        private List<Entity> _entities;
        private Dictionary<Type, List<Entity>> _components;

        private Dictionary<Type, BaseSystem> _systems;

        public Scene()
        {
            _entities = new List<Entity>();
            _components = new Dictionary<Type, List<Entity>>();
            _systems = new Dictionary<Type, BaseSystem>();
        }

        private void OnComponentAdded<T>(Entity entity, T component)
        {
            if (_components.ContainsKey(typeof(T)) == false)
                _components[typeof(T)] = new List<Entity>();
            _components[typeof(T)].Add(entity);
        }
        private void OnComponentRemoved<T>(Entity entity, T component)
        {
            if (_components.ContainsKey(typeof(T)) == false)
                return;
            _components[typeof(T)].Remove(entity);
        }

        public void AddEntity(Entity e)
        {
            _entities.Add(e);
            foreach (var comp in e.Components)
            {
                if (_components.ContainsKey(comp.Key) == false)
                    _components[comp.Key] = new List<Entity>();

                _components[comp.Key].Add(e);
            }
            e.OnComponentAdded += OnComponentAdded;
            e.OnComponentAdded += OnComponentRemoved;
        }
        public void RemoveEntity(Entity e)
        {
            foreach (var comp in e.Components)
            {
                if (_components.ContainsKey(comp.Key) == false)
                    continue;
                _components[comp.Key].Remove(e);
            }
            e.OnComponentAdded -= OnComponentAdded;
            e.OnComponentAdded -= OnComponentRemoved;
            _entities.Remove(e);
        }
        public void AddSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)))
                return;
            _systems.Add(typeof(T), new T());
        }
        public void RemoveSystem<T>() where T : BaseSystem, new()
        {
            if (_systems.ContainsKey(typeof(T)) == false)
                return;
            _systems.Remove(typeof(T));
        }
    }
}