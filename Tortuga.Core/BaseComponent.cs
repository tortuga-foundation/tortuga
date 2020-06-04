#pragma warning disable 1711

#if TORTUGA_PROFILER
using System.Collections.Generic;
#endif
using System.Threading.Tasks;

namespace Tortuga.Core
{
    /// <summary>
    /// base component
    /// NOTE: please use BaseComponent.Create instead of constructor
    /// </summary>
    public class BaseComponent
    {
#if TORTUGA_PROFILER
        
        /// <summary>
        /// Records how long each function takes for debugging
        /// </summary>
        /// <typeparam name="string">function name</typeparam>
        /// <typeparam name="long">time in miliseconds</typeparam>
        public Dictionary<string, long> FunctionDuration = new Dictionary<string, long>();
        
        internal void CreateOrUpdateDuration(string key, long duration)
        {
            if (FunctionDuration.ContainsKey(key) == false)
                FunctionDuration.Add(key, duration);
            else
                FunctionDuration[key] = duration;
        }

#endif

        /// <summary>
        /// returns the entity, this component is attached to
        /// </summary>
        public Entity MyEntity => _myEntity;
        private Entity _myEntity;

        /// <summary>
        /// this method runs once per frame
        /// </summary>
        public virtual async Task Update() { await Task.Run(() => { }); }
        /// <summary>
        /// this method runs once per frame before update
        /// </summary>
        public virtual async Task EarlyUpdate() { await Task.Run(() => { }); }
        /// <summary>
        /// All GUI commands must run inside this function
        /// </summary>
        public virtual async Task OnGui() { await Task.Run(() => { }); }
        /// <summary>
        /// this method runs once per frame after update
        /// </summary>
        public virtual async Task LateUpdate() { await Task.Run(() => { }); }
        /// <summary>
        /// This method runs when a component is attached to an entity
        /// </summary>
        public virtual async Task OnEnable() { await Task.Run(() => { }); }
        /// <summary>
        /// This method runs when a component is removed from an entity
        /// </summary>
        public virtual async Task OnDisable() { await Task.Run(() => { }); }

        /// <summary>
        /// This method is used to create a new component and setup all objects required for the component
        /// </summary>
        /// <param name="e">entity this component is attached to</param>
        /// <typeparam name="T">type of component</typeparam>
        /// <returns>an instance of the component</returns>
        public static T Create<T>(Entity e) where T : BaseComponent, new()
        {
            var t = new T();
            t._myEntity = e;
            return t;
        }
    }
}